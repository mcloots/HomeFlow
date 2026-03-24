namespace HomeFlow.Api.Contracts.Households;

public sealed record AcceptHouseholdInvitationRequest(
    string DisplayName,
    string Email);