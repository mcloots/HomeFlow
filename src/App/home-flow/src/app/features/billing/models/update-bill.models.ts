export interface UpdateBillRequest {
  title: string;
  amount: number;
  dueDateUtc: string;
  category?: string | null;
  status?: string | null;
  paidAtUtc?: string | null;
}

export interface UpdateBillResponse {
  billId: string;
  title: string;
  amount: number;
  dueDateUtc: string;
  status: string;
  category?: string | null;
  paidAtUtc?: string | null;
}
