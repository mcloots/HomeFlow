import { CommonModule } from '@angular/common';
import {
  Component,
  EventEmitter,
  Input,
  Output,
  computed,
  inject,
  signal,
} from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AppContextStore } from '../../../../core/context/app-context.store';
import { BillApiService } from '../../data-access/bill-api.service';
import { BillSummary } from '../../models/bill.models';
import { CreateBillRequest } from '../../models/create-bill.models';
import { UpdateBillRequest } from '../../models/update-bill.models';
import { AppointmentSuggestion } from '../../../gmail/models/gmail.models';

@Component({
  selector: 'app-bill-editor-modal',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './bill-editor-modal.component.html',
  styleUrl: './bill-editor-modal.component.css',
})
export class BillEditorModalComponent {
  private readonly fb = inject(FormBuilder);
  private readonly context = inject(AppContextStore);
  private readonly api = inject(BillApiService);

  private readonly isOpenState = signal(false);
  private readonly billState = signal<BillSummary | null>(null);
  private readonly suggestionState = signal<AppointmentSuggestion | null>(null);

  @Input()
  set isOpen(value: boolean) {
    this.isOpenState.set(value);

    if (value) {
      this.syncFormWithBill();
    }
  }

  @Input()
  set bill(value: BillSummary | null) {
    this.billState.set(value);
    this.syncFormWithBill();
  }

  @Input()
  set suggestion(value: AppointmentSuggestion | null) {
    this.suggestionState.set(value);
    this.syncFormWithBill();
  }

  @Output() closed = new EventEmitter<void>();
  @Output() saved = new EventEmitter<void>();

  readonly isSaving = signal(false);
  readonly error = signal<string | null>(null);
  readonly isOpenValue = this.isOpenState.asReadonly();

  readonly mode = computed(() => (this.billState() ? 'edit' : 'create'));
  readonly modalTitle = computed(() =>
    this.mode() === 'edit' ? 'Edit bill' : 'Add bill'
  );
  readonly submitLabel = computed(() =>
    this.mode() === 'edit' ? 'Save changes' : 'Create bill'
  );
  readonly sourcePreview = computed(() => {
    const suggestion = this.suggestionState();

    if (!suggestion) {
      return null;
    }

    return {
      sender: suggestion.sender,
      subject: suggestion.subject,
      snippet: suggestion.sourceSnippet,
      body: suggestion.sourceBody,
      attachmentNames: suggestion.sourceAttachmentNames,
      attachmentText: suggestion.sourceAttachmentText,
    };
  });

  readonly form = this.fb.nonNullable.group({
    title: ['', [Validators.required]],
    amount: [0, [Validators.required, Validators.min(0.01)]],
    dueDateLocal: ['', [Validators.required]],
    category: [''],
    status: ['Pending', [Validators.required]],
    paidDateLocal: [''],
  });

  close(): void {
    this.closed.emit();
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const tenantId = this.context.tenantId();
    const householdId = this.context.householdId();

    if (!tenantId || !householdId) {
      this.error.set('TenantId and HouseholdId are required.');
      return;
    }

    const raw = this.form.getRawValue();
    const normalizedTitle = raw.title.trim();
    const normalizedCategory = raw.category.trim() || null;
    const normalizedStatus = this.normalizeStatus(raw.status);
    const paidAtUtc =
      normalizedStatus === 'Paid'
        ? this.dateOnlyToUtcIso(raw.paidDateLocal || raw.dueDateLocal)
        : null;

    this.isSaving.set(true);
    this.error.set(null);

    if (this.mode() === 'create') {
      const request: CreateBillRequest = {
        tenantId,
        householdId,
        title: normalizedTitle,
        amount: Number(raw.amount),
        dueDateUtc: this.dateOnlyToUtcIso(raw.dueDateLocal),
        category: normalizedCategory,
      };

      this.api.createBill(request).subscribe({
        next: () => {
          this.isSaving.set(false);
          this.saved.emit();
        },
        error: (err: unknown) => {
          console.error(err);
          this.error.set('Failed to create bill.');
          this.isSaving.set(false);
        },
      });

      return;
    }

    const bill = this.billState();

    if (!bill) {
      this.error.set('Bill context is missing.');
      this.isSaving.set(false);
      return;
    }

    const request: UpdateBillRequest = {
      title: normalizedTitle,
      amount: Number(raw.amount),
      dueDateUtc: this.dateOnlyToUtcIso(raw.dueDateLocal),
      category: normalizedCategory,
      status: normalizedStatus,
      paidAtUtc,
    };

    this.api.updateBill(bill.billId, request).subscribe({
      next: () => {
        this.isSaving.set(false);
        this.saved.emit();
      },
      error: (err: unknown) => {
        console.error(err);
        this.error.set('Failed to update bill.');
        this.isSaving.set(false);
      },
    });
  }

  private syncFormWithBill(): void {
    if (!this.isOpenValue()) {
      return;
    }

    const bill = this.billState();
    const suggestion = this.suggestionState();

    if (!bill) {
      this.form.reset({
        title: suggestion?.suggestedTitle ?? '',
        amount: suggestion?.suggestedAmount ?? 0,
        dueDateLocal: this.toDateInputValue(suggestion?.suggestedEndsAtUtc ?? ''),
        category: '',
        status: 'Pending',
        paidDateLocal: '',
      });
      this.error.set(null);
      return;
    }

    this.form.reset({
      title: bill.title,
      amount: bill.amount,
      dueDateLocal: this.toDateInputValue(bill.dueDateUtc),
      category: bill.category ?? '',
      status: this.normalizeStatus(bill.status),
      paidDateLocal: this.toDateInputValue(bill.paidAtUtc ?? bill.dueDateUtc),
    });
    this.error.set(null);
  }

  private normalizeStatus(status: string): string {
    if (status === 'Paid') {
      return 'Paid';
    }

    return 'Pending';
  }

  private toDateInputValue(value: string | null): string {
    if (!value) {
      return '';
    }

    return value.slice(0, 10);
  }

  private dateOnlyToUtcIso(value: string): string {
    const [year, month, day] = value.split('-').map(Number);
    return new Date(Date.UTC(year, month - 1, day, 12, 0, 0)).toISOString();
  }
}
