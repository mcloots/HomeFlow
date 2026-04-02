using HomeFlow.BuildingBlocks.Application.Abstractions.Persistence;
using HomeFlow.BuildingBlocks.MultiTenancy.Models;
using HomeFlow.Modules.Chores.Application.Abstractions;
using HomeFlow.Modules.Chores.Domain.Aggregates;
using HomeFlow.Modules.Chores.Domain.Enums;
using HomeFlow.Modules.Chores.Domain.Ids;
using HomeFlow.Modules.Chores.Domain.Repositories;
using HomeFlow.Modules.Households.Domain.Ids;

namespace HomeFlow.Modules.Chores.Application.Commands.CreateChore;

public sealed class CreateChoreHandler
{
    private readonly IChoreRepository _choreRepository;
    private readonly IHouseholdMemberLookup _householdMemberLookup;
    private readonly IUnitOfWork _unitOfWork;

    public CreateChoreHandler(
        IChoreRepository choreRepository,
        IHouseholdMemberLookup householdMemberLookup,
        IUnitOfWork unitOfWork)
    {
        _choreRepository = choreRepository;
        _householdMemberLookup = householdMemberLookup;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateChoreResponse> Handle(
        CreateChoreCommand command,
        CancellationToken cancellationToken = default)
    {
        var tenantId = new TenantId(command.TenantId);
        var householdId = new HouseholdId(command.HouseholdId);
        var assignedMemberId = command.AssignedMemberId is Guid memberId
            ? new HouseholdMemberId(memberId)
            : (HouseholdMemberId?)null;

        if (assignedMemberId is not null)
        {
            var belongsToHousehold = await _householdMemberLookup.AllBelongToHouseholdAsync(
                householdId,
                [assignedMemberId.Value],
                cancellationToken);

            if (!belongsToHousehold)
                throw new InvalidOperationException("Assigned member does not belong to the household.");
        }

        var recurrence = ParseRecurrence(command.Recurrence);
        var chore = Chore.Create(
            ChoreId.New(),
            tenantId,
            householdId,
            command.Title,
            command.Description,
            command.DueDateUtc,
            assignedMemberId,
            recurrence,
            command.RecurrenceMonths);

        await _choreRepository.AddAsync(chore, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateChoreResponse(
            chore.Id.Value,
            chore.TenantId.Value,
            chore.HouseholdId.Value,
            chore.Title,
            chore.DueDateUtc,
            chore.AssignedMemberId?.Value,
            chore.Status.ToString(),
            chore.Recurrence.ToString(),
            chore.RecurrenceMonths);
    }

    internal static ChoreRecurrence ParseRecurrence(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return ChoreRecurrence.None;

        if (Enum.TryParse<ChoreRecurrence>(value, true, out var recurrence) &&
            Enum.IsDefined(recurrence))
        {
            return recurrence;
        }

        throw new InvalidOperationException("Chore recurrence is invalid.");
    }
}
