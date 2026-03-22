using HomeFlow.BuildingBlocks.MultiTenancy.Models;

namespace HomeFlow.BuildingBlocks.MultiTenancy.Abstractions;

public interface ICurrentTenant
{
    TenantId? TenantId { get; }
    bool HasTenant => TenantId.HasValue;
}