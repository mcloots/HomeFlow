import { Injectable, computed, inject, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { ChoreApiService } from './chore-api.service';
import { ChoreSummary } from '../models/chore.models';

@Injectable({ providedIn: 'root' })
export class ChoresStore {
  private readonly api = inject(ChoreApiService);

  readonly chores = signal<ChoreSummary[]>([]);
  readonly isLoading = signal(false);
  readonly error = signal<string | null>(null);

  readonly choresSorted = computed(() =>
    [...this.chores()].sort((a, b) => {
      if (a.status !== b.status) {
        return a.status.localeCompare(b.status);
      }

      return new Date(a.dueDateUtc).getTime() - new Date(b.dueDateUtc).getTime();
    })
  );

  readonly pendingChores = computed(() =>
    this.choresSorted().filter((chore) => chore.status === 'Pending')
  );

  readonly completedChores = computed(() =>
    this.choresSorted()
      .filter((chore) => chore.status === 'Completed')
      .sort(
        (a, b) =>
          new Date(b.completedAtUtc ?? b.dueDateUtc).getTime() -
          new Date(a.completedAtUtc ?? a.dueDateUtc).getTime()
      )
  );

  readonly overdueChores = computed(() =>
    this.pendingChores().filter((chore) => chore.isOverdue)
  );

  readonly recurringChores = computed(() =>
    this.pendingChores().filter((chore) => chore.recurrence !== 'None')
  );

  readonly hasChores = computed(() => this.chores().length > 0);

  async load(householdId: string): Promise<void> {
    if (!householdId.trim()) {
      this.error.set('HouseholdId is required.');
      this.chores.set([]);
      return;
    }

    this.isLoading.set(true);
    this.error.set(null);

    try {
      const response = await firstValueFrom(this.api.getChoresForHousehold(householdId));
      this.chores.set(response.chores);
    } catch (error) {
      console.error(error);
      this.error.set('Failed to load chores.');
      this.chores.set([]);
    } finally {
      this.isLoading.set(false);
    }
  }
}
