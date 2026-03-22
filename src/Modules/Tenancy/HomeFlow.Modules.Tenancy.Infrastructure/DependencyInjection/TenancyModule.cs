using HomeFlow.Modules.Tenancy.Application.Commands.CreateTenantAndHousehold;
using HomeFlow.Modules.Tenancy.Domain.Repositories;
using HomeFlow.Modules.Tenancy.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace HomeFlow.Modules.Tenancy.Infrastructure.DependencyInjection;

public static class TenancyModule
{
    public static IServiceCollection AddTenancyModule(this IServiceCollection services)
    {
        services.AddSingleton<ITenantRepository, InMemoryTenantRepository>();
        services.AddScoped<CreateTenantAndHouseholdHandler>();

        return services;
    }
}