namespace HomeFlow.Modules.Billing.Application.Queries.GetBillDetails;

public sealed record GetBillDetailsResponse(
    Guid BillId,
    Guid TenantId,
    Guid HouseholdId,
    string Title,
    decimal Amount,
    DateTime DueDateUtc,
    string Status,
    string? Category,
    DateTime? PaidAtUtc);
