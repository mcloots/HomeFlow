using HomeFlow.Modules.Households.Application.Queries.GetHouseholdDetails;
using HomeFlow.Modules.Households.Application.Queries.GetHouseholdMembers;
using Microsoft.AspNetCore.Mvc;

namespace HomeFlow.Api.Controllers.Households;

[ApiController]
[Route("api/households")]
public sealed class HouseholdsController : ControllerBase
{
    private readonly GetHouseholdDetailsHandler _getHouseholdDetailsHandler;
    private readonly GetHouseholdMembersHandler _getHouseholdMembersHandler;

    public HouseholdsController(
        GetHouseholdDetailsHandler getHouseholdDetailsHandler,
        GetHouseholdMembersHandler getHouseholdMembersHandler)
    {
        _getHouseholdDetailsHandler = getHouseholdDetailsHandler;
        _getHouseholdMembersHandler = getHouseholdMembersHandler;
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

    [HttpGet("{householdId:guid}/members")]
    [ProducesResponseType(typeof(GetHouseholdMembersResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<GetHouseholdMembersResponse>> GetMembers(
        [FromRoute] Guid householdId,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetHouseholdMembersQuery(householdId);
            var response = await _getHouseholdMembersHandler.Handle(query, cancellationToken);

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}
