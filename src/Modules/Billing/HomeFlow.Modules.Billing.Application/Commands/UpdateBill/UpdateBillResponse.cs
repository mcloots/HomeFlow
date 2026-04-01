namespace HomeFlow.Modules.Billing.Application.Commands.UpdateBill;

public sealed record UpdateBillResponse(
    Guid BillId,
    string Title,
    decimal Amount,
    DateTime DueDateUtc,
    string Status,
    string? Category,
    DateTime? PaidAtUtc);
