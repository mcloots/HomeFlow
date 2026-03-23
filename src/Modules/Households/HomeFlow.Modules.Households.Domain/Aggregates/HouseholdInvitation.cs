using HomeFlow.BuildingBlocks.Domain.Common;
using HomeFlow.BuildingBlocks.Domain.Exceptions;
using HomeFlow.Modules.Households.Domain.Enums;
using HomeFlow.Modules.Households.Domain.Ids;

namespace HomeFlow.Modules.Households.Domain.Aggregates;

public sealed class HouseholdInvitation : AggregateRoot<HouseholdInvitationId>
{
    private HouseholdInvitation()
    {
    }

    public HouseholdId HouseholdId { get; private set; }
    public string Email { get; private set; } = default!;
    public HouseholdRole Role { get; private set; }
    public InvitationStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    public static HouseholdInvitation Create(
        HouseholdInvitationId id,
        HouseholdId householdId,
        string email,
        HouseholdRole role,
        DateTime createdAtUtc)
    {
        if (householdId == default)
            throw new DomainException("HouseholdId is required.");

        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Invitation email is required.");

        return new HouseholdInvitation
        {
            Id = id,
            HouseholdId = householdId,
            Email = email.Trim().ToLowerInvariant(),
            Role = role,
            Status = InvitationStatus.Pending,
            CreatedAtUtc = createdAtUtc
        };
    }

    public void Revoke()
    {
        if (Status != InvitationStatus.Pending)
            throw new DomainException("Only pending invitations can be revoked.");

        Status = InvitationStatus.Revoked;
    }

    public void Accept()
    {
        if (Status != InvitationStatus.Pending)
            throw new DomainException("Only pending invitations can be accepted.");

        Status = InvitationStatus.Accepted;
    }

    public void Decline()
    {
        if (Status != InvitationStatus.Pending)
            throw new DomainException("Only pending invitations can be declined.");

        Status = InvitationStatus.Declined;
    }

    public void Expire()
    {
        if (Status != InvitationStatus.Pending)
            throw new DomainException("Only pending invitations can expire.");

        Status = InvitationStatus.Expired;
    }
}