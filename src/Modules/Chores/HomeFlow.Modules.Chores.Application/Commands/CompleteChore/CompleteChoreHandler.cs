using HomeFlow.BuildingBlocks.Application.Abstractions;
using HomeFlow.BuildingBlocks.Application.Abstractions.Persistence;
using HomeFlow.Modules.Chores.Application.Abstractions;
using HomeFlow.Modules.Chores.Domain.Ids;
using HomeFlow.Modules.Chores.Domain.Repositories;
using HomeFlow.Modules.Households.Domain.Ids;

namespace HomeFlow.Modules.Chores.Application.Commands.CompleteChore;

public sealed class CompleteChoreHandler
{
    private readonly IChoreRepository _choreRepository;
    private readonly IHouseholdMemberLookup _householdMemberLookup;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;

    public CompleteChoreHandler(
        IChoreRepository choreRepository,
        IHouseholdMemberLookup householdMemberLookup,
        IDateTimeProvider dateTimeProvider,
        IUnitOfWork unitOfWork)
    {
        _choreRepository = choreRepository;
        _householdMemberLookup = householdMemberLookup;
        _dateTimeProvider = dateTimeProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<CompleteChoreResponse> Handle(
        CompleteChoreCommand command,
        CancellationToken cancellationToken = default)
    {
        var chore = await _choreRepository.GetByIdAsync(
            new ChoreId(command.ChoreId),
            cancellationToken);

        if (chore is null)
            throw new InvalidOperationException("Chore was not found.");

        var completedByMemberId = new HouseholdMemberId(command.CompletedByMemberId);
        var belongsToHousehold = await _householdMemberLookup.AllBelongToHouseholdAsync(
            chore.HouseholdId,
            [completedByMemberId],
            cancellationToken);

        if (!belongsToHousehold)
            throw new InvalidOperationException("Completing member does not belong to the household.");

        var completedAtUtc = command.CompletedAtUtc ?? _dateTimeProvider.UtcNow;
        chore.MarkCompleted(completedByMemberId, completedAtUtc);

        Guid? nextChoreId = null;
        var nextOccurrence = chore.CreateNextOccurrence(ChoreId.New());

        if (nextOccurrence is not null)
        {
            nextChoreId = nextOccurrence.Id.Value;
            await _choreRepository.AddAsync(nextOccurrence, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CompleteChoreResponse(
            chore.Id.Value,
            chore.Status.ToString(),
            chore.CompletedAtUtc!.Value,
            chore.CompletedByMemberId!.Value.Value,
            nextChoreId);
    }
}
