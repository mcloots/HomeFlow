import { HttpClient, HttpParams } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { GetAppointmentsForDateRangeResponse } from '../models/schedule.models';
import { RuntimeConfigService } from '../../../core/config/runtime-config.service';
import { AppointmentDetails } from '../models/appointment-details.models';

@Injectable({ providedIn: 'root' })
export class ScheduleApiService {
  constructor(
    private readonly http: HttpClient,
    private readonly runtimeConfig: RuntimeConfigService,
  ) {}

  private get baseUrl(): string {
    return this.runtimeConfig.get().apiBaseUrl;
  }

  getAppointmentsForDateRange(
    householdId: string,
    fromUtc: string,
    toUtc: string,
  ): Observable<GetAppointmentsForDateRangeResponse> {
    const params = new HttpParams()
      .set('householdId', householdId)
      .set('fromUtc', fromUtc)
      .set('toUtc', toUtc);

    return this.http.get<GetAppointmentsForDateRangeResponse>(
      `${this.baseUrl}/appointments`,
      { params },
    );
  }

  getAppointmentDetails(appointmentId: string): Observable<AppointmentDetails> {
    return this.http.get<AppointmentDetails>(
      `${this.baseUrl}/appointments/${appointmentId}`
    );
  }
}
