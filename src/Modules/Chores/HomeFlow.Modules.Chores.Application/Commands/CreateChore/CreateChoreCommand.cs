namespace HomeFlow.Modules.Chores.Application.Commands.CreateChore;

public sealed record CreateChoreCommand(
    Guid TenantId,
    Guid HouseholdId,
    string Title,
    string? Description,
    DateTime DueDateUtc,
    Guid? AssignedMemberId,
    string? Recurrence,
    int? RecurrenceMonths);
