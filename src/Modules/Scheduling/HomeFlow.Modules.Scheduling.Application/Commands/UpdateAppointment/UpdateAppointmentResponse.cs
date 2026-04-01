namespace HomeFlow.Modules.Scheduling.Application.Commands.UpdateAppointment;

public sealed record UpdateAppointmentResponse(
    Guid AppointmentId,
    string Title,
    DateTime StartsAtUtc,
    DateTime EndsAtUtc,
    string Status,
    string Type);
