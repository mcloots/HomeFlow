namespace HomeFlow.Modules.Chores.Application.Commands.CreateChore;

public sealed record CreateChoreResponse(
    Guid ChoreId,
    Guid TenantId,
    Guid HouseholdId,
    string Title,
    DateTime DueDateUtc,
    Guid? AssignedMemberId,
    string Status,
    string Recurrence,
    int? RecurrenceMonths);
