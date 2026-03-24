import { HttpClient, HttpParams } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {
  GetAppointmentsForDateRangeResponse,
} from '../models/schedule.models';
import {
  APP_RUNTIME_CONFIG,
  AppRuntimeConfig,
} from '../../../core/config/app.config.token';

@Injectable({ providedIn: 'root' })
export class ScheduleApiService {
  constructor(
    private readonly http: HttpClient,
    @Inject(APP_RUNTIME_CONFIG) private readonly config: AppRuntimeConfig
  ) {}

  getAppointmentsForDateRange(
    householdId: string,
    fromUtc: string,
    toUtc: string
  ): Observable<GetAppointmentsForDateRangeResponse> {
    const params = new HttpParams()
      .set('householdId', householdId)
      .set('fromUtc', fromUtc)
      .set('toUtc', toUtc);

    return this.http.get<GetAppointmentsForDateRangeResponse>(
      `${this.config.apiBaseUrl}/appointments`,
      { params }
    );
  }
}