namespace HomeFlow.Modules.Scheduling.Application.Queries.GetAppointmentsForDateRange;

public sealed record AppointmentSummaryDto(
    Guid AppointmentId,
    string Title,
    DateTime StartsAtUtc,
    DateTime EndsAtUtc,
    string? Location,
    string Status,
    IReadOnlyCollection<AppointmentParticipantDto> Participants);