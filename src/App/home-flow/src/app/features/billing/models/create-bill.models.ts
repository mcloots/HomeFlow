export interface CreateBillRequest {
  tenantId: string;
  householdId: string;
  title: string;
  amount: number;
  dueDateUtc: string;
  category?: string | null;
}

export interface CreateBillResponse {
  billId: string;
  tenantId: string;
  householdId: string;
  title: string;
  amount: number;
  dueDateUtc: string;
  status: string;
  category?: string | null;
  paidAtUtc?: string | null;
}
