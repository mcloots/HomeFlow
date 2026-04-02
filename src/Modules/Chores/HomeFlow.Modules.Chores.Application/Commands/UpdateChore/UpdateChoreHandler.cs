using HomeFlow.BuildingBlocks.Application.Abstractions.Persistence;
using HomeFlow.Modules.Chores.Application.Abstractions;
using HomeFlow.Modules.Chores.Application.Commands.CreateChore;
using HomeFlow.Modules.Chores.Domain.Ids;
using HomeFlow.Modules.Chores.Domain.Repositories;
using HomeFlow.Modules.Households.Domain.Ids;

namespace HomeFlow.Modules.Chores.Application.Commands.UpdateChore;

public sealed class UpdateChoreHandler
{
    private readonly IChoreRepository _choreRepository;
    private readonly IHouseholdMemberLookup _householdMemberLookup;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateChoreHandler(
        IChoreRepository choreRepository,
        IHouseholdMemberLookup householdMemberLookup,
        IUnitOfWork unitOfWork)
    {
        _choreRepository = choreRepository;
        _householdMemberLookup = householdMemberLookup;
        _unitOfWork = unitOfWork;
    }

    public async Task<UpdateChoreResponse> Handle(
        UpdateChoreCommand command,
        CancellationToken cancellationToken = default)
    {
        var chore = await _choreRepository.GetByIdAsync(
            new ChoreId(command.ChoreId),
            cancellationToken);

        if (chore is null)
            throw new InvalidOperationException("Chore was not found.");

        var assignedMemberId = command.AssignedMemberId is Guid memberId
            ? new HouseholdMemberId(memberId)
            : (HouseholdMemberId?)null;

        if (assignedMemberId is not null)
        {
            var belongsToHousehold = await _householdMemberLookup.AllBelongToHouseholdAsync(
                chore.HouseholdId,
                [assignedMemberId.Value],
                cancellationToken);

            if (!belongsToHousehold)
                throw new InvalidOperationException("Assigned member does not belong to the household.");
        }

        chore.UpdateDetails(
            command.Title,
            command.Description,
            command.DueDateUtc,
            assignedMemberId,
            CreateChoreHandler.ParseRecurrence(command.Recurrence),
            command.RecurrenceMonths);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdateChoreResponse(
            chore.Id.Value,
            chore.Title,
            chore.DueDateUtc,
            chore.AssignedMemberId?.Value,
            chore.Status.ToString(),
            chore.Recurrence.ToString(),
            chore.RecurrenceMonths);
    }
}
