import { CommonModule } from '@angular/common';
import {
  Component,
  EventEmitter,
  Input,
  Output,
  computed,
  effect,
  inject,
  signal,
} from '@angular/core';
import {
  FormBuilder,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { AppContextStore } from '../../../../core/context/app-context.store';
import { AppointmentApiService } from '../../data-access/appointment-api.service';
import { HouseholdMembersApiService } from '../../data-access/household-members-api.service';
import { CreateAppointmentRequest } from '../../models/create-appointment.models';
import { HouseholdMemberListItem } from '../../models/household-member.models';

export interface AppointmentSuggestionInput {
  suggestedTitle: string;
  suggestedStartsAtUtc: string | null;
  suggestedEndsAtUtc: string | null;
  suggestedLocation: string | null;
  suggestedType: string;
  suggestedDescription: string | null;
}

@Component({
  selector: 'app-create-appointment-modal',
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './create-appointment-modal.component.html',
  styleUrl: './create-appointment-modal.component.css',
})
export class CreateAppointmentModalComponent {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(AppointmentApiService);
  private readonly membersApi = inject(HouseholdMembersApiService);
  private readonly context = inject(AppContextStore);

  private readonly isOpenState = signal(false);
  private readonly suggestionState = signal<AppointmentSuggestionInput | null>(null);

  @Input()
  set isOpen(value: boolean) {
    this.isOpenState.set(value);

    if (value) {
      this.applySuggestion();
    }
  }

  @Input()
  set suggestion(value: AppointmentSuggestionInput | null) {
    this.suggestionState.set(value);
    this.applySuggestion();
  }

  @Output() closed = new EventEmitter<void>();
  @Output() saved = new EventEmitter<void>();

  readonly isSaving = signal(false);
  readonly error = signal<string | null>(null);
  readonly isLoadingMembers = signal(false);
  readonly membersError = signal<string | null>(null);
  readonly availableMembers = signal<HouseholdMemberListItem[]>([]);
  readonly isOpenValue = this.isOpenState.asReadonly();
  readonly appointmentTypes = ['General', 'Payment'];
  readonly durationDays = signal(30);

  readonly contextReady = computed(
    () => this.context.hasTenantId() && this.context.hasHouseholdId()
  );
  readonly isPaymentTypeSelected = computed(
    () => this.form.controls.type.getRawValue() === 'Payment'
  );

  readonly selectedMembers = computed(() => {
    const selectedIds = new Set(this.form.controls.participantMemberIds.getRawValue());

    return this.availableMembers().filter((member) => selectedIds.has(member.memberId));
  });

  readonly form = this.fb.nonNullable.group({
    title: ['', [Validators.required]],
    description: [''],
    startsAtLocal: ['', [Validators.required]],
    endsAtLocal: ['', [Validators.required]],
    location: [''],
    type: ['General', [Validators.required]],
    participantMemberIds: this.fb.nonNullable.control<string[]>([]),
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

  private applySuggestion(): void {
    const suggestion = this.suggestionState();

    if (!this.isOpenValue()) {
      return;
    }

    if (!suggestion) {
      this.form.reset({
        title: '',
        description: '',
        startsAtLocal: '',
        endsAtLocal: '',
        location: '',
        type: 'General',
        participantMemberIds: [],
      });
      this.error.set(null);
      return;
    }

    this.form.patchValue({
      title: suggestion.suggestedTitle ?? '',
      description: suggestion.suggestedDescription ?? '',
      startsAtLocal: this.toLocalDateTimeInputValue(suggestion.suggestedStartsAtUtc),
      endsAtLocal: this.toLocalDateTimeInputValue(suggestion.suggestedEndsAtUtc),
      location: suggestion.suggestedLocation ?? '',
      type: suggestion.suggestedType ?? 'General',
      participantMemberIds: [],
    });

    this.error.set(null);
  }

  async refreshMembers(): Promise<void> {
    const householdId = this.context.householdId();

    if (!householdId) {
      return;
    }

    await this.loadMembers(householdId);
  }

  isParticipantSelected(memberId: string): boolean {
    return this.form.controls.participantMemberIds.getRawValue().includes(memberId);
  }

  toggleParticipantSelection(memberId: string): void {
    const current = this.form.controls.participantMemberIds.getRawValue();

    if (current.includes(memberId)) {
      this.form.controls.participantMemberIds.setValue(
        current.filter((id) => id !== memberId)
      );

      return;
    }

    this.form.controls.participantMemberIds.setValue([...current, memberId]);
  }

  clearParticipantSelection(): void {
    this.form.controls.participantMemberIds.setValue([]);
  }

  applyDurationDays(daysToAdd: number = this.durationDays()): void {
    const startsAtLocal = this.form.controls.startsAtLocal.getRawValue();

    if (!startsAtLocal) {
      this.error.set('Choose a start date first.');
      return;
    }

    const startDate = new Date(startsAtLocal);

    if (Number.isNaN(startDate.getTime())) {
      this.error.set('Start date is invalid.');
      return;
    }

    const safeDaysToAdd = Math.max(0, Math.floor(daysToAdd));
    const endDate = new Date(startDate);
    endDate.setDate(endDate.getDate() + safeDaysToAdd);

    this.form.controls.endsAtLocal.setValue(this.toLocalDateTimeInputValue(endDate.toISOString()));
    this.durationDays.set(safeDaysToAdd);
    this.error.set(null);
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

    const request: CreateAppointmentRequest = {
      tenantId,
      householdId,
      title: raw.title.trim(),
      description: raw.description.trim() || null,
      startsAtUtc: new Date(raw.startsAtLocal).toISOString(),
      endsAtUtc: new Date(raw.endsAtLocal).toISOString(),
      location: raw.location.trim() || null,
      type: raw.type,
      participantMemberIds: raw.participantMemberIds,
    };

    this.isSaving.set(true);
    this.error.set(null);

    this.api.createAppointment(request).subscribe({
      next: () => {
        this.isSaving.set(false);
        this.saved.emit();
      },
      error: (err: unknown) => {
        console.error(err);
        this.error.set('Failed to create appointment.');
        this.isSaving.set(false);
      },
    });
  }

  private async loadMembers(householdId: string): Promise<void> {
    this.isLoadingMembers.set(true);
    this.membersError.set(null);

    try {
      const response = await firstValueFrom(
        this.membersApi.getHouseholdMembers(householdId)
      );

      this.availableMembers.set(response.members);

      const selectedIds = new Set(this.form.controls.participantMemberIds.getRawValue());
      const validSelectedIds = response.members
        .filter((member) => selectedIds.has(member.memberId))
        .map((member) => member.memberId);

      if (validSelectedIds.length !== selectedIds.size) {
        this.form.controls.participantMemberIds.setValue(validSelectedIds);
      }
    } catch (error) {
      console.error(error);
      this.availableMembers.set([]);
      this.membersError.set('Failed to load household members.');
    } finally {
      this.isLoadingMembers.set(false);
    }
  }

  private toLocalDateTimeInputValue(utcIso: string | null): string {
    if (!utcIso) {
      return '';
    }

    const date = new Date(utcIso);

    const year = date.getFullYear();
    const month = `${date.getMonth() + 1}`.padStart(2, '0');
    const day = `${date.getDate()}`.padStart(2, '0');
    const hours = `${date.getHours()}`.padStart(2, '0');
    const minutes = `${date.getMinutes()}`.padStart(2, '0');

    return `${year}-${month}-${day}T${hours}:${minutes}`;
  }
}
