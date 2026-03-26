namespace HomeFlow.Modules.Integrations.Gmail.Application.Commands.DisconnectGmailConnection;

public sealed record DisconnectGmailConnectionCommand(
    Guid GmailConnectionId);