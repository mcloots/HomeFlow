namespace HomeFlow.Modules.Households.Application.Commands.InviteHouseholdMember;

public sealed record InviteHouseholdMemberResponse(
    Guid InvitationId,
    Guid HouseholdId,
    string Email,
    string Status);