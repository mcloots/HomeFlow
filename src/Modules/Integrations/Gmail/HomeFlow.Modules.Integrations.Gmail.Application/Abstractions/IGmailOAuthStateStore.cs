using HomeFlow.BuildingBlocks.MultiTenancy.Models;
using HomeFlow.Modules.Households.Domain.Ids;

namespace HomeFlow.Modules.Integrations.Gmail.Application.Abstractions;

public interface IGmailOAuthStateStore
{
    string Create(TenantId tenantId, HouseholdId householdId);
    GmailOAuthStatePayload? Consume(string state);
}

public sealed record GmailOAuthStatePayload(
    TenantId TenantId,
    HouseholdId HouseholdId);