import { Component } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { NgFor, NgClass, CommonModule } from '@angular/common';
import { BookingService } from '../../core/services/booking.service';
import { BusService } from '../../core/services/bus.service';

@Component({
  standalone: true,
  selector: 'app-seat-selection',
  imports: [CommonModule, MatCardModule, MatButtonModule],
  template: `
    <div class="selection-layout">
      <div class="bus-info">
        <h1>Select Seats</h1>
        <p>Bus ID: {{ busId }}</p>
      </div>

      <div class="bus-container apple-card">
        <div class="bus-front">
           <div class="steering-wheel"></div>
        </div>
        
        <div class="seats-grid">
          <div 
            *ngFor="let seat of seatIds" 
            class="seat-item"
            [ngClass]="{
              'booked': seat.isBooked,
              'selected': selected.has(seat.seatId),
              'available': !seat.isBooked
            }"
            (click)="!seat.isBooked && toggle(seat.seatId)">
            <span class="seat-label">{{ seat.seatNumber }}</span>
          </div>
        </div>
      </div>

      <div class="legend apple-card">
        <div class="legend-item"><div class="dot available"></div> Available</div>
        <div class="legend-item"><div class="dot selected"></div> Selected</div>
        <div class="legend-item"><div class="dot booked"></div> Booked</div>
      </div>

      <div class="footer-actions">
        <div class="selection-summary">
          <span class="label">Selected:</span>
          <span class="count">{{ selected.size }} Seats</span>
        </div>
        <button mat-raised-button color="accent" class="proceed-btn" [disabled]="selected.size === 0" (click)="initiateBooking()">
          Proceed to Payment
        </button>
      </div>
    </div>
  `,
  styles: [`
    .selection-layout {
      max-width: 600px;
      margin: 0 auto;
    }

    .bus-info {
      text-align: center;
      margin-bottom: 32px;
    }

    .bus-container {
      padding: 40px !important;
      position: relative;
      border-radius: 40px !important;
      background: #fff;
    }

    .bus-front {
      height: 60px;
      border-bottom: 2px solid var(--apple-secondary-bg);
      margin-bottom: 40px;
      display: flex;
      justify-content: flex-end;
      padding-right: 20px;
    }

    .steering-wheel {
      width: 30px;
      height: 30px;
      border: 3px solid var(--apple-secondary-text);
      border-radius: 50%;
    }

    .seats-grid {
      display: grid;
      grid-template-columns: repeat(4, 1fr);
      gap: 16px;
    }

    /* Gap for the aisle after 2nd column */
    .seats-grid .seat-item:nth-child(4n+2) {
      margin-right: 40px;
    }

    .seat-item {
      aspect-ratio: 1;
      border-radius: 8px;
      display: flex;
      align-items: center;
      justify-content: center;
      cursor: pointer;
      transition: all 0.2s ease;
      font-size: 12px;
      font-weight: 500;
      border: 1px solid var(--apple-border);
    }

    .seat-item.available {
      background: var(--apple-secondary-bg);
      color: var(--apple-text);
    }

    .seat-item.available:hover {
      background: #e5e5e7;
      transform: scale(1.05);
    }

    .seat-item.selected {
      background: var(--apple-blue) !important;
      color: white !important;
      border-color: var(--apple-blue);
    }

    .seat-item.booked {
      background: #eee;
      color: #ccc;
      cursor: not-allowed;
      border-style: dashed;
    }

    .legend {
      display: flex;
      justify-content: space-around;
      padding: 16px !important;
      margin-top: 24px;
    }

    .legend-item {
      display: flex;
      align-items: center;
      gap: 8px;
      font-size: 13px;
      color: var(--apple-secondary-text);
    }

    .dot {
      width: 12px;
      height: 12px;
      border-radius: 3px;
    }

    .dot.available { background: var(--apple-secondary-bg); }
    .dot.selected { background: var(--apple-blue); }
    .dot.booked { background: #eee; border: 1px dashed #ccc; }

    .footer-actions {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-top: 40px;
      background: #fff;
      padding: 20px 32px;
      border-radius: 20px;
      border: 1px solid var(--apple-border);
    }

    .selection-summary .label {
      font-size: 13px;
      color: var(--apple-secondary-text);
      display: block;
    }

    .selection-summary .count {
      font-size: 20px;
      font-weight: 600;
    }

    .proceed-btn {
      height: 44px !important;
      border-radius: 22px !important;
      padding: 0 32px !important;
    }
  `]
})
export class SeatSelectionComponent {
  busId = '';
  seatIds: { seatId: string; seatNumber: string; isBooked: boolean }[] = [];
  selected = new Set<string>();

  constructor(private route: ActivatedRoute, private bookingService: BookingService, private router: Router, private busService: BusService) {
    this.busId = this.route.snapshot.paramMap.get('busId') ?? '';
    this.busService.getSeats(this.busId).subscribe((result) => (this.seatIds = result));
  }

  toggle(seatId: string): void {
    if (this.selected.has(seatId)) this.selected.delete(seatId);
    else this.selected.add(seatId);
  }

  initiateBooking(): void {
    const seatIds = Array.from(this.selected);
    if (seatIds.length === 0) return;

    this.bookingService.initiate(this.busId, seatIds).subscribe((booking) => {
      this.router.navigate(['/payment', booking.bookingId]);
    });
  }
}
