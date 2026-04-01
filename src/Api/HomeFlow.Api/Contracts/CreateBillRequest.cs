namespace HomeFlow.Api.Contracts.Billing;

public sealed record CreateBillRequest(
    Guid TenantId,
    Guid HouseholdId,
    string Title,
    decimal Amount,
    DateTime DueDateUtc,
    string? Category);
