namespace HomeFlow.Api.Contracts.Scheduling;

public sealed record UpdateAppointmentRequest(
    string Title,
    string? Description,
    DateTime StartsAtUtc,
    DateTime EndsAtUtc,
    string? Location,
    IReadOnlyCollection<Guid> ParticipantMemberIds);