using FluentAssertions;
using HomeFlow.Modules.Scheduling.Application.Abstractions;
using HomeFlow.Modules.Scheduling.Application.Queries.GetAppointmentsForDateRange;
using Moq;
using Xunit;

namespace HomeFlow.UnitTests.Scheduling.Application;

public sealed class GetAppointmentsForDateRangeHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_Appointments()
    {
        var repository = new Mock<IAppointmentReadRepository>();

        var expectedResponse =
            new GetAppointmentsForDateRangeResponse(
                Guid.NewGuid(),
                DateTime.UtcNow,
                DateTime.UtcNow.AddDays(1),
                []);

        repository
            .Setup(x => x.GetForDateRangeAsync(
                It.IsAny<Guid>(),
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var handler = new GetAppointmentsForDateRangeHandler(repository.Object);

        var query = new GetAppointmentsForDateRangeQuery(
            Guid.NewGuid(),
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(1));

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().Be(expectedResponse);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_DateRange_Invalid()
    {
        var repository = new Mock<IAppointmentReadRepository>();

        var handler = new GetAppointmentsForDateRangeHandler(repository.Object);

        var query = new GetAppointmentsForDateRangeQuery(
            Guid.NewGuid(),
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(-1));

        var act = async () =>
            await handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}