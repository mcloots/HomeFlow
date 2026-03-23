using HomeFlow.BuildingBlocks.Domain.Common;
using HomeFlow.BuildingBlocks.Domain.Exceptions;
using HomeFlow.Modules.Households.Domain.Enums;
using HomeFlow.Modules.Households.Domain.Ids;

public sealed class HouseholdMember : Entity<HouseholdMemberId>
{
    private HouseholdMember()
    {
    }

    public string DisplayName { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public HouseholdRole Role { get; private set; }

    public static HouseholdMember Create(
        HouseholdMemberId id,
        string displayName,
        string email,
        HouseholdRole role)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new DomainException("Display name is required.");

        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email is required.");

        return new HouseholdMember
        {
            Id = id,
            DisplayName = displayName.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            Role = role
        };
    }

    public static HouseholdMember CreateOwner(
        HouseholdMemberId id,
        string displayName,
        string email)
    {
        return Create(id, displayName, email, HouseholdRole.Owner);
    }
}