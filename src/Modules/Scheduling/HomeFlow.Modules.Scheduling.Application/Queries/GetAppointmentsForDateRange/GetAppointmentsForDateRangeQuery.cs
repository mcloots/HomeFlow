namespace HomeFlow.Modules.Scheduling.Application.Queries.GetAppointmentsForDateRange;

public sealed record GetAppointmentsForDateRangeQuery(
    Guid HouseholdId,
    DateTime FromUtc,
    DateTime ToUtc);