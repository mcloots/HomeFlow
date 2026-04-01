namespace HomeFlow.Modules.Households.Application.Queries.GetHouseholdMembers;

public sealed record HouseholdMemberListItemDto(
    Guid MemberId,
    string DisplayName,
    string Email,
    string Role);
