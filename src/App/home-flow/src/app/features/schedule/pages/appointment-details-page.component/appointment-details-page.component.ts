import { Component, effect, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { AppointmentDetailsStore } from '../../data-access/appointment-details.store';
import { CommonModule, DatePipe } from '@angular/common';
import { AppointmentApiService } from '../../data-access/appointment-api.service';
import { UpdateAppointmentRequest } from '../../models/update-appointment.models';

@Component({
  selector: 'app-appointment-details-page.component',
  imports: [CommonModule, DatePipe, RouterLink],
  templateUrl: './appointment-details-page.component.html',
  styleUrl: './appointment-details-page.component.css',
})
export class AppointmentDetailsPageComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly appointmentApi = inject(AppointmentApiService);
  readonly store = inject(AppointmentDetailsStore);

  readonly isUpdating = signal(false);
  readonly actionError = signal<string | null>(null);

  constructor() {
    effect(() => {
      const appointmentId = this.route.snapshot.paramMap.get('appointmentId') ?? '';
      if (appointmentId) {
        void this.store.load(appointmentId);
      }
    });
  }

  markAsDone(): void {
    void this.updateStatus('Done');
  }

  markAsScheduled(): void {
    void this.updateStatus('Scheduled');
  }

  getStatusClasses(status: string): Record<string, boolean> {
    return {
      'bg-blue-100 text-blue-700': status === 'Scheduled',
      'bg-emerald-100 text-emerald-700': status === 'Done',
      'bg-slate-200 text-slate-700': status === 'Cancelled',
    };
  }

  getTypeClasses(type: string): Record<string, boolean> {
    return {
      'bg-sky-100 text-sky-700': type === 'General',
      'bg-amber-100 text-amber-700': type === 'Payment',
    };
  }

  private async updateStatus(status: string): Promise<void> {
    const appointment = this.store.appointment();

    if (!appointment) {
      return;
    }

    const request: UpdateAppointmentRequest = {
      title: appointment.title,
      description: appointment.description ?? null,
      startsAtUtc: appointment.startsAtUtc,
      endsAtUtc: appointment.endsAtUtc,
      location: appointment.location ?? null,
      type: appointment.type,
      status,
      participantMemberIds: appointment.participants.map((participant) => participant.householdMemberId),
    };

    this.isUpdating.set(true);
    this.actionError.set(null);

    this.appointmentApi.updateAppointment(appointment.appointmentId, request).subscribe({
      next: async () => {
        this.isUpdating.set(false);
        await this.store.load(appointment.appointmentId);
      },
      error: (error: unknown) => {
        console.error(error);
        this.actionError.set('Failed to update appointment status.');
        this.isUpdating.set(false);
      },
    });
  }
}
