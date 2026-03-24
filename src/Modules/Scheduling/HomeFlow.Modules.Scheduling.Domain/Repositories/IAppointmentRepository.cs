using HomeFlow.Modules.Scheduling.Domain.Aggregates;
using HomeFlow.Modules.Scheduling.Domain.Ids;

namespace HomeFlow.Modules.Scheduling.Domain.Repositories;

public interface IAppointmentRepository
{
    Task AddAsync(Appointment appointment, CancellationToken cancellationToken = default);
    Task<Appointment?> GetByIdAsync(AppointmentId appointmentId, CancellationToken cancellationToken = default);
}