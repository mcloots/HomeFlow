using HomeFlow.Api.Contracts.Scheduling;
using HomeFlow.Modules.Scheduling.Application.Commands.CreateAppointment;
using Microsoft.AspNetCore.Mvc;

namespace HomeFlow.Api.Controllers.Scheduling;

[ApiController]
[Route("api/appointments")]
public sealed class AppointmentsController : ControllerBase
{
    private readonly CreateAppointmentHandler _createAppointmentHandler;

    public AppointmentsController(CreateAppointmentHandler createAppointmentHandler)
    {
        _createAppointmentHandler = createAppointmentHandler;
    }

    [HttpPost]
    [ProducesResponseType(typeof(CreateAppointmentResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<CreateAppointmentResponse>> Create(
        [FromBody] CreateAppointmentRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new CreateAppointmentCommand(
                request.TenantId,
                request.HouseholdId,
                request.Title,
                request.Description,
                request.StartsAtUtc,
                request.EndsAtUtc,
                request.Location,
                request.ParticipantMemberIds);

            var response = await _createAppointmentHandler.Handle(command, cancellationToken);

            return CreatedAtAction(nameof(Create), new { appointmentId = response.AppointmentId }, response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}