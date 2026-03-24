namespace HomeFlow.Modules.Scheduling.Domain.Ids;

public readonly record struct AppointmentParticipantId(Guid Value)
{
    public static AppointmentParticipantId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}