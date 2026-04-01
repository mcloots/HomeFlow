export interface BillSummary {
  billId: string;
  title: string;
  amount: number;
  dueDateUtc: string;
  status: string;
  category?: string | null;
  paidAtUtc?: string | null;
}

export interface GetBillsForHouseholdResponse {
  householdId: string;
  bills: BillSummary[];
}

export interface BillDetails {
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
