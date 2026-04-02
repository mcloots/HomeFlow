import { CommonModule, DatePipe } from '@angular/common';
import { Component, computed, effect, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { AppContextStore } from '../../../../core/context/app-context.store';
import { HouseholdMembersApiService } from '../../../schedule/data-access/household-members-api.service';
import { HouseholdMemberListItem } from '../../../schedule/models/household-member.models';
import { ChoreApiService } from '../../data-access/chore-api.service';
import { ChoresStore } from '../../data-access/chores.store';
import { ChoreSummary } from '../../models/chore.models';
import { CompleteChoreRequest } from '../../models/complete-chore.models';
import { ChoreEditorModalComponent } from '../../components/chore-editor-modal.component/chore-editor-modal.component';

@Component({
  selector: 'app-chores-page',
  imports: [CommonModule, FormsModule, DatePipe, ChoreEditorModalComponent],
  templateUrl: './chores-page.component.html',
  styleUrl: './chores-page.component.css',
})
export class ChoresPageComponent {
  readonly context = inject(AppContextStore);
  readonly store = inject(ChoresStore);
  private readonly api = inject(ChoreApiService);
  private readonly householdMembersApi = inject(HouseholdMembersApiService);

  readonly tenantIdInput = signal('');
  readonly householdIdInput = signal('');
  readonly members = signal<HouseholdMemberListItem[]>([]);
  readonly membersLoading = signal(false);
  readonly membersError = signal<string | null>(null);
  readonly selectedCompletionMemberId = signal('');

  readonly isModalOpen = signal(false);
  readonly selectedChore = signal<ChoreSummary | null>(null);
  readonly actionError = signal<string | null>(null);
  readonly actionBusyChoreId = signal<string | null>(null);

  readonly summaryCards = computed(() => [
    {
      label: 'Open chores',
      value: this.store.pendingChores().length,
      accent: 'text-sky-700',
      surface: 'bg-sky-50 border-sky-200',
    },
    {
      label: 'Overdue',
      value: this.store.overdueChores().length,
      accent: 'text-rose-700',
      surface: 'bg-rose-50 border-rose-200',
    },
    {
      label: 'Recurring',
      value: this.store.recurringChores().length,
      accent: 'text-amber-700',
      surface: 'bg-amber-50 border-amber-200',
    },
    {
      label: 'Completed',
      value: this.store.completedChores().length,
      accent: 'text-emerald-700',
      surface: 'bg-emerald-50 border-emerald-200',
    },
  ]);

  readonly completionMemberName = computed(
    () =>
      this.members().find((member) => member.memberId === this.selectedCompletionMemberId())
        ?.displayName ?? null
  );

  constructor() {
    effect(() => {
      const householdId = this.context.householdId();

      if (householdId) {
        void this.store.load(householdId);
        void this.loadMembers(householdId);
      }
    });
  }

  applyContext(): void {
    this.context.setTenantId(this.tenantIdInput());
    this.context.setHouseholdId(this.householdIdInput());
  }

  openCreateModal(): void {
    this.selectedChore.set(null);
    this.isModalOpen.set(true);
  }

  openEditModal(chore: ChoreSummary): void {
    this.selectedChore.set(chore);
    this.isModalOpen.set(true);
  }

  closeModal(): void {
    this.isModalOpen.set(false);
    this.selectedChore.set(null);
  }

  async handleSaved(): Promise<void> {
    this.closeModal();
    const householdId = this.context.householdId();

    if (householdId) {
      await this.store.load(householdId);
    }
  }

  dismissActionError(): void {
    this.actionError.set(null);
  }

  getChoreCardClasses(chore: ChoreSummary): Record<string, boolean> {
    return {
      'border-rose-200 bg-rose-50/80': chore.status === 'Pending' && chore.isOverdue,
      'border-emerald-200 bg-emerald-50/80': chore.status === 'Completed',
      'border-sky-200 bg-sky-50/80': chore.status === 'Pending' && !chore.isOverdue,
    };
  }

  getStatusPillClasses(chore: ChoreSummary): Record<string, boolean> {
    return {
      'bg-rose-100 text-rose-700': chore.status === 'Pending' && chore.isOverdue,
      'bg-emerald-100 text-emerald-700': chore.status === 'Completed',
      'bg-sky-100 text-sky-700': chore.status === 'Pending' && !chore.isOverdue,
    };
  }

  getStatusLabel(chore: ChoreSummary): string {
    if (chore.status === 'Completed') {
      return 'Completed';
    }

    return chore.isOverdue ? 'Overdue' : 'Pending';
  }

  getRecurrenceLabel(chore: ChoreSummary): string {
    if (chore.recurrence === 'None') {
      return 'One-off';
    }

    if (!chore.recurrenceMonths) {
      return chore.recurrence;
    }

    return `${chore.recurrence} for ${chore.recurrenceMonths} month${chore.recurrenceMonths === 1 ? '' : 's'}`;
  }

  async markCompleted(chore: ChoreSummary): Promise<void> {
    const completedByMemberId = this.selectedCompletionMemberId() || chore.assignedMemberId;

    if (!completedByMemberId) {
      this.actionError.set('Choose who completed the chore before marking it done.');
      return;
    }

    this.actionBusyChoreId.set(chore.choreId);
    this.actionError.set(null);

    const request: CompleteChoreRequest = {
      completedByMemberId,
      completedAtUtc: new Date().toISOString(),
    };

    try {
      await firstValueFrom(this.api.completeChore(chore.choreId, request));

      const householdId = this.context.householdId();
      if (householdId) {
        await this.store.load(householdId);
      }
    } catch (error) {
      console.error(error);
      this.actionError.set('Failed to complete chore.');
    } finally {
      this.actionBusyChoreId.set(null);
    }
  }

  private async loadMembers(householdId: string): Promise<void> {
    this.membersLoading.set(true);
    this.membersError.set(null);

    try {
      const response = await firstValueFrom(this.householdMembersApi.getHouseholdMembers(householdId));
      this.members.set(response.members);

      if (!this.selectedCompletionMemberId() && response.members.length > 0) {
        this.selectedCompletionMemberId.set(response.members[0].memberId);
      }

      if (
        this.selectedCompletionMemberId() &&
        !response.members.some((member) => member.memberId === this.selectedCompletionMemberId())
      ) {
        this.selectedCompletionMemberId.set(response.members[0]?.memberId ?? '');
      }
    } catch (error) {
      console.error(error);
      this.members.set([]);
      this.membersError.set('Failed to load household members.');
      this.selectedCompletionMemberId.set('');
    } finally {
      this.membersLoading.set(false);
    }
  }
}
