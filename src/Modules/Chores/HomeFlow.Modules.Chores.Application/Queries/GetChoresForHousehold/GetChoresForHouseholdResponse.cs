namespace HomeFlow.Modules.Chores.Application.Queries.GetChoresForHousehold;

public sealed record GetChoresForHouseholdResponse(
    Guid HouseholdId,
    IReadOnlyCollection<ChoreSummaryDto> Chores);
