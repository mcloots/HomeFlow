namespace HomeFlow.Api.Contracts.Scheduling;

public sealed record CreateAppointmentRequest(
    Guid TenantId,
    Guid HouseholdId,
    string Title,
    string? Description,
    DateTime StartsAtUtc,
    DateTime EndsAtUtc,
    string? Location,
    string? Type,
    IReadOnlyCollection<Guid> ParticipantMemberIds);
