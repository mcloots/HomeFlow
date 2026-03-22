using HomeFlow.BuildingBlocks.MultiTenancy.Models;

namespace HomeFlow.BuildingBlocks.MultiTenancy.Abstractions;

public interface ITenantOwned
{
    TenantId TenantId { get; }
}