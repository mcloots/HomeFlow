using HomeFlow.Modules.Integrations.Gmail.Application.Commands.CompleteGmailConnect;
using HomeFlow.Modules.Integrations.Gmail.Application.Commands.DisconnectGmailConnection;
using HomeFlow.Modules.Integrations.Gmail.Application.Commands.StartGmailConnect;
using HomeFlow.Modules.Integrations.Gmail.Application.Queries.GetCurrentGmailConnectionByHousehold;
using Microsoft.AspNetCore.Mvc;

namespace HomeFlow.Api.Controllers.Integrations;

[ApiController]
[Route("api/integrations/gmail")]
public sealed class GmailIntegrationsController : ControllerBase
{
    private readonly StartGmailConnectHandler _startHandler;
    private readonly CompleteGmailConnectHandler _completeHandler;
    private readonly DisconnectGmailConnectionHandler _disconnectHandler;
    private readonly GetCurrentGmailConnectionByHouseholdHandler _getCurrentHandler;

    public GmailIntegrationsController(
        StartGmailConnectHandler startHandler,
        CompleteGmailConnectHandler completeHandler,
        DisconnectGmailConnectionHandler disconnectHandler,
        GetCurrentGmailConnectionByHouseholdHandler getCurrentHandler)
    {
        _startHandler = startHandler;
        _completeHandler = completeHandler;
        _disconnectHandler = disconnectHandler;
        _getCurrentHandler = getCurrentHandler;
    }

    [HttpPost("connect/start")]
    public async Task<ActionResult<StartGmailConnectResponse>> Start(
        [FromBody] StartGmailConnectRequest request,
        CancellationToken cancellationToken)
    {
        var command = new StartGmailConnectCommand(
            request.TenantId,
            request.HouseholdId);

        var response = await _startHandler.Handle(command, cancellationToken);

        return Ok(response);
    }

    [HttpGet("connect/callback")]
    public async Task<ActionResult<CompleteGmailConnectResponse>> Callback(
        [FromQuery] string code,
        [FromQuery] string state,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new CompleteGmailConnectCommand(code, state);
            var response = await _completeHandler.Handle(command, cancellationToken);

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("connections/current")]
    public async Task<ActionResult<GetCurrentGmailConnectionByHouseholdResponse>> GetCurrent(
        [FromQuery] Guid householdId,
        CancellationToken cancellationToken)
    {
        var query = new GetCurrentGmailConnectionByHouseholdQuery(householdId);
        var result = await _getCurrentHandler.Handle(query, cancellationToken);

        if (result is null)
            return NotFound();

        return Ok(result);
    }

    [HttpPost("connections/{gmailConnectionId:guid}/disconnect")]
    public async Task<IActionResult> Disconnect(
        [FromRoute] Guid gmailConnectionId,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new DisconnectGmailConnectionCommand(gmailConnectionId);
            await _disconnectHandler.Handle(command, cancellationToken);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

public sealed record StartGmailConnectRequest(
    Guid TenantId,
    Guid HouseholdId);