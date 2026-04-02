import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { RuntimeConfigService } from '../../../core/config/runtime-config.service';
import { CompleteChoreRequest, CompleteChoreResponse } from '../models/complete-chore.models';
import { CreateChoreRequest, CreateChoreResponse } from '../models/create-chore.models';
import { GetChoresForHouseholdResponse } from '../models/chore.models';
import { UpdateChoreRequest, UpdateChoreResponse } from '../models/update-chore.models';

@Injectable({ providedIn: 'root' })
export class ChoreApiService {
  constructor(
    private readonly http: HttpClient,
    private readonly runtimeConfig: RuntimeConfigService
  ) {}

  private get baseUrl(): string {
    return this.runtimeConfig.get().apiBaseUrl;
  }

  getChoresForHousehold(householdId: string): Observable<GetChoresForHouseholdResponse> {
    const params = new HttpParams().set('householdId', householdId);

    return this.http.get<GetChoresForHouseholdResponse>(`${this.baseUrl}/chores`, { params });
  }

  createChore(request: CreateChoreRequest): Observable<CreateChoreResponse> {
    return this.http.post<CreateChoreResponse>(`${this.baseUrl}/chores`, request);
  }

  updateChore(choreId: string, request: UpdateChoreRequest): Observable<UpdateChoreResponse> {
    return this.http.put<UpdateChoreResponse>(`${this.baseUrl}/chores/${choreId}`, request);
  }

  completeChore(choreId: string, request: CompleteChoreRequest): Observable<CompleteChoreResponse> {
    return this.http.post<CompleteChoreResponse>(`${this.baseUrl}/chores/${choreId}/complete`, request);
  }
}
