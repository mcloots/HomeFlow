export interface AppointmentDetailsParticipant {
  householdMemberId: string;
  displayName: string;
}

export interface AppointmentDetails {
  appointmentId: string;
  tenantId: string;
  householdId: string;
  title: string;
  description?: string | null;
  startsAtUtc: string;
  endsAtUtc: string;
  location?: string | null;
  type: string;
  status: string;
  participants: AppointmentDetailsParticipant[];
}
