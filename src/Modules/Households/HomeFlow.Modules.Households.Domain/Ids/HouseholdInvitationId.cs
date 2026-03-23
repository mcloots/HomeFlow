namespace HomeFlow.Modules.Households.Domain.Ids;

public readonly record struct HouseholdInvitationId(Guid Value)
{
    public static HouseholdInvitationId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}