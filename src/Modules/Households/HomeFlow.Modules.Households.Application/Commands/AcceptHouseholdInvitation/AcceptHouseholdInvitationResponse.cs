namespace HomeFlow.Modules.Households.Application.Commands.AcceptHouseholdInvitation;

public sealed record AcceptHouseholdInvitationResponse(
    Guid InvitationId,
    Guid HouseholdId,
    Guid MemberId,
    string Email,
    string Status);