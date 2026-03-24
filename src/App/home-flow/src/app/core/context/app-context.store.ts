import { Injectable, computed, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class AppContextStore {
  readonly tenantId = signal('');
  readonly householdId = signal('');

  readonly hasTenantId = computed(() => this.tenantId().trim().length > 0);
  readonly hasHouseholdId = computed(() => this.householdId().trim().length > 0);
  readonly isReady = computed(() => this.hasTenantId() && this.hasHouseholdId());

  setTenantId(value: string): void {
    this.tenantId.set(value.trim());
  }

  setHouseholdId(value: string): void {
    this.householdId.set(value.trim());
  }
}