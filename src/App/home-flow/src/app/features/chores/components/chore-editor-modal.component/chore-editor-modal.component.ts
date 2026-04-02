import { CommonModule } from '@angular/common';
import {
  Component,
  EventEmitter,
  Input,
  Output,
  effect,
  inject,
  signal,
} from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { AppContextStore } from '../../../../core/context/app-context.store';
import { HouseholdMembersApiService } from '../../../schedule/data-access/household-members-api.service';
import { HouseholdMemberListItem } from '../../../schedule/models/household-member.models';
import { ChoreApiService } from '../../data-access/chore-api.service';
import { ChoreSummary } from '../../models/chore.models';
import { CreateChoreRequest } from '../../models/create-chore.models';
import { UpdateChoreRequest } from '../../models/update-chore.models';

@Component({
  selector: 'app-chore-editor-modal',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './chore-editor-modal.component.html',
  styleUrl: './chore-editor-modal.component.css',
})
export class ChoreEditorModalComponent {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(ChoreApiService);
  private readonly householdMembersApi = inject(HouseholdMembersApiService);
  private readonly context = inject(AppContextStore);

  private readonly isOpenState = signal(false);
  private readonly choreState = signal<ChoreSummary | null>(null);

  @Input()
  set isOpen(value: boolean) {
    this.isOpenState.set(value);

    if (value) {
      this.syncFormWithChore();
    }
  }

  @Input()
  set chore(value: ChoreSummary | null) {
    this.choreState.set(value);
    this.syncFormWithChore();
  }

  @Output() closed = new EventEmitter<void>();
  @Output() saved = new EventEmitter<void>();

  readonly availableMembers = signal<HouseholdMemberListItem[]>([]);
  readonly isLoadingMembers = signal(false);
  readonly membersError = signal<string | null>(null);
  readonly isSaving = signal(false);
  readonly error = signal<string | null>(null);
  readonly isOpenValue = this.isOpenState.asReadonly();
  readonly currentChore = this.choreState.asReadonly();

  readonly form = this.fb.nonNullable.group({
    title: ['', [Validators.required]],
    description: [''],
    dueDateLocal: ['', [Validators.required]],
    assignedMemberId: [''],
    recurrence: ['None', [Validators.required]],
    recurrenceMonths: [6, [Validators.min(1)]],
  });

  constructor() {
    effect(() => {
      const isOpen = this.isOpenValue();
      const householdId = this.context.householdId();

      if (!isOpen || !householdId) {
        return;
      }

      void this.loadMembers(householdId);
    });
  }

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
    const normalizedAssignedMemberId = raw.assignedMemberId || null;
    const normalizedRecurrenceMonths =
      raw.recurrence === 'None' ? null : Number(raw.recurrenceMonths);

    this.isSaving.set(true);
    this.error.set(null);

    if (!this.choreState()) {
      const request: CreateChoreRequest = {
        tenantId,
        householdId,
        title: raw.title.trim(),
        description: raw.description.trim() || null,
        dueDateUtc: this.dateOnlyToUtcIso(raw.dueDateLocal),
        assignedMemberId: normalizedAssignedMemberId,
        recurrence: raw.recurrence,
        recurrenceMonths: normalizedRecurrenceMonths,
      };

      this.api.createChore(request).subscribe({
        next: () => {
          this.isSaving.set(false);
          this.saved.emit();
        },
        error: (err: unknown) => {
          console.error(err);
          this.error.set('Failed to create chore.');
          this.isSaving.set(false);
        },
      });

      return;
    }

    const chore = this.choreState();

    if (!chore) {
      this.error.set('Chore context is missing.');
      this.isSaving.set(false);
      return;
    }

    const request: UpdateChoreRequest = {
      title: raw.title.trim(),
      description: raw.description.trim() || null,
      dueDateUtc: this.dateOnlyToUtcIso(raw.dueDateLocal),
      assignedMemberId: normalizedAssignedMemberId,
      recurrence: raw.recurrence,
      recurrenceMonths: normalizedRecurrenceMonths,
    };

    this.api.updateChore(chore.choreId, request).subscribe({
      next: () => {
        this.isSaving.set(false);
        this.saved.emit();
      },
      error: (err: unknown) => {
        console.error(err);
        this.error.set('Failed to update chore.');
        this.isSaving.set(false);
      },
    });
  }

  private syncFormWithChore(): void {
    if (!this.isOpenValue()) {
      return;
    }

    const chore = this.choreState();

    if (!chore) {
      this.form.reset({
        title: '',
        description: '',
        dueDateLocal: '',
        assignedMemberId: '',
        recurrence: 'None',
        recurrenceMonths: 6,
      });
      this.error.set(null);
      return;
    }

    this.form.reset({
      title: chore.title,
      description: chore.description ?? '',
      dueDateLocal: this.toDateInputValue(chore.dueDateUtc),
      assignedMemberId: chore.assignedMemberId ?? '',
      recurrence: chore.recurrence,
      recurrenceMonths: chore.recurrenceMonths ?? 6,
    });
    this.error.set(null);
  }

  private async loadMembers(householdId: string): Promise<void> {
    this.isLoadingMembers.set(true);
    this.membersError.set(null);

    try {
      const response = await firstValueFrom(this.householdMembersApi.getHouseholdMembers(householdId));
      this.availableMembers.set(response.members);

      const assignedMemberId = this.form.controls.assignedMemberId.getRawValue();

      if (
        assignedMemberId &&
        !response.members.some((member) => member.memberId === assignedMemberId)
      ) {
        this.form.controls.assignedMemberId.setValue('');
      }
    } catch (error) {
      console.error(error);
      this.availableMembers.set([]);
      this.membersError.set('Failed to load household members.');
    } finally {
      this.isLoadingMembers.set(false);
    }
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
