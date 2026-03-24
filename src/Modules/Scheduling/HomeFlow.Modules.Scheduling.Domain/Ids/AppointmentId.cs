namespace HomeFlow.Modules.Scheduling.Domain.Ids;

public readonly record struct AppointmentId(Guid Value)
{
    public static AppointmentId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}