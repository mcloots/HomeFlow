using FluentAssertions;
using HomeFlow.BuildingBlocks.Domain.Exceptions;
using HomeFlow.BuildingBlocks.MultiTenancy.Models;
using HomeFlow.Modules.Households.Domain.Ids;
using HomeFlow.Modules.Scheduling.Domain.Aggregates;
using HomeFlow.Modules.Scheduling.Domain.Enums;
using HomeFlow.Modules.Scheduling.Domain.Ids;
using Xunit;

namespace HomeFlow.UnitTests.Scheduling.Domain;

public sealed class AppointmentTests
{
    [Fact]
    public void Create_Should_Set_Initial_Values()
    {
        var appointment = Appointment.Create(
            AppointmentId.New(),
            TenantId.New(),
            HouseholdId.New(),
            "Dentist",
            "Check-up",
            new DateTime(2026, 3, 30, 8, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 3, 30, 9, 0, 0, DateTimeKind.Utc),
            "Clinic");

        appointment.Title.Should().Be("Dentist");
        appointment.Status.Should().Be(AppointmentStatus.Scheduled);
        appointment.Participants.Should().BeEmpty();
    }

    [Fact]
    public void Create_Should_Throw_When_End_Is_Not_After_Start()
    {
        var act = () => Appointment.Create(
            AppointmentId.New(),
            TenantId.New(),
            HouseholdId.New(),
            "Dentist",
            null,
            new DateTime(2026, 3, 30, 9, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 3, 30, 9, 0, 0, DateTimeKind.Utc),
            null);

        act.Should().Throw<DomainException>()
            .WithMessage("*after start*");
    }

    [Fact]
    public void AddParticipant_Should_Add_Participant()
    {
        var appointment = Appointment.Create(
            AppointmentId.New(),
            TenantId.New(),
            HouseholdId.New(),
            "Dentist",
            null,
            new DateTime(2026, 3, 30, 8, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 3, 30, 9, 0, 0, DateTimeKind.Utc),
            null);

        appointment.AddParticipant(
            AppointmentParticipantId.New(),
            HouseholdMemberId.New());

        appointment.Participants.Should().HaveCount(1);
    }

    [Fact]
    public void AddParticipant_Should_Throw_When_Participant_Already_Exists()
    {
        var appointment = Appointment.Create(
            AppointmentId.New(),
            TenantId.New(),
            HouseholdId.New(),
            "Dentist",
            null,
            new DateTime(2026, 3, 30, 8, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 3, 30, 9, 0, 0, DateTimeKind.Utc),
            null);

        var memberId = HouseholdMemberId.New();

        appointment.AddParticipant(AppointmentParticipantId.New(), memberId);

        var act = () => appointment.AddParticipant(AppointmentParticipantId.New(), memberId);

        act.Should().Throw<DomainException>()
            .WithMessage("*already a participant*");
    }

    [Fact]
    public void Cancel_Should_Set_Status_To_Cancelled()
    {
        var appointment = Appointment.Create(
            AppointmentId.New(),
            TenantId.New(),
            HouseholdId.New(),
            "Dentist",
            null,
            new DateTime(2026, 3, 30, 8, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 3, 30, 9, 0, 0, DateTimeKind.Utc),
            null);

        appointment.Cancel();

        appointment.Status.Should().Be(AppointmentStatus.Cancelled);
    }
}