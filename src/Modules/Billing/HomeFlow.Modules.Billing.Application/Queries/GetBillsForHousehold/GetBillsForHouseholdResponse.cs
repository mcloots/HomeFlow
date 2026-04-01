namespace HomeFlow.Modules.Billing.Application.Queries.GetBillsForHousehold;

public sealed record GetBillsForHouseholdResponse(
    Guid HouseholdId,
    IReadOnlyCollection<BillSummaryDto> Bills);
