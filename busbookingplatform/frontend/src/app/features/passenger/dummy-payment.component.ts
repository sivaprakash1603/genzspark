import { Component, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { PaymentService } from '../../core/services/payment.service';

@Component({
  standalone: true,
  selector: 'app-dummy-payment',
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="payment-layout">
      <div class="apple-card" style="padding: 40px; width: 100%; max-width: 500px; text-align: center;">
        <h2 style="margin-top: 0; margin-bottom: 8px; font-weight: 700; font-size: 1.8rem;">Complete Payment</h2>
        <div style="background: #fff9db; border: 1px solid #fab005; border-radius: 12px; padding: 12px; margin-bottom: 24px; display: flex; align-items: center; justify-content: center; gap: 8px;">
          <span style="color: #f08c00; font-weight: 600;">Time remaining:</span>
          <span style="font-family: monospace; font-size: 1.2rem; font-weight: 700; color: #e67700;">{{ timeLeft }}</span>
        </div>
        <p style="color: var(--text-muted); margin-bottom: 32px;">Enter your card details to finalize the booking.</p>
        
        <div class="booking-ref">
          <div style="display: flex; justify-content: space-between; margin-bottom: 12px;">
            <span>Booking ID</span>
            <strong>{{ bookingId }}</strong>
          </div>
          <div style="display: flex; justify-content: space-between; border-top: 1px dashed var(--border); padding-top: 12px;">
            <span>Total Amount</span>
            <strong style="color: var(--success); font-size: 1.3rem;">₹{{ amount }}</strong>
          </div>
        </div>

        <form [formGroup]="form" style="text-align: left;">
          <div style="margin-bottom: 20px;">
            <label class="search-field-label">CARD NUMBER</label>
            <input class="search-input" formControlName="cardNumber" placeholder="0000 0000 0000 0000" style="height: 52px; font-family: monospace; font-size: 1.1rem; letter-spacing: 2px;" />
          </div>

          <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 20px; margin-bottom: 32px;">
            <div>
              <label class="search-field-label">EXPIRY DATE</label>
              <input class="search-input" formControlName="expiry" placeholder="MM/YY" style="height: 52px; font-family: monospace; font-size: 1.1rem;" />
            </div>
            <div>
              <label class="search-field-label">CVV</label>
              <input class="search-input" formControlName="cvv" placeholder="123" type="password" style="height: 52px; font-family: monospace; font-size: 1.1rem;" />
            </div>
          </div>

          <div style="display: flex; gap: 16px;">
            <button class="solid-button" type="button" (click)="pay(true)" [disabled]="isProcessing || form.invalid" style="flex: 2; height: 52px; font-size: 1.1rem;">
              {{ isProcessing ? 'Processing...' : 'Pay Securely' }}
            </button>
            <button class="ghost-button" type="button" (click)="pay(false)" [disabled]="isProcessing" style="flex: 1; height: 52px; border-color: var(--danger); color: var(--danger);">
              Simulate Failure
            </button>
          </div>
        </form>

        <div *ngIf="result" class="result-banner" [ngClass]="result === 'Success' ? 'success' : 'error'">
          {{ result === 'Success' ? 'Payment successful. Redirecting to your ticket...' : 'Payment failed. Please try again.' }}
        </div>
      </div>
    </div>
  `,
  styles: [`
    .payment-layout {
      padding: 40px 20px;
      display: flex;
      align-items: center;
      justify-content: center;
    }

    .booking-ref {
      background: var(--surface-raised);
      border: 1px dashed var(--border);
      border-radius: 12px;
      padding: 16px 20px;
      margin-bottom: 32px;
      display: flex;
      flex-direction: column;
      gap: 4px;
    }

    .booking-ref span {
      color: var(--text-muted);
      font-size: 0.85rem;
      font-weight: 600;
      letter-spacing: 1px;
    }

    .booking-ref strong {
      font-family: monospace;
      color: var(--accent);
      font-size: 1.1rem;
    }

    .result-banner {
      margin-top: 24px;
      padding: 16px;
      border-radius: 12px;
      font-weight: 600;
    }

    .result-banner.success {
      background: #ecfdf5;
      color: #10b981;
      border: 1px solid #10b981;
    }

    .result-banner.error {
      background: #fef2f2;
      color: #ef4444;
      border: 1px solid #ef4444;
    }
  `]
})
export class DummyPaymentComponent {
  bookingId = '';
  amount: number | null = null;
  result = '';
  isProcessing = false;
  timeLeft = '10:00';
  timerHandle: any;
  secondsRemaining = 600;

  form = this.fb.group({
    cardNumber: ['', Validators.required],
    expiry: ['', Validators.required],
    cvv: ['', Validators.required]
  });

  constructor(private fb: FormBuilder, private route: ActivatedRoute, private router: Router, private paymentService: PaymentService) {
    this.bookingId = this.route.snapshot.paramMap.get('bookingId') ?? '';
    this.route.queryParams.subscribe(params => {
      if (params['amount']) {
        this.amount = +params['amount'];
      }
    });
    this.startTimer();
  }

  ngOnDestroy(): void {
    if (this.timerHandle) clearInterval(this.timerHandle);
  }

  startTimer(): void {
    this.timerHandle = setInterval(() => {
      this.secondsRemaining--;
      if (this.secondsRemaining <= 0) {
        clearInterval(this.timerHandle);
        alert('Your seat lock has expired. Please start over.');
        this.router.navigate(['/search']);
        return;
      }

      const mins = Math.floor(this.secondsRemaining / 60);
      const secs = this.secondsRemaining % 60;
      this.timeLeft = `${mins}:${secs.toString().padStart(2, '0')}`;
    }, 1000);
  }

  pay(isSuccess: boolean): void {
    if (this.form.invalid || this.isProcessing) return;

    this.isProcessing = true;
    const transactionId = crypto.randomUUID();

    this.paymentService
      .process({
        bookingId: this.bookingId,
        transactionId,
        isSuccess,
        cardNumber: this.form.value.cardNumber ?? ''
      })
      .subscribe((response) => {
        this.isProcessing = false;
        this.result = response.paymentStatus;
        if (response.paymentStatus === 'Success') {
          setTimeout(() => {
            this.router.navigate(['/ticket', this.bookingId]);
          }, 600);
        }
      });
  }
}
