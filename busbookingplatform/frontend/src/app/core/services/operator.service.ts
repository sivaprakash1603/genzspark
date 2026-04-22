import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class OperatorService {
  constructor(private http: HttpClient) {}

  disableBus(busId: string): Observable<void> {
    return this.http.post<void>(`${environment.apiUrl}/operator/buses/${busId}/disable-temporary`, {});
  }

  enableBus(busId: string): Observable<void> {
    return this.http.post<void>(`${environment.apiUrl}/operator/buses/${busId}/enable-temporary`, {});
  }

  removeBus(busId: string): Observable<void> {
    return this.http.delete<void>(`${environment.apiUrl}/operator/buses/${busId}`);
  }

  getBookings(): Observable<unknown[]> {
    return this.http.get<unknown[]>(`${environment.apiUrl}/operator/bookings`);
  }

  getRevenue(): Observable<{ totalRevenue: number; confirmedBookings: number }> {
    return this.http.get<{ totalRevenue: number; confirmedBookings: number }>(`${environment.apiUrl}/operator/revenue`);
  }
}
