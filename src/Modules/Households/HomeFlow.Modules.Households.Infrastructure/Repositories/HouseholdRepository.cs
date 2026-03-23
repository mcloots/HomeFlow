using HomeFlow.BuildingBlocks.Infrastructure.Persistence;
using HomeFlow.Modules.Households.Domain.Aggregates;
using HomeFlow.Modules.Households.Domain.Ids;
using HomeFlow.Modules.Households.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HomeFlow.Modules.Households.Infrastructure.Repositories;

public sealed class HouseholdRepository : IHouseholdRepository
{
    private readonly HomeFlowDbContext _dbContext;

    public HouseholdRepository(HomeFlowDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Household household, CancellationToken cancellationToken = default)
    {
        await _dbContext.Households.AddAsync(household, cancellationToken);
    }

    public Task<Household?> GetByIdAsync(HouseholdId householdId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Households
            .SingleOrDefaultAsync(x => x.Id == householdId, cancellationToken);
    }

    public Task<bool> MemberEmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLower();

        return _dbContext.Set<HouseholdMember>()
            .AnyAsync(x => x.Email == normalizedEmail, cancellationToken);
    }
}