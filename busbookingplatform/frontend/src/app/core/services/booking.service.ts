import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { BookingResponse } from '../models/app.models';

@Injectable({ providedIn: 'root' })
export class BookingService {
  constructor(private http: HttpClient) {}

  initiate(busId: string, seatIds: string[]): Observable<BookingResponse> {
    return this.http.post<BookingResponse>(`${environment.apiUrl}/bookings/initiate`, { busId, seatIds });
  }

  myBookings(): Observable<BookingResponse[]> {
    return this.http.get<BookingResponse[]>(`${environment.apiUrl}/bookings/my`);
  }

  cancel(bookingId: string, reason: string): Observable<void> {
    return this.http.post<void>(`${environment.apiUrl}/bookings/${bookingId}/cancel`, { reason });
  }
}
