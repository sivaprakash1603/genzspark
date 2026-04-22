import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { BookingService } from '../../core/services/booking.service';
import { BookingResponse } from '../../core/models/app.models';

@Component({
  standalone: true,
  selector: 'app-booking-history',
  imports: [CommonModule, MatCardModule, MatTableModule, MatButtonModule],
  template: `
    <div class="history-layout">
      <div class="header">
        <h1>My Bookings</h1>
        <p>View and manage your travel history</p>
      </div>

      <div class="apple-card no-padding">
        <table mat-table [dataSource]="bookings" class="full-width">
          <ng-container matColumnDef="bookingId">
            <th mat-header-cell *matHeaderCellDef>Reference</th>
            <td mat-cell *matCellDef="let b" class="ref-cell">
               {{ b.bookingId.substring(0, 8) }}...
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
                *ngIf="b.bookingStatus === 'Confirmed' || b.bookingStatus === 'Initiated'">
                Cancel
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

    .empty-state {
      padding: 60px;
      text-align: center;
      color: var(--apple-secondary-text);
    }
  `]
})
export class BookingHistoryComponent implements OnInit {
  bookings: BookingResponse[] = [];
  columns = ['bookingId', 'bookingStatus', 'totalAmount', 'bookedAt', 'action'];

  constructor(private bookingService: BookingService) {}

  ngOnInit(): void {
    this.load();
  }

  cancel(bookingId: string): void {
    this.bookingService.cancel(bookingId, 'Passenger cancelled').subscribe(() => this.load());
  }

  private load(): void {
    this.bookingService.myBookings().subscribe((data) => (this.bookings = data));
  }
}
