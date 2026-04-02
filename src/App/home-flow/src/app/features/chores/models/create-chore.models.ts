export interface CreateChoreRequest {
  tenantId: string;
  householdId: string;
  title: string;
  description?: string | null;
  dueDateUtc: string;
  assignedMemberId?: string | null;
  recurrence?: string | null;
  recurrenceMonths?: number | null;
}

export interface CreateChoreResponse {
  choreId: string;
  tenantId: string;
  householdId: string;
  title: string;
  dueDateUtc: string;
  assignedMemberId?: string | null;
  status: string;
  recurrence: string;
  recurrenceMonths?: number | null;
}
