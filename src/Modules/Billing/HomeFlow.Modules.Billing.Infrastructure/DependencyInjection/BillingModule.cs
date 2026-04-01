using HomeFlow.Modules.Billing.Application.Abstractions;
using HomeFlow.Modules.Billing.Application.Commands.CreateBill;
using HomeFlow.Modules.Billing.Application.Commands.UpdateBill;
using HomeFlow.Modules.Billing.Application.Queries.GetBillDetails;
using HomeFlow.Modules.Billing.Application.Queries.GetBillsForHousehold;
using HomeFlow.Modules.Billing.Domain.Repositories;
using HomeFlow.Modules.Billing.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace HomeFlow.Modules.Billing.Infrastructure.DependencyInjection;

public static class BillingModule
{
    public static IServiceCollection AddBillingModule(this IServiceCollection services)
    {
        services.AddScoped<IBillRepository, BillRepository>();
        services.AddScoped<IBillReadRepository, BillReadRepository>();

        services.AddScoped<CreateBillHandler>();
        services.AddScoped<UpdateBillHandler>();
        services.AddScoped<GetBillDetailsHandler>();
        services.AddScoped<GetBillsForHouseholdHandler>();

        return services;
    }
}
