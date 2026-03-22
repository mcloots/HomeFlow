using HomeFlow.Modules.Households.Domain.Aggregates;
using HomeFlow.Modules.Households.Domain.Ids;
using HomeFlow.Modules.Households.Domain.Repositories;
using HomeFlow.Modules.Tenancy.Domain.Aggregates;
using HomeFlow.Modules.Tenancy.Domain.Repositories;
using HomeFlow.BuildingBlocks.MultiTenancy.Models;

namespace HomeFlow.Modules.Tenancy.Application.Commands.CreateTenantAndHousehold;

public sealed class CreateTenantAndHouseholdHandler
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IHouseholdRepository _householdRepository;

    public CreateTenantAndHouseholdHandler(
        ITenantRepository tenantRepository,
        IHouseholdRepository householdRepository)
    {
        _tenantRepository = tenantRepository;
        _householdRepository = householdRepository;
    }

    public async Task<CreateTenantAndHouseholdResponse> Handle(
        CreateTenantAndHouseholdCommand command,
        CancellationToken cancellationToken = default)
    {
        var tenantId = TenantId.New();
        var householdId = HouseholdId.New();

        var tenant = Tenant.Create(tenantId, command.TenantName);
        var household = Household.Create(householdId, tenantId, command.HouseholdName);

        await _tenantRepository.AddAsync(tenant, cancellationToken);
        await _householdRepository.AddAsync(household, cancellationToken);

        return new CreateTenantAndHouseholdResponse(
            tenantId.Value,
            householdId.Value,
            tenant.Name,
            household.Name);
    }
}