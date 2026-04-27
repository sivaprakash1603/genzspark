import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../core/services/auth.service';
import { Router } from '@angular/router';

@Component({
  standalone: true,
  selector: 'app-login',
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="auth-split">
      <div class="auth-banner">
        <h1>{{ mode === 'login' ? 'Welcome back.' : 'Join Us.' }}</h1>
        <p style="color: rgba(255,255,255,0.9); margin-top: 16px; font-size: 1.1rem; line-height: 1.6;">
          {{ mode === 'login' ? 'Sign in to manage bookings, payments, and tickets.' : 'Register once and keep your passenger or operator workspace ready.' }}
        </p>
        <div style="margin-top: 48px;">
          <div style="display: flex; align-items: center; gap: 12px; margin-bottom: 20px;">
            <div style="width: 28px; height: 28px; border-radius: 50%; background: rgba(255,255,255,0.2); display: flex; align-items: center; justify-content: center; font-weight: bold;">✓</div>
            <span style="font-size: 1.05rem;">Fast: One account, all trips</span>
          </div>
          <div style="display: flex; align-items: center; gap: 12px; margin-bottom: 20px;">
            <div style="width: 28px; height: 28px; border-radius: 50%; background: rgba(255,255,255,0.2); display: flex; align-items: center; justify-content: center; font-weight: bold;">✓</div>
            <span style="font-size: 1.05rem;">Secure: JWT-backed sessions</span>
          </div>
        </div>
      </div>

        <div class="auth-form-container">
          <div class="auth-tabs">
            <div class="auth-tab" [class.active]="mode === 'login'" (click)="mode = 'login'">Sign In</div>
            <div class="auth-tab" [class.active]="mode === 'register'" (click)="mode = 'register'">Register</div>
          </div>

          <div class="empty-state" *ngIf="errorMessage" style="padding: 16px; text-align: left; margin-bottom: 24px; border-color: var(--danger); color: var(--danger); background: rgba(220, 38, 38, 0.05);">
            {{ errorMessage }}
          </div>

          <form *ngIf="mode === 'login'" [formGroup]="loginForm" (ngSubmit)="login()" class="layout-stack">
            <input class="search-input" type="text" formControlName="username" placeholder="Username" />
            <input class="search-input" type="password" formControlName="password" placeholder="Password" />
            <button type="submit" class="solid-button" [disabled]="loginForm.invalid" style="width: 100%;">Sign in</button>
          </form>

          <div *ngIf="mode === 'register'" class="layout-stack">
            <div style="display: flex; gap: 12px; margin-bottom: 8px;">
              <button type="button" class="ghost-button" [class.active]="regType === 'passenger'" (click)="regType = 'passenger'" style="flex: 1;">Passenger</button>
              <button type="button" class="ghost-button" [class.active]="regType === 'operator'" (click)="regType = 'operator'" style="flex: 1;">Operator</button>
            </div>

            <form [formGroup]="regType === 'passenger' ? passengerForm : operatorForm" (ngSubmit)="register()" class="layout-stack">
              <input class="search-input" type="text" formControlName="username" placeholder="Username" />
              <input class="search-input" type="email" formControlName="email" placeholder="Email" />
              <input class="search-input" type="password" formControlName="password" placeholder="Password" />
              <button type="submit" class="solid-button" [disabled]="regType === 'passenger' ? passengerForm.invalid : operatorForm.invalid" style="width: 100%;">
                Create Account
              </button>
            </form>
          </div>
        </div>
      </div>

      <!-- Operator Pending Approval Modal -->
      <div *ngIf="showApprovalModal" style="position: fixed; top: 0; left: 0; right: 0; bottom: 0; background: rgba(0,0,0,0.8); backdrop-filter: blur(4px); display: flex; align-items: center; justify-content: center; z-index: 1000;">
        <div style="background: var(--surface); padding: 40px; border-radius: 16px; max-width: 400px; text-align: center; border: 1px solid var(--border); box-shadow: 0 20px 40px rgba(0,0,0,0.5); color: var(--text-main);">
          <div style="font-size: 56px; margin-bottom: 16px; line-height: 1;">⏳</div>
          <h2 style="margin-bottom: 12px; font-size: 1.6rem; color: var(--text-main);">Account Under Review</h2>
          <p style="color: var(--text-muted); margin-bottom: 32px; line-height: 1.6;">
            Your Operator account requires authorization from an Admin. Please wait for approval before attempting to sign in.
          </p>
          <button class="solid-button" type="button" (click)="closeApprovalModal()" style="width: 100%; height: 48px;">I Understand</button>
        </div>
      </div>
    `
})
export class LoginComponent {
  mode: 'login' | 'register' = 'login';
  regType: 'passenger' | 'operator' = 'passenger';
  errorMessage = '';
  showApprovalModal = false;

  loginForm = this.fb.group({
    username: ['', Validators.required],
    password: ['', Validators.required]
  });

  passengerForm = this.fb.group({
    username: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required]
  });

  operatorForm = this.fb.group({
    username: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required]
  });

  constructor(private fb: FormBuilder, private auth: AuthService, private router: Router) {}

  login(): void {
    if (this.loginForm.invalid) return;
    this.errorMessage = '';
    this.auth.login(this.loginForm.value.username!, this.loginForm.value.password!).subscribe({
      next: () => {
        const role = this.auth.getRole();
        if (role === 'Admin') this.router.navigateByUrl('/admin');
        else if (role === 'Operator') this.router.navigateByUrl('/operator');
        else this.router.navigateByUrl('/search');
      },
      error: (err) => {
        const msg = err.error?.message || 'Invalid username or password';
        if (msg === 'Operator not approved or disabled') {
          this.showApprovalModal = true;
        } else {
          this.errorMessage = msg;
        }
        console.error('Login error:', err);
      }
    });
  }

  register(): void {
    if (this.regType === 'passenger') {
      if (this.passengerForm.invalid) return;
      this.auth.registerPassenger(this.passengerForm.getRawValue() as any).subscribe(() => {
          this.mode = 'login';
          this.passengerForm.reset();
      });
    } else {
      if (this.operatorForm.invalid) return;
      this.auth.registerOperator(this.operatorForm.getRawValue() as any).subscribe(() => {
          this.showApprovalModal = true;
          this.operatorForm.reset();
      });
    }
  }

  closeApprovalModal(): void {
    this.showApprovalModal = false;
    this.mode = 'login';
  }
}
