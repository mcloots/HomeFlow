using HomeFlow.Modules.Scheduling.Application.Abstractions;
using HomeFlow.Modules.Scheduling.Application.Commands.CreateAppointment;
using HomeFlow.Modules.Scheduling.Domain.Repositories;
using HomeFlow.Modules.Scheduling.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace HomeFlow.Modules.Scheduling.Infrastructure.DependencyInjection;

public static class SchedulingModule
{
    public static IServiceCollection AddSchedulingModule(this IServiceCollection services)
    {
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        services.AddScoped<IHouseholdMemberLookup, HouseholdMemberLookup>();
        services.AddScoped<CreateAppointmentHandler>();

        return services;
    }
}