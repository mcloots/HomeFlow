namespace HomeFlow.Modules.Households.Application.Commands.DeclineHouseholdInvitation;

public sealed record DeclineHouseholdInvitationResponse(
    Guid InvitationId,
    string Email,
    string Status);