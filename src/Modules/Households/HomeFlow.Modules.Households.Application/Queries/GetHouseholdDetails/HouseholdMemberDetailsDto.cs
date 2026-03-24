namespace HomeFlow.Modules.Households.Application.Queries.GetHouseholdDetails;

public sealed record HouseholdMemberDetailsDto(
    Guid MemberId,
    string DisplayName,
    string Email,
    string Role);