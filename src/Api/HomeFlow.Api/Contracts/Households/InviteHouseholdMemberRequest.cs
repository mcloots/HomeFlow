namespace HomeFlow.Api.Contracts.Households;

public sealed record InviteHouseholdMemberRequest(
    string InvitedEmail,
    int Role,
    string RequestedByEmail);