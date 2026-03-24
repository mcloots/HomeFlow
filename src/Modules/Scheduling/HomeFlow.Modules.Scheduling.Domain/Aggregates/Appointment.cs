using HomeFlow.BuildingBlocks.Domain.Common;
using HomeFlow.BuildingBlocks.Domain.Exceptions;
using HomeFlow.BuildingBlocks.MultiTenancy.Abstractions;
using HomeFlow.BuildingBlocks.MultiTenancy.Models;
using HomeFlow.Modules.Households.Domain.Ids;
using HomeFlow.Modules.Scheduling.Domain.Entities;
using HomeFlow.Modules.Scheduling.Domain.Enums;
using HomeFlow.Modules.Scheduling.Domain.Ids;

namespace HomeFlow.Modules.Scheduling.Domain.Aggregates;

public sealed class Appointment : AggregateRoot<AppointmentId>, ITenantOwned
{
    private readonly List<AppointmentParticipant> _participants = [];

    private Appointment()
    {
    }

    public TenantId TenantId { get; private set; }
    public HouseholdId HouseholdId { get; private set; }
    public string Title { get; private set; } = default!;
    public string? Description { get; private set; }
    public DateTime StartsAtUtc { get; private set; }
    public DateTime EndsAtUtc { get; private set; }
    public string? Location { get; private set; }
    public AppointmentStatus Status { get; private set; }

    public IReadOnlyCollection<AppointmentParticipant> Participants => _participants.AsReadOnly();

    public static Appointment Create(
        AppointmentId id,
        TenantId tenantId,
        HouseholdId householdId,
        string title,
        string? description,
        DateTime startsAtUtc,
        DateTime endsAtUtc,
        string? location)
    {
        if (tenantId == default)
            throw new DomainException("TenantId is required.");

        if (householdId == default)
            throw new DomainException("HouseholdId is required.");

        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Title is required.");

        if (endsAtUtc <= startsAtUtc)
            throw new DomainException("End time must be after start time.");

        return new Appointment
        {
            Id = id,
            TenantId = tenantId,
            HouseholdId = householdId,
            Title = title.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            StartsAtUtc = startsAtUtc,
            EndsAtUtc = endsAtUtc,
            Location = string.IsNullOrWhiteSpace(location) ? null : location.Trim(),
            Status = AppointmentStatus.Scheduled
        };
    }

    public void AddParticipant(AppointmentParticipantId id, HouseholdMemberId householdMemberId)
    {
        EnsureNotCancelled();

        if (_participants.Any(x => x.HouseholdMemberId == householdMemberId))
            throw new DomainException("This household member is already a participant.");

        _participants.Add(AppointmentParticipant.Create(id, householdMemberId));
    }

    public void Cancel()
    {
        EnsureNotCancelled();
        Status = AppointmentStatus.Cancelled;
    }

    private void EnsureNotCancelled()
    {
        if (Status == AppointmentStatus.Cancelled)
            throw new DomainException("Cancelled appointments cannot be modified.");
    }
}