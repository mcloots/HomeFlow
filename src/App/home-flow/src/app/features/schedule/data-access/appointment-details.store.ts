import { Injectable, inject, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { ScheduleApiService } from './schedule-api.service';
import { AppointmentDetails } from '../models/appointment-details.models';

@Injectable({ providedIn: 'root' })
export class AppointmentDetailsStore {
  private readonly api = inject(ScheduleApiService);

  readonly appointment = signal<AppointmentDetails | null>(null);
  readonly isLoading = signal(false);
  readonly error = signal<string | null>(null);

  async load(appointmentId: string): Promise<void> {
    if (!appointmentId.trim()) {
      this.error.set('AppointmentId is required.');
      this.appointment.set(null);
      return;
    }

    this.isLoading.set(true);
    this.error.set(null);

    try {
      const result = await firstValueFrom(
        this.api.getAppointmentDetails(appointmentId)
      );

      this.appointment.set(result);
    } catch (error) {
      console.error(error);
      this.error.set('Failed to load appointment details.');
      this.appointment.set(null);
    } finally {
      this.isLoading.set(false);
    }
  }
}