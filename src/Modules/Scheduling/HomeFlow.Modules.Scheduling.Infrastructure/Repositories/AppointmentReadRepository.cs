using HomeFlow.BuildingBlocks.Infrastructure.Persistence;
using HomeFlow.Modules.Households.Domain.Ids;
using HomeFlow.Modules.Scheduling.Application.Abstractions;
using HomeFlow.Modules.Scheduling.Application.Queries.GetAppointmentsForDateRange;
using HomeFlow.Modules.Scheduling.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;

namespace HomeFlow.Modules.Scheduling.Infrastructure.Repositories;

public sealed class AppointmentReadRepository : IAppointmentReadRepository
{
    private readonly HomeFlowDbContext _dbContext;

    public AppointmentReadRepository(HomeFlowDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetAppointmentsForDateRangeResponse> GetForDateRangeAsync(
        Guid householdId,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default)
    {
        var typedHouseholdId = new HouseholdId(householdId);

        var appointments = await _dbContext.Set<Appointment>()
            .Where(x =>
                x.HouseholdId == typedHouseholdId &&
                x.StartsAtUtc < toUtc &&
                x.EndsAtUtc > fromUtc)
            .Include(x => x.Participants)
            .ToListAsync(cancellationToken);

        var memberIds = appointments
            .SelectMany(a => a.Participants)
            .Select(p => p.HouseholdMemberId)
            .Distinct()
            .ToList();

        var members = await _dbContext.Set<HouseholdMember>()
            .Where(x => memberIds.Contains(x.Id))
            .ToDictionaryAsync(
                x => x.Id,
                x => x.DisplayName,
                cancellationToken);

        var appointmentDtos = appointments
            .Select(a =>
                new AppointmentSummaryDto(
                    a.Id.Value,
                    a.Title,
                    a.StartsAtUtc,
                    a.EndsAtUtc,
                    a.Location,
                    a.Status.ToString(),
                    a.Participants
                        .Select(p =>
                            new AppointmentParticipantDto(
                                p.HouseholdMemberId.Value,
                                members[p.HouseholdMemberId]))
                        .ToList()))
            .ToList();

        return new GetAppointmentsForDateRangeResponse(
            householdId,
            fromUtc,
            toUtc,
            appointmentDtos);
    }
}