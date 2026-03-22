using Microsoft.Extensions.DependencyInjection;

namespace HomeFlow.Modules.Subscriptions.Infrastructure.DependencyInjection;

public static class SubscriptionsModule
{
    public static IServiceCollection AddSubscriptionsModule(this IServiceCollection services)
    {
        return services;
    }
}