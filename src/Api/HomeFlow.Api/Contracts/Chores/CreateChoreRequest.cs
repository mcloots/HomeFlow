namespace HomeFlow.Api.Contracts.Chores;

public sealed record CreateChoreRequest(
    Guid TenantId,
    Guid HouseholdId,
    string Title,
    string? Description,
    DateTime DueDateUtc,
    Guid? AssignedMemberId,
    string? Recurrence,
    int? RecurrenceMonths);
