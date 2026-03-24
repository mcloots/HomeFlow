using HomeFlow.Modules.Households.Application.Queries.GetHouseholdDetails;
using Microsoft.AspNetCore.Mvc;

namespace HomeFlow.Api.Controllers.Households;

[ApiController]
[Route("api/households")]
public sealed class HouseholdsController : ControllerBase
{
    private readonly GetHouseholdDetailsHandler _getHouseholdDetailsHandler;

    public HouseholdsController(GetHouseholdDetailsHandler getHouseholdDetailsHandler)
    {
        _getHouseholdDetailsHandler = getHouseholdDetailsHandler;
    }

    [HttpGet("{householdId:guid}")]
    [ProducesResponseType(typeof(GetHouseholdDetailsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<GetHouseholdDetailsResponse>> GetById(
        [FromRoute] Guid householdId,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetHouseholdDetailsQuery(householdId);
            var response = await _getHouseholdDetailsHandler.Handle(query, cancellationToken);

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}