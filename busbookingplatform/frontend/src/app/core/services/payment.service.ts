import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PaymentResult } from '../models/app.models';

@Injectable({ providedIn: 'root' })
export class PaymentService {
  constructor(private http: HttpClient) {}

  process(payload: {
    bookingId: string;
    transactionId: string;
    isSuccess: boolean;
    cardNumber: string;
  }): Observable<PaymentResult> {
    return this.http.post<PaymentResult>(`${environment.apiUrl}/payments/process`, payload);
  }
}
