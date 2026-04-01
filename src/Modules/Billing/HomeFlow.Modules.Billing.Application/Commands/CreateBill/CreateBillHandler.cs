using HomeFlow.BuildingBlocks.Application.Abstractions.Persistence;
using HomeFlow.BuildingBlocks.MultiTenancy.Models;
using HomeFlow.Modules.Billing.Domain.Aggregates;
using HomeFlow.Modules.Billing.Domain.Ids;
using HomeFlow.Modules.Billing.Domain.Repositories;
using HomeFlow.Modules.Households.Domain.Ids;

namespace HomeFlow.Modules.Billing.Application.Commands.CreateBill;

public sealed class CreateBillHandler
{
    private readonly IBillRepository _billRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateBillHandler(
        IBillRepository billRepository,
        IUnitOfWork unitOfWork)
    {
        _billRepository = billRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateBillResponse> Handle(
        CreateBillCommand command,
        CancellationToken cancellationToken = default)
    {
        var bill = Bill.Create(
            BillId.New(),
            new TenantId(command.TenantId),
            new HouseholdId(command.HouseholdId),
            command.Title,
            command.Amount,
            command.DueDateUtc,
            command.Category);

        await _billRepository.AddAsync(bill, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateBillResponse(
            bill.Id.Value,
            bill.TenantId.Value,
            bill.HouseholdId.Value,
            bill.Title,
            bill.Amount,
            bill.DueDateUtc,
            bill.Status.ToString(),
            bill.Category,
            bill.PaidAtUtc);
    }
}
