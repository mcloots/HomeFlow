using FluentAssertions;
using HomeFlow.BuildingBlocks.Application.Abstractions.Persistence;
using HomeFlow.Modules.Households.Domain.Repositories;
using HomeFlow.Modules.Tenancy.Application.Commands.CreateTenantAndHousehold;
using HomeFlow.Modules.Tenancy.Domain.Repositories;
using Moq;
using Xunit;

namespace HomeFlow.UnitTests.Tenancy.Application;

public sealed class CreateTenantAndHouseholdHandlerTests
{
    [Fact]
    public async Task Handle_Should_Create_Tenant_Household_And_OwnerMember()
    {
        var tenantRepository = new Mock<ITenantRepository>();
        var householdRepository = new Mock<IHouseholdRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        householdRepository
            .Setup(x => x.MemberEmailExistsAsync("mich@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new CreateTenantAndHouseholdHandler(
            tenantRepository.Object,
            householdRepository.Object,
            unitOfWork.Object);

        var command = new CreateTenantAndHouseholdCommand(
            "Cloots Family",
            "Main Household",
            "Mich",
            "mich@example.com");

        var result = await handler.Handle(command, CancellationToken.None);

        result.TenantName.Should().Be("Cloots Family");
        result.HouseholdName.Should().Be("Main Household");
        result.OwnerMemberId.Should().NotBe(Guid.Empty);

        tenantRepository.Verify(x => x.AddAsync(It.IsAny<Modules.Tenancy.Domain.Aggregates.Tenant>(), It.IsAny<CancellationToken>()), Times.Once);
        householdRepository.Verify(x => x.AddAsync(It.IsAny<Modules.Households.Domain.Aggregates.Household>(), It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Owner_Email_Already_Exists()
    {
        var tenantRepository = new Mock<ITenantRepository>();
        var householdRepository = new Mock<IHouseholdRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        householdRepository
            .Setup(x => x.MemberEmailExistsAsync("mich@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new CreateTenantAndHouseholdHandler(
            tenantRepository.Object,
            householdRepository.Object,
            unitOfWork.Object);

        var command = new CreateTenantAndHouseholdCommand(
            "Cloots Family",
            "Main Household",
            "Mich",
            "mich@example.com");

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");

        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}