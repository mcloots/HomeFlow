using HomeFlow.BuildingBlocks.Domain.Common;
using HomeFlow.BuildingBlocks.Domain.Exceptions;
using HomeFlow.BuildingBlocks.MultiTenancy.Models;
using HomeFlow.Modules.Tenancy.Domain.Enums;

namespace HomeFlow.Modules.Tenancy.Domain.Aggregates;

public sealed class Tenant : AggregateRoot<TenantId>
{
    private Tenant()
    {
    }

    public string Name { get; private set; } = default!;
    public TenantStatus Status { get; private set; }

    public static Tenant Create(TenantId id, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Tenant name is required.");

        return new Tenant
        {
            Id = id,
            Name = name.Trim(),
            Status = TenantStatus.Active
        };
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Tenant name is required.");

        Name = name.Trim();
    }

    public void Suspend()
    {
        Status = TenantStatus.Suspended;
    }
}