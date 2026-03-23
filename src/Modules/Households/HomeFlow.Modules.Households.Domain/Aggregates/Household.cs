using HomeFlow.BuildingBlocks.Domain.Common;
using HomeFlow.BuildingBlocks.Domain.Exceptions;
using HomeFlow.BuildingBlocks.MultiTenancy.Abstractions;
using HomeFlow.BuildingBlocks.MultiTenancy.Models;
using HomeFlow.Modules.Households.Domain.Enums;
using HomeFlow.Modules.Households.Domain.Ids;

namespace HomeFlow.Modules.Households.Domain.Aggregates;

public sealed class Household : AggregateRoot<HouseholdId>, ITenantOwned
{
    private readonly List<HouseholdMember> _members = [];

    private Household()
    {
    }

    public TenantId TenantId { get; private set; }
    public string Name { get; private set; } = default!;
    public HouseholdStatus Status { get; private set; }
    public IReadOnlyCollection<HouseholdMember> Members => _members.AsReadOnly();

    public static Household Create(HouseholdId id, TenantId tenantId, string name)
    {
        if (tenantId == default)
            throw new DomainException("TenantId is required.");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Household name is required.");

        return new Household
        {
            Id = id,
            TenantId = tenantId,
            Name = name.Trim(),
            Status = HouseholdStatus.Active
        };
    }

    public HouseholdMember AddOwnerMember(HouseholdMemberId memberId, string displayName, string email)
    {
        EnsureActive();

        if (_members.Any(m => string.Equals(m.Email, email.Trim(), StringComparison.OrdinalIgnoreCase)))
            throw new DomainException("A member with this email already exists in the household.");

        var member = HouseholdMember.CreateOwner(memberId, displayName, email);
        _members.Add(member);

        return member;
    }

    public void Rename(string name)
    {
        EnsureActive();

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Household name is required.");

        Name = name.Trim();
    }

    public void Archive()
    {
        EnsureActive();
        Status = HouseholdStatus.Archived;
    }

    private void EnsureActive()
    {
        if (Status != HouseholdStatus.Active)
            throw new DomainException("Only active households can be modified.");
    }
}