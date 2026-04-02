namespace HomeFlow.Api.Contracts.Chores;

public sealed record CompleteChoreRequest(
    Guid CompletedByMemberId,
    DateTime? CompletedAtUtc);
