namespace HomeFlow.Modules.Scheduling.Application.Commands.CreateAppointment;

public sealed record CreateAppointmentResponse(
    Guid AppointmentId,
    Guid TenantId,
    Guid HouseholdId,
    string Title,
    DateTime StartsAtUtc,
    DateTime EndsAtUtc,
    string Status);