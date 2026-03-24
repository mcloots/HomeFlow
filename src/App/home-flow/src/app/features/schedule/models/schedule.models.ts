export interface AppointmentParticipant {
  householdMemberId: string;
  displayName: string;
}

export interface AppointmentSummary {
  appointmentId: string;
  title: string;
  startsAtUtc: string;
  endsAtUtc: string;
  location?: string | null;
  status: string;
  participants: AppointmentParticipant[];
}

export interface GetAppointmentsForDateRangeResponse {
  householdId: string;
  fromUtc: string;
  toUtc: string;
  appointments: AppointmentSummary[];
}