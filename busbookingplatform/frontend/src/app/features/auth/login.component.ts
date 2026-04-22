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
    <div class="auth-wrap">
      <div class="auth-box premium-card">
        <div class="auth-header">
          <h2>{{ mode === 'login' ? 'Welcome back' : 'Create account' }}</h2>
          <p>{{ mode === 'login' ? 'Enter your credentials to continue' : 'Join our premium travel network' }}</p>
        </div>

        <div class="mode-toggle">
          <button [class.active]="mode === 'login'" (click)="mode = 'login'">Login</button>
          <button [class.active]="mode === 'register'" (click)="mode = 'register'">Register</button>
        </div>

        <div class="error-msg" *ngIf="errorMessage">
          {{ errorMessage }}
        </div>

        <!-- Login Form -->
        <form *ngIf="mode === 'login'" [formGroup]="loginForm" (ngSubmit)="login()">
          <div class="input-group">
            <label>Username</label>
            <input type="text" formControlName="username" placeholder="johndoe" />
          </div>
          <div class="input-group">
            <label>Password</label>
            <input type="password" formControlName="password" placeholder="••••••••" />
          </div>
          <button type="submit" class="btn-primary" [disabled]="loginForm.invalid">Sign In</button>
        </form>

        <!-- Register Form -->
        <div *ngIf="mode === 'register'">
          <div class="type-selector">
            <button [class.selected]="regType === 'passenger'" (click)="regType = 'passenger'">
              Passenger
              <span class="desc">Book trips</span>
            </button>
            <button [class.selected]="regType === 'operator'" (click)="regType = 'operator'">
              Operator
              <span class="desc">Manage buses</span>
            </button>
          </div>

          <form [formGroup]="regType === 'passenger' ? passengerForm : operatorForm" (ngSubmit)="register()">
            <div class="input-group">
              <label>Username</label>
              <input type="text" formControlName="username" placeholder="johndoe" />
            </div>
            <div class="input-group">
              <label>Email</label>
              <input type="email" formControlName="email" placeholder="john@example.com" />
            </div>
            <div class="input-group">
              <label>Password</label>
              <input type="password" formControlName="password" placeholder="••••••••" />
            </div>
            
            <!-- Extra field for Operators -->
            <div class="input-group animate-slide" *ngIf="regType === 'operator'" [formGroup]="operatorForm">
              <label>Vehicle Number</label>
              <input type="text" formControlName="vehicleNumber" placeholder="KA-01-HH-1234" />
            </div>

            <button type="submit" class="btn-primary" [disabled]="regType === 'passenger' ? passengerForm.invalid : operatorForm.invalid">
              Create {{ regType === 'passenger' ? 'Passenger' : 'Operator' }} Account
            </button>
          </form>
        </div>

        <div class="auth-footer">
          <p *ngIf="mode === 'login'">New to the platform? <span class="link" (click)="mode = 'register'">Create an account</span></p>
          <p *ngIf="mode === 'register'">Already have an account? <span class="link" (click)="mode = 'login'">Sign in</span></p>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .auth-wrap {
      display: flex;
      justify-content: center;
      padding-top: 60px;
    }

    .auth-box {
      width: 100%;
      max-width: 440px;
      padding: 40px !important;
    }

    .auth-header {
      text-align: center;
      margin-bottom: 32px;
    }

    .auth-header h2 { margin-bottom: 8px; }
    .auth-header p { font-size: 14px; color: var(--text-muted); }

    .mode-toggle {
      display: flex;
      background: var(--surface-raised);
      padding: 4px;
      border-radius: 8px;
      margin-bottom: 32px;
    }

    .mode-toggle button {
      flex: 1;
      background: none;
      border: none;
      color: var(--text-muted);
      padding: 10px;
      font-size: 13px;
      font-weight: 600;
      cursor: pointer;
      border-radius: 6px;
      transition: all 0.2s;
    }

    .mode-toggle button.active {
      background: var(--border);
      color: #fff;
    }

    .error-msg {
      background: rgba(255, 0, 0, 0.1);
      color: #ff4d4d;
      padding: 12px;
      border-radius: 6px;
      margin-bottom: 24px;
      font-size: 13px;
      text-align: center;
      border: 1px solid rgba(255, 0, 0, 0.2);
    }

    /* Type Selector */
    .type-selector {
      display: flex;
      gap: 12px;
      margin-bottom: 24px;
    }

    .type-selector button {
      flex: 1;
      background: var(--surface-raised);
      border: 1px solid var(--border);
      padding: 12px;
      border-radius: 8px;
      color: var(--text-muted);
      cursor: pointer;
      text-align: left;
      transition: all 0.2s;
    }

    .type-selector button.selected {
      border-color: var(--accent);
      background: rgba(0, 112, 243, 0.05);
      color: #fff;
    }

    .type-selector .desc {
      display: block;
      font-size: 11px;
      opacity: 0.6;
      font-weight: 400;
      margin-top: 2px;
    }

    .input-group {
      margin-bottom: 20px;
    }

    .input-group label {
      display: block;
      font-size: 12px;
      font-weight: 600;
      margin-bottom: 8px;
      color: #ccc;
    }

    .input-group input {
      width: 100%;
      background: #111;
      border: 1px solid var(--border);
      color: #fff;
      padding: 12px 16px;
      border-radius: 6px;
      outline: none;
      transition: border-color 0.2s;
      font-family: inherit;
    }

    .input-group input:focus {
      border-color: var(--accent);
    }

    .btn-primary {
      width: 100%;
      background: #fff;
      color: #000;
      border: none;
      padding: 14px;
      border-radius: 6px;
      font-weight: 700;
      cursor: pointer;
      margin-top: 12px;
      transition: transform 0.2s, opacity 0.2s;
    }

    .btn-primary:hover { transform: translateY(-1px); }
    .btn-primary:disabled { opacity: 0.5; cursor: not-allowed; }

    .auth-footer {
      margin-top: 32px;
      text-align: center;
      font-size: 13px;
      color: var(--text-muted);
    }

    .link {
      color: var(--accent);
      cursor: pointer;
      font-weight: 600;
    }

    .animate-slide {
      animation: slideDown 0.3s ease-out;
    }

    @keyframes slideDown {
      from { opacity: 0; transform: translateY(-10px); }
      to { opacity: 1; transform: translateY(0); }
    }
  `]
})
export class LoginComponent {
  mode: 'login' | 'register' = 'login';
  regType: 'passenger' | 'operator' = 'passenger';
  errorMessage = '';

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
    password: ['', Validators.required],
    vehicleNumber: ['', Validators.required]
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
        this.errorMessage = err.error?.message || 'Invalid username or password';
        console.error('Login error:', err);
      }
    });
  }

  register(): void {
    if (this.regType === 'passenger') {
      if (this.passengerForm.invalid) return;
      this.auth.registerPassenger(this.passengerForm.getRawValue() as any).subscribe(() => {
          this.mode = 'login';
      });
    } else {
      if (this.operatorForm.invalid) return;
      this.auth.registerOperator(this.operatorForm.getRawValue() as any).subscribe(() => {
          this.mode = 'login';
      });
    }
  }
}
