using HomeFlow.Modules.Billing.Domain.Aggregates;
using HomeFlow.Modules.Billing.Domain.Ids;

namespace HomeFlow.Modules.Billing.Domain.Repositories;

public interface IBillRepository
{
    Task AddAsync(Bill bill, CancellationToken cancellationToken = default);
    Task<Bill?> GetByIdAsync(BillId billId, CancellationToken cancellationToken = default);
}
