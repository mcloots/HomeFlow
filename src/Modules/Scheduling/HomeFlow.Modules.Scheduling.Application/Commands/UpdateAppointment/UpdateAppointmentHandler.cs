using HomeFlow.BuildingBlocks.Application.Abstractions.Persistence;
using HomeFlow.Modules.Households.Domain.Ids;
using HomeFlow.Modules.Scheduling.Application.Abstractions;
using HomeFlow.Modules.Scheduling.Domain.Ids;
using HomeFlow.Modules.Scheduling.Domain.Repositories;

namespace HomeFlow.Modules.Scheduling.Application.Commands.UpdateAppointment;

public sealed class UpdateAppointmentHandler
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IHouseholdMemberLookup _householdMemberLookup;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateAppointmentHandler(
        IAppointmentRepository appointmentRepository,
        IHouseholdMemberLookup householdMemberLookup,
        IUnitOfWork unitOfWork)
    {
        _appointmentRepository = appointmentRepository;
        _householdMemberLookup = householdMemberLookup;
        _unitOfWork = unitOfWork;
    }

    public async Task<UpdateAppointmentResponse> Handle(
        UpdateAppointmentCommand command,
        CancellationToken cancellationToken = default)
    {
        var appointmentId = new AppointmentId(command.AppointmentId);

        var appointment = await _appointmentRepository.GetByIdAsync(
            appointmentId,
            cancellationToken);

        if (appointment is null)
            throw new InvalidOperationException("Appointment was not found.");

        var participantIds = command.ParticipantMemberIds
            .Distinct()
            .Select(x => new HouseholdMemberId(x))
            .ToList();

        var allParticipantsBelongToHousehold = await _householdMemberLookup.AllBelongToHouseholdAsync(
            appointment.HouseholdId,
            participantIds,
            cancellationToken);

        if (!allParticipantsBelongToHousehold)
            throw new InvalidOperationException("One or more participants do not belong to the household.");

        appointment.UpdateDetails(
            command.Title,
            command.Description,
            command.StartsAtUtc,
            command.EndsAtUtc,
            command.Location);

        appointment.ReplaceParticipants(participantIds);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdateAppointmentResponse(
            appointment.Id.Value,
            appointment.Title,
            appointment.StartsAtUtc,
            appointment.EndsAtUtc,
            appointment.Status.ToString());
    }
}