import { Routes } from '@angular/router';
import { SchedulePageComponent } from './features/schedule/pages/schedule-page.component/schedule-page.component';
import { AppointmentDetailsPageComponent } from './features/schedule/pages/appointment-details-page.component/appointment-details-page.component';
import { BillingPageComponent } from './features/billing/pages/billing-page.component/billing-page.component';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'schedule' },
  { path: 'schedule', component: SchedulePageComponent },
  { path: 'billing', component: BillingPageComponent },
  { path: 'appointments/:appointmentId', component: AppointmentDetailsPageComponent },
];
