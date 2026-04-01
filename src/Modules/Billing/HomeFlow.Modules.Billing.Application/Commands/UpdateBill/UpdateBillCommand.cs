namespace HomeFlow.Modules.Billing.Application.Commands.UpdateBill;

public sealed record UpdateBillCommand(
    Guid BillId,
    string Title,
    decimal Amount,
    DateTime DueDateUtc,
    string? Category,
    string? Status,
    DateTime? PaidAtUtc);
