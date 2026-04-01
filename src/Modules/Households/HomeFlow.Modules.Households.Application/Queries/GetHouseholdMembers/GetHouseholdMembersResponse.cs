namespace HomeFlow.Modules.Households.Application.Queries.GetHouseholdMembers;

public sealed record GetHouseholdMembersResponse(
    Guid HouseholdId,
    IReadOnlyCollection<HouseholdMemberListItemDto> Members);
