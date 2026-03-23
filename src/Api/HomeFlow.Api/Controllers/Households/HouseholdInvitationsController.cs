using HomeFlow.Api.Contracts.Households;
using HomeFlow.Modules.Households.Application.Commands.InviteHouseholdMember;
using HomeFlow.Modules.Households.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace HomeFlow.Api.Controllers.Households;

[ApiController]
[Route("api/households/{householdId:guid}/invitations")]
public sealed class HouseholdInvitationsController : ControllerBase
{
    private readonly InviteHouseholdMemberHandler _handler;

    public HouseholdInvitationsController(InviteHouseholdMemberHandler handler)
    {
        _handler = handler;
    }

    [HttpPost]
    [ProducesResponseType(typeof(InviteHouseholdMemberResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<InviteHouseholdMemberResponse>> Invite(
        [FromRoute] Guid householdId,
        [FromBody] InviteHouseholdMemberRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new InviteHouseholdMemberCommand(
                householdId,
                request.InvitedEmail,
                (HouseholdRole)request.Role,
                request.RequestedByEmail);

            var response = await _handler.Handle(command, cancellationToken);

            return CreatedAtAction(nameof(Invite), new { householdId, invitationId = response.InvitationId }, response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}