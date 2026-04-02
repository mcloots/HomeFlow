using HomeFlow.Modules.Chores.Application.Abstractions;
using HomeFlow.Modules.Chores.Application.Commands.CompleteChore;
using HomeFlow.Modules.Chores.Application.Commands.CreateChore;
using HomeFlow.Modules.Chores.Application.Commands.UpdateChore;
using HomeFlow.Modules.Chores.Application.Queries.GetChoresForHousehold;
using HomeFlow.Modules.Chores.Domain.Repositories;
using HomeFlow.Modules.Chores.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace HomeFlow.Modules.Chores.Infrastructure.DependencyInjection;

public static class ChoresModule
{
    public static IServiceCollection AddChoresModule(this IServiceCollection services)
    {
        services.AddScoped<IChoreRepository, ChoreRepository>();
        services.AddScoped<IChoreReadRepository, ChoreReadRepository>();
        services.AddScoped<IHouseholdMemberLookup, HouseholdMemberLookup>();

        services.AddScoped<CreateChoreHandler>();
        services.AddScoped<UpdateChoreHandler>();
        services.AddScoped<CompleteChoreHandler>();
        services.AddScoped<GetChoresForHouseholdHandler>();

        return services;
    }
}
