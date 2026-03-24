using HomeFlow.BuildingBlocks.Infrastructure.Persistence;
using HomeFlow.Modules.Households.Domain.Ids;
using HomeFlow.Modules.Scheduling.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace HomeFlow.Modules.Scheduling.Infrastructure.Repositories;

public sealed class HouseholdMemberLookup : IHouseholdMemberLookup
{
    private readonly HomeFlowDbContext _dbContext;

    public HouseholdMemberLookup(HomeFlowDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> AllBelongToHouseholdAsync(
        HouseholdId householdId,
        IReadOnlyCollection<HouseholdMemberId> memberIds,
        CancellationToken cancellationToken = default)
    {
        if (memberIds.Count == 0)
            return true;

        var count = await _dbContext.Set<HouseholdMember>()
            .Where(x => memberIds.Contains(x.Id))
            .Where(x => EF.Property<HouseholdId>(x, "HouseholdId") == householdId)
            .CountAsync(cancellationToken);

        return count == memberIds.Count;
    }
}