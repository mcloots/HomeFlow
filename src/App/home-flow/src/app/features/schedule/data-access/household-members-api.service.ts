import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { RuntimeConfigService } from '../../../core/config/runtime-config.service';
import { GetHouseholdMembersResponse } from '../models/household-member.models';

@Injectable({ providedIn: 'root' })
export class HouseholdMembersApiService {
  constructor(
    private readonly http: HttpClient,
    private readonly runtimeConfig: RuntimeConfigService,
  ) {}

  private get baseUrl(): string {
    return this.runtimeConfig.get().apiBaseUrl;
  }

  getHouseholdMembers(householdId: string): Observable<GetHouseholdMembersResponse> {
    return this.http.get<GetHouseholdMembersResponse>(
      `${this.baseUrl}/households/${householdId}/members`
    );
  }
}
