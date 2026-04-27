import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { BusSearchResult, SeatResponse } from '../models/app.models';

@Injectable({ providedIn: 'root' })
export class BusService {
  constructor(private http: HttpClient) {}

  search(source: string, destination: string, date: string): Observable<BusSearchResult[]> {
    return this.http.get<BusSearchResult[]>(`${environment.apiUrl}/buses/search`, {
      params: { source, destination, date }
    });
  }

  getSeats(busId: string, journeyDate: string): Observable<SeatResponse[]> {
    return this.http.get<SeatResponse[]>(`${environment.apiUrl}/buses/${busId}/seats`, {
      params: { journeyDate }
    });
  }

  addBus(payload: unknown): Observable<void> {
    return this.http.post<void>(`${environment.apiUrl}/operator/buses`, payload);
  }

  getLocations(query: string = ''): Observable<{ id: string; name: string }[]> {
    return this.http.get<{ id: string; name: string }[]>(`${environment.apiUrl}/buses/locations`, {
      params: { query }
    });
  }
}
