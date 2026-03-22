namespace HomeFlow.Api.Contracts.Onboarding;

public sealed record CreateTenantAndHouseholdRequest(
    string TenantName,
    string HouseholdName);