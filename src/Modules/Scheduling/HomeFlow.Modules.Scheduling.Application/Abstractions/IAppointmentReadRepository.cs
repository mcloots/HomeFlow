using HomeFlow.Modules.Scheduling.Application.Queries.GetAppointmentsForDateRange;

namespace HomeFlow.Modules.Scheduling.Application.Abstractions;

public interface IAppointmentReadRepository
{
    Task<GetAppointmentsForDateRangeResponse> GetForDateRangeAsync(
        Guid householdId,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default);
}