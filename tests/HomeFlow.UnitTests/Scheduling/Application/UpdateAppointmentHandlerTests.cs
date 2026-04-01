using FluentAssertions;
using HomeFlow.BuildingBlocks.Application.Abstractions.Persistence;
using HomeFlow.BuildingBlocks.MultiTenancy.Models;
using HomeFlow.Modules.Households.Domain.Ids;
using HomeFlow.Modules.Scheduling.Application.Abstractions;
using HomeFlow.Modules.Scheduling.Application.Commands.UpdateAppointment;
using HomeFlow.Modules.Scheduling.Domain.Aggregates;
using HomeFlow.Modules.Scheduling.Domain.Enums;
using HomeFlow.Modules.Scheduling.Domain.Ids;
using HomeFlow.Modules.Scheduling.Domain.Repositories;
using Moq;
using Xunit;

namespace HomeFlow.UnitTests.Scheduling.Application;

public sealed class UpdateAppointmentHandlerTests
{
    [Fact]
    public async Task Handle_Should_Update_Appointment_When_Request_Is_Valid()
    {
        var appointmentRepository = new Mock<IAppointmentRepository>();
        var householdMemberLookup = new Mock<IHouseholdMemberLookup>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var householdId = HouseholdId.New();
        var memberId = HouseholdMemberId.New();

        var appointment = Appointment.Create(
            AppointmentId.New(),
            TenantId.New(),
            householdId,
            "Dentist",
            "Old description",
            new DateTime(2026, 3, 30, 8, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 3, 30, 9, 0, 0, DateTimeKind.Utc),
            "Old clinic");

        appointment.AddParticipant(AppointmentParticipantId.New(), memberId);

        appointmentRepository
            .Setup(x => x.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        householdMemberLookup
            .Setup(x => x.AllBelongToHouseholdAsync(
                householdId,
                It.IsAny<IReadOnlyCollection<HouseholdMemberId>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new UpdateAppointmentHandler(
            appointmentRepository.Object,
            householdMemberLookup.Object,
            unitOfWork.Object);

        var command = new UpdateAppointmentCommand(
            appointment.Id.Value,
            "Updated dentist",
            "Bring insurance documents",
            new DateTime(2026, 3, 30, 10, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 3, 30, 11, 0, 0, DateTimeKind.Utc),
            "New clinic",
            "Payment",
            "Done",
            new[] { memberId.Value });

        var result = await handler.Handle(command, CancellationToken.None);

        result.Title.Should().Be("Updated dentist");
        result.Type.Should().Be("Payment");
        result.Status.Should().Be("Done");
        result.StartsAtUtc.Should().Be(new DateTime(2026, 3, 30, 10, 0, 0, DateTimeKind.Utc));
        result.EndsAtUtc.Should().Be(new DateTime(2026, 3, 30, 11, 0, 0, DateTimeKind.Utc));

        appointment.Title.Should().Be("Updated dentist");
        appointment.Description.Should().Be("Bring insurance documents");
        appointment.Location.Should().Be("New clinic");
        appointment.Type.Should().Be(AppointmentType.Payment);
        appointment.Status.Should().Be(AppointmentStatus.Done);
        appointment.Participants.Should().HaveCount(1);

        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Appointment_Does_Not_Exist()
    {
        var appointmentRepository = new Mock<IAppointmentRepository>();
        var householdMemberLookup = new Mock<IHouseholdMemberLookup>();
        var unitOfWork = new Mock<IUnitOfWork>();

        appointmentRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<AppointmentId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Appointment?)null);

        var handler = new UpdateAppointmentHandler(
            appointmentRepository.Object,
            householdMemberLookup.Object,
            unitOfWork.Object);

        var command = new UpdateAppointmentCommand(
            Guid.NewGuid(),
            "Updated dentist",
            null,
            new DateTime(2026, 3, 30, 10, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 3, 30, 11, 0, 0, DateTimeKind.Utc),
            null,
            null,
            null,
            []);

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Appointment was not found*");

        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Participant_Does_Not_Belong_To_Household()
    {
        var appointmentRepository = new Mock<IAppointmentRepository>();
        var householdMemberLookup = new Mock<IHouseholdMemberLookup>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var appointment = Appointment.Create(
            AppointmentId.New(),
            TenantId.New(),
            HouseholdId.New(),
            "Dentist",
            null,
            new DateTime(2026, 3, 30, 8, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 3, 30, 9, 0, 0, DateTimeKind.Utc),
            null);

        appointmentRepository
            .Setup(x => x.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        householdMemberLookup
            .Setup(x => x.AllBelongToHouseholdAsync(
                appointment.HouseholdId,
                It.IsAny<IReadOnlyCollection<HouseholdMemberId>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new UpdateAppointmentHandler(
            appointmentRepository.Object,
            householdMemberLookup.Object,
            unitOfWork.Object);

        var command = new UpdateAppointmentCommand(
            appointment.Id.Value,
            "Updated dentist",
            null,
            new DateTime(2026, 3, 30, 10, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 3, 30, 11, 0, 0, DateTimeKind.Utc),
            null,
            null,
            null,
            new[] { Guid.NewGuid() });

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*do not belong to the household*");

        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
