using System.Collections.Concurrent;
using HomeFlow.Modules.Households.Domain.Aggregates;
using HomeFlow.Modules.Households.Domain.Ids;
using HomeFlow.Modules.Households.Domain.Repositories;

namespace HomeFlow.Modules.Households.Infrastructure.Repositories;

public sealed class InMemoryHouseholdRepository : IHouseholdRepository
{
    private static readonly ConcurrentDictionary<Guid, Household> Store = new();

    public Task AddAsync(Household household, CancellationToken cancellationToken = default)
    {
        Store[household.Id.Value] = household;
        return Task.CompletedTask;
    }

    public Task<Household?> GetByIdAsync(HouseholdId householdId, CancellationToken cancellationToken = default)
    {
        Store.TryGetValue(householdId.Value, out var household);
        return Task.FromResult(household);
    }
}