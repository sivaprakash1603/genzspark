import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class PaymentService {
  constructor(private http: HttpClient) {}

  process(payload: {
    bookingId: string;
    transactionId: string;
    isSuccess: boolean;
    cardNumber: string;
  }): Observable<unknown> {
    return this.http.post(`${environment.apiUrl}/payments/process`, payload);
  }
}
