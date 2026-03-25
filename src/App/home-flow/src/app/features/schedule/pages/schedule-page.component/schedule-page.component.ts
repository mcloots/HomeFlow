import { Component, computed, inject, signal } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AppContextStore } from '../../../../core/context/app-context.store';
import { ScheduleStore } from '../../data-access/schedule.store';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-schedule-page.component',
  imports: [CommonModule, FormsModule, DatePipe, RouterLink],
  templateUrl: './schedule-page.component.html',
  styleUrl: './schedule-page.component.css',
})
export class SchedulePageComponent {
  readonly context = inject(AppContextStore);
  readonly store = inject(ScheduleStore);

  readonly tenantIdInput = signal('');
  readonly householdIdInput = signal('');

  readonly fromLocalInput = signal(this.toLocalDateTimeInputValue(this.store.fromUtc()));
  readonly toLocalInput = signal(this.toLocalDateTimeInputValue(this.store.toUtc()));

  readonly canLoad = computed(() => this.context.hasHouseholdId());

  applyContext(): void {
    this.context.setTenantId(this.tenantIdInput());
    this.context.setHouseholdId(this.householdIdInput());
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
}
