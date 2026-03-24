using FluentAssertions;
using HomeFlow.BuildingBlocks.Application.Abstractions.Persistence;
using HomeFlow.BuildingBlocks.MultiTenancy.Models;
using HomeFlow.Modules.Households.Application.Commands.AcceptHouseholdInvitation;
using HomeFlow.Modules.Households.Domain.Aggregates;
using HomeFlow.Modules.Households.Domain.Enums;
using HomeFlow.Modules.Households.Domain.Ids;
using HomeFlow.Modules.Households.Domain.Repositories;
using Moq;
using Xunit;

namespace HomeFlow.UnitTests.Households.Application;

public sealed class AcceptHouseholdInvitationHandlerTests
{
    [Fact]
    public async Task Handle_Should_Accept_Invitation_And_Add_Member()
    {
        var invitationRepository = new Mock<IHouseholdInvitationRepository>();
        var householdRepository = new Mock<IHouseholdRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var household = Household.Create(
            HouseholdId.New(),
            TenantId.New(),
            "Main Household");

        household.AddOwnerMember(
            HouseholdMemberId.New(),
            "Mich",
            "mich@example.com");

        var invitation = HouseholdInvitation.Create(
            HouseholdInvitationId.New(),
            household.Id,
            "wife@example.com",
            HouseholdRole.Member,
            DateTime.UtcNow);

        invitationRepository
            .Setup(x => x.GetByIdAsync(invitation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitation);

        householdRepository
            .Setup(x => x.GetByIdAsync(household.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(household);

        householdRepository
            .Setup(x => x.MemberEmailExistsAsync("wife@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new AcceptHouseholdInvitationHandler(
            invitationRepository.Object,
            householdRepository.Object,
            unitOfWork.Object);

        var command = new AcceptHouseholdInvitationCommand(
            invitation.Id.Value,
            "Wife Cloots",
            "wife@example.com");

        var result = await handler.Handle(command, CancellationToken.None);

        result.InvitationId.Should().Be(invitation.Id.Value);
        result.HouseholdId.Should().Be(household.Id.Value);
        result.MemberId.Should().NotBe(Guid.Empty);
        result.Email.Should().Be("wife@example.com");
        result.Status.Should().Be("Accepted");

        invitation.Status.Should().Be(InvitationStatus.Accepted);
        household.Members.Should().ContainSingle(x =>
            x.Email == "wife@example.com" &&
            x.DisplayName == "Wife Cloots" &&
            x.Role == HouseholdRole.Member);

        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Invitation_Does_Not_Exist()
    {
        var invitationRepository = new Mock<IHouseholdInvitationRepository>();
        var householdRepository = new Mock<IHouseholdRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        invitationRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<HouseholdInvitationId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((HouseholdInvitation?)null);

        var handler = new AcceptHouseholdInvitationHandler(
            invitationRepository.Object,
            householdRepository.Object,
            unitOfWork.Object);

        var command = new AcceptHouseholdInvitationCommand(
            Guid.NewGuid(),
            "Wife Cloots",
            "wife@example.com");

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Invitation was not found*");

        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Email_Does_Not_Match_Invitation_Email()
    {
        var invitationRepository = new Mock<IHouseholdInvitationRepository>();
        var householdRepository = new Mock<IHouseholdRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var invitation = HouseholdInvitation.Create(
            HouseholdInvitationId.New(),
            HouseholdId.New(),
            "wife@example.com",
            HouseholdRole.Member,
            DateTime.UtcNow);

        invitationRepository
            .Setup(x => x.GetByIdAsync(invitation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitation);

        var handler = new AcceptHouseholdInvitationHandler(
            invitationRepository.Object,
            householdRepository.Object,
            unitOfWork.Object);

        var command = new AcceptHouseholdInvitationCommand(
            invitation.Id.Value,
            "Wife Cloots",
            "other@example.com");

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*does not match*");

        invitation.Status.Should().Be(InvitationStatus.Pending);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Household_Does_Not_Exist()
    {
        var invitationRepository = new Mock<IHouseholdInvitationRepository>();
        var householdRepository = new Mock<IHouseholdRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var invitation = HouseholdInvitation.Create(
            HouseholdInvitationId.New(),
            HouseholdId.New(),
            "wife@example.com",
            HouseholdRole.Member,
            DateTime.UtcNow);

        invitationRepository
            .Setup(x => x.GetByIdAsync(invitation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitation);

        householdRepository
            .Setup(x => x.GetByIdAsync(invitation.HouseholdId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Household?)null);

        var handler = new AcceptHouseholdInvitationHandler(
            invitationRepository.Object,
            householdRepository.Object,
            unitOfWork.Object);

        var command = new AcceptHouseholdInvitationCommand(
            invitation.Id.Value,
            "Wife Cloots",
            "wife@example.com");

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Household was not found*");

        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Member_Email_Already_Exists()
    {
        var invitationRepository = new Mock<IHouseholdInvitationRepository>();
        var householdRepository = new Mock<IHouseholdRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var household = Household.Create(
            HouseholdId.New(),
            TenantId.New(),
            "Main Household");

        var invitation = HouseholdInvitation.Create(
            HouseholdInvitationId.New(),
            household.Id,
            "wife@example.com",
            HouseholdRole.Member,
            DateTime.UtcNow);

        invitationRepository
            .Setup(x => x.GetByIdAsync(invitation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitation);

        householdRepository
            .Setup(x => x.GetByIdAsync(household.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(household);

        householdRepository
            .Setup(x => x.MemberEmailExistsAsync("wife@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new AcceptHouseholdInvitationHandler(
            invitationRepository.Object,
            householdRepository.Object,
            unitOfWork.Object);

        var command = new AcceptHouseholdInvitationCommand(
            invitation.Id.Value,
            "Wife Cloots",
            "wife@example.com");

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");

        invitation.Status.Should().Be(InvitationStatus.Pending);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}