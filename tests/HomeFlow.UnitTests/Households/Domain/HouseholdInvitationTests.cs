using FluentAssertions;
using HomeFlow.BuildingBlocks.Domain.Exceptions;
using HomeFlow.Modules.Households.Domain.Aggregates;
using HomeFlow.Modules.Households.Domain.Enums;
using HomeFlow.Modules.Households.Domain.Ids;
using Xunit;

namespace HomeFlow.UnitTests.Households.Domain;

public sealed class HouseholdInvitationTests
{
    [Fact]
    public void Create_Should_Set_Pending_Status()
    {
        var invitation = HouseholdInvitation.Create(
            HouseholdInvitationId.New(),
            HouseholdId.New(),
            "wife@example.com",
            HouseholdRole.Member,
            DateTime.UtcNow);

        invitation.Email.Should().Be("wife@example.com");
        invitation.Role.Should().Be(HouseholdRole.Member);
        invitation.Status.Should().Be(InvitationStatus.Pending);
    }

    [Fact]
    public void Accept_Should_Set_Status_To_Accepted()
    {
        var invitation = HouseholdInvitation.Create(
            HouseholdInvitationId.New(),
            HouseholdId.New(),
            "wife@example.com",
            HouseholdRole.Member,
            DateTime.UtcNow);

        invitation.Accept();

        invitation.Status.Should().Be(InvitationStatus.Accepted);
    }

    [Fact]
    public void Decline_Should_Set_Status_To_Declined()
    {
        var invitation = HouseholdInvitation.Create(
            HouseholdInvitationId.New(),
            HouseholdId.New(),
            "wife@example.com",
            HouseholdRole.Member,
            DateTime.UtcNow);

        invitation.Decline();

        invitation.Status.Should().Be(InvitationStatus.Declined);
    }

    [Fact]
    public void Accept_Should_Throw_When_Invitation_Is_Not_Pending()
    {
        var invitation = HouseholdInvitation.Create(
            HouseholdInvitationId.New(),
            HouseholdId.New(),
            "wife@example.com",
            HouseholdRole.Member,
            DateTime.UtcNow);

        invitation.Decline();

        var act = () => invitation.Accept();

        act.Should().Throw<DomainException>()
            .WithMessage("*pending*");
    }
}