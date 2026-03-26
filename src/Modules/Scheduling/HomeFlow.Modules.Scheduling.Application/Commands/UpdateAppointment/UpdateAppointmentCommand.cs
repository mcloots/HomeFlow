namespace HomeFlow.Modules.Scheduling.Application.Commands.UpdateAppointment;

public sealed record UpdateAppointmentCommand(
    Guid AppointmentId,
    string Title,
    string? Description,
    DateTime StartsAtUtc,
    DateTime EndsAtUtc,
    string? Location,
    IReadOnlyCollection<Guid> ParticipantMemberIds);