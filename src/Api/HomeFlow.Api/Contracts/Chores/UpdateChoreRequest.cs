namespace HomeFlow.Api.Contracts.Chores;

public sealed record UpdateChoreRequest(
    string Title,
    string? Description,
    DateTime DueDateUtc,
    Guid? AssignedMemberId,
    string? Recurrence,
    int? RecurrenceMonths);
