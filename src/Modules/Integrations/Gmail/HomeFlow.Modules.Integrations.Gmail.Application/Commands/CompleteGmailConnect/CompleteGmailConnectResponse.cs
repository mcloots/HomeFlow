namespace HomeFlow.Modules.Integrations.Gmail.Application.Commands.CompleteGmailConnect;

public sealed record CompleteGmailConnectResponse(
    Guid GmailConnectionId,
    Guid TenantId,
    Guid HouseholdId,
    string GoogleEmail,
    string Status);