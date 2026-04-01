using HomeFlow.Api.Contracts.Scheduling;
using HomeFlow.Modules.Scheduling.Application.Commands.CreateAppointment;
using HomeFlow.Modules.Scheduling.Application.Commands.UpdateAppointment;
using HomeFlow.Modules.Scheduling.Application.Queries.GetAppointmentDetails;
using HomeFlow.Modules.Scheduling.Application.Queries.GetAppointmentsForDateRange;
using Microsoft.AspNetCore.Mvc;

namespace HomeFlow.Api.Controllers.Scheduling;

[ApiController]
[Route("api/appointments")]
public sealed class AppointmentsController : ControllerBase
{
    private readonly CreateAppointmentHandler _createAppointmentHandler;
    private readonly GetAppointmentsForDateRangeHandler _getForDateRangeHandler;
    private readonly GetAppointmentDetailsHandler _getDetailsHandler;
    private readonly UpdateAppointmentHandler _updateAppointmentHandler;

    public AppointmentsController(
        CreateAppointmentHandler createAppointmentHandler,
        GetAppointmentsForDateRangeHandler getForDateRangeHandler,
        GetAppointmentDetailsHandler getDetailsHandler,
        UpdateAppointmentHandler updateAppointmentHandler)
    {
        _createAppointmentHandler = createAppointmentHandler;
        _getForDateRangeHandler = getForDateRangeHandler;
        _getDetailsHandler = getDetailsHandler;
        _updateAppointmentHandler = updateAppointmentHandler;
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
                request.Type,
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

            var result = await _getForDateRangeHandler.Handle(query, cancellationToken);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{appointmentId:guid}")]
    [ProducesResponseType(typeof(GetAppointmentDetailsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<GetAppointmentDetailsResponse>> GetById(
    [FromRoute] Guid appointmentId,
    CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetAppointmentDetailsQuery(appointmentId);
            var result = await _getDetailsHandler.Handle(query, cancellationToken);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpPut("{appointmentId:guid}")]
    [ProducesResponseType(typeof(UpdateAppointmentResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<UpdateAppointmentResponse>> Update(
    [FromRoute] Guid appointmentId,
    [FromBody] UpdateAppointmentRequest request,
    CancellationToken cancellationToken)
    {
        try
        {
            var command = new UpdateAppointmentCommand(
                appointmentId,
                request.Title,
                request.Description,
                request.StartsAtUtc,
                request.EndsAtUtc,
                request.Location,
                request.Type,
                request.Status,
                request.ParticipantMemberIds);

            var response = await _updateAppointmentHandler.Handle(command, cancellationToken);

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
