export interface UpdateAppointmentRequest {
  title: string;
  description?: string | null;
  startsAtUtc: string;
  endsAtUtc: string;
  location?: string | null;
  type?: string | null;
  status?: string | null;
  participantMemberIds: string[];
}
