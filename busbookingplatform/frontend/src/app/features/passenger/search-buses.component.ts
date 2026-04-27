import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { BusService } from '../../core/services/bus.service';
import { BusSearchResult } from '../../core/models/app.models';

@Component({
  standalone: true,
  selector: 'app-search-buses',
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <section class="hero-panel">
      <div class="hero-copy">
        <div class="eyebrow">Search journeys</div>
        <h1>Find buses without the clutter.</h1>
        <p>Enter the city pair and browse live results with a calmer BookMyTrip-style layout.</p>

        <div class="search-panel" style="margin-top: 32px; text-align: left;">
          <form [formGroup]="form" (ngSubmit)="search()">
            <div class="search-grid">
              <div>
                <label class="search-field-label">FROM</label>
                <input class="search-input" formControlName="source" placeholder="Leaving from" list="location-list" />
              </div>
              <div>
                <label class="search-field-label">TO</label>
                <input class="search-input" formControlName="destination" placeholder="Going to" list="location-list" />
              </div>

              <datalist id="location-list">
                <option *ngFor="let loc of locations" [value]="loc.name">{{ loc.name }}</option>
              </datalist>
              <div>
                <label class="search-field-label">DATE</label>
                <input class="search-input" type="date" formControlName="date" />
              </div>
              <div>
                <button class="solid-button" type="submit" [disabled]="form.invalid" style="width: 100%; height: 52px; font-size: 1.05rem;">Search</button>
              </div>
            </div>
          </form>
        </div>
      </div>
    </section>

    <section class="layout-stack" style="margin-top: 24px;" *ngIf="searched">
      <div class="section-title">
        <div>
          <h2>Search results</h2>
          <p>{{ buses.length > 0 ? 'Select a trip to continue to seat selection.' : 'No journeys found for this route.' }}</p>
        </div>
      </div>

      <div class="results-grid" *ngIf="buses.length > 0; else emptyState">
        <article class="result-card glass-card" *ngFor="let b of buses">
          <div class="section-title" style="margin-bottom: 0;">
            <div>
              <div class="muted">{{ b.boardingPoint }}</div>
              <h3>{{ b.busName }}</h3>
            </div>
            <strong style="font-size: 1.4rem;">₹{{ b.totalPrice }}</strong>
          </div>

          <div class="route-row">
            <div>
              <div class="muted">{{ b.departureTime | date: 'HH:mm':'UTC' }}</div>
              <h3 style="margin: 0;">{{ b.source }}</h3>
              <div class="muted" style="font-size: 0.8rem;">{{ b.boardingPoint }}</div>
            </div>
            <div class="route-divider" style="flex-direction: column; width: 60px;">
               <div style="font-size: 0.75rem; color: var(--text-muted); text-align: center; white-space: nowrap;">{{ formatDuration(b.durationMinutes) }}</div>
               <div style="height: 1px; background: var(--border); width: 100%; margin: 4px 0;"></div>
            </div>
            <div style="text-align: right;">
              <div class="muted">{{ b.arrivalTime | date: 'HH:mm':'UTC' }}</div>
              <h3 style="margin: 0;">{{ b.destination }}</h3>
              <div class="muted" style="font-size: 0.8rem;">{{ b.dropPoint }}</div>
            </div>
          </div>

          <div class="actions" style="justify-content: space-between; align-items: center; margin-top: 0;">
            <div class="chip-row">
              <span class="feature-pill">{{ b.source }}</span>
              <span class="feature-pill">{{ b.destination }}</span>
              <span class="feature-pill">AC</span>
            </div>
            <button class="solid-button" type="button" (click)="book(b)">Select seats</button>
          </div>
        </article>
      </div>

      <ng-template #emptyState>
        <div class="empty-state">No journeys found for this route.</div>
      </ng-template>
    </section>
  `,
})
export class SearchBusesComponent {
  form = this.fb.group({
    source: ['', Validators.required],
    destination: ['', Validators.required],
    date: [new Date().toISOString().slice(0, 10), Validators.required]
  });
  buses: BusSearchResult[] = [];
  locations: { id: string; name: string }[] = [];
  searched = false;

  constructor(
    private fb: FormBuilder, 
    private busService: BusService, 
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit() {
    this.busService.getLocations().subscribe(data => this.locations = data);
    
    // Single source of truth: URL query params
    this.route.queryParams.subscribe(params => {
      if (params['source'] && params['destination'] && params['date']) {
        this.form.patchValue({
          source: params['source'],
          destination: params['destination'],
          date: params['date']
        });
        this.performSearch();
      }
    });
  }

  formatDuration(mins: number): string {
    const h = Math.floor(mins / 60);
    const m = mins % 60;
    return h > 0 ? `${h}h ${m}m` : `${m}m`;
  }

  search(): void {
    if (this.form.invalid) return;
    
    // Just update the URL - the subscription in ngOnInit will handle the rest
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: {
        source: this.form.value.source,
        destination: this.form.value.destination,
        date: this.form.value.date
      },
      queryParamsHandling: 'merge'
    });
  }

  private performSearch(): void {
    this.searched = true;
    this.busService.search(
      this.form.value.source!, 
      this.form.value.destination!, 
      this.form.value.date!
    ).subscribe((result) => (this.buses = result));
  }

  book(b: BusSearchResult): void {
    // Use form value if available, otherwise fallback to query params
    const date = this.form.value.date || this.route.snapshot.queryParams['date'];
    this.router.navigate(['/seat-selection', b.busId], {
      queryParams: { 
        date: date,
        layout: b.seatLayoutType
      }
    });
  }
}
