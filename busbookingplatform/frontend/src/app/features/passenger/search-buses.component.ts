import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { Router } from '@angular/router';
import { BusService } from '../../core/services/bus.service';
import { BusSearchResult } from '../../core/models/app.models';

@Component({
  standalone: true,
  selector: 'app-search-buses',
  imports: [CommonModule, ReactiveFormsModule, MatFormFieldModule, MatInputModule, MatButtonModule],
  template: `
    <div class="hero">
      <h1 class="animate-in">Find your next journey</h1>
      <p class="animate-in-delay">Premium bus travel across the country, simplified.</p>
    </div>

    <div class="search-bar premium-card">
      <form [formGroup]="form" (ngSubmit)="search()">
        <div class="field">
          <label>From</label>
          <input type="text" formControlName="source" placeholder="Departure City" />
        </div>
        <div class="divider"></div>
        <div class="field">
          <label>To</label>
          <input type="text" formControlName="destination" placeholder="Arrival City" />
        </div>
        <button type="submit" [disabled]="form.invalid" class="submit-btn">
          Search
        </button>
      </form>
    </div>

    <div class="results" *ngIf="buses.length > 0">
      <div class="result-card premium-card" *ngFor="let b of buses">
        <div class="bus-main">
          <div class="bus-header">
            <span class="operator">{{ b.busName }}</span>
            <span class="price">₹{{ b.totalPrice }}</span>
          </div>
          <div class="journey">
            <div class="point">
              <span class="time">{{ b.departureTime | date: 'HH:mm' }}</span>
              <span class="city">{{ b.boardingPoint }}</span>
            </div>
            <div class="path">
              <div class="line"></div>
              <span class="duration">{{ b.durationMinutes }}m</span>
            </div>
            <div class="point">
              <span class="time">--:--</span> <!-- Calculate Arrival if possible -->
              <span class="city">{{ b.dropPoint }}</span>
            </div>
          </div>
        </div>
        <div class="bus-footer">
          <div class="features">
            <span class="tag">AC</span>
            <span class="tag">WiFi</span>
            <span class="tag">Power</span>
          </div>
          <button class="book-trigger" (click)="book(b)">Select Seats</button>
        </div>
      </div>
    </div>

    <div class="empty-results" *ngIf="buses.length === 0 && searched">
       <p>No journeys found for this route.</p>
    </div>
  `,
  styles: [`
    .hero {
      text-align: center;
      margin-bottom: 64px;
    }

    .hero h1 { margin-bottom: 8px; }

    .search-bar {
      max-width: 900px;
      margin: -32px auto 64px;
      padding: 12px !important;
    }

    .search-bar form {
      display: flex;
      align-items: center;
      gap: 12px;
    }

    .field {
      flex: 1;
      padding: 8px 16px;
    }

    .field label {
      display: block;
      font-size: 10px;
      text-transform: uppercase;
      color: var(--text-muted);
      margin-bottom: 4px;
      font-weight: 700;
    }

    .field input {
      background: none;
      border: none;
      color: #fff;
      font-size: 16px;
      width: 100%;
      outline: none;
      font-family: inherit;
    }

    .divider {
      width: 1px;
      height: 40px;
      background: var(--border);
    }

    .submit-btn {
      background: #fff;
      color: #000;
      border: none;
      padding: 0 32px;
      height: 52px;
      border-radius: 8px;
      font-weight: 700;
      cursor: pointer;
      transition: transform 0.2s;
    }

    .submit-btn:hover { transform: translateY(-2px); }
    .submit-btn:disabled { opacity: 0.5; cursor: not-allowed; }

    .results {
      display: grid;
      grid-template-columns: 1fr;
      gap: 24px;
      max-width: 900px;
      margin: 0 auto;
    }

    .result-card {
      padding: 24px !important;
      display: flex;
      flex-direction: column;
      gap: 24px;
    }

    .bus-header {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      margin-bottom: 24px;
    }

    .operator { font-size: 20px; font-weight: 700; }
    .price { font-size: 24px; font-weight: 700; color: #fff; }

    .journey {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 32px;
    }

    .point { display: flex; flex-direction: column; gap: 4px; }
    .time { font-size: 24px; font-weight: 600; }
    .city { font-size: 14px; color: var(--text-muted); }

    .path {
      flex: 1;
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 8px;
    }

    .line {
      width: 100%;
      height: 2px;
      background: var(--border);
      position: relative;
    }

    .line::before, .line::after {
      content: '';
      position: absolute;
      top: -3px;
      width: 8px;
      height: 8px;
      background: var(--border);
      border-radius: 50%;
    }
    .line::before { left: 0; }
    .line::after { right: 0; background: var(--accent); }

    .duration { font-size: 12px; color: var(--text-muted); }

    .bus-footer {
      display: flex;
      justify-content: space-between;
      align-items: center;
      border-top: 1px solid var(--border);
      padding-top: 24px;
    }

    .features { display: flex; gap: 8px; }
    .tag {
      font-size: 11px;
      padding: 4px 10px;
      background: var(--surface-raised);
      border: 1px solid var(--border);
      border-radius: 4px;
      color: var(--text-muted);
    }

    .book-trigger {
      background: var(--accent);
      color: #fff;
      border: none;
      padding: 10px 24px;
      border-radius: 6px;
      font-weight: 600;
      cursor: pointer;
      transition: all 0.2s;
    }

    .book-trigger:hover { background: #0081ff; transform: scale(1.05); }

    .animate-in { animation: fadeInDown 0.6s ease-out forwards; }
    .animate-in-delay { animation: fadeInDown 0.6s 0.2s ease-out forwards; opacity: 0; }

    @keyframes fadeInDown {
      from { opacity: 0; transform: translateY(-20px); }
      to { opacity: 1; transform: translateY(0); }
    }
  `]
})
export class SearchBusesComponent {
  form = this.fb.group({
    source: ['', Validators.required],
    destination: ['', Validators.required]
  });
  buses: BusSearchResult[] = [];
  searched = false;

  constructor(private fb: FormBuilder, private busService: BusService, private router: Router) {}

  search(): void {
    if (this.form.invalid) return;
    this.searched = true;
    this.busService.search(this.form.value.source!, this.form.value.destination!).subscribe((result) => (this.buses = result));
  }

  book(bus: BusSearchResult): void {
    this.router.navigate(['/seat-selection', bus.busId]);
  }
}
