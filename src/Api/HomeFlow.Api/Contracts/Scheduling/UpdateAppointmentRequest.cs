namespace HomeFlow.Api.Contracts.Scheduling;

public sealed record UpdateAppointmentRequest(
    string Title,
    string? Description,
    DateTime StartsAtUtc,
    DateTime EndsAtUtc,
    string? Location,
    string? Type,
    string? Status,
    IReadOnlyCollection<Guid> ParticipantMemberIds);
