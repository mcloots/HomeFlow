using FluentAssertions;
using HomeFlow.Modules.Households.Application.Abstractions;
using HomeFlow.Modules.Households.Application.Queries.GetHouseholdMembers;
using Moq;
using Xunit;

namespace HomeFlow.UnitTests.Households.Application;

public sealed class GetHouseholdMembersHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_Members_When_Household_Exists()
    {
        var repository = new Mock<IHouseholdReadRepository>();
        var householdId = Guid.NewGuid();

        var response = new GetHouseholdMembersResponse(
            householdId,
            [
                new HouseholdMemberListItemDto(
                    Guid.NewGuid(),
                    "Mich",
                    "mich@example.com",
                    "Owner")
            ]);

        repository
            .Setup(x => x.GetMembersByHouseholdIdAsync(householdId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var handler = new GetHouseholdMembersHandler(repository.Object);

        var result = await handler.Handle(new GetHouseholdMembersQuery(householdId), CancellationToken.None);

        result.Should().Be(response);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Household_Does_Not_Exist()
    {
        var repository = new Mock<IHouseholdReadRepository>();

        repository
            .Setup(x => x.GetMembersByHouseholdIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetHouseholdMembersResponse?)null);

        var handler = new GetHouseholdMembersHandler(repository.Object);

        var act = async () => await handler.Handle(new GetHouseholdMembersQuery(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Household was not found*");
    }
}
