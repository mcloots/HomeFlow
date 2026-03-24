using HomeFlow.Api.Contracts.Households;
using HomeFlow.Modules.Households.Application.Commands.AcceptHouseholdInvitation;
using HomeFlow.Modules.Households.Application.Commands.DeclineHouseholdInvitation;
using HomeFlow.Modules.Households.Application.Commands.RevokeHouseholdInvitation;
using Microsoft.AspNetCore.Mvc;

namespace HomeFlow.Api.Controllers.Households;

[ApiController]
[Route("api/household-invitations/{invitationId:guid}")]
public sealed class HouseholdInvitationResponsesController : ControllerBase
{
    private readonly AcceptHouseholdInvitationHandler _acceptHandler;
    private readonly DeclineHouseholdInvitationHandler _declineHandler;
    private readonly RevokeHouseholdInvitationHandler _revokeHandler;

    public HouseholdInvitationResponsesController(
        AcceptHouseholdInvitationHandler acceptHandler,
        DeclineHouseholdInvitationHandler declineHandler,
        RevokeHouseholdInvitationHandler revokeHandler)
    {
        _acceptHandler = acceptHandler;
        _declineHandler = declineHandler;
        _revokeHandler = revokeHandler;
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

    [HttpPost("revoke")]
    [ProducesResponseType(typeof(RevokeHouseholdInvitationResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<RevokeHouseholdInvitationResponse>> Revoke(
    [FromRoute] Guid invitationId,
    [FromBody] RevokeHouseholdInvitationRequest request,
    CancellationToken cancellationToken)
    {
        try
        {
            var command = new RevokeHouseholdInvitationCommand(
                invitationId,
                request.RequestedByEmail);

            var response = await _revokeHandler.Handle(command, cancellationToken);

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}