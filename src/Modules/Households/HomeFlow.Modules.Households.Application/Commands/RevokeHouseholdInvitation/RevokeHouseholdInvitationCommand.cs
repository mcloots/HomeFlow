namespace HomeFlow.Modules.Households.Application.Commands.RevokeHouseholdInvitation;

public sealed record RevokeHouseholdInvitationCommand(
    Guid InvitationId,
    string RequestedByEmail);