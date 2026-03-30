export interface CreateAppointmentRequest {
  tenantId: string;
  householdId: string;
  title: string;
  description?: string | null;
  startsAtUtc: string;
  endsAtUtc: string;
  location?: string | null;
  participantMemberIds: string[];
}