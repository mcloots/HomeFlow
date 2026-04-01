using FluentAssertions;
using HomeFlow.Modules.Scheduling.Application.Abstractions;
using HomeFlow.Modules.Scheduling.Application.Queries.GetAppointmentDetails;
using Moq;
using Xunit;

namespace HomeFlow.UnitTests.Scheduling.Application;

public sealed class GetAppointmentDetailsHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_Appointment_Details_When_Appointment_Exists()
    {
        var repository = new Mock<IAppointmentReadRepository>();

        var response = new GetAppointmentDetailsResponse(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Dentist",
            "Annual check-up",
            new DateTime(2026, 3, 30, 8, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 3, 30, 9, 0, 0, DateTimeKind.Utc),
            "Clinic",
            "General",
            "Scheduled",
            new[]
            {
                new AppointmentDetailsParticipantDto(
                    Guid.NewGuid(),
                    "Mich")
            });

        repository
            .Setup(x => x.GetDetailsByIdAsync(response.AppointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var handler = new GetAppointmentDetailsHandler(repository.Object);

        var query = new GetAppointmentDetailsQuery(response.AppointmentId);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.AppointmentId.Should().Be(response.AppointmentId);
        result.Title.Should().Be("Dentist");
        result.Participants.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Appointment_Does_Not_Exist()
    {
        var repository = new Mock<IAppointmentReadRepository>();

        repository
            .Setup(x => x.GetDetailsByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetAppointmentDetailsResponse?)null);

        var handler = new GetAppointmentDetailsHandler(repository.Object);

        var query = new GetAppointmentDetailsQuery(Guid.NewGuid());

        var act = async () => await handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Appointment was not found*");
    }
}
