using HomeFlow.Modules.Scheduling.Application.Abstractions;

namespace HomeFlow.Modules.Scheduling.Application.Queries.GetAppointmentDetails;

public sealed class GetAppointmentDetailsHandler
{
    private readonly IAppointmentReadRepository _readRepository;

    public GetAppointmentDetailsHandler(IAppointmentReadRepository readRepository)
    {
        _readRepository = readRepository;
    }

    public async Task<GetAppointmentDetailsResponse> Handle(
        GetAppointmentDetailsQuery query,
        CancellationToken cancellationToken = default)
    {
        var result = await _readRepository.GetDetailsByIdAsync(
            query.AppointmentId,
            cancellationToken);

        if (result is null)
            throw new InvalidOperationException("Appointment was not found.");

        return result;
    }
}