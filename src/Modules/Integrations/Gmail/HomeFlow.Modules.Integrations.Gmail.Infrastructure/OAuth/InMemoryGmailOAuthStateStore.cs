using System.Collections.Concurrent;
using HomeFlow.BuildingBlocks.MultiTenancy.Models;
using HomeFlow.Modules.Households.Domain.Ids;
using HomeFlow.Modules.Integrations.Gmail.Application.Abstractions;

namespace HomeFlow.Modules.Integrations.Gmail.Infrastructure.OAuth;

public sealed class InMemoryGmailOAuthStateStore : IGmailOAuthStateStore
{
    private static readonly ConcurrentDictionary<string, GmailOAuthStatePayload> Store = new();

    public string Create(TenantId tenantId, HouseholdId householdId)
    {
        var state = Guid.NewGuid().ToString("N");
        Store[state] = new GmailOAuthStatePayload(tenantId, householdId);
        return state;
    }

    public GmailOAuthStatePayload? Consume(string state)
    {
        Store.TryRemove(state, out var payload);
        return payload;
    }
}