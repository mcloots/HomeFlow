namespace HomeFlow.Modules.Billing.Application.Queries.GetBillsForHousehold;

public sealed record BillSummaryDto(
    Guid BillId,
    string Title,
    decimal Amount,
    DateTime DueDateUtc,
    string Status,
    string? Category,
    DateTime? PaidAtUtc);
