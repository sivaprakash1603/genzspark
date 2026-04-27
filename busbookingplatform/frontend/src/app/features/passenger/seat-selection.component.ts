import { Component } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { NgFor, NgClass, CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatIconModule } from '@angular/material/icon';
import { MatRadioModule } from '@angular/material/radio';
import { BookingService } from '../../core/services/booking.service';
import { BusService } from '../../core/services/bus.service';
import { PassengerDetails, SeatResponse } from '../../core/models/app.models';

@Component({
  standalone: true,
  selector: 'app-seat-selection',
  imports: [CommonModule, FormsModule, MatCardModule, MatButtonModule, MatInputModule, MatFormFieldModule, MatSnackBarModule, MatIconModule, MatRadioModule],
  template: `
    <div class="selection-layout">
      <div class="bus-info">
        <h1>Select Seats</h1>
        <p>Bus ID: {{ busId }}</p>
      </div>

      <div class="bus-container">
        <div class="driver-cabin">
           <div class="driver-seat"></div>
        </div>
        
        <div class="cabin-divider"></div>
        
        <div class="seats-grid layout-2-2">
          <div 
            *ngFor="let seat of seatIds" 
            class="seat-item"
            [ngClass]="{
              'booked': seat.isBooked,
              'locked': seat.lockedBy === 'Other',
              'selected': selected.has(seat.seatId),
              'available': !seat.isBooked && seat.lockedBy !== 'Other'
            }"
            (click)="!seat.isBooked && seat.lockedBy !== 'Other' && toggle(seat.seatId)">
            <span class="seat-label">{{ seat.seatNumber }}</span>
          </div>
        </div>
      </div>

      <div class="legend apple-card">
        <div class="legend-item"><div class="dot available"></div> Available</div>
        <div class="legend-item"><div class="dot selected"></div> Selected</div>
        <div class="legend-item"><div class="dot locked"></div> Held by others</div>
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

      <div class="passenger-panel apple-card" *ngIf="selectedSeats.length > 0">
        <h3>Passenger Details</h3>
        <p>Add details for each selected seat.</p>

        <div class="passenger-grid" *ngFor="let seatId of selectedSeats">
          <h4>Seat {{ seatLabel(seatId) }}</h4>
          <div class="passenger-fields">
            <div>
              <label class="search-field-label">NAME</label>
              <input class="search-input" [(ngModel)]="passengerDetails[seatId].name" [ngModelOptions]="{standalone: true}" placeholder="e.g. John Doe" />
            </div>

            <div>
              <label class="search-field-label">AGE</label>
              <input class="search-input" type="number" min="1" [(ngModel)]="passengerDetails[seatId].age" [ngModelOptions]="{standalone: true}" placeholder="e.g. 25" />
            </div>

            <div>
              <label class="search-field-label">GENDER</label>
              <select class="search-input" [(ngModel)]="passengerDetails[seatId].gender" [ngModelOptions]="{standalone: true}">
                <option value="" disabled selected>Select Gender</option>
                <option value="Male">Male</option>
                <option value="Female">Female</option>
                <option value="Other">Other</option>
              </select>
            </div>
          </div>
        </div>
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
      padding: 30px;
      position: relative;
      border-radius: 40px;
      border: 4px solid #ced4da;
      background: #f8f9fa;
      width: fit-content;
      margin: 0 auto;
      box-shadow: 0 10px 30px rgba(0,0,0,0.05);
    }

    .driver-cabin {
      height: 60px;
      display: flex;
      justify-content: flex-end;
      align-items: center;
      padding-right: 10px;
    }

    .driver-seat {
      width: 40px;
      height: 40px;
      background: #e9ecef;
      border-radius: 8px 20px 20px 8px;
      position: relative;
      border: 2px solid #adb5bd;
    }

    .driver-seat::after {
      content: '';
      position: absolute;
      right: -15px;
      top: 50%;
      transform: translateY(-50%);
      width: 8px;
      height: 24px;
      background: #6c757d;
      border-radius: 4px;
    }

    .cabin-divider {
      height: 2px;
      background: #dee2e6;
      margin: 20px 0 30px 0;
      width: 100%;
    }

    .seats-grid {
      display: grid;
      gap: 12px;
      padding: 10px;
    }

    .layout-2-2 {
      grid-template-columns: 40px 40px 30px 40px 40px;
    }

    /* Skip the aisle column (col 3) */
    .layout-2-2 > :nth-child(4n+3) { grid-column-start: 4; }

    .seat-item {
      width: 40px;
      height: 40px;
      border: 2px solid #ddd;
      border-radius: 6px;
      display: flex;
      align-items: center;
      justify-content: center;
      cursor: pointer;
      transition: all 0.2s;
      background: white;
      position: relative;
    }

    /* Headrest visual */
    .seat-item::before {
      content: '';
      position: absolute;
      top: -4px;
      left: 10%;
      width: 80%;
      height: 6px;
      border-radius: 4px 4px 0 0;
      background: #e0e0e0;
      transition: all 0.2s ease;
    }

    .seat-item.available:hover {
      border-color: var(--apple-blue);
      color: var(--apple-blue);
      transform: translateY(-2px);
    }
    
    .seat-item.available:hover::before {
      background: var(--apple-blue);
    }

    .seat-item.selected {
      background: var(--success) !important;
      border-color: var(--success);
      color: white !important;
    }

    .seat-item.selected::before {
      background: #117b34; /* darker shade of success green */
    }

    .seat-item.booked {
      background: #f1f3f5;
      border-color: #dee2e6;
      color: #adb5bd;
      cursor: not-allowed;
    }

    .seat-item.booked::before {
      background: #dee2e6;
    }

    .seat-item.locked {
      background: #fff9db;
      border-color: #fab005;
      color: #f08c00;
      cursor: not-allowed;
    }

    .seat-item.locked::before {
      background: #fab005;
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
    .dot.selected { background: var(--success); }
    .dot.locked { background: #fff9db; border: 1px solid #fab005; }
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

    .passenger-panel {
      margin-top: 32px;
      padding: 32px !important;
      border-radius: 24px !important;
    }

    .passenger-panel h3 {
      margin-bottom: 8px;
      font-size: 1.5rem;
      font-weight: 700;
    }

    .passenger-panel p {
      margin-bottom: 24px;
      color: var(--text-muted);
    }

    .passenger-grid {
      background: var(--surface-raised);
      border-radius: 16px;
      padding: 24px;
      margin-bottom: 16px;
      border: 1px solid var(--border);
    }

    .passenger-grid h4 {
      margin-top: 0;
      margin-bottom: 16px;
      font-size: 1.1rem;
      font-weight: 600;
      color: var(--accent);
    }

    .passenger-fields {
      display: grid;
      grid-template-columns: 2fr 1fr 1fr;
      gap: 16px;
    }

    @media (max-width: 760px) {
      .passenger-fields {
        grid-template-columns: 1fr;
      }
    }
  `]
})
export class SeatSelectionComponent {
  busId = '';
  layoutType = 'Seater';
  seatIds: SeatResponse[] = [];
  selected = new Set<string>();
  journeyDate = '';
  passengerDetails: Record<string, { name: string; age: number | null; gender: string }> = {};

