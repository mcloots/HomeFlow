import { Injectable, computed, inject, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { AppContextStore } from '../../../core/context/app-context.store';
import { GmailApiService } from './gmail-api.service';
import {
  AppointmentSuggestion,
  GmailConnection,
} from '../models/gmail.models';

@Injectable({ providedIn: 'root' })
export class GmailStore {
  private readonly api = inject(GmailApiService);
  private readonly context = inject(AppContextStore);

  readonly connection = signal<GmailConnection | null>(null);
  readonly suggestions = signal<AppointmentSuggestion[]>([]);

  readonly isLoadingConnection = signal(false);
  readonly isConnecting = signal(false);
  readonly isScanning = signal(false);
  readonly isDisconnecting = signal(false);

  readonly connectionError = signal<string | null>(null);
  readonly suggestionError = signal<string | null>(null);

  readonly hasConnection = computed(() => this.connection() !== null);
  readonly hasSuggestions = computed(() => this.suggestions().length > 0);

  readonly suggestionsSorted = computed(() =>
    [...this.suggestions()].sort((a, b) => {
      const aTime = a.suggestedStartsAtUtc ? new Date(a.suggestedStartsAtUtc).getTime() : Number.MAX_SAFE_INTEGER;
      const bTime = b.suggestedStartsAtUtc ? new Date(b.suggestedStartsAtUtc).getTime() : Number.MAX_SAFE_INTEGER;
      return aTime - bTime;
    })
  );

  async loadCurrentConnection(): Promise<void> {
    const householdId = this.context.householdId();

    if (!householdId) {
      this.connection.set(null);
      return;
    }

    this.isLoadingConnection.set(true);
    this.connectionError.set(null);

    try {
      const result = await firstValueFrom(this.api.getCurrentConnection(householdId));
      this.connection.set(result);
    } catch (error: unknown) {
      this.connection.set(null);
      this.connectionError.set('No active Gmail connection found.');
      console.error(error);
    } finally {
      this.isLoadingConnection.set(false);
    }
  }

  async startConnect(): Promise<void> {
    const tenantId = this.context.tenantId();
    const householdId = this.context.householdId();

    if (!tenantId || !householdId) {
      this.connectionError.set('TenantId and HouseholdId are required.');
      return;
    }

    this.isConnecting.set(true);
    this.connectionError.set(null);

    try {
      const result = await firstValueFrom(this.api.startConnect(tenantId, householdId));
      window.location.href = result.authorizationUrl;
    } catch (error: unknown) {
      this.connectionError.set('Failed to start Gmail connection.');
      console.error(error);
    } finally {
      this.isConnecting.set(false);
    }
  }

  async scanSuggestions(fromUtc: string, toUtc: string): Promise<void> {
    const householdId = this.context.householdId();

    if (!householdId) {
      this.suggestionError.set('HouseholdId is required.');
      return;
    }

    this.isScanning.set(true);
    this.suggestionError.set(null);

    try {
      const result = await firstValueFrom(
        this.api.scanSuggestions(householdId, fromUtc, toUtc)
      );

      this.suggestions.set(result.suggestions);
    } catch (error: unknown) {
      this.suggestions.set([]);
      this.suggestionError.set('Failed to scan Gmail suggestions.');
      console.error(error);
    } finally {
      this.isScanning.set(false);
    }
  }

  async disconnect(): Promise<void> {
    const connection = this.connection();

    if (!connection) {
      return;
    }

    this.isDisconnecting.set(true);
    this.connectionError.set(null);

    try {
      await firstValueFrom(this.api.disconnect(connection.gmailConnectionId));
      this.connection.set(null);
      this.suggestions.set([]);
    } catch (error: unknown) {
      this.connectionError.set('Failed to disconnect Gmail.');
      console.error(error);
    } finally {
      this.isDisconnecting.set(false);
    }
  }
}