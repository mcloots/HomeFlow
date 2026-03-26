using HomeFlow.Modules.Integrations.Gmail.Application.Abstractions;
using HomeFlow.Modules.Integrations.Gmail.Application.Commands.CompleteGmailConnect;
using HomeFlow.Modules.Integrations.Gmail.Application.Commands.DisconnectGmailConnection;
using HomeFlow.Modules.Integrations.Gmail.Application.Commands.StartGmailConnect;
using HomeFlow.Modules.Integrations.Gmail.Application.Queries.GetCurrentGmailConnectionByHousehold;
using HomeFlow.Modules.Integrations.Gmail.Domain.Repositories;
using HomeFlow.Modules.Integrations.Gmail.Infrastructure.Configuration;
using HomeFlow.Modules.Integrations.Gmail.Infrastructure.OAuth;
using HomeFlow.Modules.Integrations.Gmail.Infrastructure.Repositories;
using HomeFlow.Modules.Integrations.Gmail.Infrastructure.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HomeFlow.Modules.Integrations.Gmail.Infrastructure.DependencyInjection;

public static class GmailModule
{
    public static IServiceCollection AddGmailModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<GmailOAuthOptions>(
            configuration.GetSection(GmailOAuthOptions.SectionName));

        services.AddScoped<IGmailConnectionRepository, GmailConnectionRepository>();
        services.AddScoped<IGmailOAuthClient, GmailOAuthClient>();
        services.AddSingleton<IGmailOAuthStateStore, InMemoryGmailOAuthStateStore>();
        services.AddScoped<ITokenEncryptionService, DataProtectionTokenEncryptionService>();
        services.AddScoped<IGoogleIdTokenReader, GoogleIdTokenReader>();

        services.AddScoped<StartGmailConnectHandler>();
        services.AddScoped<CompleteGmailConnectHandler>();
        services.AddScoped<DisconnectGmailConnectionHandler>();
        services.AddScoped<GetCurrentGmailConnectionByHouseholdHandler>();

        return services;
    }
}