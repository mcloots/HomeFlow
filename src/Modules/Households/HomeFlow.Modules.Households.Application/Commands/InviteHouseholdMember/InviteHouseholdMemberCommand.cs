using HomeFlow.Modules.Households.Domain.Enums;

namespace HomeFlow.Modules.Households.Application.Commands.InviteHouseholdMember;

public sealed record InviteHouseholdMemberCommand(
    Guid HouseholdId,
    string InvitedEmail,
    HouseholdRole Role,
    string RequestedByEmail);