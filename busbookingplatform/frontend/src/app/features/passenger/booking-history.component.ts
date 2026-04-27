import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { FormsModule } from '@angular/forms';
import { BookingService } from '../../core/services/booking.service';
import { BookingResponse } from '../../core/models/app.models';

@Component({
  standalone: true,
  selector: 'app-booking-history',
  imports: [CommonModule, MatCardModule, MatTableModule, MatButtonModule, MatIconModule, MatFormFieldModule, MatInputModule, MatSnackBarModule, FormsModule],
  template: `
    <div class="history-layout">
      <div class="header">
        <h1>My Bookings</h1>
        <p>View and manage your travel history</p>
      </div>

      <div class="actions" style="margin-bottom: 16px;">
        <button mat-button [class.active-filter]="activeFilter === 'all'" (click)="setFilter('all')">All</button>
        <button mat-button [class.active-filter]="activeFilter === 'upcoming'" (click)="setFilter('upcoming')">Upcoming</button>
        <button mat-button [class.active-filter]="activeFilter === 'completed'" (click)="setFilter('completed')">Completed</button>
        <button mat-button [class.active-filter]="activeFilter === 'cancelled'" (click)="setFilter('cancelled')">Cancelled</button>
      </div>

      <div class="apple-card no-padding">
        <table mat-table [dataSource]="bookings" class="full-width">
          <ng-container matColumnDef="route">
            <th mat-header-cell *matHeaderCellDef>Route</th>
            <td mat-cell *matCellDef="let b">
              <div style="font-weight: 500;">{{ b.source }} → {{ b.destination }}</div>
              <div class="muted" style="font-size: 11px;">{{ b.busName }}</div>
            </td>
          </ng-container>

          <ng-container matColumnDef="journeyDate">
            <th mat-header-cell *matHeaderCellDef>Journey Date</th>
            <td mat-cell *matCellDef="let b">
               {{ b.journeyDate | date: 'mediumDate':'UTC' }}
            </td>
          </ng-container>

          <ng-container matColumnDef="bookingStatus">
            <th mat-header-cell *matHeaderCellDef>Status</th>
            <td mat-cell *matCellDef="let b">
              <span class="status-badge" [ngClass]="b.bookingStatus.toLowerCase()">
                {{ b.bookingStatus }}
              </span>
            </td>
          </ng-container>

          <ng-container matColumnDef="totalAmount">
            <th mat-header-cell *matHeaderCellDef>Total Paid</th>
            <td mat-cell *matCellDef="let b">
              <span class="amount">₹{{ b.totalAmount }}</span>
            </td>
          </ng-container>

          <ng-container matColumnDef="bookedAt">
            <th mat-header-cell *matHeaderCellDef>Date</th>
            <td mat-cell *matCellDef="let b">
              <div class="date-cell">
                <span class="date">{{ b.bookedAt | date: 'mediumDate' }}</span>
                <span class="time">{{ b.bookedAt | date: 'shortTime' }}</span>
              </div>
            </td>
          </ng-container>

          <ng-container matColumnDef="action">
            <th mat-header-cell *matHeaderCellDef></th>
            <td mat-cell *matCellDef="let b" class="action-cell">
              <button 
                mat-button 
                class="cancel-btn" 
                (click)="cancel(b.bookingId)" 
                *ngIf="b.bookingStatus === 'Confirmed'">
                Cancel
              </button>

              <button
                mat-button
                class="ticket-btn"
                (click)="downloadTicket(b.bookingId)"
                *ngIf="b.bookingStatus === 'Confirmed'">
                Ticket
              </button>
            </td>
          </ng-container>

          <tr mat-header-row *matHeaderRowDef="columns"></tr>
          <tr mat-row *matRowDef="let row; columns: columns"></tr>
        </table>

        <div class="empty-state" *ngIf="bookings.length === 0">
          <p>You haven't made any bookings yet.</p>
        </div>
      </div>

      <!-- Cancellation Modal -->
      <div class="modal-overlay" *ngIf="showCancelModal">
        <div class="modal-card">
          <div class="modal-header">
            <mat-icon style="color: #ef4444;">warning</mat-icon>
            <h3>Cancel Booking</h3>
          </div>
          <div class="modal-body">
            <p style="margin-bottom: 16px; font-weight: 500; color: #1f2937;">Cancellation & Refund Policy:</p>
            <div class="policy-list" style="background: #f9fafb; padding: 12px; border-radius: 8px; margin-bottom: 20px;">
              <div class="policy-item">
                <span>Before 72 hours</span>
                <span style="color: #059669; font-weight: 600;">100% Refund</span>
              </div>
              <div class="policy-item">
                <span>24 - 72 hours</span>
                <span style="color: #059669; font-weight: 600;">80% Refund</span>
              </div>
              <div class="policy-item">
                <span>12 - 24 hours</span>
                <span style="color: #d97706; font-weight: 600;">50% Refund</span>
              </div>
              <div class="policy-item">
                <span>Less than 12 hours</span>
                <span style="color: #dc2626; font-weight: 600;">25% Refund</span>
              </div>
            </div>

            <div style="margin-top: 24px;">
              <label style="display: block; font-size: 0.85rem; font-weight: 500; color: #374151; margin-bottom: 8px;">Reason for Cancellation</label>
              <textarea 
                [(ngModel)]="cancelReason" 
                placeholder="Please tell us why you are cancelling..."
                style="width: 100%; border: 1.5px solid #e5e7eb; border-radius: 12px; padding: 12px; font-size: 0.95rem; font-family: inherit; resize: none; transition: all 0.2s;"
                rows="4"
                onfocus="this.style.borderColor='#ef4444'; this.style.ring='4px rgba(239, 68, 68, 0.1)';"
                onblur="this.style.borderColor='#e5e7eb';">
              </textarea>
            </div>
            <p style="font-size: 0.75rem; color: #6b7280; margin-top: 12px; display: flex; align-items: center; gap: 4px;">
              <mat-icon style="font-size: 14px; width: 14px; height: 14px;">info</mat-icon>
              Refunds will be credited within 5-7 business days.
            </p>
          </div>
          <div class="modal-footer" style="padding: 16px 24px; background: #f9fafb; display: flex; justify-content: flex-end; gap: 12px;">
            <button mat-button (click)="showCancelModal = false" [disabled]="isCancelling">Keep Booking</button>
            <button mat-flat-button color="warn" (click)="confirmCancel()" [disabled]="!cancelReason.trim() || isCancelling" style="background-color: #ef4444; color: white;">
              {{ isCancelling ? 'Processing...' : 'Confirm Cancellation' }}
            </button>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .history-layout {
      max-width: 900px;
      margin: 0 auto;
    }

    .header {
      margin-bottom: 32px;
    }

    .header h1 {
      font-size: 34px;
      letter-spacing: -0.02em;
      margin-bottom: 4px;
    }

    .header p {
      color: var(--apple-secondary-text);
      font-size: 17px;
    }

    .no-padding {
      padding: 0 !important;
      overflow: hidden;
    }

    .full-width {
      width: 100%;
    }

    .ref-cell {
      font-family: monospace;
      font-size: 12px;
      color: var(--apple-secondary-text);
    }

    .status-badge {
      font-size: 12px;
      font-weight: 500;
      padding: 4px 10px;
      border-radius: 6px;
      text-transform: capitalize;
    }

    .status-badge.confirmed, .status-badge.success {
      background: #e1f5fe;
      color: #0288d1;
    }

    .status-badge.cancelled, .status-badge.failed {
      background: #ffebee;
      color: #d32f2f;
    }

    .status-badge.initiated, .status-badge.pending {
      background: #fff8e1;
      color: #fbc02d;
    }

    .amount {
      font-weight: 600;
    }

    .date-cell {
      display: flex;
      flex-direction: column;
    }

    .date {
      font-weight: 500;
      font-size: 14px;
    }

    .time {
      font-size: 11px;
      color: var(--apple-secondary-text);
    }

    .action-cell {
      text-align: right;
    }

    .cancel-btn {
      color: #ff3b30 !important;
      font-size: 13px !important;
      font-weight: 500 !important;
    }

    .ticket-btn {
      color: var(--accent) !important;
      font-size: 13px !important;
      font-weight: 600 !important;
    }

    .active-filter {
      background: color-mix(in srgb, var(--accent) 16%, transparent) !important;
      border-radius: 999px;
    }

    .empty-state {
      padding: 60px;
      text-align: center;
      color: var(--apple-secondary-text);
    }

    .modal-overlay {
      position: fixed;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background: rgba(0, 0, 0, 0.4);
      backdrop-filter: blur(4px);
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 1000;
      padding: 20px;
    }

    .modal-card {
      background: white;
      border-radius: 16px;
      width: 100%;
      max-width: 450px;
      box-shadow: 0 20px 25px -5px rgba(0,0,0,0.1), 0 10px 10px -5px rgba(0,0,0,0.04);
      overflow: hidden;
      animation: modalSlide 0.3s ease-out;
    }

    @keyframes modalSlide {
      from { transform: translateY(20px); opacity: 0; }
      to { transform: translateY(0); opacity: 1; }
    }

    .modal-header {
      padding: 24px;
      background: #fdf2f2;
      border-bottom: 1px solid #fee2e2;
      display: flex;
      align-items: center;
      gap: 12px;
    }

    .modal-header h3 {
      margin: 0;
      color: #991b1b;
      font-size: 1.125rem;
      font-weight: 600;
    }

    .modal-body {
      padding: 24px;
    }

    .policy-item {
      display: flex;
      justify-content: space-between;
      padding: 8px 0;
      border-bottom: 1px dashed #eee;
      font-size: 0.9rem;
    }

    .policy-item:last-child {
      border-bottom: none;
    }

    .modal-footer {
      padding: 16px 24px;
      background: #f9fafb;
      display: flex;
      justify-content: flex-end;
      gap: 12px;
    }
  `]
})
export class BookingHistoryComponent implements OnInit {
  bookings: BookingResponse[] = [];
  activeFilter = 'all';
  columns = ['route', 'journeyDate', 'bookingStatus', 'totalAmount', 'bookedAt', 'action'];
  
