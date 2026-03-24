import { InjectionToken } from '@angular/core';

export interface AppRuntimeConfig {
  apiBaseUrl: string;
}

export const APP_RUNTIME_CONFIG = new InjectionToken<AppRuntimeConfig>(
  'APP_RUNTIME_CONFIG'
);