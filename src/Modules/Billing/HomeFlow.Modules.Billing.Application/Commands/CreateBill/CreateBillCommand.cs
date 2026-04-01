namespace HomeFlow.Modules.Billing.Application.Commands.CreateBill;

public sealed record CreateBillCommand(
    Guid TenantId,
    Guid HouseholdId,
    string Title,
    decimal Amount,
    DateTime DueDateUtc,
    string? Category);
