import { Component, computed, inject, signal } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AppContextStore } from '../../../../core/context/app-context.store';
import { ScheduleStore } from '../../data-access/schedule.store';


@Component({
  selector: 'app-schedule-page.component',
  imports: [CommonModule, FormsModule, DatePipe],
  templateUrl: './schedule-page.component.html',
  styleUrl: './schedule-page.component.css',
})
export class SchedulePageComponent {
  readonly context = inject(AppContextStore);
  readonly store = inject(ScheduleStore);

  readonly tenantIdInput = signal('');
  readonly householdIdInput = signal('');
  readonly fromUtcInput = signal(this.store.fromUtc());
  readonly toUtcInput = signal(this.store.toUtc());

  readonly canLoad = computed(() => this.context.hasHouseholdId());

  applyContext(): void {
    this.context.setTenantId(this.tenantIdInput());
    this.context.setHouseholdId(this.householdIdInput());
  }

  loadSchedule(): void {
    this.store.setDateRange(this.fromUtcInput(), this.toUtcInput());
    void this.store.load(this.context.householdId());
  }
}
