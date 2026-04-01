using FluentAssertions;
using HomeFlow.BuildingBlocks.Application.Abstractions.Persistence;
using HomeFlow.Modules.Households.Domain.Ids;
using HomeFlow.Modules.Scheduling.Application.Abstractions;
using HomeFlow.Modules.Scheduling.Application.Commands.CreateAppointment;
using HomeFlow.Modules.Scheduling.Domain.Enums;
using HomeFlow.Modules.Scheduling.Domain.Repositories;
using Moq;
using Xunit;

namespace HomeFlow.UnitTests.Scheduling.Application;

public sealed class CreateAppointmentHandlerTests
{
    [Fact]
    public async Task Handle_Should_Create_Appointment_When_Request_Is_Valid()
    {
        var appointmentRepository = new Mock<IAppointmentRepository>();
        var householdMemberLookup = new Mock<IHouseholdMemberLookup>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var participantIds = new[] { Guid.NewGuid() };

        householdMemberLookup
            .Setup(x => x.AllBelongToHouseholdAsync(
                It.IsAny<HouseholdId>(),
                It.IsAny<IReadOnlyCollection<HouseholdMemberId>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new CreateAppointmentHandler(
            appointmentRepository.Object,
            householdMemberLookup.Object,
            unitOfWork.Object);

        var command = new CreateAppointmentCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Dentist",
            "Check-up",
            new DateTime(2026, 3, 30, 8, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 3, 30, 9, 0, 0, DateTimeKind.Utc),
            "Clinic",
            "Payment",
            participantIds);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Title.Should().Be("Dentist");
        result.Type.Should().Be("Payment");
        result.Status.Should().Be("Scheduled");

        appointmentRepository.Verify(x => x.AddAsync(It.IsAny<Modules.Scheduling.Domain.Aggregates.Appointment>(), It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_A_Participant_Does_Not_Belong_To_Household()
    {
        var appointmentRepository = new Mock<IAppointmentRepository>();
        var householdMemberLookup = new Mock<IHouseholdMemberLookup>();
        var unitOfWork = new Mock<IUnitOfWork>();

        householdMemberLookup
            .Setup(x => x.AllBelongToHouseholdAsync(
                It.IsAny<HouseholdId>(),
                It.IsAny<IReadOnlyCollection<HouseholdMemberId>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new CreateAppointmentHandler(
            appointmentRepository.Object,
            householdMemberLookup.Object,
            unitOfWork.Object);

        var command = new CreateAppointmentCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Dentist",
            "Check-up",
            new DateTime(2026, 3, 30, 8, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 3, 30, 9, 0, 0, DateTimeKind.Utc),
            "Clinic",
            AppointmentType.General.ToString(),
            new[] { Guid.NewGuid() });

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*do not belong to the household*");

        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
