import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AdminService {
  constructor(private http: HttpClient) {}

  getPendingOperators(): Observable<unknown[]> {
    return this.http.get<unknown[]>(`${environment.apiUrl}/admin/operators/pending`);
  }

  approveOperator(operatorId: string): Observable<void> {
    return this.http.post<void>(`${environment.apiUrl}/admin/operators/${operatorId}/approve`, {});
  }

  rejectOperator(operatorId: string, reason: string): Observable<void> {
    return this.http.post<void>(`${environment.apiUrl}/admin/operators/${operatorId}/reject`, { reason });
  }

  getUsers(): Observable<unknown[]> {
    return this.http.get<unknown[]>(`${environment.apiUrl}/admin/users`);
  }

  getBuses(): Observable<any[]> {
    return this.http.get<any[]>(`${environment.apiUrl}/admin/buses`);
  }

  approveBus(busId: string): Observable<void> {
    return this.http.post<void>(`${environment.apiUrl}/admin/buses/${busId}/approve`, {});
  }

  rejectBus(busId: string, reason: string): Observable<void> {
    return this.http.post<void>(`${environment.apiUrl}/admin/buses/${busId}/reject`, { reason });
  }

  getBookings(): Observable<unknown[]> {
    return this.http.get<unknown[]>(`${environment.apiUrl}/admin/bookings`);
  }

  getSystemLogs(): Observable<any[]> {
    return this.http.get<any[]>(`${environment.apiUrl}/admin/system-logs`);
  }

  getStats(): Observable<any> {
    return this.http.get<any>(`${environment.apiUrl}/admin/stats`);
  }
}
