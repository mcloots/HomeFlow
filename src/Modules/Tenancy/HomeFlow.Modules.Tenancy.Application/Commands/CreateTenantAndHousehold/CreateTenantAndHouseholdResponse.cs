namespace HomeFlow.Modules.Tenancy.Application.Commands.CreateTenantAndHousehold;

public sealed record CreateTenantAndHouseholdResponse(
    Guid TenantId,
    Guid HouseholdId,
    Guid OwnerMemberId,
    string TenantName,
    string HouseholdName);