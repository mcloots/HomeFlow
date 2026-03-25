import { Injectable, computed, signal, inject } from '@angular/core';
import { RuntimeConfigService } from '../config/runtime-config.service';

const TENANT_KEY = 'homeflow.tenantId';
const HOUSEHOLD_KEY = 'homeflow.householdId';

@Injectable({ providedIn: 'root' })
export class AppContextStore {
  private readonly config = inject(RuntimeConfigService);

  readonly tenantId = signal('');
  readonly householdId = signal('');

  readonly hasTenantId = computed(() =>
    this.tenantId().trim().length > 0
  );

  readonly hasHouseholdId = computed(() =>
    this.householdId().trim().length > 0
  );

  readonly isReady = computed(() =>
    this.hasTenantId() && this.hasHouseholdId()
  );

  constructor() {
    this.initialize();
  }

  private initialize(): void {
    const config = this.config.get();

    const storedTenant =
      localStorage.getItem(TENANT_KEY) ??
      config.defaultTenantId;

    const storedHousehold =
      localStorage.getItem(HOUSEHOLD_KEY) ??
      config.defaultHouseholdId;

    if (storedTenant) {
      this.tenantId.set(storedTenant);
    }

    if (storedHousehold) {
      this.householdId.set(storedHousehold);
    }
  }

  setTenantId(value: string): void {
    const trimmed = value.trim();

    this.tenantId.set(trimmed);

    localStorage.setItem(TENANT_KEY, trimmed);
  }

  setHouseholdId(value: string): void {
    const trimmed = value.trim();

    this.householdId.set(trimmed);

    localStorage.setItem(HOUSEHOLD_KEY, trimmed);
  }
}