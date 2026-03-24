import { ApplicationConfig } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { routes } from './app.routes';
import { APP_RUNTIME_CONFIG } from './core/config/app.config.token';

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideHttpClient(),
    {
      provide: APP_RUNTIME_CONFIG,
      useValue: {
        apiBaseUrl: 'https://localhost:7098/api',
      },
    },
  ],
};