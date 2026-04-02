export interface CompleteChoreRequest {
  completedByMemberId: string;
  completedAtUtc?: string | null;
}

export interface CompleteChoreResponse {
  choreId: string;
  status: string;
  completedAtUtc: string;
  completedByMemberId: string;
  nextChoreId?: string | null;
}
