import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { BusService } from '../../core/services/bus.service';
import { OperatorService } from '../../core/services/operator.service';
import { RouteService } from '../../core/services/route.service';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';

@Component({
  standalone: true,
  selector: 'app-operator-dashboard',
  imports: [CommonModule, ReactiveFormsModule, MatSnackBarModule],
  styles: [`
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

    .modal-content {
      width: 100%;
      max-width: 450px;
      padding: 32px;
      background: white;
      border-radius: 20px;
      box-shadow: 0 20px 40px rgba(0, 0, 0, 0.2);
      z-index: 1001;
    }
  `],
  template: `
    <div class="page-shell">
      <div class="hero-panel" style="padding: 32px; text-align: left; margin-bottom: 24px;">
        <div class="eyebrow">Operator Workspace</div>
        <h1 style="font-size: 2.2rem; margin-bottom: 8px;">Fleet Management</h1>
        <p>Manage your buses, view analytics, and monitor bookings.</p>
      </div>

      <div style="padding: 0; overflow: hidden;">
        <div class="auth-tabs" style="margin-bottom: 24px; max-width: 800px;">
          <div class="auth-tab" [class.active]="activeTab === 'analytics'" (click)="activeTab = 'analytics'">Overview</div>
          <div class="auth-tab" [class.active]="activeTab === 'fleet'" (click)="activeTab = 'fleet'">My Fleet</div>
          <div class="auth-tab" [class.active]="activeTab === 'offices'" (click)="activeTab = 'offices'">My Offices</div>
          <div class="auth-tab" [class.active]="activeTab === 'add-bus'" (click)="activeTab = 'add-bus'">Schedule a Route</div>
          <div class="auth-tab" [class.active]="activeTab === 'maintenance'" (click)="activeTab = 'maintenance'">Manage Schedules</div>
        </div>

        <!-- Tab 1: Analytics & Bookings -->
        <div *ngIf="activeTab === 'analytics'">
          <div class="surface-section" style="padding: 24px; display: flex; flex-direction: column; gap: 32px;">
            <div style="display: grid; grid-template-columns: repeat(auto-fit, minmax(300px, 1fr)); gap: 24px;">
              <div class="surface-section" style="border: 1px solid var(--border); box-shadow: none; background: rgba(0, 140, 255, 0.02);">
                <div style="color: var(--text-muted); font-size: 0.8rem; text-transform: uppercase; letter-spacing: 0.05em; margin-bottom: 8px;">Total Revenue</div>
                <div style="font-size: 2.5rem; font-weight: 700; color: var(--accent);">₹{{ revenue.totalRevenue }}</div>
              </div>
              <div class="surface-section" style="border: 1px solid var(--border); box-shadow: none;">
                <div style="color: var(--text-muted); font-size: 0.8rem; text-transform: uppercase; letter-spacing: 0.05em; margin-bottom: 8px;">Confirmed Bookings</div>
                <div style="font-size: 2.5rem; font-weight: 700;">{{ revenue.confirmedBookings }}</div>
              </div>
            </div>

            <div>
              <h3 style="margin-bottom: 20px;">Recent Bookings</h3>
              <div class="surface-section" style="border: 1px solid var(--border); box-shadow: none; padding: 0; overflow: hidden;">
                <table style="width: 100%; border-collapse: collapse; font-size: 0.9rem; text-align: left;">
                  <thead style="background: var(--bg-soft); border-bottom: 1px solid var(--border);">
                    <tr>
                      <th style="padding: 12px 16px;">PASSENGER</th>
                      <th style="padding: 12px 16px;">ROUTE / BUS</th>
                      <th style="padding: 12px 16px;">JOURNEY DATE</th>
                      <th style="padding: 12px 16px; text-align: right;">AMOUNT</th>
                      <th style="padding: 12px 16px; text-align: center;">STATUS</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr *ngFor="let b of bookings" style="border-bottom: 1px solid var(--border);">
                      <td style="padding: 12px 16px; font-weight: 600;">{{ b.passengerName }}</td>
                      <td style="padding: 12px 16px;">
                        <div style="font-weight: 500;">{{ b.route }}</div>
                        <div style="font-size: 0.75rem; color: var(--text-muted);">{{ b.busName }}</div>
                      </td>
                      <td style="padding: 12px 16px;">{{ b.journeyDate | date:'mediumDate' }}</td>
                      <td style="padding: 12px 16px; text-align: right; font-weight: 600;">₹{{ b.amount | number:'1.0-0' }}</td>
                      <td style="padding: 12px 16px; text-align: center;">
                        <span class="status-pill" 
                          [style.background]="b.bookingStatus === 'Confirmed' ? 'rgba(22, 163, 74, 0.1)' : 'rgba(217, 119, 6, 0.1)'"
                          [style.color]="b.bookingStatus === 'Confirmed' ? 'var(--success)' : 'var(--warning)'">
                          {{ b.bookingStatus }}
                        </span>
                      </td>
                    </tr>
                    <tr *ngIf="bookings.length === 0">
                      <td colspan="5" style="padding: 40px; text-align: center; color: var(--text-muted);">No bookings found yet.</td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>
          </div>
        </div>

        <!-- Tab: My Fleet (Vehicles) -->
        <div *ngIf="activeTab === 'fleet'">
          <div class="surface-section" style="padding: 24px; margin-bottom: 24px;">
            <h3 style="margin-bottom: 24px;">Add a New Bus to Fleet</h3>
            <form [formGroup]="vehicleForm" (ngSubmit)="addVehicle()">
              <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 24px; margin-bottom: 24px;">
                <div>
                  <label class="search-field-label">VEHICLE NUMBER</label>
                  <input class="search-input" formControlName="vehicleNumber" placeholder="e.g. KA-01-AB-1234" style="height: 48px;" />
                </div>
                <div>
                  <label class="search-field-label">BUS NAME (BRANDING)</label>
                  <input class="search-input" formControlName="busName" placeholder="e.g. Express Travels Sleeper" style="height: 48px;" />
                </div>
                <div>
                  <label class="search-field-label">SEAT LAYOUT</label>
                  <select class="search-input" formControlName="seatLayoutType" style="height: 48px; width: 100%; display: block; appearance: auto;">
                    <option value="2x2">2x2 Seater</option>
                  </select>
                </div>
                <div>
                  <label class="search-field-label">TOTAL SEATS</label>
                  <input class="search-input" type="number" formControlName="totalSeats" style="height: 48px;" />
                </div>
              </div>
              <div style="display: flex; justify-content: flex-end;">
                <button class="solid-button" type="submit" [disabled]="vehicleForm.invalid" style="height: 48px; padding: 0 40px;">Add Vehicle to Fleet</button>
              </div>
            </form>
          </div>

          <div class="surface-section" style="padding: 24px;">
            <h3 style="margin-bottom: 24px;">My Fleet ({{ vehicles.length }} Vehicles)</h3>
            <div class="results-grid">
              <div class="result-card" *ngFor="let v of vehicles">
                <div style="font-weight: 700; font-size: 1.1rem; margin-bottom: 4px;">{{ v.busName }}</div>
                <div class="muted" style="margin-bottom: 12px; font-family: monospace;">{{ v.vehicleNumber }}</div>
                <div class="chip-row" style="justify-content: space-between; align-items: center;">
                  <div style="display: flex; gap: 8px;">
                    <span class="feature-pill">{{ v.seatLayoutType }} Layout</span>
                    <span class="feature-pill">{{ v.totalSeats }} Seats</span>
                  </div>
                  <button class="ghost-button" type="button" (click)="removeVehicle(v.id)" style="height: 32px; padding: 0 12px; font-size: 0.75rem; border-color: var(--danger); color: var(--danger);">Remove</button>
                </div>
              </div>
              <div class="empty-state" *ngIf="vehicles.length === 0" style="grid-column: 1 / -1;">
                No vehicles added to your fleet yet.
              </div>
            </div>
          </div>
        </div>

        <!-- Tab 1.5: My Offices -->
        <div *ngIf="activeTab === 'offices'">
          <div class="surface-section" style="padding: 24px; margin-bottom: 24px;">
            <h3 style="margin-bottom: 24px;">Add New Office</h3>
            <form [formGroup]="officeForm" (ngSubmit)="addOffice()">
              <div style="display: grid; grid-template-columns: 1fr 2fr; gap: 24px; margin-bottom: 24px;">
                <div>
                  <label class="search-field-label">CITY</label>
                  <select class="search-input" formControlName="cityName" style="height: 48px; width: 100%; display: block; appearance: auto;">
                    <option value="" disabled selected>Select a City</option>
                    <option *ngFor="let city of availableCities" [value]="city">{{ city }}</option>
                  </select>
                </div>
                <div>
                  <label class="search-field-label">OFFICE ADDRESS (BOARDING / DROP POINT)</label>
                  <input class="search-input" formControlName="address" placeholder="e.g. Koyambedu Bus Stand" style="height: 48px;" />
                </div>
              </div>
              <div style="display: flex; justify-content: flex-end;">
                <button class="solid-button" type="submit" [disabled]="officeForm.invalid" style="height: 48px; padding: 0 40px;">Add Office</button>
              </div>
            </form>
          </div>

          <div class="surface-section" style="padding: 24px;">
            <h3 style="margin-bottom: 24px;">My Offices ({{ offices.length }})</h3>
            <div class="results-grid">
              <div class="result-card" *ngFor="let o of offices">
                <div style="display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 8px;">
                  <div style="font-weight: 700; font-size: 1.1rem;">{{ o.cityName }}</div>
                  <button class="ghost-button" type="button" (click)="removeOffice(o.id)" style="height: 24px; padding: 0 8px; font-size: 0.65rem; border-color: var(--danger); color: var(--danger);">Remove</button>
                </div>
                <div class="muted">{{ o.address }}</div>
              </div>
              <div class="empty-state" *ngIf="offices.length === 0" style="grid-column: 1 / -1;">
                No offices added yet. You must add offices for the Source and Destination cities before scheduling a bus!
              </div>
            </div>
          </div>
        </div>

        <!-- Tab 2: Schedule a Route (Add Bus) -->
        <div *ngIf="activeTab === 'add-bus'">
          <div class="surface-section" style="padding: 24px;">
            <h3 style="margin-bottom: 24px;">Schedule a Bus on a Route</h3>
            <form [formGroup]="form" (ngSubmit)="addBus()">
              <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 24px; margin-bottom: 24px;">
                <div style="grid-column: 1 / -1;">
                  <label class="search-field-label">SELECT VEHICLE FROM FLEET</label>
                  <select class="search-input" formControlName="vehicleId" style="height: 48px; width: 100%; display: block; appearance: auto;">
                    <option value="" disabled selected>Select a Vehicle</option>
                    <option *ngFor="let v of vehicles" [value]="v.id">{{ v.busName }} ({{ v.vehicleNumber }})</option>
                  </select>
                </div>
                <div>
                  <label class="search-field-label">ROUTE</label>
                  <select class="search-input" formControlName="routeId" style="height: 48px; width: 100%; display: block; appearance: auto;">
                    <option value="" disabled selected>Select Route</option>
                    <option *ngFor="let route of routes" [value]="route.id">{{ route.source }} ➔ {{ route.destination }}</option>
                  </select>
                </div>
                <!-- Boarding and Drop Points are now automatically fetched from the Operator's Offices -->
                <div style="grid-column: 1 / -1;">
                  <label class="search-field-label" style="display: block; margin-bottom: 8px;">AVAILABLE DAYS (RECURRING WEEKLY)</label>
                  <div style="display: flex; gap: 16px; flex-wrap: wrap;">
                    <label *ngFor="let day of weekDays; let i = index" style="display: flex; align-items: center; gap: 8px; cursor: pointer;">
                      <input type="checkbox" [checked]="selectedDays.has(i)" (change)="toggleDay(i)" style="width: 20px; height: 20px; cursor: pointer; accent-color: var(--accent);" />
                      <span style="font-weight: 500; color: var(--text-main);">{{ day }}</span>
                    </label>
                  </div>
                </div>
                <div>
                  <label class="search-field-label">DEPARTURE TIME</label>
                  <input class="search-input" type="time" formControlName="departureTime" style="height: 48px;" />
                </div>
                <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 16px;">
                  <div>
                    <label class="search-field-label">DURATION (HRS)</label>
                    <input class="search-input" type="number" formControlName="durationHours" min="0" style="height: 48px;" />
                  </div>
                  <div>
                    <label class="search-field-label">DURATION (MINS)</label>
                    <input class="search-input" type="number" formControlName="durationMins" min="0" max="59" style="height: 48px;" />
                  </div>
                </div>
                <div>
                  <label class="search-field-label">BASE PRICE</label>
                  <input class="search-input" type="number" formControlName="basePrice" style="height: 48px;" />
                </div>
              </div>
              <div style="display: flex; justify-content: flex-end;">
                <button class="solid-button" type="submit" [disabled]="form.invalid || selectedDays.size === 0" style="height: 48px; padding: 0 40px;">Submit Schedule for Approval</button>
              </div>
            </form>
          </div>
        </div>

        <!-- Tab 3: Maintenance -->
        <div *ngIf="activeTab === 'maintenance'">
          <div class="surface-section" style="padding: 24px;">
            <h3 style="margin-bottom: 24px;">Schedule Maintenance & Controls</h3>
            <div *ngIf="operatorBuses.length === 0" class="empty-state">
              No schedules found. Go to 'Schedule a Route' to create one.
            </div>
            <div class="results-grid" *ngIf="operatorBuses.length > 0">
              <div class="surface-section" *ngFor="let bus of operatorBuses" style="border: 1px solid var(--border); box-shadow: none;">
                <div style="display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 12px;">
                  <div>
                    <h4 style="font-size: 1.1rem; margin-bottom: 4px;">{{ bus.source }} ➔ {{ bus.destination }}</h4>
                    <div class="muted">{{ bus.busName }} • Departs: {{ bus.departureTime }}</div>
                  </div>
                  <span class="status-pill" 
                        [style.background]="bus.isMarkedForRemoval ? 'rgba(239, 68, 68, 0.1)' : (bus.approvalStatus === 'Pending' ? 'rgba(245, 158, 11, 0.1)' : ((!bus.isTemporarilyDisabled && bus.isActive) ? 'rgba(16, 185, 129, 0.1)' : 'rgba(239, 68, 68, 0.1)'))"
                        [style.color]="bus.isMarkedForRemoval ? 'var(--danger)' : (bus.approvalStatus === 'Pending' ? 'var(--warning)' : ((!bus.isTemporarilyDisabled && bus.isActive) ? 'var(--success)' : 'var(--danger)'))">
                    {{ bus.isMarkedForRemoval ? 'Retiring' : (bus.approvalStatus === 'Pending' ? 'Waiting for Approval' : (bus.approvalStatus === 'Approved' ? (bus.isTemporarilyDisabled ? 'Temporarily Disabled' : (bus.isActive ? 'Active' : 'Deactivated')) : bus.approvalStatus)) }}
                  </span>
                </div>
                <div style="margin-bottom: 16px; font-family: ui-monospace, monospace; font-size: 0.75rem; color: var(--text-muted);">
                  ID: {{ bus.busId }}
                </div>
                <div style="display: flex; gap: 8px; flex-wrap: wrap;">
                  <button *ngIf="bus.isTemporarilyDisabled" class="solid-button" type="button" (click)="enable(bus.busId)" style="height: 36px; padding: 0 16px; font-size: 0.85rem;">Enable</button>
                  <button *ngIf="!bus.isTemporarilyDisabled && !bus.isMarkedForRemoval" class="ghost-button" type="button" (click)="disable(bus.busId)" style="height: 36px; padding: 0 16px; font-size: 0.85rem; border-color: var(--warning); color: var(--warning);">Disable</button>
                  <button *ngIf="!bus.isMarkedForRemoval" class="ghost-button" type="button" (click)="remove(bus.busId)" style="height: 36px; padding: 0 16px; font-size: 0.85rem; border-color: var(--danger); color: var(--danger);">Remove</button>
                </div>

                <div *ngIf="bus.isMarkedForRemoval" style="margin-top: 12px; padding: 12px; background: rgba(239, 68, 68, 0.05); border-radius: 8px; border: 1px solid rgba(239, 68, 68, 0.1);">
                   <div style="color: var(--danger); font-size: 0.8rem; font-weight: 600;">SCHEDULED FOR REMOVAL</div>
                   <div style="font-size: 0.75rem; color: var(--text-muted);">This bus has active bookings until <strong>{{ bus.retirementDate | date:'mediumDate' }}</strong>. It will be hidden from search after this date.</div>
                </div>

                <!-- Maintenance Scheduling -->
                <div style="margin-top: 16px; padding-top: 16px; border-top: 1px dashed var(--border);">
                  <div style="font-size: 0.8rem; font-weight: 600; margin-bottom: 8px; color: var(--text-muted);">SCHEDULE MAINTENANCE</div>
                  <div style="display: grid; grid-template-columns: 1fr 1fr auto; gap: 8px; align-items: flex-end;">
                    <div>
                      <label style="font-size: 0.7rem; color: var(--text-muted);">FROM</label>
                      <input type="date" class="search-input" style="height: 32px; font-size: 0.8rem;" #mStart [value]="bus.maintenanceStart | date:'yyyy-MM-dd'" />
                    </div>
                    <div>
                      <label style="font-size: 0.7rem; color: var(--text-muted);">UNTIL</label>
                      <input type="date" class="search-input" style="height: 32px; font-size: 0.8rem;" #mEnd [value]="bus.maintenanceEnd | date:'yyyy-MM-dd'" />
                    </div>
                    <button class="solid-button" type="button" (click)="setMaintenance(bus.busId, mStart.value, mEnd.value)" style="height: 32px; padding: 0 12px; font-size: 0.75rem;">Set</button>
                  </div>
                  <div *ngIf="bus.maintenanceStart" style="margin-top: 8px; font-size: 0.75rem; color: var(--warning);">
                    * Under maintenance from {{ bus.maintenanceStart | date:'mediumDate' }} to {{ bus.maintenanceEnd | date:'mediumDate' }}
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

      </div>

      <!-- Custom Retirement Modal -->
      <div class="modal-overlay" *ngIf="showRetireModal" (click)="showRetireModal = false">
        <div class="modal-content apple-card" (click)="$event.stopPropagation()">
          <div style="text-align: center; margin-bottom: 24px;">
            <div style="width: 64px; height: 64px; background: rgba(239, 68, 68, 0.1); color: var(--danger); border-radius: 50%; display: flex; align-items: center; justify-content: center; margin: 0 auto 16px;">
              <span style="font-size: 2rem;">!</span>
            </div>
            <h2 style="margin-bottom: 8px;">
               {{ removalType === 'bus' ? 'Schedule Bus Retirement' : (removalType === 'vehicle' ? 'Remove Vehicle' : 'Remove Office') }}
            </h2>
            <p class="muted">
               {{ removalType === 'bus' ? 'You are about to remove this bus schedule from your active list.' : 
                  (removalType === 'vehicle' ? 'You are about to permanently remove this vehicle from your records.' : 
                  'You are about to remove this office location.') }}
            </p>
          </div>

          <div style="background: rgba(0, 0, 0, 0.02); padding: 16px; border-radius: 12px; margin-bottom: 24px; border: 1px solid var(--border);">
            <div *ngIf="removalType === 'bus'">
              <div style="display: flex; justify-content: space-between; margin-bottom: 12px;">
                <span class="muted">Last Booked Journey:</span>
                <span style="font-weight: 600;">{{ (targetRetirementDate | date:'mediumDate') || 'No future bookings' }}</span>
              </div>
              <div style="font-size: 0.85rem; color: var(--text-muted);">
                <span *ngIf="targetRetirementDate">
                  The bus will remain active to fulfill current bookings but will be <strong>unavailable in search results</strong> for all dates after <strong>{{ targetRetirementDate | date:'mediumDate' }}</strong>.
                </span>
                <span *ngIf="!targetRetirementDate">
                  This bus has no future bookings and will be removed <strong>immediately</strong>.
                </span>
              </div>
            </div>
            <div *ngIf="removalType === 'vehicle'">
              <div style="font-size: 0.85rem; color: var(--text-muted);">
                <strong>Important:</strong> You cannot remove a vehicle that is currently assigned to active schedules. Please ensure all schedules using this vehicle are retired or deleted first.
              </div>
            </div>
            <div *ngIf="removalType === 'office'">
              <div style="font-size: 0.85rem; color: var(--text-muted);">
                Removing an office will not affect existing schedules, but this location will no longer be available when creating new routes.
              </div>
            </div>
          </div>

          <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 12px;">
            <button class="ghost-button" (click)="showRetireModal = false">Cancel</button>
            <button class="solid-button" style="background: var(--danger);" (click)="confirmGenericRemoval()">Confirm Removal</button>
          </div>
        </div>
      </div>
    </div>
  `
})
export class OperatorDashboardComponent implements OnInit {
  activeTab = 'analytics';
  routes: { id: string; source: string; destination: string }[] = [];
  vehicles: any[] = [];
  operatorBuses: any[] = [];
  offices: any[] = [];
  bookings: any[] = [];
  revenue: { totalRevenue: number; confirmedBookings: number } = { totalRevenue: 0, confirmedBookings: 0 };

