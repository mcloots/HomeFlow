namespace HomeFlow.Modules.Integrations.Gmail.Application.Commands.StartGmailConnect;

public sealed record StartGmailConnectCommand(
    Guid TenantId,
    Guid HouseholdId);