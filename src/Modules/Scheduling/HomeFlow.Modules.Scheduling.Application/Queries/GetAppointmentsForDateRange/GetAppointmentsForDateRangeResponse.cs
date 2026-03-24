namespace HomeFlow.Modules.Scheduling.Application.Queries.GetAppointmentsForDateRange;

public sealed record GetAppointmentsForDateRangeResponse(
    Guid HouseholdId,
    DateTime FromUtc,
    DateTime ToUtc,
    IReadOnlyCollection<AppointmentSummaryDto> Appointments);