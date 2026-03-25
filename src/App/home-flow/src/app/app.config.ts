import { ApplicationConfig, inject, provideAppInitializer } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { routes } from './app.routes';
import { RuntimeConfigService } from './core/config/runtime-config.service';

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideHttpClient(),
    RuntimeConfigService,
    provideAppInitializer(() => {
      const config = inject(RuntimeConfigService);
      return config.load();
    }),
  ],
};