import { Component, effect, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { AppointmentDetailsStore } from '../../data-access/appointment-details.store';
import { CommonModule, DatePipe } from '@angular/common';

@Component({
  selector: 'app-appointment-details-page.component',
  imports: [CommonModule, DatePipe, RouterLink],
  templateUrl: './appointment-details-page.component.html',
  styleUrl: './appointment-details-page.component.css',
})
export class AppointmentDetailsPageComponent {
  private readonly route = inject(ActivatedRoute);
  readonly store = inject(AppointmentDetailsStore);

  constructor() {
    effect(() => {
      const appointmentId = this.route.snapshot.paramMap.get('appointmentId') ?? '';
      if (appointmentId) {
        void this.store.load(appointmentId);
      }
    });
  }
}
