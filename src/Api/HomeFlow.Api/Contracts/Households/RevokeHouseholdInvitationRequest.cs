namespace HomeFlow.Api.Contracts.Households;

public sealed record RevokeHouseholdInvitationRequest(
    string RequestedByEmail);