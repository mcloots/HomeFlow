namespace HomeFlow.Modules.Scheduling.Application.Queries.GetAppointmentsForDateRange;

public sealed record AppointmentParticipantDto(
    Guid HouseholdMemberId,
    string DisplayName);