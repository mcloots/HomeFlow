using HomeFlow.Api.Contracts.Billing;
using HomeFlow.Modules.Billing.Application.Commands.CreateBill;
using HomeFlow.Modules.Billing.Application.Commands.UpdateBill;
using HomeFlow.Modules.Billing.Application.Queries.GetBillDetails;
using HomeFlow.Modules.Billing.Application.Queries.GetBillsForHousehold;
using Microsoft.AspNetCore.Mvc;

namespace HomeFlow.Api.Controllers.Billing;

[ApiController]
[Route("api/bills")]
public sealed class BillsController : ControllerBase
{
    private readonly CreateBillHandler _createBillHandler;
    private readonly UpdateBillHandler _updateBillHandler;
    private readonly GetBillDetailsHandler _getBillDetailsHandler;
    private readonly GetBillsForHouseholdHandler _getBillsForHouseholdHandler;

    public BillsController(
        CreateBillHandler createBillHandler,
        UpdateBillHandler updateBillHandler,
        GetBillDetailsHandler getBillDetailsHandler,
        GetBillsForHouseholdHandler getBillsForHouseholdHandler)
    {
        _createBillHandler = createBillHandler;
        _updateBillHandler = updateBillHandler;
        _getBillDetailsHandler = getBillDetailsHandler;
        _getBillsForHouseholdHandler = getBillsForHouseholdHandler;
    }

    [HttpPost]
    [ProducesResponseType(typeof(CreateBillResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<CreateBillResponse>> Create(
        [FromBody] CreateBillRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _createBillHandler.Handle(
                new CreateBillCommand(
                    request.TenantId,
                    request.HouseholdId,
                    request.Title,
                    request.Amount,
                    request.DueDateUtc,
                    request.Category),
                cancellationToken);

            return CreatedAtAction(nameof(GetById), new { billId = response.BillId }, response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(GetBillsForHouseholdResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<GetBillsForHouseholdResponse>> GetForHousehold(
        [FromQuery] Guid householdId,
        CancellationToken cancellationToken)
    {
        var result = await _getBillsForHouseholdHandler.Handle(
            new GetBillsForHouseholdQuery(householdId),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{billId:guid}")]
    [ProducesResponseType(typeof(GetBillDetailsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<GetBillDetailsResponse>> GetById(
        [FromRoute] Guid billId,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _getBillDetailsHandler.Handle(
                new GetBillDetailsQuery(billId),
                cancellationToken);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpPut("{billId:guid}")]
    [ProducesResponseType(typeof(UpdateBillResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<UpdateBillResponse>> Update(
        [FromRoute] Guid billId,
        [FromBody] UpdateBillRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _updateBillHandler.Handle(
                new UpdateBillCommand(
                    billId,
                    request.Title,
                    request.Amount,
                    request.DueDateUtc,
                    request.Category,
                    request.Status,
                    request.PaidAtUtc),
                cancellationToken);

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
