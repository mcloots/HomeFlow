namespace HomeFlow.Modules.Households.Application.Queries.GetHouseholdDetails;

public sealed record HouseholdInvitationDetailsDto(
    Guid InvitationId,
    string Email,
    string Role,
    string Status,
    DateTime CreatedAtUtc);