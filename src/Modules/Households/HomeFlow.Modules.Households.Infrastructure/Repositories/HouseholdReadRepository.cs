using HomeFlow.BuildingBlocks.Infrastructure.Persistence;
using HomeFlow.Modules.Households.Application.Abstractions;
using HomeFlow.Modules.Households.Application.Queries.GetHouseholdDetails;
using HomeFlow.Modules.Households.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;

namespace HomeFlow.Modules.Households.Infrastructure.Repositories;

public sealed class HouseholdReadRepository : IHouseholdReadRepository
{
    private readonly HomeFlowDbContext _dbContext;

    public HouseholdReadRepository(HomeFlowDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetHouseholdDetailsResponse?> GetDetailsByIdAsync(
        Guid householdId,
        CancellationToken cancellationToken = default)
    {
        var householdTypedId = new HomeFlow.Modules.Households.Domain.Ids.HouseholdId(householdId);

        var household = await _dbContext.Households
            .Include(x => x.Members)
            .SingleOrDefaultAsync(x => x.Id == householdTypedId, cancellationToken);

        if (household is null)
            return null;

        var invitations = await _dbContext.HouseholdInvitations
            .Where(x => x.HouseholdId == household.Id)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return new GetHouseholdDetailsResponse(
            household.Id.Value,
            household.TenantId.Value,
            household.Name,
            household.Status.ToString(),
            household.Members
                .Select(m => new HouseholdMemberDetailsDto(
                    m.Id.Value,
                    m.DisplayName,
                    m.Email,
                    m.Role.ToString()))
                .ToList(),
            invitations
                .Select(i => new HouseholdInvitationDetailsDto(
                    i.Id.Value,
                    i.Email,
                    i.Role.ToString(),
                    i.Status.ToString(),
                    i.CreatedAtUtc))
                .ToList());
    }
}