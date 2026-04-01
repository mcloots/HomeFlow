namespace HomeFlow.Modules.Billing.Domain.Ids;

public readonly record struct BillId(Guid Value)
{
    public static BillId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
