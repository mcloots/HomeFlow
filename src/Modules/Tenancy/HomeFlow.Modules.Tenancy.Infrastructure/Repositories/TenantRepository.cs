using HomeFlow.BuildingBlocks.Infrastructure.Persistence;
using HomeFlow.BuildingBlocks.MultiTenancy.Models;
using HomeFlow.Modules.Tenancy.Domain.Aggregates;
using HomeFlow.Modules.Tenancy.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HomeFlow.Modules.Tenancy.Infrastructure.Repositories;

public sealed class TenantRepository : ITenantRepository
{
    private readonly HomeFlowDbContext _dbContext;

    public TenantRepository(HomeFlowDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        await _dbContext.Tenants.AddAsync(tenant, cancellationToken);
    }

    public Task<Tenant?> GetByIdAsync(TenantId tenantId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Tenants
            .SingleOrDefaultAsync(x => x.Id == tenantId, cancellationToken);
    }
}