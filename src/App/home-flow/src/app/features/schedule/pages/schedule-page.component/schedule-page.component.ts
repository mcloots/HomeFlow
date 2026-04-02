import { Component, computed, effect, inject, signal } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AppContextStore } from '../../../../core/context/app-context.store';
import { ScheduleStore } from '../../data-access/schedule.store';
import { RouterLink, ActivatedRoute, Router } from '@angular/router';
import { GmailStore } from '../../../gmail/data-access/gmail.store';
import { CreateAppointmentModalComponent } from '../../components/create-appointment-modal.component/create-appointment-modal.component';
import { AppointmentSuggestion } from '../../../gmail/models/gmail.models';
import { AppointmentSummary } from '../../models/schedule.models';
import { BillEditorModalComponent } from '../../../billing/components/bill-editor-modal.component/bill-editor-modal.component';
import { BillingStore } from '../../../billing/data-access/billing.store';
import { BillSummary } from '../../../billing/models/bill.models';
import { ChoresStore } from '../../../chores/data-access/chores.store';
import { ChoreSummary } from '../../../chores/models/chore.models';
import { ChoreEditorModalComponent } from '../../../chores/components/chore-editor-modal.component/chore-editor-modal.component';

@Component({
  selector: 'app-schedule-page.component',
  imports: [CommonModule, FormsModule, DatePipe, RouterLink, CreateAppointmentModalComponent, BillEditorModalComponent, ChoreEditorModalComponent],
  templateUrl: './schedule-page.component.html',
  styleUrl: './schedule-page.component.css',
})
export class SchedulePageComponent {
  private static readonly calendarDayCount = 42;

  readonly context = inject(AppContextStore);
  readonly store = inject(ScheduleStore);
  readonly billingStore = inject(BillingStore);
  readonly choresStore = inject(ChoresStore);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly tenantIdInput = signal('');
  readonly householdIdInput = signal('');

  readonly canLoad = computed(() => this.context.hasHouseholdId());
  readonly todayKey = this.toLocalDateKey(new Date());
  readonly weekdayLabels = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'];
  readonly currentMonth = signal(this.startOfMonth(new Date()));
  readonly currentMonthLabel = computed(() =>
    this.currentMonth().toLocaleDateString(undefined, {
      month: 'long',
      year: 'numeric',
    })
  );
  readonly currentMonthRangeLabel = computed(() => {
    const monthStart = this.currentMonth();
    const monthEnd = this.endOfMonth(monthStart);

    return `${monthStart.toLocaleDateString(undefined, {
      month: 'short',
      day: 'numeric',
    })} - ${monthEnd.toLocaleDateString(undefined, {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
    })}`;
  });
  readonly calendarItemsByDay = computed(() => {
    const itemsByDay = new Map<string, CalendarItem[]>();

    for (const appointment of this.store.appointmentsSorted()) {
      const startDate = this.startOfDay(new Date(appointment.startsAtUtc));
      const endDate = this.inclusiveEndDay(new Date(appointment.startsAtUtc), new Date(appointment.endsAtUtc));

      for (
        let currentDate = startDate;
        currentDate.getTime() <= endDate.getTime();
        currentDate = this.addDays(currentDate, 1)
      ) {
        const dayKey = this.toLocalDateKey(currentDate);
        const current = itemsByDay.get(dayKey) ?? [];
        current.push({
          key: `appointment-${appointment.appointmentId}`,
          kind: 'appointment',
          sortUtc: appointment.startsAtUtc,
          appointment,
        });
        itemsByDay.set(dayKey, current);
      }
    }

    for (const bill of this.billingStore.billsSorted()) {
      const dayKey = this.toLocalDateKey(new Date(bill.dueDateUtc));
      const current = itemsByDay.get(dayKey) ?? [];
      current.push({
        key: `bill-${bill.billId}`,
        kind: 'bill',
        sortUtc: bill.dueDateUtc,
        bill,
      });
      itemsByDay.set(dayKey, current);
    }

    for (const chore of this.choresStore.pendingChores()) {
      const dayKey = this.toLocalDateKey(new Date(chore.dueDateUtc));
      const current = itemsByDay.get(dayKey) ?? [];
      current.push({
        key: `chore-${chore.choreId}`,
        kind: 'chore',
        sortUtc: chore.dueDateUtc,
        chore,
      });
      itemsByDay.set(dayKey, current);
    }

    for (const [dayKey, items] of itemsByDay.entries()) {
      itemsByDay.set(
        dayKey,
        [...items].sort(
          (a, b) => new Date(a.sortUtc).getTime() - new Date(b.sortUtc).getTime()
        )
      );
    }

    return itemsByDay;
  });
  readonly calendarDays = computed(() => {
    const monthStart = this.currentMonth();
    const gridStart = this.startOfCalendarGrid(monthStart);
    const days = [];

    for (let index = 0; index < SchedulePageComponent.calendarDayCount; index += 1) {
      const date = this.addDays(gridStart, index);
      const dayKey = this.toLocalDateKey(date);

      days.push({
        key: dayKey,
        date,
        dateLabel: date.getDate(),
        isCurrentMonth: date.getMonth() === monthStart.getMonth(),
        isToday: dayKey === this.todayKey,
        items: this.calendarItemsByDay().get(dayKey) ?? [],
      });
    }

    return days;
  });

