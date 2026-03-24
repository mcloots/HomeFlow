using HomeFlow.Api.Contracts.Scheduling;
using HomeFlow.Modules.Scheduling.Application.Commands.CreateAppointment;
using HomeFlow.Modules.Scheduling.Application.Queries.GetAppointmentsForDateRange;
using Microsoft.AspNetCore.Mvc;

namespace HomeFlow.Api.Controllers.Scheduling;

[ApiController]
[Route("api/appointments")]
public sealed class AppointmentsController : ControllerBase
{
    private readonly CreateAppointmentHandler _createAppointmentHandler;
    private readonly GetAppointmentsForDateRangeHandler _getHandler;

    public AppointmentsController(
    CreateAppointmentHandler createAppointmentHandler,
    GetAppointmentsForDateRangeHandler getHandler)
    {
        _createAppointmentHandler = createAppointmentHandler;
        _getHandler = getHandler;
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

    [HttpGet]
    [ProducesResponseType(typeof(GetAppointmentsForDateRangeResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<GetAppointmentsForDateRangeResponse>> GetForDateRange(
    [FromQuery] Guid householdId,
    [FromQuery] DateTime fromUtc,
    [FromQuery] DateTime toUtc,
    CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetAppointmentsForDateRangeQuery(
                householdId,
                fromUtc,
                toUtc);

            var result = await _getHandler.Handle(query, cancellationToken);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}