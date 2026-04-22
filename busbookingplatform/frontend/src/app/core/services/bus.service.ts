import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { BusSearchResult } from '../models/app.models';

@Injectable({ providedIn: 'root' })
export class BusService {
  constructor(private http: HttpClient) {}

  search(source: string, destination: string): Observable<BusSearchResult[]> {
    return this.http.get<BusSearchResult[]>(`${environment.apiUrl}/buses/search`, {
      params: { source, destination }
    });
  }

  getSeats(busId: string): Observable<{ seatId: string; seatNumber: string; isBooked: boolean }[]> {
    return this.http.get<{ seatId: string; seatNumber: string; isBooked: boolean }[]>(`${environment.apiUrl}/buses/${busId}/seats`);
  }

  addBus(payload: unknown): Observable<void> {
    return this.http.post<void>(`${environment.apiUrl}/operator/buses`, payload);
  }
}
