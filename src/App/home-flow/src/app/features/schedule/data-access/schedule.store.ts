import { Injectable, computed, inject, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { ScheduleApiService } from './schedule-api.service';
import { AppointmentSummary } from '../models/schedule.models';

@Injectable({ providedIn: 'root' })
export class ScheduleStore {
  private readonly api = inject(ScheduleApiService);

  readonly appointments = signal<AppointmentSummary[]>([]);
  readonly isLoading = signal(false);
  readonly error = signal<string | null>(null);

  readonly fromUtc = signal(this.startOfWeekUtc());
  readonly toUtc = signal(this.endOfWeekUtc());

  readonly hasAppointments = computed(() => this.appointments().length > 0);

  readonly appointmentsSorted = computed(() =>
    [...this.appointments()].sort(
      (a, b) =>
        new Date(a.startsAtUtc).getTime() - new Date(b.startsAtUtc).getTime()
    )
  );

  readonly groupedByDay = computed(() => {
    const groups = new Map<string, AppointmentSummary[]>();

    for (const appointment of this.appointmentsSorted()) {
      const dayKey = appointment.startsAtUtc.slice(0, 10);

      const current = groups.get(dayKey) ?? [];
      current.push(appointment);
      groups.set(dayKey, current);
    }

    return Array.from(groups.entries()).map(([day, items]) => ({
      day,
      items,
    }));
  });

  async load(householdId: string): Promise<void> {
    if (!householdId.trim()) {
      this.error.set('HouseholdId is required.');
      this.appointments.set([]);
      return;
    }

    this.isLoading.set(true);
    this.error.set(null);

    try {
      const response = await firstValueFrom(
        this.api.getAppointmentsForDateRange(
          householdId,
          this.fromUtc(),
          this.toUtc()
        )
      );

      this.appointments.set(response.appointments);
    } catch (error) {
      console.error(error);
      this.error.set('Failed to load appointments.');
      this.appointments.set([]);
    } finally {
      this.isLoading.set(false);
    }
  }

  setDateRange(fromUtc: string, toUtc: string): void {
    this.fromUtc.set(fromUtc);
    this.toUtc.set(toUtc);
  }

  private startOfWeekUtc(): string {
    const now = new Date();
    const utc = new Date(Date.UTC(now.getUTCFullYear(), now.getUTCMonth(), now.getUTCDate()));
    const day = utc.getUTCDay();
    const diff = day === 0 ? -6 : 1 - day;
    utc.setUTCDate(utc.getUTCDate() + diff);
    utc.setUTCHours(0, 0, 0, 0);
    return utc.toISOString();
  }

  private endOfWeekUtc(): string {
    const start = new Date(this.startOfWeekUtc());
    start.setUTCDate(start.getUTCDate() + 7);
    start.setUTCMilliseconds(-1);
    return start.toISOString();
  }
}