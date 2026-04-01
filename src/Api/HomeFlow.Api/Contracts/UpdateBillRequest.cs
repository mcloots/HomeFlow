namespace HomeFlow.Api.Contracts.Billing;

public sealed record UpdateBillRequest(
    string Title,
    decimal Amount,
    DateTime DueDateUtc,
    string? Category,
    string? Status,
    DateTime? PaidAtUtc);
