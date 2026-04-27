import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class RouteService {
  constructor(private http: HttpClient) {}

  getRoutes(): Observable<{ id: string; source: string; destination: string }[]> {
    return this.http.get<{ id: string; source: string; destination: string }[]>(`${environment.apiUrl}/buses/routes`);
  }

  createSource(name: string): Observable<unknown> {
    return this.http.post(`${environment.apiUrl}/admin/sources`, { name });
  }

  createDestination(name: string): Observable<unknown> {
    return this.http.post(`${environment.apiUrl}/admin/destinations`, { name });
  }

  createRoute(sourceId: string, destinationId: string): Observable<unknown> {
    return this.http.post(`${environment.apiUrl}/admin/routes`, { sourceId, destinationId });
  }

  createRouteByName(sourceName: string, destinationName: string): Observable<unknown> {
    return this.http.post(`${environment.apiUrl}/admin/routes/quick`, { sourceName, destinationName });
  }
}
