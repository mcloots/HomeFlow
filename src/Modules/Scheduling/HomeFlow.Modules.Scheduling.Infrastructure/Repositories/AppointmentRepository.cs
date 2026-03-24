using HomeFlow.BuildingBlocks.Infrastructure.Persistence;
using HomeFlow.Modules.Scheduling.Domain.Aggregates;
using HomeFlow.Modules.Scheduling.Domain.Ids;
using HomeFlow.Modules.Scheduling.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HomeFlow.Modules.Scheduling.Infrastructure.Repositories;

public sealed class AppointmentRepository : IAppointmentRepository
{
    private readonly HomeFlowDbContext _dbContext;

    public AppointmentRepository(HomeFlowDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Appointment appointment, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<Appointment>().AddAsync(appointment, cancellationToken);
    }

    public Task<Appointment?> GetByIdAsync(AppointmentId appointmentId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Set<Appointment>()
            .Include(x => x.Participants)
            .SingleOrDefaultAsync(x => x.Id == appointmentId, cancellationToken);
    }
}