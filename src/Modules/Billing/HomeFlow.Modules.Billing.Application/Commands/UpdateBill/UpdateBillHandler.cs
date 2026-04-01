using HomeFlow.BuildingBlocks.Application.Abstractions.Persistence;
using HomeFlow.Modules.Billing.Domain.Enums;
using HomeFlow.Modules.Billing.Domain.Ids;
using HomeFlow.Modules.Billing.Domain.Repositories;

namespace HomeFlow.Modules.Billing.Application.Commands.UpdateBill;

public sealed class UpdateBillHandler
{
    private readonly IBillRepository _billRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateBillHandler(
        IBillRepository billRepository,
        IUnitOfWork unitOfWork)
    {
        _billRepository = billRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<UpdateBillResponse> Handle(
        UpdateBillCommand command,
        CancellationToken cancellationToken = default)
    {
        var bill = await _billRepository.GetByIdAsync(new BillId(command.BillId), cancellationToken);

        if (bill is null)
            throw new InvalidOperationException("Bill was not found.");

        bill.UpdateDetails(
            command.Title,
            command.Amount,
            command.DueDateUtc,
            command.Category);

        if (!string.IsNullOrWhiteSpace(command.Status))
        {
            bill.SetStatus(ParseBillStatus(command.Status), command.PaidAtUtc);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdateBillResponse(
            bill.Id.Value,
            bill.Title,
            bill.Amount,
            bill.DueDateUtc,
            bill.Status.ToString(),
            bill.Category,
            bill.PaidAtUtc);
    }

    private static BillStatus ParseBillStatus(string value)
    {
        if (Enum.TryParse<BillStatus>(value, true, out var billStatus) &&
            Enum.IsDefined(billStatus))
        {
            return billStatus;
        }

        throw new InvalidOperationException("Bill status is invalid.");
    }
}