  showCancelModal = false;
  selectedBookingId: string | null = null;
  cancelReason = '';
  isCancelling = false;

  constructor(private bookingService: BookingService, private snackBar: MatSnackBar) {}

  ngOnInit(): void {
    this.load();
  }

  openCancelModal(bookingId: string): void {
    this.selectedBookingId = bookingId;
    this.cancelReason = '';
    this.showCancelModal = true;
  }

  confirmCancel(): void {
    if (!this.selectedBookingId || !this.cancelReason.trim()) return;

    this.isCancelling = true;
    this.bookingService.cancel(this.selectedBookingId, this.cancelReason).subscribe({
      next: () => {
        this.snackBar.open('Booking cancelled successfully', 'Close', { duration: 3000 });
        this.showCancelModal = false;
        this.isCancelling = false;
        this.load();
      },
      error: (err) => {
        this.snackBar.open(err.error?.message || 'Failed to cancel booking', 'Close', { duration: 3000 });
        this.isCancelling = false;
      }
    });
  }

  cancel(bookingId: string): void {
    this.openCancelModal(bookingId);
  }

  setFilter(filter: string): void {
    this.activeFilter = filter;
    this.load();
  }

  downloadTicket(bookingId: string): void {
    this.bookingService.downloadTicket(bookingId).subscribe((blob) => {
      const url = URL.createObjectURL(blob);
      const anchor = document.createElement('a');
      anchor.href = url;
      anchor.download = `ticket-${bookingId}.pdf`;
      anchor.click();
      URL.revokeObjectURL(url);
    });
  }

  private load(): void {
    this.bookingService.myBookings(this.activeFilter).subscribe((data) => (this.bookings = data));
  }
}
