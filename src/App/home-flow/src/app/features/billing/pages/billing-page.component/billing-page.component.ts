import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { Component, computed, effect, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AppContextStore } from '../../../../core/context/app-context.store';
import { BillingStore } from '../../data-access/billing.store';
import { BillApiService } from '../../data-access/bill-api.service';
import { BillSummary } from '../../models/bill.models';
import { UpdateBillRequest } from '../../models/update-bill.models';
import { BillEditorModalComponent } from '../../components/bill-editor-modal.component/bill-editor-modal.component';

@Component({
  selector: 'app-billing-page',
  imports: [CommonModule, FormsModule, CurrencyPipe, DatePipe, BillEditorModalComponent],
  templateUrl: './billing-page.component.html',
  styleUrl: './billing-page.component.css',
})
export class BillingPageComponent {
  readonly context = inject(AppContextStore);
  readonly store = inject(BillingStore);
  private readonly api = inject(BillApiService);

  readonly tenantIdInput = signal('');
  readonly householdIdInput = signal('');

  readonly isModalOpen = signal(false);
  readonly selectedBill = signal<BillSummary | null>(null);
  readonly actionError = signal<string | null>(null);
  readonly actionBusyBillId = signal<string | null>(null);

  readonly summaryCards = computed(() => [
    {
      label: 'Pending',
      value: this.store.pendingBills().length,
      accent: 'text-amber-700',
      surface: 'bg-amber-50 border-amber-200',
    },
    {
      label: 'Overdue',
      value: this.store.overdueBills().length,
      accent: 'text-rose-700',
      surface: 'bg-rose-50 border-rose-200',
    },
    {
      label: 'Paid',
      value: this.store.paidBills().length,
      accent: 'text-emerald-700',
      surface: 'bg-emerald-50 border-emerald-200',
    },
  ]);

  constructor() {
    effect(() => {
      const householdId = this.context.householdId();

      if (householdId) {
        void this.store.load(householdId);
      }
    });
  }

  applyContext(): void {
    this.context.setTenantId(this.tenantIdInput());
    this.context.setHouseholdId(this.householdIdInput());
  }

  openCreateModal(): void {
    this.selectedBill.set(null);
    this.isModalOpen.set(true);
  }

  openEditModal(bill: BillSummary): void {
    this.selectedBill.set(bill);
    this.isModalOpen.set(true);
  }

  closeModal(): void {
    this.isModalOpen.set(false);
    this.selectedBill.set(null);
  }

  async handleSaved(): Promise<void> {
    this.closeModal();
    const householdId = this.context.householdId();

    if (householdId) {
      await this.store.load(householdId);
    }
  }

  dismissActionError(): void {
    this.actionError.set(null);
  }

  getBillCardClasses(bill: BillSummary): Record<string, boolean> {
    return {
      'border-rose-200 bg-rose-50/80': bill.status === 'Overdue',
      'border-emerald-200 bg-emerald-50/80': bill.status === 'Paid',
      'border-amber-200 bg-amber-50/80': bill.status === 'Pending',
    };
  }

  getStatusPillClasses(bill: BillSummary): Record<string, boolean> {
    return {
      'bg-rose-100 text-rose-700': bill.status === 'Overdue',
      'bg-emerald-100 text-emerald-700': bill.status === 'Paid',
      'bg-amber-100 text-amber-700': bill.status === 'Pending',
    };
  }

  async markAsPaid(bill: BillSummary): Promise<void> {
    await this.updateBillStatus(bill, 'Paid');
  }

  async markAsPending(bill: BillSummary): Promise<void> {
    await this.updateBillStatus(bill, 'Pending');
  }

  private async updateBillStatus(bill: BillSummary, status: 'Pending' | 'Paid'): Promise<void> {
    this.actionBusyBillId.set(bill.billId);
    this.actionError.set(null);

    const request: UpdateBillRequest = {
      title: bill.title,
      amount: bill.amount,
      dueDateUtc: bill.dueDateUtc,
      category: bill.category ?? null,
      status,
      paidAtUtc: status === 'Paid' ? new Date().toISOString() : null,
    };

    this.api.updateBill(bill.billId, request).subscribe({
      next: async () => {
        this.actionBusyBillId.set(null);

        const householdId = this.context.householdId();
        if (householdId) {
          await this.store.load(householdId);
        }
      },
      error: (err: unknown) => {
        console.error(err);
        this.actionBusyBillId.set(null);
        this.actionError.set('Failed to update bill status.');
      },
    });
  }
}
