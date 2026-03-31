import { Component, computed, effect, inject, signal } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AppContextStore } from '../../../../core/context/app-context.store';
import { ScheduleStore } from '../../data-access/schedule.store';
import { RouterLink, ActivatedRoute, Router } from '@angular/router';
import { GmailStore } from '../../../gmail/data-access/gmail.store';
import { CreateAppointmentModalComponent } from '../../components/create-appointment-modal.component/create-appointment-modal.component';
import { AppointmentSuggestion } from '../../../gmail/models/gmail.models';

@Component({
  selector: 'app-schedule-page.component',
  imports: [CommonModule, FormsModule, DatePipe, RouterLink, CreateAppointmentModalComponent],
  templateUrl: './schedule-page.component.html',
  styleUrl: './schedule-page.component.css',
})
export class SchedulePageComponent {
  readonly context = inject(AppContextStore);
  readonly store = inject(ScheduleStore);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly tenantIdInput = signal('');
  readonly householdIdInput = signal('');

  readonly fromLocalInput = signal(this.toLocalDateTimeInputValue(this.store.fromUtc()));
  readonly toLocalInput = signal(this.toLocalDateTimeInputValue(this.store.toUtc()));

  readonly canLoad = computed(() => this.context.hasHouseholdId());

  readonly gmailStore = inject(GmailStore);

  readonly gmailFromLocalInput = signal(this.toLocalDateTimeInputValue(this.store.fromUtc()));
  readonly gmailToLocalInput = signal(this.toLocalDateTimeInputValue(this.store.toUtc()));

  readonly isCreateModalOpen = signal(false);
  readonly selectedSuggestion = signal<AppointmentSuggestion | null>(null);

  readonly pageMessage = signal<string | null>(null);
  readonly pageMessageType = signal<'success' | 'error' | null>(null);

  applyContext(): void {
    this.context.setTenantId(this.tenantIdInput());
    this.context.setHouseholdId(this.householdIdInput());
  }

  constructor() {
    effect(() => {
      if (this.context.hasHouseholdId()) {
        //void this.store.load(this.context.householdId());
        void this.gmailStore.loadCurrentConnection();
      }
    });

    this.route.queryParamMap.subscribe((params) => {
      const gmailConnected = params.get('gmailConnected');
      const error = params.get('error');

      if (gmailConnected === 'true') {
        this.pageMessage.set('Gmail was connected successfully.');
        this.pageMessageType.set('success');

        void this.gmailStore.loadCurrentConnection();
        this.clearQueryParams();
      }

      if (gmailConnected === 'false' && error) {
        this.pageMessage.set(error);
        this.pageMessageType.set('error');

        this.clearQueryParams();
      }
    });
  }

  private clearQueryParams(): void {
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: {},
      replaceUrl: true,
    });
  }

  loadSchedule(): void {
    const fromUtc = this.localDateTimeInputToUtcIso(this.fromLocalInput());
    const toUtc = this.localDateTimeInputToUtcIso(this.toLocalInput());

    this.store.setDateRange(fromUtc, toUtc);
    void this.store.load(this.context.householdId());
  }

  private toLocalDateTimeInputValue(utcIso: string): string {
    const date = new Date(utcIso);
    const year = date.getFullYear();
    const month = `${date.getMonth() + 1}`.padStart(2, '0');
    const day = `${date.getDate()}`.padStart(2, '0');
    const hours = `${date.getHours()}`.padStart(2, '0');
    const minutes = `${date.getMinutes()}`.padStart(2, '0');

    return `${year}-${month}-${day}T${hours}:${minutes}`;
  }

  private localDateTimeInputToUtcIso(localValue: string): string {
    return new Date(localValue).toISOString();
  }

  useSuggestion(suggestion: AppointmentSuggestion): void {
    this.selectedSuggestion.set(suggestion);
    this.isCreateModalOpen.set(true);
  }

  connectGmail(): void {
    void this.gmailStore.startConnect();
  }

  scanGmailSuggestions(): void {
    const fromUtc = this.localDateTimeInputToUtcIso(this.gmailFromLocalInput());
    const toUtc = this.localDateTimeInputToUtcIso(this.gmailToLocalInput());

    void this.gmailStore.scanSuggestions(fromUtc, toUtc);
  }

  disconnectGmail(): void {
    void this.gmailStore.disconnect();
  }

  closeCreateModal(): void {
    this.isCreateModalOpen.set(false);
    this.selectedSuggestion.set(null);
  }

  handleAppointmentSaved(): void {
    this.closeCreateModal();
    void this.store.load(this.context.householdId());
  }

  dismissPageMessage(): void {
    this.pageMessage.set(null);
    this.pageMessageType.set(null);
  }
}
