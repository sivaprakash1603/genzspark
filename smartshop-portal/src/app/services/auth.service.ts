import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { BehaviorSubject, Observable, catchError, map, tap, throwError } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private static readonly storageKey = 'smartshop-portal-user';
  private readonly http = inject(HttpClient);
  private readonly userSubject = new BehaviorSubject<AuthUser | null>(this.readStoredUser());

  readonly user$ = this.userSubject.asObservable();
  readonly isLoggedIn$ = this.user$.pipe(map((user) => user !== null));

  get currentUser(): AuthUser | null {
    return this.userSubject.value;
  }

  login(credentials: LoginCredentials): Observable<AuthUser> {
    return this.http.post<AuthUser>('https://dummyjson.com/auth/login', credentials).pipe(
      map((user) => this.normalizeUser(user)),
      tap((user) => this.storeUser(user)),
      catchError((error: HttpErrorResponse) => {
        const message = typeof error.error?.message === 'string' ? error.error.message : 'Unable to login. Please try again.';
        return throwError(() => new Error(message));
      })
    );
  }

  logout(): void {
    sessionStorage.removeItem(AuthService.storageKey);
    this.userSubject.next(null);
  }

  private storeUser(user: AuthUser): void {
    sessionStorage.setItem(AuthService.storageKey, JSON.stringify(user));
    this.userSubject.next(user);
  }

  private readStoredUser(): AuthUser | null {
    const storedUser = sessionStorage.getItem(AuthService.storageKey);

    if (!storedUser) {
      return null;
    }

    try {
      return this.normalizeUser(JSON.parse(storedUser) as AuthUser);
    } catch {
      return null;
    }
  }

  private normalizeUser(user: AuthUser): AuthUser {
    return {
      ...user,
      firstName: user.firstName?.trim() ?? '',
      lastName: user.lastName?.trim() ?? '',
    };
  }
}

export interface LoginCredentials {
  username: string;
  password: string;
}

export interface AuthUser {
  id: number;
  username: string;
  email: string;
  firstName: string;
  lastName: string;
  gender?: string;
  image?: string;
  token: string;
}

  