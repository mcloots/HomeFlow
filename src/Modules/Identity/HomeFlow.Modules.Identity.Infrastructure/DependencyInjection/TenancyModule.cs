using Microsoft.Extensions.DependencyInjection;

namespace HomeFlow.Modules.Tenancy.Infrastructure.DependencyInjection;

public static class TenancyModule
{
    public static IServiceCollection AddTenancyModule(this IServiceCollection services)
    {
        return services;
    }
}