  weekDays = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];
  selectedDays = new Set([0, 1, 2, 3, 4, 5, 6]);

  showRetireModal = false;
  removalType: 'bus' | 'vehicle' | 'office' = 'bus';
  targetBusId = '';
  targetRetirementDate: Date | null = null;

  vehicleForm = this.fb.group({
    vehicleNumber: ['', Validators.required],
    busName: ['', Validators.required],
    seatLayoutType: ['2x2', Validators.required],
    totalSeats: [40, Validators.required]
  });

  form = this.fb.group({
    vehicleId: ['', Validators.required],
    routeId: ['', Validators.required],
    departureTime: ['', Validators.required],
    durationHours: [0, [Validators.required, Validators.min(0)]],
    durationMins: [0, [Validators.required, Validators.min(0), Validators.max(59)]],
    basePrice: [0, Validators.required]
  });

  officeForm = this.fb.group({
    cityName: ['', Validators.required],
    address: ['', Validators.required]
  });


  constructor(
    private fb: FormBuilder,
    private busService: BusService,
    private operatorService: OperatorService,
    private routeService: RouteService,
    private http: HttpClient,
    private snackBar: MatSnackBar
  ) {}

  private showMessage(msg: string, isError = false) {
    this.snackBar.open(msg, 'Close', {
      duration: 3000,
      horizontalPosition: 'end',
      verticalPosition: 'top',
      panelClass: isError ? ['error-snackbar'] : ['success-snackbar']
    });
  }

  ngOnInit() {
    this.routeService.getRoutes().subscribe((routes) => (this.routes = routes));
    this.reloadOperatorData();
    this.loadVehicles();
  }

  get availableCities(): string[] {
    const cities = new Set<string>();
    this.routes.forEach(r => {
      cities.add(r.source);
      cities.add(r.destination);
    });
    return Array.from(cities).sort();
  }

  loadVehicles() {
    this.http.get<any[]>(`${environment.apiUrl}/operator/vehicles`).subscribe({
      next: (data) => this.vehicles = data,
      error: (err) => console.error('Failed to load vehicles', err)
    });
  }

  addVehicle() {
    if (this.vehicleForm.invalid) return;
    this.http.post(`${environment.apiUrl}/operator/vehicles`, this.vehicleForm.value).subscribe({
      next: () => {
        this.showMessage('Vehicle added to fleet!');
        this.vehicleForm.reset({ seatLayoutType: '2x2', totalSeats: 40 });
        this.loadVehicles();
      },
      error: (err) => this.showMessage(err.error.message || 'Failed to add vehicle', true)
    });
  }

  addOffice() {
    if (this.officeForm.invalid) return;
    const { cityName, address } = this.officeForm.value;
    this.operatorService.addOffice(cityName!, address!).subscribe({
      next: () => {
        this.showMessage('Office added successfully!');
        this.officeForm.reset();
        this.reloadOperatorData();
      },
      error: (err) => this.showMessage(err.error?.message || 'Failed to add office', true)
    });
  }

  removeVehicle(vehicleId: string) {
    if (!vehicleId) return;
    this.targetBusId = vehicleId;
    this.removalType = 'vehicle';
    this.showRetireModal = true;
  }

  removeOffice(officeId: string) {
    if (!officeId) return;
    this.targetBusId = officeId;
    this.removalType = 'office';
    this.showRetireModal = true;
  }

  confirmGenericRemoval(): void {
    if (this.removalType === 'bus') {
      this.confirmRemoval();
    } else if (this.removalType === 'vehicle') {
      this.confirmVehicleRemoval();
    } else if (this.removalType === 'office') {
      this.confirmOfficeRemoval();
    }
  }

  confirmOfficeRemoval(): void {
    this.showRetireModal = false;
    this.operatorService.removeOffice(this.targetBusId).subscribe({
      next: () => {
        this.showMessage('Office removed successfully');
        this.reloadOperatorData();
      },
      error: (err) => this.showMessage(err.error?.message || 'Failed to remove office', true)
    });
  }

  confirmVehicleRemoval(): void {
    this.showRetireModal = false;
    this.operatorService.removeVehicle(this.targetBusId).subscribe({
      next: () => {
        this.showMessage('Vehicle removed from fleet');
        this.loadVehicles();
      },
      error: (err) => this.showMessage(err.error?.message || 'Failed to remove vehicle', true)
    });
  }

  toggleDay(index: number) {
    if (this.selectedDays.has(index)) {
      this.selectedDays.delete(index);
    } else {
      this.selectedDays.add(index);
    }
  }

  addBus(): void {
    if (this.form.invalid || this.selectedDays.size === 0) return;
    const rawValue = this.form.getRawValue();
    const timeSpan = `${rawValue.departureTime}:00`;
    
    const payload = {
      vehicleId: rawValue.vehicleId,
      routeId: rawValue.routeId,
      departureTime: timeSpan,
      availableDays: Array.from(this.selectedDays),
      durationMinutes: ((rawValue.durationHours ?? 0) * 60) + (rawValue.durationMins ?? 0),
      basePrice: rawValue.basePrice
    };

    this.busService.addBus(payload).subscribe({
      next: () => {
        this.form.reset({ durationHours: 0, durationMins: 0, basePrice: 0 });
        this.selectedDays = new Set([0, 1, 2, 3, 4, 5, 6]);
        this.activeTab = 'analytics';
      }
    });
  }

  disable(busId: string): void {
    if (!busId) return;
    this.operatorService.disableBus(busId).subscribe(() => {
      this.showMessage('Bus disabled temporarily');
      this.reloadOperatorData();
    });
  }

  enable(busId: string): void {
    if (!busId) return;
    this.operatorService.enableBus(busId).subscribe(() => {
      this.showMessage('Bus enabled and active');
      this.reloadOperatorData();
    });
  }

  remove(busId: string): void {
    if (!busId) return;
    
    this.targetBusId = busId;
    this.removalType = 'bus';
    const busBookings = this.bookings.filter(b => b.busId === busId && b.bookingStatus === 'Confirmed');
    if (busBookings.length > 0) {
      const maxDate = new Date(Math.max(...busBookings.map(b => new Date(b.journeyDate).getTime())));
      this.targetRetirementDate = maxDate;
    } else {
      this.targetRetirementDate = null;
    }
    
    this.showRetireModal = true;
  }

  confirmRemoval(): void {
    this.showRetireModal = false;
    this.operatorService.removeBus(this.targetBusId).subscribe(() => {
      this.showMessage('Bus removal scheduled/completed');
      this.reloadOperatorData();
    });
  }

  setMaintenance(busId: string, start: string, end: string): void {
    this.operatorService.scheduleMaintenance(busId, start || null, end || null).subscribe({
      next: () => {
        this.showMessage('Maintenance window updated');
        this.reloadOperatorData();
      },
      error: (err) => this.showMessage(err.error?.message || 'Failed to update maintenance window', true)
    });
  }

  private reloadOperatorData() {
    this.operatorService.getBookings().subscribe((b) => (this.bookings = b));
    this.operatorService.getRevenue().subscribe((r) => (this.revenue = r));
    this.operatorService.getMyBuses().subscribe((data) => (this.operatorBuses = data));
    this.operatorService.getOffices().subscribe((data) => (this.offices = data));
  }
}
