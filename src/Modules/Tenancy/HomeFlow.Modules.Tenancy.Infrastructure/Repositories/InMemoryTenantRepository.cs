using System.Collections.Concurrent;
using HomeFlow.BuildingBlocks.MultiTenancy.Models;
using HomeFlow.Modules.Tenancy.Domain.Aggregates;
using HomeFlow.Modules.Tenancy.Domain.Repositories;

namespace HomeFlow.Modules.Tenancy.Infrastructure.Repositories;

public sealed class InMemoryTenantRepository : ITenantRepository
{
    private static readonly ConcurrentDictionary<Guid, Tenant> Store = new();

    public Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        Store[tenant.Id.Value] = tenant;
        return Task.CompletedTask;
    }

    public Task<Tenant?> GetByIdAsync(TenantId tenantId, CancellationToken cancellationToken = default)
    {
        Store.TryGetValue(tenantId.Value, out var tenant);
        return Task.FromResult(tenant);
    }
}