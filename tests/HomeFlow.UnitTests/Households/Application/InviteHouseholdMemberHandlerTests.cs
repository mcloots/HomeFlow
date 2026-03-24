using FluentAssertions;
using HomeFlow.BuildingBlocks.Application.Abstractions;
using HomeFlow.BuildingBlocks.Application.Abstractions.Persistence;
using HomeFlow.BuildingBlocks.MultiTenancy.Models;
using HomeFlow.Modules.Households.Application.Commands.InviteHouseholdMember;
using HomeFlow.Modules.Households.Domain.Aggregates;
using HomeFlow.Modules.Households.Domain.Enums;
using HomeFlow.Modules.Households.Domain.Ids;
using HomeFlow.Modules.Households.Domain.Repositories;
using Moq;
using Xunit;

namespace HomeFlow.UnitTests.Households.Application;

public sealed class InviteHouseholdMemberHandlerTests
{
    [Fact]
    public async Task Handle_Should_Create_Invitation_When_Request_Is_Valid()
    {
        var householdRepository = new Mock<IHouseholdRepository>();
        var invitationRepository = new Mock<IHouseholdInvitationRepository>();
        var dateTimeProvider = new Mock<IDateTimeProvider>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var household = Household.Create(
            HouseholdId.New(),
            TenantId.New(),
            "Main Household");

        household.AddOwnerMember(
            HouseholdMemberId.New(),
            "Mich",
            "mich@example.com");

        householdRepository
            .Setup(x => x.GetByIdAsync(household.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(household);

        householdRepository
            .Setup(x => x.MemberEmailExistsAsync("wife@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        invitationRepository
            .Setup(x => x.PendingInvitationExistsAsync(household.Id, "wife@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        dateTimeProvider.Setup(x => x.UtcNow).Returns(new DateTime(2026, 3, 24, 12, 0, 0, DateTimeKind.Utc));

        var handler = new InviteHouseholdMemberHandler(
            householdRepository.Object,
            invitationRepository.Object,
            dateTimeProvider.Object,
            unitOfWork.Object);

        var command = new InviteHouseholdMemberCommand(
            household.Id.Value,
            "wife@example.com",
            HouseholdRole.Member,
            "mich@example.com");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Email.Should().Be("wife@example.com");
        result.Status.Should().Be("Pending");

        invitationRepository.Verify(x => x.AddAsync(It.IsAny<HouseholdInvitation>(), It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Requester_Has_No_Management_Permissions()
    {
        var householdRepository = new Mock<IHouseholdRepository>();
        var invitationRepository = new Mock<IHouseholdInvitationRepository>();
        var dateTimeProvider = new Mock<IDateTimeProvider>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var household = Household.Create(
            HouseholdId.New(),
            TenantId.New(),
            "Main Household");

        household.AddMember(
            HouseholdMemberId.New(),
            "Normal User",
            "user@example.com",
            HouseholdRole.Member);

        householdRepository
            .Setup(x => x.GetByIdAsync(household.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(household);

        var handler = new InviteHouseholdMemberHandler(
            householdRepository.Object,
            invitationRepository.Object,
            dateTimeProvider.Object,
            unitOfWork.Object);

        var command = new InviteHouseholdMemberCommand(
            household.Id.Value,
            "wife@example.com",
            HouseholdRole.Member,
            "user@example.com");

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not allowed*");
    }
}