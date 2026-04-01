using HomeFlow.Modules.Households.Application.Abstractions;
using HomeFlow.Modules.Households.Application.Commands.AcceptHouseholdInvitation;
using HomeFlow.Modules.Households.Application.Commands.DeclineHouseholdInvitation;
using HomeFlow.Modules.Households.Application.Commands.InviteHouseholdMember;
using HomeFlow.Modules.Households.Application.Commands.RevokeHouseholdInvitation;
using HomeFlow.Modules.Households.Application.Queries.GetHouseholdDetails;
using HomeFlow.Modules.Households.Application.Queries.GetHouseholdMembers;
using HomeFlow.Modules.Households.Domain.Repositories;
using HomeFlow.Modules.Households.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace HomeFlow.Modules.Households.Infrastructure.DependencyInjection;

public static class HouseholdsModule
{
    public static IServiceCollection AddHouseholdsModule(this IServiceCollection services)
    {
        services.AddScoped<IHouseholdRepository, HouseholdRepository>();
        services.AddScoped<IHouseholdInvitationRepository, HouseholdInvitationRepository>();
        services.AddScoped<IHouseholdReadRepository, HouseholdReadRepository>();

        services.AddScoped<InviteHouseholdMemberHandler>();
        services.AddScoped<AcceptHouseholdInvitationHandler>();
        services.AddScoped<DeclineHouseholdInvitationHandler>();
        services.AddScoped<GetHouseholdDetailsHandler>();
        services.AddScoped<GetHouseholdMembersHandler>();
        services.AddScoped<RevokeHouseholdInvitationHandler>();

        return services;
    }
}
