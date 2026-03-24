using HomeFlow.BuildingBlocks.Domain.Common;
using HomeFlow.BuildingBlocks.Domain.Exceptions;
using HomeFlow.Modules.Households.Domain.Ids;
using HomeFlow.Modules.Scheduling.Domain.Ids;

namespace HomeFlow.Modules.Scheduling.Domain.Entities;

public sealed class AppointmentParticipant : Entity<AppointmentParticipantId>
{
    private AppointmentParticipant()
    {
    }

    public HouseholdMemberId HouseholdMemberId { get; private set; }

    public static AppointmentParticipant Create(
        AppointmentParticipantId id,
        HouseholdMemberId householdMemberId)
    {
        if (householdMemberId == default)
            throw new DomainException("HouseholdMemberId is required.");

        return new AppointmentParticipant
        {
            Id = id,
            HouseholdMemberId = householdMemberId
        };
    }
}