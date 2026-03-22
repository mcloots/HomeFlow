using Microsoft.Extensions.DependencyInjection;

namespace HomeFlow.Modules.Identity.Infrastructure.DependencyInjection;

public static class IdentityModule
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services)
    {
        return services;
    }
}