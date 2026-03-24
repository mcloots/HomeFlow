namespace HomeFlow.Modules.Scheduling.Application.Queries.GetAppointmentDetails;

public sealed record AppointmentDetailsParticipantDto(
    Guid HouseholdMemberId,
    string DisplayName);