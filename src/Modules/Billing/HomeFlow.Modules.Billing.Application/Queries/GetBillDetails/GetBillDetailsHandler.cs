using HomeFlow.Modules.Billing.Application.Abstractions;

namespace HomeFlow.Modules.Billing.Application.Queries.GetBillDetails;

public sealed class GetBillDetailsHandler
{
    private readonly IBillReadRepository _readRepository;

    public GetBillDetailsHandler(IBillReadRepository readRepository)
    {
        _readRepository = readRepository;
    }

    public async Task<GetBillDetailsResponse> Handle(
        GetBillDetailsQuery query,
        CancellationToken cancellationToken = default)
    {
        var result = await _readRepository.GetDetailsByIdAsync(query.BillId, cancellationToken);

        if (result is null)
            throw new InvalidOperationException("Bill was not found.");

        return result;
    }
}
