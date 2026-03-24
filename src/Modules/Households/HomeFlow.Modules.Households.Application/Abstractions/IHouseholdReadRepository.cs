using HomeFlow.Modules.Households.Application.Queries.GetHouseholdDetails;

namespace HomeFlow.Modules.Households.Application.Abstractions;

/*
 Waarom in Application?

Omdat dit een query/read abstraction is voor een application use case, 
niet een domeinrepository voor aggregate persistence.
 */
public interface IHouseholdReadRepository
{
    Task<GetHouseholdDetailsResponse?> GetDetailsByIdAsync(
        Guid householdId,
        CancellationToken cancellationToken = default);
}