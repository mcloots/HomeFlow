namespace HomeFlow.Modules.Scheduling.Application.Commands.CreateAppointment;

public sealed record CreateAppointmentCommand(
    Guid TenantId,
    Guid HouseholdId,
    string Title,
    string? Description,
    DateTime StartsAtUtc,
    DateTime EndsAtUtc,
    string? Location,
    string? Type,
    IReadOnlyCollection<Guid> ParticipantMemberIds);
