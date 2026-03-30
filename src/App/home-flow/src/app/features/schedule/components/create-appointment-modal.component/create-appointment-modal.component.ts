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
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { AppointmentApiService } from '../../data-access/appointment-api.service';
import { AppContextStore } from '../../../../core/context/app-context.store';
import { CreateAppointmentRequest } from '../../models/create-appointment.models';

export interface AppointmentSuggestionInput {
  suggestedTitle: string;
  suggestedStartsAtUtc: string | null;
  suggestedEndsAtUtc: string | null;
  suggestedLocation: string | null;
  suggestedDescription: string | null;
}

@Component({
  selector: 'app-create-appointment-modal',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './create-appointment-modal.component.html',
  styleUrl: './create-appointment-modal.component.css',
})
export class CreateAppointmentModalComponent {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(AppointmentApiService);
  private readonly context = inject(AppContextStore);

  @Input() isOpen = signal(false);
  @Input() suggestion = signal<AppointmentSuggestionInput | null>(null);

  @Output() closed = new EventEmitter<void>();
  @Output() saved = new EventEmitter<void>();

  readonly isSaving = signal(false);
  readonly error = signal<string | null>(null);

  readonly contextReady = computed(
    () => this.context.hasTenantId() && this.context.hasHouseholdId()
  );

  readonly form = this.fb.nonNullable.group({
    title: ['', [Validators.required]],
    description: [''],
    startsAtLocal: ['', [Validators.required]],
    endsAtLocal: ['', [Validators.required]],
    location: [''],
    participantMemberIds: [''],
  });

  ngOnChanges(): void {
    this.applySuggestion();
  }

  private applySuggestion(): void {
    const suggestion = this.suggestion();
    if (!suggestion) {
      return;
    }

    this.form.patchValue({
      title: suggestion.suggestedTitle ?? '',
      description: suggestion.suggestedDescription ?? '',
      startsAtLocal: this.toLocalDateTimeInputValue(suggestion.suggestedStartsAtUtc),
      endsAtLocal: this.toLocalDateTimeInputValue(suggestion.suggestedEndsAtUtc),
      location: suggestion.suggestedLocation ?? '',
      participantMemberIds: '',
    });

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

    const participantMemberIds = raw.participantMemberIds
      .split(',')
      .map((x) => x.trim())
      .filter((x) => x.length > 0);

    const request: CreateAppointmentRequest = {
      tenantId,
      householdId,
      title: raw.title.trim(),
      description: raw.description.trim() || null,
      startsAtUtc: new Date(raw.startsAtLocal).toISOString(),
      endsAtUtc: new Date(raw.endsAtLocal).toISOString(),
      location: raw.location.trim() || null,
      participantMemberIds,
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
