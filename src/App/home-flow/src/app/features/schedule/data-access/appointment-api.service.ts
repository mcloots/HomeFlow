import { HttpClient } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { RuntimeConfigService } from '../../../core/config/runtime-config.service';
import { CreateAppointmentRequest } from '../models/create-appointment.models';
import { UpdateAppointmentRequest } from '../models/update-appointment.models';

export interface CreateAppointmentResponse {
  appointmentId: string;
  tenantId: string;
  householdId: string;
  title: string;
  startsAtUtc: string;
  endsAtUtc: string;
  status: string;
  type: string;
}

export interface UpdateAppointmentResponse {
  appointmentId: string;
  title: string;
  startsAtUtc: string;
  endsAtUtc: string;
  status: string;
  type: string;
}

@Injectable({ providedIn: 'root' })
export class AppointmentApiService {
  constructor(
    private readonly http: HttpClient,
    private readonly runtimeConfig: RuntimeConfigService,
  ) {}

  private get baseUrl(): string {
    return this.runtimeConfig.get().apiBaseUrl;
  }

  createAppointment(
    request: CreateAppointmentRequest
  ): Observable<CreateAppointmentResponse> {
    return this.http.post<CreateAppointmentResponse>(
      `${this.baseUrl}/appointments`,
      request
    );
  }

  updateAppointment(
    appointmentId: string,
    request: UpdateAppointmentRequest
  ): Observable<UpdateAppointmentResponse> {
    return this.http.put<UpdateAppointmentResponse>(
      `${this.baseUrl}/appointments/${appointmentId}`,
      request
    );
  }
}
