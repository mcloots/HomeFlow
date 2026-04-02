export interface ChoreSummary {
  choreId: string;
  title: string;
  description?: string | null;
  dueDateUtc: string;
  status: string;
  isOverdue: boolean;
  recurrence: string;
  recurrenceMonths?: number | null;
  assignedMemberId?: string | null;
  assignedMemberName?: string | null;
  completedAtUtc?: string | null;
  completedByMemberId?: string | null;
  completedByMemberName?: string | null;
}

export interface GetChoresForHouseholdResponse {
  householdId: string;
  chores: ChoreSummary[];
}
