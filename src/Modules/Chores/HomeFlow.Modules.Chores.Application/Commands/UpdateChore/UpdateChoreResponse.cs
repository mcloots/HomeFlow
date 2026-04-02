namespace HomeFlow.Modules.Chores.Application.Commands.UpdateChore;

public sealed record UpdateChoreResponse(
    Guid ChoreId,
    string Title,
    DateTime DueDateUtc,
    Guid? AssignedMemberId,
    string Status,
    string Recurrence,
    int? RecurrenceMonths);
