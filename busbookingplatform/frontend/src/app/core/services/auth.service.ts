import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthResponse } from '../models/app.models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly tokenKey = 'bus_booking_token';
  private readonly roleKey = 'bus_booking_role';

  constructor(private http: HttpClient) {}

  login(username: string, password: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${environment.apiUrl}/auth/login`, { username, password }).pipe(
      tap((result: any) => {
        const token = result.token || result.Token;
        const role = result.role || result.Role;
        if (token) localStorage.setItem(this.tokenKey, token);
        if (role) localStorage.setItem(this.roleKey, role);
      })
    );
  }

  registerPassenger(payload: { username: string; email: string; password: string }): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${environment.apiUrl}/auth/register/passenger`, payload);
  }

  registerOperator(payload: { username: string; email: string; password: string; vehicleNumber: string }): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${environment.apiUrl}/auth/register/operator`, payload);
  }

  getToken(): string | null {
    const val = localStorage.getItem(this.tokenKey);
    return val === 'undefined' ? null : val;
  }

  getRole(): string | null {
    const val = localStorage.getItem(this.roleKey);
    return val === 'undefined' ? null : val;
  }

  isLoggedIn(): boolean {
    return !!this.getToken();
  }

  logout(): void {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.roleKey);
  }
}
