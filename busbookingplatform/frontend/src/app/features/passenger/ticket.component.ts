import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { BookingService } from '../../core/services/booking.service';

@Component({
  standalone: true,
  selector: 'app-ticket',
  imports: [CommonModule, RouterLink, MatCardModule, MatButtonModule],
  template: `
    <mat-card class="page-card">
      <section class="layout-stack" style="padding: 24px; max-width: 760px; margin: 0 auto;">
        <div class="eyebrow">Ticket ready</div>
        <h2>Your booking is confirmed.</h2>
        <p>Download your ticket now or access it later from booking history.</p>

        <div class="surface-section">
          <div class="route-row">
            <div>
              <div class="muted">Booking ID</div>
              <h3>{{ bookingId }}</h3>
            </div>
            <div style="text-align: right;">
              <div class="muted">Status</div>
              <span class="status-pill">Confirmed</span>
            </div>
          </div>
        </div>

        <div class="actions">
          <button mat-raised-button color="primary" (click)="downloadTicket()" [disabled]="isDownloading">
            {{ isDownloading ? 'Downloading...' : 'Download Ticket' }}
          </button>
          <a mat-button routerLink="/bookings">View Bookings</a>
          <a mat-button routerLink="/search">Book Another Trip</a>
        </div>

        <p *ngIf="errorMessage" style="color: var(--danger);">{{ errorMessage }}</p>
      </section>
    </mat-card>
  `
})
export class TicketComponent {
  bookingId = '';
  isDownloading = false;
  errorMessage = '';

  constructor(private route: ActivatedRoute, private bookingService: BookingService) {
    this.bookingId = this.route.snapshot.paramMap.get('bookingId') ?? '';
  }

  downloadTicket(): void {
    if (!this.bookingId || this.isDownloading) {
      return;
    }

    this.errorMessage = '';
    this.isDownloading = true;

    this.bookingService.downloadTicket(this.bookingId).subscribe({
      next: (blob) => {
        this.isDownloading = false;
        const url = URL.createObjectURL(blob);
        const anchor = document.createElement('a');
        anchor.href = url;
        anchor.download = `ticket-${this.bookingId}.pdf`;
        anchor.click();
        URL.revokeObjectURL(url);
      },
      error: () => {
        this.isDownloading = false;
        this.errorMessage = 'Unable to download ticket right now. Try again from booking history.';
      }
    });
  }
}