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

  scheduleMaintenance(busId: string, start: string | null, end: string | null): Observable<void> {
    return this.http.post<void>(`${environment.apiUrl}/operator/buses/${busId}/maintenance`, { start, end });
  }

  removeBus(busId: string): Observable<void> {
    return this.http.delete<void>(`${environment.apiUrl}/operator/buses/${busId}`);
  }

  getBookings(): Observable<unknown[]> {
    return this.http.get<unknown[]>(`${environment.apiUrl}/operator/bookings`);
  }

  getMyBuses(): Observable<any[]> {
    return this.http.get<any[]>(`${environment.apiUrl}/operator/buses`);
  }

  getRevenue(): Observable<{ totalRevenue: number; confirmedBookings: number }> {
    return this.http.get<{ totalRevenue: number; confirmedBookings: number }>(`${environment.apiUrl}/operator/revenue`);
  }

  getOffices(): Observable<any[]> {
    return this.http.get<any[]>(`${environment.apiUrl}/operator/offices`);
  }

  addOffice(cityName: string, address: string): Observable<void> {
    return this.http.post<void>(`${environment.apiUrl}/operator/offices`, { cityName, address });
  }

  removeVehicle(vehicleId: string): Observable<void> {
    return this.http.delete<void>(`${environment.apiUrl}/operator/vehicles/${vehicleId}`);
  }

  removeOffice(officeId: string): Observable<void> {
    return this.http.delete<void>(`${environment.apiUrl}/operator/offices/${officeId}`);
  }
}
