import { Injectable } from '@angular/core';

export interface RuntimeConfig {
  apiBaseUrl: string;
  defaultTenantId: string;
  defaultHouseholdId: string;
}

@Injectable({ providedIn: 'root' })
export class RuntimeConfigService {
  private config!: RuntimeConfig;

  async load(): Promise<void> {
    const response = await fetch('/config/runtime-config.json');

    if (!response.ok) {
      throw new Error('Failed to load runtime configuration');
    }

    this.config = await response.json();
  }

  get(): RuntimeConfig {
    return this.config;
  }
}