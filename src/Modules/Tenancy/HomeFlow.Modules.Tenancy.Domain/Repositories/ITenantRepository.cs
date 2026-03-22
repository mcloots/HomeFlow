using HomeFlow.BuildingBlocks.MultiTenancy.Models;
using HomeFlow.Modules.Tenancy.Domain.Aggregates;

namespace HomeFlow.Modules.Tenancy.Domain.Repositories;

public interface ITenantRepository
{
    Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default);
    Task<Tenant?> GetByIdAsync(TenantId tenantId, CancellationToken cancellationToken = default);
}