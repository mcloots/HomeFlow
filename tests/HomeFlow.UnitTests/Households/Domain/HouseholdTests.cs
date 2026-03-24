using FluentAssertions;
using HomeFlow.BuildingBlocks.Domain.Exceptions;
using HomeFlow.BuildingBlocks.MultiTenancy.Models;
using HomeFlow.Modules.Households.Domain.Aggregates;
using HomeFlow.Modules.Households.Domain.Enums;
using HomeFlow.Modules.Households.Domain.Ids;
using Xunit;

namespace HomeFlow.UnitTests.Households.Domain;

public sealed class HouseholdTests
{
    [Fact]
    public void Create_Should_Set_Initial_Values()
    {
        var householdId = HouseholdId.New();
        var tenantId = TenantId.New();

        var household = Household.Create(householdId, tenantId, "Main Household");

        household.Id.Should().Be(householdId);
        household.TenantId.Should().Be(tenantId);
        household.Name.Should().Be("Main Household");
        household.Status.Should().Be(HouseholdStatus.Active);
        household.Members.Should().BeEmpty();
    }

    [Fact]
    public void AddOwnerMember_Should_Add_Owner_To_Household()
    {
        var household = Household.Create(HouseholdId.New(), TenantId.New(), "Main Household");

        var member = household.AddOwnerMember(
            HouseholdMemberId.New(),
            "Mich",
            "mich@example.com");

        household.Members.Should().HaveCount(1);
        household.Members.Should().ContainSingle(x => x.Email == "mich@example.com");
        member.Role.Should().Be(HouseholdRole.Owner);
    }

    [Fact]
    public void AddMember_Should_Add_Regular_Member()
    {
        var household = Household.Create(HouseholdId.New(), TenantId.New(), "Main Household");

        var member = household.AddMember(
            HouseholdMemberId.New(),
            "Wife",
            "wife@example.com",
            HouseholdRole.Member);

        household.Members.Should().HaveCount(1);
        member.Role.Should().Be(HouseholdRole.Member);
    }

    [Fact]
    public void AddMember_Should_Throw_When_Email_Already_Exists()
    {
        var household = Household.Create(HouseholdId.New(), TenantId.New(), "Main Household");

        household.AddOwnerMember(
            HouseholdMemberId.New(),
            "Mich",
            "mich@example.com");

        var act = () => household.AddMember(
            HouseholdMemberId.New(),
            "Mich 2",
            "mich@example.com",
            HouseholdRole.Member);

        act.Should().Throw<DomainException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public void Rename_Should_Throw_When_Household_Is_Archived()
    {
        var household = Household.Create(HouseholdId.New(), TenantId.New(), "Main Household");
        household.Archive();

        var act = () => household.Rename("New Name");

        act.Should().Throw<DomainException>()
            .WithMessage("*active households*");
    }

    [Fact]
    public void HasManagementPermissions_Should_Return_True_For_Owner()
    {
        var household = Household.Create(HouseholdId.New(), TenantId.New(), "Main Household");

        household.AddOwnerMember(
            HouseholdMemberId.New(),
            "Mich",
            "mich@example.com");

        var result = household.HasManagementPermissions("mich@example.com");

        result.Should().BeTrue();
    }

    [Fact]
    public void HasManagementPermissions_Should_Return_False_For_Regular_Member()
    {
        var household = Household.Create(HouseholdId.New(), TenantId.New(), "Main Household");

        household.AddMember(
            HouseholdMemberId.New(),
            "User",
            "user@example.com",
            HouseholdRole.Member);

        var result = household.HasManagementPermissions("user@example.com");

        result.Should().BeFalse();
    }
}