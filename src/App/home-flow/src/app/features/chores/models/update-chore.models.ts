export interface UpdateChoreRequest {
  title: string;
  description?: string | null;
  dueDateUtc: string;
  assignedMemberId?: string | null;
  recurrence?: string | null;
  recurrenceMonths?: number | null;
}

export interface UpdateChoreResponse {
  choreId: string;
  title: string;
  dueDateUtc: string;
  assignedMemberId?: string | null;
  status: string;
  recurrence: string;
  recurrenceMonths?: number | null;
}
