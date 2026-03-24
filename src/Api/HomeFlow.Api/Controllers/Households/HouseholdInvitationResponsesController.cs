using HomeFlow.Api.Contracts.Households;
using HomeFlow.Modules.Households.Application.Commands.AcceptHouseholdInvitation;
using HomeFlow.Modules.Households.Application.Commands.DeclineHouseholdInvitation;
using Microsoft.AspNetCore.Mvc;

namespace HomeFlow.Api.Controllers.Households;

[ApiController]
[Route("api/household-invitations/{invitationId:guid}")]
public sealed class HouseholdInvitationResponsesController : ControllerBase
{
    private readonly AcceptHouseholdInvitationHandler _acceptHandler;
    private readonly DeclineHouseholdInvitationHandler _declineHandler;

    public HouseholdInvitationResponsesController(
        AcceptHouseholdInvitationHandler acceptHandler,
        DeclineHouseholdInvitationHandler declineHandler)
    {
        _acceptHandler = acceptHandler;
        _declineHandler = declineHandler;
    }

    [HttpPost("accept")]
    [ProducesResponseType(typeof(AcceptHouseholdInvitationResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AcceptHouseholdInvitationResponse>> Accept(
        [FromRoute] Guid invitationId,
        [FromBody] AcceptHouseholdInvitationRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new AcceptHouseholdInvitationCommand(
                invitationId,
                request.DisplayName,
                request.Email);

            var response = await _acceptHandler.Handle(command, cancellationToken);

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("decline")]
    [ProducesResponseType(typeof(DeclineHouseholdInvitationResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<DeclineHouseholdInvitationResponse>> Decline(
        [FromRoute] Guid invitationId,
        [FromBody] DeclineHouseholdInvitationRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new DeclineHouseholdInvitationCommand(
                invitationId,
                request.Email);

            var response = await _declineHandler.Handle(command, cancellationToken);

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}