  readonly gmailStore = inject(GmailStore);

  readonly gmailFromLocalInput = signal(this.toLocalDateInputValue(this.store.fromUtc()));
  readonly gmailToLocalInput = signal(this.toLocalDateInputValue(this.store.toUtc()));

  readonly isCreateModalOpen = signal(false);
  readonly isBillModalOpen = signal(false);
  readonly isChoreModalOpen = signal(false);
  readonly selectedSuggestion = signal<AppointmentSuggestion | null>(null);
  readonly selectedBill = signal<BillSummary | null>(null);
  readonly selectedChore = signal<ChoreSummary | null>(null);

  readonly pageMessage = signal<string | null>(null);
  readonly pageMessageType = signal<'success' | 'error' | null>(null);

  applyContext(): void {
    this.context.setTenantId(this.tenantIdInput());
    this.context.setHouseholdId(this.householdIdInput());
  }

  constructor() {
    effect(() => {
      const householdId = this.context.householdId();
      const monthStart = this.currentMonth();

      if (householdId) {
        void this.loadCalendarMonth(householdId, monthStart);
        void this.billingStore.load(householdId);
        void this.choresStore.load(householdId);
      }
    });

    effect(() => {
      if (this.context.hasHouseholdId()) {
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

  previousMonth(): void {
    this.currentMonth.set(this.addMonths(this.currentMonth(), -1));
  }

  nextMonth(): void {
    this.currentMonth.set(this.addMonths(this.currentMonth(), 1));
  }

  goToCurrentMonth(): void {
    this.currentMonth.set(this.startOfMonth(new Date()));
  }

  private toLocalDateInputValue(utcIso: string): string {
    const date = new Date(utcIso);
    const year = date.getFullYear();
    const month = `${date.getMonth() + 1}`.padStart(2, '0');
    const day = `${date.getDate()}`.padStart(2, '0');

    return `${year}-${month}-${day}`;
  }

  private localDateInputToUtcIso(localValue: string, endOfDay = false): string {
    const date = new Date(`${localValue}T00:00:00`);

    if (endOfDay) {
      date.setHours(23, 59, 59, 999);
    }

    return date.toISOString();
  }

  useSuggestion(suggestion: AppointmentSuggestion): void {
    this.selectedBill.set(null);
    this.selectedSuggestion.set(suggestion);
    this.isCreateModalOpen.set(true);
  }

  useSuggestionForBill(suggestion: AppointmentSuggestion): void {
    this.selectedBill.set(null);
    this.selectedSuggestion.set(suggestion);
    this.isBillModalOpen.set(true);
  }

  getAppointmentCardClasses(appointment: AppointmentSummary): Record<string, boolean> {
    const isDone = appointment.status === 'Done';
    const isCancelled = appointment.status === 'Cancelled';

    return {
      'border-slate-200 bg-slate-100/90 opacity-75': isCancelled,
      'border-emerald-200 bg-emerald-50/90': isDone,
      'border-sky-200 bg-sky-50/90': !isDone && !isCancelled,
    };
  }

  getAppointmentMetaText(appointment: AppointmentSummary): string {
    return new Date(appointment.startsAtUtc).toLocaleTimeString(undefined, {
      hour: 'numeric',
      minute: '2-digit',
    });
  }

  getAppointmentTypeLabel(appointment: AppointmentSummary): string {
    return appointment.type;
  }

  connectGmail(): void {
    void this.gmailStore.startConnect();
  }

  scanGmailSuggestions(): void {
    const fromUtc = this.localDateInputToUtcIso(this.gmailFromLocalInput());
    const toUtc = this.localDateInputToUtcIso(this.gmailToLocalInput(), true);

    void this.gmailStore.scanSuggestions(fromUtc, toUtc);
  }

  disconnectGmail(): void {
    void this.gmailStore.disconnect();
  }

  closeCreateModal(): void {
    this.isCreateModalOpen.set(false);
  }

  handleAppointmentSaved(): void {
    this.closeCreateModal();
    this.selectedSuggestion.set(null);
    const householdId = this.context.householdId();

    if (householdId) {
      void this.loadCalendarMonth(householdId, this.currentMonth());
    }
  }

  closeBillModal(): void {
    this.isBillModalOpen.set(false);
    this.selectedSuggestion.set(null);
    this.selectedBill.set(null);
  }

  closeChoreModal(): void {
    this.isChoreModalOpen.set(false);
    this.selectedChore.set(null);
  }

  async handleChoreSaved(): Promise<void> {
    this.closeChoreModal();
    const householdId = this.context.householdId();

    if (householdId) {
      await this.choresStore.load(householdId);
    }
  }

  async handleBillSaved(): Promise<void> {
    this.closeBillModal();
    const householdId = this.context.householdId();

    if (householdId) {
      await this.billingStore.load(householdId);
    }
  }

  editBillFromCalendar(bill: BillSummary): void {
    this.selectedSuggestion.set(null);
    this.selectedBill.set(bill);
    this.isCreateModalOpen.set(false);
    this.isBillModalOpen.set(true);
  }

  editChoreFromCalendar(chore: ChoreSummary): void {
    this.selectedChore.set(chore);
    this.isBillModalOpen.set(false);
    this.isCreateModalOpen.set(false);
    this.isChoreModalOpen.set(true);
  }

  getBillCardClasses(bill: BillSummary): Record<string, boolean> {
    return {
      'border-rose-200 bg-rose-50/90': bill.status === 'Overdue',
      'border-emerald-200 bg-emerald-50/90': bill.status === 'Paid',
      'border-amber-200 bg-amber-50/90': bill.status === 'Pending',
    };
  }

  getBillMetaText(bill: BillSummary, dayKey: string): string {
    if (dayKey === this.toLocalDateKey(new Date(bill.dueDateUtc))) {
      return bill.status === 'Paid' ? 'Paid' : 'Due today';
    }

    return `Due ${new Date(bill.dueDateUtc).toLocaleDateString(undefined, {
      month: 'short',
      day: 'numeric',
    })}`;
  }

  getChoreCardClasses(chore: ChoreSummary): Record<string, boolean> {
    return {
      'border-rose-200 bg-rose-50/90': chore.isOverdue,
      'border-amber-200 bg-amber-50/90': !chore.isOverdue,
    };
  }

  getChoreMetaText(chore: ChoreSummary, dayKey: string): string {
    if (dayKey === this.toLocalDateKey(new Date(chore.dueDateUtc))) {
      return chore.isOverdue ? 'Overdue' : 'Due today';
    }

    return `Due ${new Date(chore.dueDateUtc).toLocaleDateString(undefined, {
      month: 'short',
      day: 'numeric',
    })}`;
  }

  readonly totalCalendarItems = computed(
    () => this.store.appointments().length + this.billingStore.bills().length + this.choresStore.pendingChores().length
  );

  dismissPageMessage(): void {
    this.pageMessage.set(null);
    this.pageMessageType.set(null);
  }

  private async loadCalendarMonth(householdId: string, monthStart: Date): Promise<void> {
    const monthEnd = this.endOfMonth(monthStart);

    this.store.setDateRange(monthStart.toISOString(), monthEnd.toISOString());
    await this.store.load(householdId);
  }

  private startOfMonth(date: Date): Date {
    return new Date(date.getFullYear(), date.getMonth(), 1);
  }

  private endOfMonth(date: Date): Date {
    return new Date(date.getFullYear(), date.getMonth() + 1, 0, 23, 59, 59, 999);
  }

  private addMonths(date: Date, months: number): Date {
    return new Date(date.getFullYear(), date.getMonth() + months, 1);
  }

  private addDays(date: Date, days: number): Date {
    const nextDate = new Date(date);
    nextDate.setDate(nextDate.getDate() + days);
    return nextDate;
  }

  private startOfDay(date: Date): Date {
    return new Date(date.getFullYear(), date.getMonth(), date.getDate());
  }

  private inclusiveEndDay(startDate: Date, endDate: Date): Date {
    const normalizedEnd = this.startOfDay(endDate);
    const endsAtStartOfDay =
      endDate.getHours() === 0 &&
      endDate.getMinutes() === 0 &&
      endDate.getSeconds() === 0 &&
      endDate.getMilliseconds() === 0;

    if (endsAtStartOfDay && normalizedEnd.getTime() > this.startOfDay(startDate).getTime()) {
      return this.addDays(normalizedEnd, -1);
    }

    return normalizedEnd;
  }

  private startOfCalendarGrid(monthStart: Date): Date {
    const gridStart = new Date(monthStart);
    const dayOfWeek = (gridStart.getDay() + 6) % 7;
    gridStart.setDate(gridStart.getDate() - dayOfWeek);
    return gridStart;
  }

  private toLocalDateKey(date: Date): string {
    const year = date.getFullYear();
    const month = `${date.getMonth() + 1}`.padStart(2, '0');
    const day = `${date.getDate()}`.padStart(2, '0');

    return `${year}-${month}-${day}`;
  }
}

type CalendarItem =
  | {
      key: string;
      kind: 'appointment';
      sortUtc: string;
      appointment: AppointmentSummary;
    }
  | {
      key: string;
      kind: 'bill';
      sortUtc: string;
      bill: BillSummary;
    }
  | {
      key: string;
      kind: 'chore';
      sortUtc: string;
      chore: ChoreSummary;
    };
