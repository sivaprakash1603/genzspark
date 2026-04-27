import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from './core/services/auth.service';
import { ThemeService } from './core/services/theme.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    <div class="app-surface">
      <header class="topbar">
        <div class="topbar-inner">
          <a class="brand-block" routerLink="/">
            <div class="brand-mark"></div>
            <div class="brand-copy">
              <strong>BusBooking</strong>
              <span>BookMyTrip inspired travel UI</span>
            </div>
          </a>

          <nav class="nav-links">
            <a class="nav-chip" routerLink="/" routerLinkActive="active" [routerLinkActiveOptions]="{ exact: true }">Home</a>
            <a class="nav-chip" routerLink="/search" routerLinkActive="active">Search</a>
            <a class="nav-chip" *ngIf="auth.getRole() === 'Passenger'" routerLink="/bookings" routerLinkActive="active">Bookings</a>
            <a class="nav-chip" *ngIf="auth.getRole() === 'Operator'" routerLink="/operator" routerLinkActive="active">Operator</a>
            <a class="nav-chip" *ngIf="auth.getRole() === 'Admin'" routerLink="/admin" routerLinkActive="active">Admin</a>
          </nav>

          <div class="topbar-actions">
            <button type="button" class="theme-toggle" (click)="toggleTheme()">{{ themeLabel }}</button>

            <ng-container *ngIf="auth.isLoggedIn(); else loginAction">
              <span class="status-pill">{{ auth.getRole() }}</span>
              <button type="button" class="ghost-button" (click)="auth.logout()">Sign out</button>
            </ng-container>

            <ng-template #loginAction>
              <a class="solid-button" routerLink="/login">Sign in</a>
            </ng-template>
          </div>
        </div>
      </header>

      <main class="page-shell shell-content">
        <router-outlet></router-outlet>
      </main>
    </div>
  `
})
export class AppComponent {
  constructor(public auth: AuthService, private theme: ThemeService) {}

  get themeLabel(): string {
    return this.theme.getTheme() === 'dark' ? 'Light mode' : 'Dark mode';
  }

  toggleTheme(): void {
    this.theme.toggle();
  }
}
