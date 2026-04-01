namespace HomeFlow.Modules.Scheduling.Application.Queries.GetAppointmentDetails;

public sealed record GetAppointmentDetailsResponse(
    Guid AppointmentId,
    Guid TenantId,
    Guid HouseholdId,
    string Title,
    string? Description,
    DateTime StartsAtUtc,
    DateTime EndsAtUtc,
    string? Location,
    string Type,
    string Status,
    IReadOnlyCollection<AppointmentDetailsParticipantDto> Participants);
