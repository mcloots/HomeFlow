namespace HomeFlow.Modules.Integrations.Gmail.Application.Commands.CompleteGmailConnect;

public sealed record CompleteGmailConnectCommand(
    string Code,
    string State);