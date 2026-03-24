using HomeFlow.Modules.Scheduling.Application.Abstractions;

namespace HomeFlow.Modules.Scheduling.Application.Queries.GetAppointmentsForDateRange;

public sealed class GetAppointmentsForDateRangeHandler
{
    private readonly IAppointmentReadRepository _readRepository;

    public GetAppointmentsForDateRangeHandler(
        IAppointmentReadRepository readRepository)
    {
        _readRepository = readRepository;
    }

    public Task<GetAppointmentsForDateRangeResponse> Handle(
        GetAppointmentsForDateRangeQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query.ToUtc <= query.FromUtc)
            throw new InvalidOperationException("ToUtc must be after FromUtc.");

        return _readRepository.GetForDateRangeAsync(
            query.HouseholdId,
            query.FromUtc,
            query.ToUtc,
            cancellationToken);
    }
}