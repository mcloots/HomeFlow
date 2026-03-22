using HomeFlow.Modules.Households.Domain.Repositories;
using HomeFlow.Modules.Households.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace HomeFlow.Modules.Households.Infrastructure.DependencyInjection;

public static class HouseholdsModule
{
    public static IServiceCollection AddHouseholdsModule(this IServiceCollection services)
    {
        services.AddSingleton<IHouseholdRepository, InMemoryHouseholdRepository>();

        return services;
    }
}