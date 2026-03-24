namespace HomeFlow.Modules.Households.Application.Queries.GetHouseholdDetails;

public sealed record GetHouseholdDetailsResponse(
    Guid HouseholdId,
    Guid TenantId,
    string Name,
    string Status,
    IReadOnlyCollection<HouseholdMemberDetailsDto> Members,
    IReadOnlyCollection<HouseholdInvitationDetailsDto> Invitations);