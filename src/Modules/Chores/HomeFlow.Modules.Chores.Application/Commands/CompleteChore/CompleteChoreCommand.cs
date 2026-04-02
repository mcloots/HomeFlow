namespace HomeFlow.Modules.Chores.Application.Commands.CompleteChore;

public sealed record CompleteChoreCommand(
    Guid ChoreId,
    Guid CompletedByMemberId,
    DateTime? CompletedAtUtc);
