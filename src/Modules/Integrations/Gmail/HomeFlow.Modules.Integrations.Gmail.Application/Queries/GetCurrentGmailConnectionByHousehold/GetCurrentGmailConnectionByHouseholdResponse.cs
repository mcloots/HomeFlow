namespace HomeFlow.Modules.Integrations.Gmail.Application.Queries.GetCurrentGmailConnectionByHousehold;

public sealed record GetCurrentGmailConnectionByHouseholdResponse(
    Guid GmailConnectionId,
    Guid TenantId,
    Guid HouseholdId,
    string GoogleEmail,
    string Status,
    DateTime ConnectedAtUtc);