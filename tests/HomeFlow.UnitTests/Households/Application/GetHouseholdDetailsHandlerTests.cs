using FluentAssertions;
using HomeFlow.Modules.Households.Application.Abstractions;
using HomeFlow.Modules.Households.Application.Queries.GetHouseholdDetails;
using Moq;
using Xunit;

namespace HomeFlow.UnitTests.Households.Application;

public sealed class GetHouseholdDetailsHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_Household_Details_When_Household_Exists()
    {
        var readRepository = new Mock<IHouseholdReadRepository>();

        var response = new GetHouseholdDetailsResponse(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Main Household",
            "Active",
            new[]
            {
                new HouseholdMemberDetailsDto(
                    Guid.NewGuid(),
                    "Mich Cloots",
                    "mich@example.com",
                    "Owner")
            },
            new[]
            {
                new HouseholdInvitationDetailsDto(
                    Guid.NewGuid(),
                    "wife@example.com",
                    "Member",
                    "Pending",
                    DateTime.UtcNow)
            });

        readRepository
            .Setup(x => x.GetDetailsByIdAsync(response.HouseholdId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var handler = new GetHouseholdDetailsHandler(readRepository.Object);

        var query = new GetHouseholdDetailsQuery(response.HouseholdId);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.HouseholdId.Should().Be(response.HouseholdId);
        result.Name.Should().Be("Main Household");
        result.Members.Should().HaveCount(1);
        result.Invitations.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Household_Does_Not_Exist()
    {
        var readRepository = new Mock<IHouseholdReadRepository>();

        readRepository
            .Setup(x => x.GetDetailsByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetHouseholdDetailsResponse?)null);

        var handler = new GetHouseholdDetailsHandler(readRepository.Object);

        var query = new GetHouseholdDetailsQuery(Guid.NewGuid());

        var act = async () => await handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Household was not found*");
    }
}