import { Injectable, computed, inject, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { BillApiService } from './bill-api.service';
import { BillSummary } from '../models/bill.models';

@Injectable({ providedIn: 'root' })
export class BillingStore {
  private readonly api = inject(BillApiService);

  readonly bills = signal<BillSummary[]>([]);
  readonly isLoading = signal(false);
  readonly error = signal<string | null>(null);

  readonly billsSorted = computed(() =>
    [...this.bills()].sort(
      (a, b) => new Date(a.dueDateUtc).getTime() - new Date(b.dueDateUtc).getTime()
    )
  );

  readonly hasBills = computed(() => this.bills().length > 0);

  readonly pendingBills = computed(() =>
    this.billsSorted().filter((bill) => bill.status === 'Pending')
  );

  readonly overdueBills = computed(() =>
    this.billsSorted().filter((bill) => bill.status === 'Overdue')
  );

  readonly paidBills = computed(() =>
    this.billsSorted().filter((bill) => bill.status === 'Paid')
  );

  readonly totalOpenAmount = computed(() =>
    this.bills()
      .filter((bill) => bill.status !== 'Paid')
      .reduce((sum, bill) => sum + bill.amount, 0)
  );

  async load(householdId: string): Promise<void> {
    if (!householdId.trim()) {
      this.error.set('HouseholdId is required.');
      this.bills.set([]);
      return;
    }

    this.isLoading.set(true);
    this.error.set(null);

    try {
      const response = await firstValueFrom(this.api.getBillsForHousehold(householdId));
      this.bills.set(response.bills);
    } catch (error) {
      console.error(error);
      this.error.set('Failed to load bills.');
      this.bills.set([]);
    } finally {
      this.isLoading.set(false);
    }
  }
}
