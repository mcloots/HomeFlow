import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { RuntimeConfigService } from '../../../core/config/runtime-config.service';
import { BillDetails, GetBillsForHouseholdResponse } from '../models/bill.models';
import { CreateBillRequest, CreateBillResponse } from '../models/create-bill.models';
import { UpdateBillRequest, UpdateBillResponse } from '../models/update-bill.models';

@Injectable({ providedIn: 'root' })
export class BillApiService {
  constructor(
    private readonly http: HttpClient,
    private readonly runtimeConfig: RuntimeConfigService
  ) {}

  private get baseUrl(): string {
    return this.runtimeConfig.get().apiBaseUrl;
  }

  getBillsForHousehold(householdId: string): Observable<GetBillsForHouseholdResponse> {
    const params = new HttpParams().set('householdId', householdId);

    return this.http.get<GetBillsForHouseholdResponse>(`${this.baseUrl}/bills`, {
      params,
    });
  }

  getBillDetails(billId: string): Observable<BillDetails> {
    return this.http.get<BillDetails>(`${this.baseUrl}/bills/${billId}`);
  }

  createBill(request: CreateBillRequest): Observable<CreateBillResponse> {
    return this.http.post<CreateBillResponse>(`${this.baseUrl}/bills`, request);
  }

  updateBill(billId: string, request: UpdateBillRequest): Observable<UpdateBillResponse> {
    return this.http.put<UpdateBillResponse>(`${this.baseUrl}/bills/${billId}`, request);
  }
}
