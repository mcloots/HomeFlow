namespace HomeFlow.Modules.Households.Application.Commands.RevokeHouseholdInvitation;

public sealed record RevokeHouseholdInvitationResponse(
    Guid InvitationId,
    Guid HouseholdId,
    string Email,
    string Status);