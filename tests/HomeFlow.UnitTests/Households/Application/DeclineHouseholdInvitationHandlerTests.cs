using FluentAssertions;
using HomeFlow.BuildingBlocks.Application.Abstractions.Persistence;
using HomeFlow.Modules.Households.Application.Commands.DeclineHouseholdInvitation;
using HomeFlow.Modules.Households.Domain.Aggregates;
using HomeFlow.Modules.Households.Domain.Enums;
using HomeFlow.Modules.Households.Domain.Ids;
using HomeFlow.Modules.Households.Domain.Repositories;
using Moq;
using Xunit;

namespace HomeFlow.UnitTests.Households.Application;

public sealed class DeclineHouseholdInvitationHandlerTests
{
    [Fact]
    public async Task Handle_Should_Decline_Invitation()
    {
        var invitationRepository = new Mock<IHouseholdInvitationRepository>();
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

        var handler = new DeclineHouseholdInvitationHandler(
            invitationRepository.Object,
            unitOfWork.Object);

        var command = new DeclineHouseholdInvitationCommand(
            invitation.Id.Value,
            "wife@example.com");

        var result = await handler.Handle(command, CancellationToken.None);

        result.InvitationId.Should().Be(invitation.Id.Value);
        result.Email.Should().Be("wife@example.com");
        result.Status.Should().Be("Declined");

        invitation.Status.Should().Be(InvitationStatus.Declined);

        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Invitation_Does_Not_Exist()
    {
        var invitationRepository = new Mock<IHouseholdInvitationRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        invitationRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<HouseholdInvitationId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((HouseholdInvitation?)null);

        var handler = new DeclineHouseholdInvitationHandler(
            invitationRepository.Object,
            unitOfWork.Object);

        var command = new DeclineHouseholdInvitationCommand(
            Guid.NewGuid(),
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

        var handler = new DeclineHouseholdInvitationHandler(
            invitationRepository.Object,
            unitOfWork.Object);

        var command = new DeclineHouseholdInvitationCommand(
            invitation.Id.Value,
            "other@example.com");

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*does not match*");

        invitation.Status.Should().Be(InvitationStatus.Pending);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Invitation_Is_Not_Pending()
    {
        var invitationRepository = new Mock<IHouseholdInvitationRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var invitation = HouseholdInvitation.Create(
            HouseholdInvitationId.New(),
            HouseholdId.New(),
            "wife@example.com",
            HouseholdRole.Member,
            DateTime.UtcNow);

        invitation.Accept();

        invitationRepository
            .Setup(x => x.GetByIdAsync(invitation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitation);

        var handler = new DeclineHouseholdInvitationHandler(
            invitationRepository.Object,
            unitOfWork.Object);

        var command = new DeclineHouseholdInvitationCommand(
            invitation.Id.Value,
            "wife@example.com");

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<HomeFlow.BuildingBlocks.Domain.Exceptions.DomainException>()
            .WithMessage("*pending*");

        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}