import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { PaymentService } from '../../core/services/payment.service';

@Component({
  standalone: true,
  selector: 'app-dummy-payment',
  imports: [CommonModule, ReactiveFormsModule, MatCardModule, MatFormFieldModule, MatInputModule, MatButtonModule],
  template: `
    <mat-card class="page-card">
      <h2>Dummy Payment</h2>
      <p>Booking Id: {{ bookingId }}</p>

      <form [formGroup]="form" class="form-grid">
        <mat-form-field appearance="outline"><mat-label>Card Number</mat-label><input matInput formControlName="cardNumber" /></mat-form-field>
        <mat-form-field appearance="outline"><mat-label>Expiry</mat-label><input matInput formControlName="expiry" /></mat-form-field>
        <mat-form-field appearance="outline"><mat-label>CVV</mat-label><input matInput formControlName="cvv" /></mat-form-field>
      </form>

      <div class="actions">
        <button mat-raised-button color="primary" (click)="pay()">Pay</button>
      </div>

      <p *ngIf="result" style="margin-top: 12px;">Payment {{ result }}.</p>
    </mat-card>
  `
})
export class DummyPaymentComponent {
  bookingId = '';
  result = '';

  form = this.fb.group({
    cardNumber: ['', Validators.required],
    expiry: ['', Validators.required],
    cvv: ['', Validators.required]
  });

  constructor(private fb: FormBuilder, private route: ActivatedRoute, private paymentService: PaymentService) {
    this.bookingId = this.route.snapshot.paramMap.get('bookingId') ?? '';
  }

  pay(): void {
    if (this.form.invalid) return;
    const isSuccess = Math.random() >= 0.5;
    const transactionId = crypto.randomUUID();

    this.paymentService
      .process({
        bookingId: this.bookingId,
        transactionId,
        isSuccess,
        cardNumber: this.form.value.cardNumber ?? ''
      })
      .subscribe(() => {
        this.result = isSuccess ? 'Success' : 'Failed';
      });
  }
}
