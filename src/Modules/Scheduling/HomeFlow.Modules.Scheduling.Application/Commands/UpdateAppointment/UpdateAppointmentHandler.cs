using HomeFlow.BuildingBlocks.Application.Abstractions.Persistence;
using HomeFlow.Modules.Households.Domain.Ids;
using HomeFlow.Modules.Scheduling.Application.Abstractions;
using HomeFlow.Modules.Scheduling.Domain.Enums;
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

        var appointmentType = ParseAppointmentType(command.Type, appointment.Type);
        var appointmentStatus = ParseAppointmentStatus(command.Status, appointment.Status);

        appointment.UpdateDetails(
            command.Title,
            command.Description,
            command.StartsAtUtc,
            command.EndsAtUtc,
            command.Location);

        appointment.ReplaceParticipants(participantIds);
        appointment.SetType(appointmentType);
        appointment.SetStatus(appointmentStatus);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdateAppointmentResponse(
            appointment.Id.Value,
            appointment.Title,
            appointment.StartsAtUtc,
            appointment.EndsAtUtc,
            appointment.Status.ToString(),
            appointment.Type.ToString());
    }

    private static AppointmentType ParseAppointmentType(string? value, AppointmentType fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
            return fallback;

        if (Enum.TryParse<AppointmentType>(value, true, out var appointmentType) &&
            Enum.IsDefined(appointmentType))
        {
            return appointmentType;
        }

        throw new InvalidOperationException("Appointment type is invalid.");
    }

    private static AppointmentStatus ParseAppointmentStatus(string? value, AppointmentStatus fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
            return fallback;

        if (Enum.TryParse<AppointmentStatus>(value, true, out var appointmentStatus) &&
            Enum.IsDefined(appointmentStatus))
        {
            return appointmentStatus;
        }

        throw new InvalidOperationException("Appointment status is invalid.");
    }
}