  constructor(
    private route: ActivatedRoute, 
    private busService: BusService, 
    private bookingService: BookingService, 
    private router: Router,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit() {
    this.busId = this.route.snapshot.params['busId'];
    this.journeyDate = this.route.snapshot.queryParams['date'];
    this.layoutType = this.route.snapshot.queryParams['layout'] || 'Seater';
    this.loadSeats();
  }

  loadSeats() {
    this.busService.getSeats(this.busId, this.journeyDate).subscribe((result) => {
      this.seatIds = result;
      // Auto-select seats that are already locked by 'You'
      result.forEach(seat => {
        if (seat.lockedBy === 'You' && !this.selected.has(seat.seatId)) {
          this.selected.add(seat.seatId);
          if (!this.passengerDetails[seat.seatId]) {
            this.passengerDetails[seat.seatId] = { name: '', age: null, gender: '' };
          }
        }
      });
    });
  }

  toggle(seatId: string): void {
    const seat = this.seatIds.find(x => x.seatId === seatId);
    if (!seat || seat.isBooked) return;

    if (this.selected.has(seatId)) {
      this.bookingService.unlockSeat(seatId).subscribe(() => {
        this.selected.delete(seatId);
        delete this.passengerDetails[seatId];
      });
    } else {
      // Check if already locked by someone else
      if (seat.lockedBy === 'Other') {
        this.snackBar.open('This seat is currently held by another user.', 'Close', { duration: 3000 });
        return;
      }

      this.bookingService.lockSeat(seatId, this.journeyDate).subscribe({
        next: (success) => {
          if (success) {
            this.selected.add(seatId);
            this.passengerDetails[seatId] = { name: '', age: null, gender: '' };
          } else {
            this.snackBar.open('Failed to lock seat. It might be taken.', 'Close', { duration: 3000 });
            this.loadSeats();
          }
        },
        error: () => {
          this.snackBar.open('Seat is already locked by another user.', 'Close', { duration: 3000 });
          this.loadSeats();
        }
      });
    }
  }

  initiateBooking(): void {
    const seatIds = Array.from(this.selected);
    if (seatIds.length === 0) return;

    const passengers: PassengerDetails[] = [];
    for (const seatId of seatIds) {
      const detail = this.passengerDetails[seatId];
      if (!detail || !detail.name.trim() || !detail.gender.trim() || !detail.age || detail.age <= 0) {
        return;
      }

      passengers.push({
        seatId,
        name: detail.name.trim(),
        age: detail.age,
        gender: detail.gender.trim()
      });
    }

    this.bookingService.initiate(this.busId, this.journeyDate, passengers).subscribe((booking) => {
      this.router.navigate(['/payment', booking.bookingId], { queryParams: { amount: booking.totalAmount } });
    });
  }

  get selectedSeats(): string[] {
    return Array.from(this.selected);
  }

  seatLabel(seatId: string): string {
    return this.seatIds.find((x) => x.seatId === seatId)?.seatNumber ?? seatId;
  }
}
