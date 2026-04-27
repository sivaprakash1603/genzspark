import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { BusService } from '../../core/services/bus.service';
import { RouteService } from '../../core/services/route.service';
import { BusSearchResult } from '../../core/models/app.models';
import { OnInit } from '@angular/core';

@Component({
  standalone: true,
  selector: 'app-home',
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <section class="hero-panel">
      <div class="hero-copy">
        <div class="eyebrow">Travel made easy</div>
        <h1>Book intercity bus trips with a cleaner, calmer experience.</h1>
        <p>
          Discover routes, compare fares, and reserve seats in a simple flow inspired by BookMyTrip.
        </p>

        <div class="search-panel" style="margin-top: 32px; text-align: left;">
          <form [formGroup]="form" (ngSubmit)="search()">
            <div class="search-grid">
              <div>
                <label class="search-field-label">FROM</label>
                <input class="search-input" formControlName="source" placeholder="Leaving from" />
              </div>
              <div>
                <label class="search-field-label">TO</label>
                <input class="search-input" formControlName="destination" placeholder="Going to" />
              </div>
              <div>
                <label class="search-field-label">DATE</label>
                <input class="search-input" type="date" formControlName="date" />
              </div>
              <div>
                <button class="solid-button" type="submit" [disabled]="form.invalid" style="width: 100%; height: 52px; font-size: 1.05rem;">Search Buses</button>
              </div>
            </div>
          </form>
          <div class="chip-row" style="margin-top: 20px;">
            <span class="feature-pill">AC coaches</span>
            <span class="feature-pill">Verified operators</span>
            <span class="feature-pill">Live seat map</span>
          </div>
        </div>

        <div class="hero-stats">
          <div class="stat-card">
            <strong>1000+</strong>
            <span>Routes</span>
          </div>
          <div class="stat-card">
            <strong>Instant</strong>
            <span>Tickets</span>
          </div>
          <div class="stat-card">
            <strong>Secure</strong>
            <span>Payments</span>
          </div>
        </div>
      </div>
    </section>

    <section class="layout-stack" style="margin-top: 24px;">
      <div class="section-title">
        <div>
          <h2>Popular routes</h2>
          <p>Quick jumps that work well for demo and testing.</p>
        </div>
      </div>

      <div class="results-grid">
        <article class="surface-section" *ngFor="let route of featuredRoutes" 
          (click)="selectRoute(route)"
          style="cursor: pointer; transition: all 0.2s ease; border: 1px solid var(--surface-border);"
          onmouseover="this.style.borderColor='var(--accent)'; this.style.transform='translateY(-4px)'"
          onmouseout="this.style.borderColor='var(--surface-border)'; this.style.transform='translateY(0)'">
          <div class="route-row">
            <div>
              <div class="muted">Departure</div>
              <h3>{{ route.from }}</h3>
            </div>
            <div class="route-divider"></div>
            <div style="text-align: right;">
              <div class="muted">Arrival</div>
              <h3>{{ route.to }}</h3>
            </div>
          </div>
          <div class="actions" style="margin-top: 16px;">
            <span class="route-pill">{{ route.duration }}</span>
            <span class="route-pill">{{ route.price }}</span>
            <span style="color: var(--accent); font-size: 0.8rem; font-weight: 600; margin-left: auto;">Book Now →</span>
          </div>
        </article>
      </div>

      <div class="results-grid" *ngIf="buses.length > 0">
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
              <div class="muted">{{ b.departureTime | date: 'HH:mm' }}</div>
              <h3>{{ b.boardingPoint }}</h3>
            </div>
            <div class="route-divider"></div>
            <div style="text-align: right;">
              <div class="muted">{{ b.durationMinutes }} mins</div>
              <h3>{{ b.dropPoint }}</h3>
            </div>
          </div>
          <div class="actions" style="justify-content: space-between; align-items: center; margin-top: 0;">
            <div class="chip-row">
              <span class="feature-pill">{{ b.source }}</span>
              <span class="feature-pill">{{ b.destination }}</span>
            </div>
            <button class="solid-button" type="button" (click)="book(b)">Select seats</button>
          </div>
        </article>
      </div>
    </section>
  `
})
export class HomeComponent implements OnInit {
  form = this.fb.group({
    source: ['', Validators.required],
    destination: ['', Validators.required],
    date: [new Date().toISOString().slice(0, 10), Validators.required]
  });

  buses: BusSearchResult[] = [];
  featuredRoutes: any[] = [];

  constructor(
    private fb: FormBuilder, 
    private busService: BusService, 
    private routeService: RouteService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.routeService.getRoutes().subscribe(routes => {
      this.featuredRoutes = routes.map(r => ({
        from: r.source,
        to: r.destination,
        duration: 'Direct',
        price: 'Best Fares'
      }));
    });
  }

  selectRoute(route: any): void {
    this.form.patchValue({
      source: route.from,
      destination: route.to
    });
    window.scrollTo({ top: 0, behavior: 'smooth' });
    // Small delay to ensure scroll happens first
    setTimeout(() => {
      document.querySelector<HTMLInputElement>('input[type="date"]')?.showPicker();
    }, 500);
  }

  search(): void {
    if (this.form.invalid) {
      return;
    }
    
    this.router.navigate(['/search'], { 
      queryParams: { 
        source: this.form.value.source, 
        destination: this.form.value.destination, 
        date: this.form.value.date 
      } 
    });
  }

  book(bus: BusSearchResult): void {
    this.router.navigate(['/seat-selection', bus.busId], {
      queryParams: { 
        date: this.form.value.date,
        layout: bus.seatLayoutType
      }
    });
  }
}
