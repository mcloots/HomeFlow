using HomeFlow.BuildingBlocks.Application.Abstractions;
using HomeFlow.BuildingBlocks.Infrastructure.Persistence;
using HomeFlow.Modules.Chores.Application.Abstractions;
using HomeFlow.Modules.Chores.Application.Queries.GetChoresForHousehold;
using HomeFlow.Modules.Chores.Domain.Aggregates;
using HomeFlow.Modules.Chores.Domain.Enums;
using HomeFlow.Modules.Households.Domain.Ids;
using Microsoft.EntityFrameworkCore;

namespace HomeFlow.Modules.Chores.Infrastructure.Repositories;

public sealed class ChoreReadRepository : IChoreReadRepository
{
    private readonly HomeFlowDbContext _dbContext;
    private readonly IDateTimeProvider _dateTimeProvider;

    public ChoreReadRepository(
        HomeFlowDbContext dbContext,
        IDateTimeProvider dateTimeProvider)
    {
        _dbContext = dbContext;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<GetChoresForHouseholdResponse> GetForHouseholdAsync(
        Guid householdId,
        CancellationToken cancellationToken = default)
    {
        var typedHouseholdId = new HouseholdId(householdId);

        var chores = await _dbContext.Set<Chore>()
            .Where(x => x.HouseholdId == typedHouseholdId)
            .OrderBy(x => x.Status)
            .ThenBy(x => x.DueDateUtc)
            .ToListAsync(cancellationToken);

        var memberIds = chores
            .SelectMany(chore =>
            {
                var ids = new List<HouseholdMemberId>();

                if (chore.AssignedMemberId is { } assignedMemberId)
                    ids.Add(assignedMemberId);

                if (chore.CompletedByMemberId is { } completedByMemberId)
                    ids.Add(completedByMemberId);

                return ids;
            })
            .Distinct()
            .ToList();

        var members = await _dbContext.Set<HouseholdMember>()
            .Where(x => memberIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.DisplayName, cancellationToken);

        var choreDtos = chores
            .Select(chore => new ChoreSummaryDto(
                chore.Id.Value,
                chore.Title,
                chore.Description,
                chore.DueDateUtc,
                chore.Status.ToString(),
                chore.Status == ChoreStatus.Pending && chore.DueDateUtc < _dateTimeProvider.UtcNow,
                chore.Recurrence.ToString(),
                chore.RecurrenceMonths,
                chore.AssignedMemberId?.Value,
                chore.AssignedMemberId is { } assignedMember ? members.GetValueOrDefault(assignedMember) : null,
                chore.CompletedAtUtc,
                chore.CompletedByMemberId?.Value,
                chore.CompletedByMemberId is { } completedByMember ? members.GetValueOrDefault(completedByMember) : null))
            .ToList();

        return new GetChoresForHouseholdResponse(householdId, choreDtos);
    }
}
