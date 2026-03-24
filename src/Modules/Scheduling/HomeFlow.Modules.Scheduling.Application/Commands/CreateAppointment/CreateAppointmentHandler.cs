using HomeFlow.BuildingBlocks.Application.Abstractions.Persistence;
using HomeFlow.BuildingBlocks.MultiTenancy.Models;
using HomeFlow.Modules.Households.Domain.Ids;
using HomeFlow.Modules.Scheduling.Application.Abstractions;
using HomeFlow.Modules.Scheduling.Domain.Aggregates;
using HomeFlow.Modules.Scheduling.Domain.Ids;
using HomeFlow.Modules.Scheduling.Domain.Repositories;

namespace HomeFlow.Modules.Scheduling.Application.Commands.CreateAppointment;

public sealed class CreateAppointmentHandler
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IHouseholdMemberLookup _householdMemberLookup;
    private readonly IUnitOfWork _unitOfWork;

    public CreateAppointmentHandler(
        IAppointmentRepository appointmentRepository,
        IHouseholdMemberLookup householdMemberLookup,
        IUnitOfWork unitOfWork)
    {
        _appointmentRepository = appointmentRepository;
        _householdMemberLookup = householdMemberLookup;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateAppointmentResponse> Handle(
        CreateAppointmentCommand command,
        CancellationToken cancellationToken = default)
    {
        var tenantId = new TenantId(command.TenantId);
        var householdId = new HouseholdId(command.HouseholdId);

        var participantIds = command.ParticipantMemberIds
            .Distinct()
            .Select(x => new HouseholdMemberId(x))
            .ToList();

        var allParticipantsBelongToHousehold = await _householdMemberLookup.AllBelongToHouseholdAsync(
            householdId,
            participantIds,
            cancellationToken);

        if (!allParticipantsBelongToHousehold)
            throw new InvalidOperationException("One or more participants do not belong to the household.");

        var appointmentId = AppointmentId.New();

        var appointment = Appointment.Create(
            appointmentId,
            tenantId,
            householdId,
            command.Title,
            command.Description,
            command.StartsAtUtc,
            command.EndsAtUtc,
            command.Location);

        foreach (var participantId in participantIds)
        {
            appointment.AddParticipant(AppointmentParticipantId.New(), participantId);
        }

        await _appointmentRepository.AddAsync(appointment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateAppointmentResponse(
            appointment.Id.Value,
            appointment.TenantId.Value,
            appointment.HouseholdId.Value,
            appointment.Title,
            appointment.StartsAtUtc,
            appointment.EndsAtUtc,
            appointment.Status.ToString());
    }
}