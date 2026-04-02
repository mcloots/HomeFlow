namespace HomeFlow.Modules.Chores.Application.Commands.UpdateChore;

public sealed record UpdateChoreCommand(
    Guid ChoreId,
    string Title,
    string? Description,
    DateTime DueDateUtc,
    Guid? AssignedMemberId,
    string? Recurrence,
    int? RecurrenceMonths);
