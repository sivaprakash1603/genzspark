import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { BookingResponse, PassengerDetails } from '../models/app.models';

@Injectable({ providedIn: 'root' })
export class BookingService {
  constructor(private http: HttpClient) {}

  initiate(busId: string, journeyDate: string, passengers: PassengerDetails[]): Observable<BookingResponse> {
    return this.http.post<BookingResponse>(`${environment.apiUrl}/bookings/initiate`, { busId, journeyDate, passengers });
  }

  myBookings(filter = 'all'): Observable<BookingResponse[]> {
    return this.http.get<BookingResponse[]>(`${environment.apiUrl}/bookings/my`, { params: { filter } });
  }

  cancel(bookingId: string, reason: string): Observable<void> {
    return this.http.post<void>(`${environment.apiUrl}/bookings/${bookingId}/cancel`, { reason });
  }

  downloadTicket(bookingId: string): Observable<Blob> {
    return this.http.get(`${environment.apiUrl}/bookings/${bookingId}/ticket`, { responseType: 'blob' });
  }

  lockSeat(seatId: string, journeyDate: string): Observable<boolean> {
    return this.http.post<boolean>(`${environment.apiUrl}/bookings/seats/${seatId}/lock`, {}, { params: { journeyDate } });
  }

  unlockSeat(seatId: string): Observable<boolean> {
    return this.http.post<boolean>(`${environment.apiUrl}/bookings/seats/${seatId}/unlock`, {});
  }
}
