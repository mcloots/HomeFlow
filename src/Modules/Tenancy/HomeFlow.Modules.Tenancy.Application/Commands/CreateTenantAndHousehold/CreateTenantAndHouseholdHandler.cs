using HomeFlow.Modules.Households.Domain.Aggregates;
using HomeFlow.Modules.Households.Domain.Ids;
using HomeFlow.Modules.Households.Domain.Repositories;
using HomeFlow.Modules.Tenancy.Domain.Aggregates;
using HomeFlow.Modules.Tenancy.Domain.Repositories;
using HomeFlow.BuildingBlocks.MultiTenancy.Models;
using HomeFlow.BuildingBlocks.Application.Abstractions.Persistence;

namespace HomeFlow.Modules.Tenancy.Application.Commands.CreateTenantAndHousehold;

public sealed class CreateTenantAndHouseholdHandler
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IHouseholdRepository _householdRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTenantAndHouseholdHandler(
        ITenantRepository tenantRepository,
        IHouseholdRepository householdRepository,
        IUnitOfWork unitOfWork)
    {
        _tenantRepository = tenantRepository;
        _householdRepository = householdRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateTenantAndHouseholdResponse> Handle(
        CreateTenantAndHouseholdCommand command,
        CancellationToken cancellationToken = default)
    {
        var tenantId = TenantId.New();
        var householdId = HouseholdId.New();
        var ownerMemberId = HouseholdMemberId.New();

        var tenant = Tenant.Create(tenantId, command.TenantName);
        var household = Household.Create(householdId, tenantId, command.HouseholdName);

        household.AddOwnerMember(
            ownerMemberId,
            command.OwnerDisplayName,
            command.OwnerEmail);

        await _tenantRepository.AddAsync(tenant, cancellationToken);
        await _householdRepository.AddAsync(household, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateTenantAndHouseholdResponse(
            tenantId.Value,
            householdId.Value,
            ownerMemberId.Value,
            tenant.Name,
            household.Name);
    }
}