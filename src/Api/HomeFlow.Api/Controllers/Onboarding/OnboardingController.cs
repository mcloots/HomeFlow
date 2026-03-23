using HomeFlow.Api.Contracts.Onboarding;
using HomeFlow.Modules.Tenancy.Application.Commands.CreateTenantAndHousehold;
using Microsoft.AspNetCore.Mvc;

namespace HomeFlow.Api.Controllers.Onboarding;

[ApiController]
[Route("api/[controller]")]
public sealed class OnboardingController : ControllerBase
{
    private readonly CreateTenantAndHouseholdHandler _handler;

    public OnboardingController(CreateTenantAndHouseholdHandler handler)
    {
        _handler = handler;
    }

    [HttpPost("tenant-household")]
    [ProducesResponseType(typeof(CreateTenantAndHouseholdResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<CreateTenantAndHouseholdResponse>> CreateTenantAndHousehold(
        [FromBody] CreateTenantAndHouseholdRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateTenantAndHouseholdCommand(
            request.TenantName,
            request.HouseholdName,
            request.OwnerDisplayName,
            request.OwnerEmail);

        var response = await _handler.Handle(command, cancellationToken);

        return CreatedAtAction(
            nameof(CreateTenantAndHousehold),
            new { tenantId = response.TenantId },
            response);
    }
}