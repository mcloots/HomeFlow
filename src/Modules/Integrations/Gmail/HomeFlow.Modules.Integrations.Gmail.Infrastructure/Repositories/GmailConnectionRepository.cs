using HomeFlow.BuildingBlocks.Infrastructure.Persistence;
using HomeFlow.Modules.Households.Domain.Ids;
using HomeFlow.Modules.Integrations.Gmail.Domain.Aggregates;
using HomeFlow.Modules.Integrations.Gmail.Domain.Enums;
using HomeFlow.Modules.Integrations.Gmail.Domain.Ids;
using HomeFlow.Modules.Integrations.Gmail.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HomeFlow.Modules.Integrations.Gmail.Infrastructure.Repositories;

public sealed class GmailConnectionRepository : IGmailConnectionRepository
{
    private readonly HomeFlowDbContext _dbContext;

    public GmailConnectionRepository(HomeFlowDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(GmailConnection connection, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<GmailConnection>().AddAsync(connection, cancellationToken);
    }

    public Task<GmailConnection?> GetByIdAsync(GmailConnectionId connectionId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Set<GmailConnection>()
            .SingleOrDefaultAsync(x => x.Id == connectionId, cancellationToken);
    }

    public Task<GmailConnection?> GetActiveByHouseholdIdAsync(HouseholdId householdId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Set<GmailConnection>()
            .SingleOrDefaultAsync(
                x => x.HouseholdId == householdId && x.Status == GmailConnectionStatus.Active,
                cancellationToken);
    }
}