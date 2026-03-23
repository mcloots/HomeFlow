namespace HomeFlow.Modules.Tenancy.Application.Commands.CreateTenantAndHousehold;

public sealed record CreateTenantAndHouseholdCommand(
    string TenantName,
    string HouseholdName,
    string OwnerDisplayName,
    string OwnerEmail);