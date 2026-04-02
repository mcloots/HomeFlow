namespace HomeFlow.Modules.Chores.Application.Queries.GetChoresForHousehold;

public sealed record ChoreSummaryDto(
    Guid ChoreId,
    string Title,
    string? Description,
    DateTime DueDateUtc,
    string Status,
    bool IsOverdue,
    string Recurrence,
    int? RecurrenceMonths,
    Guid? AssignedMemberId,
    string? AssignedMemberName,
    DateTime? CompletedAtUtc,
    Guid? CompletedByMemberId,
    string? CompletedByMemberName);
