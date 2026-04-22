import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { AuthService } from './core/services/auth.service';
import { NgIf } from '@angular/common';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, MatToolbarModule, MatButtonModule, NgIf],
  template: `
    <mat-toolbar>
      <div class="nav-container">
        <div class="brand" routerLink="/">
          <div class="logo-box"></div>
          <span class="logo-text">Velocis</span>
        </div>
        
        <div class="nav-links">
          <a mat-button routerLink="/search" routerLinkActive="active">Explore</a>
          <a mat-button *ngIf="auth.getRole() === 'Passenger'" routerLink="/bookings" routerLinkActive="active">Bookings</a>
          <a mat-button *ngIf="auth.getRole() === 'Operator'" routerLink="/operator" routerLinkActive="active">Operator</a>
          <a mat-button *ngIf="auth.getRole() === 'Admin'" routerLink="/admin" routerLinkActive="active">Admin</a>
        </div>

        <div class="spacer"></div>

        <div class="auth-section">
          <ng-container *ngIf="auth.isLoggedIn(); else loginBtn">
            <span class="badge">{{ auth.getRole() }}</span>
            <button class="logout-link" (click)="auth.logout()">Sign out</button>
          </ng-container>
          <ng-template #loginBtn>
            <a mat-button routerLink="/login" class="login-trigger">Sign in</a>
          </ng-template>
        </div>
      </div>
    </mat-toolbar>
    
    <main class="page-shell">
      <router-outlet></router-outlet>
    </main>
  `,
  styles: [`
    .nav-container {
      width: 100%;
      max-width: 1200px;
      margin: 0 auto;
      display: flex;
      align-items: center;
      padding: 0 24px;
    }

    .brand {
      display: flex;
      align-items: center;
      gap: 12px;
      cursor: pointer;
      margin-right: 48px;
    }

    .logo-box {
      width: 24px;
      height: 24px;
      background: var(--accent-gradient);
      border-radius: 4px;
      transform: rotate(45deg);
    }

    .logo-text {
      font-weight: 700;
      font-size: 20px;
      letter-spacing: -0.05em;
      color: #fff;
    }

    .nav-links {
      display: flex;
      gap: 4px;
    }

    .nav-links a {
      color: var(--text-muted);
      font-size: 14px;
      transition: color 0.2s;
    }

    .nav-links a:hover, .nav-links a.active {
      color: #fff;
    }

    .spacer { flex: 1; }

    .auth-section {
      display: flex;
      align-items: center;
      gap: 20px;
    }

    .badge {
      font-size: 10px;
      text-transform: uppercase;
      letter-spacing: 0.1em;
      font-weight: 700;
      color: var(--accent);
      background: rgba(0, 112, 243, 0.1);
      padding: 2px 8px;
      border-radius: 100px;
      border: 1px solid rgba(0, 112, 243, 0.2);
    }

    .logout-link {
      background: none;
      border: none;
      color: var(--text-muted);
      font-size: 13px;
      cursor: pointer;
      padding: 0;
    }

    .logout-link:hover { color: #ff4d4d; }

    .login-trigger {
      color: #fff !important;
      border: 1px solid var(--border) !important;
      border-radius: 6px !important;
    }
  `]
})
export class AppComponent {
  constructor(public auth: AuthService) {}
}
