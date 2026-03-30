import { HttpClient, HttpParams } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { RuntimeConfigService } from '../../../core/config/runtime-config.service';
import {
  GmailConnection,
  ScanGmailSuggestionsResponse,
} from '../models/gmail.models';

@Injectable({ providedIn: 'root' })
export class GmailApiService {
  constructor(
    private readonly http: HttpClient,
    private readonly runtimeConfig: RuntimeConfigService,
  ) {}

  private get baseUrl(): string {
    return this.runtimeConfig.get().apiBaseUrl;
  }

  startConnect(tenantId: string, householdId: string): Observable<{ authorizationUrl: string }> {
    return this.http.post<{ authorizationUrl: string }>(
      `${this.baseUrl}/integrations/gmail/connect/start`,
      {
        tenantId,
        householdId,
      }
    );
  }

  getCurrentConnection(householdId: string): Observable<GmailConnection> {
    const params = new HttpParams().set('householdId', householdId);

    return this.http.get<GmailConnection>(
      `${this.baseUrl}/integrations/gmail/connections/current`,
      { params }
    );
  }

  scanSuggestions(
    householdId: string,
    fromUtc: string,
    toUtc: string
  ): Observable<ScanGmailSuggestionsResponse> {
    const params = new HttpParams()
      .set('householdId', householdId)
      .set('fromUtc', fromUtc)
      .set('toUtc', toUtc);

    return this.http.get<ScanGmailSuggestionsResponse>(
      `${this.baseUrl}/integrations/gmail/scan-suggestions`,
      { params }
    );
  }

  disconnect(gmailConnectionId: string): Observable<void> {
    return this.http.post<void>(
      `${this.baseUrl}/integrations/gmail/connections/${gmailConnectionId}/disconnect`,
      {}
    );
  }
}