namespace HomeFlow.Modules.Chores.Application.Commands.CompleteChore;

public sealed record CompleteChoreResponse(
    Guid ChoreId,
    string Status,
    DateTime CompletedAtUtc,
    Guid CompletedByMemberId,
    Guid? NextChoreId);
