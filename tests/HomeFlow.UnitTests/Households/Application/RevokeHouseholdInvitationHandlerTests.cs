using FluentAssertions;
using HomeFlow.BuildingBlocks.Application.Abstractions.Persistence;
using HomeFlow.BuildingBlocks.Domain.Exceptions;
using HomeFlow.BuildingBlocks.MultiTenancy.Models;
using HomeFlow.Modules.Households.Application.Commands.RevokeHouseholdInvitation;
using HomeFlow.Modules.Households.Domain.Aggregates;
using HomeFlow.Modules.Households.Domain.Enums;
using HomeFlow.Modules.Households.Domain.Ids;
using HomeFlow.Modules.Households.Domain.Repositories;
using Moq;
using Xunit;

namespace HomeFlow.UnitTests.Households.Application;

public sealed class RevokeHouseholdInvitationHandlerTests
{
    [Fact]
    public async Task Handle_Should_Revoke_Invitation_When_Requester_Has_Management_Permissions()
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

        var handler = new RevokeHouseholdInvitationHandler(
            invitationRepository.Object,
            householdRepository.Object,
            unitOfWork.Object);

        var command = new RevokeHouseholdInvitationCommand(
            invitation.Id.Value,
            "mich@example.com");

        var result = await handler.Handle(command, CancellationToken.None);

        result.InvitationId.Should().Be(invitation.Id.Value);
        result.HouseholdId.Should().Be(household.Id.Value);
        result.Email.Should().Be("wife@example.com");
        result.Status.Should().Be("Revoked");

        invitation.Status.Should().Be(InvitationStatus.Revoked);

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

        var handler = new RevokeHouseholdInvitationHandler(
            invitationRepository.Object,
            householdRepository.Object,
            unitOfWork.Object);

        var command = new RevokeHouseholdInvitationCommand(
            Guid.NewGuid(),
            "mich@example.com");

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Invitation was not found*");

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

        var handler = new RevokeHouseholdInvitationHandler(
            invitationRepository.Object,
            householdRepository.Object,
            unitOfWork.Object);

        var command = new RevokeHouseholdInvitationCommand(
            invitation.Id.Value,
            "mich@example.com");

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Household was not found*");

        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Requester_Has_No_Management_Permissions()
    {
        var invitationRepository = new Mock<IHouseholdInvitationRepository>();
        var householdRepository = new Mock<IHouseholdRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var household = Household.Create(
            HouseholdId.New(),
            TenantId.New(),
            "Main Household");

        household.AddMember(
            HouseholdMemberId.New(),
            "User",
            "user@example.com",
            HouseholdRole.Member);

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

        var handler = new RevokeHouseholdInvitationHandler(
            invitationRepository.Object,
            householdRepository.Object,
            unitOfWork.Object);

        var command = new RevokeHouseholdInvitationCommand(
            invitation.Id.Value,
            "user@example.com");

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not allowed*");

        invitation.Status.Should().Be(InvitationStatus.Pending);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Invitation_Is_Not_Pending()
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

        invitation.Accept();

        invitationRepository
            .Setup(x => x.GetByIdAsync(invitation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitation);

        householdRepository
            .Setup(x => x.GetByIdAsync(household.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(household);

        var handler = new RevokeHouseholdInvitationHandler(
            invitationRepository.Object,
            householdRepository.Object,
            unitOfWork.Object);

        var command = new RevokeHouseholdInvitationCommand(
            invitation.Id.Value,
            "mich@example.com");

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*pending*");

        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}