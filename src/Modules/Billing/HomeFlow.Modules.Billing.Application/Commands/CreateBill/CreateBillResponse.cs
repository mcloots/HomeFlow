namespace HomeFlow.Modules.Billing.Application.Commands.CreateBill;

public sealed record CreateBillResponse(
    Guid BillId,
    Guid TenantId,
    Guid HouseholdId,
    string Title,
    decimal Amount,
    DateTime DueDateUtc,
    string Status,
    string? Category,
    DateTime? PaidAtUtc);
