import { Routes } from '@angular/router';
import { SchedulePageComponent } from './features/schedule/pages/schedule-page.component/schedule-page.component';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'schedule' },
  { path: 'schedule', component: SchedulePageComponent },
];