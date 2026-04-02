using HomeFlow.Api.Contracts.Chores;
using HomeFlow.Modules.Chores.Application.Commands.CompleteChore;
using HomeFlow.Modules.Chores.Application.Commands.CreateChore;
using HomeFlow.Modules.Chores.Application.Commands.UpdateChore;
using HomeFlow.Modules.Chores.Application.Queries.GetChoresForHousehold;
using Microsoft.AspNetCore.Mvc;

namespace HomeFlow.Api.Controllers;

[ApiController]
[Route("api/chores")]
public sealed class ChoresController : ControllerBase
{
    private readonly CreateChoreHandler _createChoreHandler;
    private readonly UpdateChoreHandler _updateChoreHandler;
    private readonly CompleteChoreHandler _completeChoreHandler;
    private readonly GetChoresForHouseholdHandler _getChoresForHouseholdHandler;

    public ChoresController(
        CreateChoreHandler createChoreHandler,
        UpdateChoreHandler updateChoreHandler,
        CompleteChoreHandler completeChoreHandler,
        GetChoresForHouseholdHandler getChoresForHouseholdHandler)
    {
        _createChoreHandler = createChoreHandler;
        _updateChoreHandler = updateChoreHandler;
        _completeChoreHandler = completeChoreHandler;
        _getChoresForHouseholdHandler = getChoresForHouseholdHandler;
    }

    [HttpGet]
    [ProducesResponseType(typeof(GetChoresForHouseholdResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<GetChoresForHouseholdResponse>> GetForHousehold(
        [FromQuery] Guid householdId,
        CancellationToken cancellationToken)
    {
        var response = await _getChoresForHouseholdHandler.Handle(
            new GetChoresForHouseholdQuery(householdId),
            cancellationToken);

        return Ok(response);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CreateChoreResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<CreateChoreResponse>> Create(
        [FromBody] CreateChoreRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _createChoreHandler.Handle(
                new CreateChoreCommand(
                    request.TenantId,
                    request.HouseholdId,
                    request.Title,
                    request.Description,
                    request.DueDateUtc,
                    request.AssignedMemberId,
                    request.Recurrence,
                    request.RecurrenceMonths),
                cancellationToken);

            return CreatedAtAction(nameof(GetForHousehold), new { householdId = response.HouseholdId }, response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{choreId:guid}")]
    [ProducesResponseType(typeof(UpdateChoreResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<UpdateChoreResponse>> Update(
        [FromRoute] Guid choreId,
        [FromBody] UpdateChoreRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _updateChoreHandler.Handle(
                new UpdateChoreCommand(
                    choreId,
                    request.Title,
                    request.Description,
                    request.DueDateUtc,
                    request.AssignedMemberId,
                    request.Recurrence,
                    request.RecurrenceMonths),
                cancellationToken);

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{choreId:guid}/complete")]
    [ProducesResponseType(typeof(CompleteChoreResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<CompleteChoreResponse>> Complete(
        [FromRoute] Guid choreId,
        [FromBody] CompleteChoreRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _completeChoreHandler.Handle(
                new CompleteChoreCommand(
                    choreId,
                    request.CompletedByMemberId,
                    request.CompletedAtUtc),
                cancellationToken);

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
