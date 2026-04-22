import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { BusService } from '../../core/services/bus.service';
import { OperatorService } from '../../core/services/operator.service';
import { RouteService } from '../../core/services/route.service';

@Component({
  standalone: true,
  selector: 'app-operator-dashboard',
  imports: [CommonModule, ReactiveFormsModule, MatCardModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatSelectModule],
  template: `
    <mat-card class="page-card">
      <h2>Operator Dashboard</h2>

      <h3>Add Bus</h3>
      <form [formGroup]="form" class="form-grid">
        <mat-form-field appearance="outline">
          <mat-label>Route</mat-label>
          <mat-select formControlName="routeId">
            <mat-option *ngFor="let route of routes" [value]="route.id">{{ route.source }} -> {{ route.destination }}</mat-option>
          </mat-select>
        </mat-form-field>
        <mat-form-field appearance="outline"><mat-label>Bus Name</mat-label><input matInput formControlName="busName" /></mat-form-field>
        <mat-form-field appearance="outline"><mat-label>Boarding Point</mat-label><input matInput formControlName="boardingPoint" /></mat-form-field>
        <mat-form-field appearance="outline"><mat-label>Drop Point</mat-label><input matInput formControlName="dropPoint" /></mat-form-field>
        <mat-form-field appearance="outline"><mat-label>Departure Time (UTC)</mat-label><input matInput formControlName="departureTime" /></mat-form-field>
        <mat-form-field appearance="outline"><mat-label>Duration Minutes</mat-label><input matInput type="number" formControlName="durationMinutes" /></mat-form-field>
        <mat-form-field appearance="outline"><mat-label>Seat Layout</mat-label><input matInput formControlName="seatLayoutType" /></mat-form-field>
        <mat-form-field appearance="outline"><mat-label>Total Seats</mat-label><input matInput type="number" formControlName="totalSeats" /></mat-form-field>
        <mat-form-field appearance="outline"><mat-label>Base Price</mat-label><input matInput type="number" formControlName="basePrice" /></mat-form-field>
      </form>
      <div class="actions">
        <button mat-raised-button color="primary" (click)="addBus()">Add Bus</button>
      </div>

      <h3 style="margin-top:16px;">Bus Maintenance</h3>
      <form [formGroup]="maintenanceForm" class="form-grid">
        <mat-form-field appearance="outline"><mat-label>Bus Id</mat-label><input matInput formControlName="busId" /></mat-form-field>
      </form>
      <div class="actions">
        <button mat-button color="warn" (click)="disable()">Disable Temporarily</button>
        <button mat-button color="primary" (click)="enable()">Enable</button>
        <button mat-button color="warn" (click)="remove()">Remove Bus</button>
      </div>

      <h3 style="margin-top:16px;">Revenue</h3>
      <p>Total Revenue: {{ revenue.totalRevenue }}</p>
      <p>Confirmed Bookings: {{ revenue.confirmedBookings }}</p>

      <h3>Recent Bookings</h3>
      <pre>{{ bookings | json }}</pre>
    </mat-card>
  `
})
export class OperatorDashboardComponent {
  routes: { id: string; source: string; destination: string }[] = [];
  bookings: unknown[] = [];
  revenue: { totalRevenue: number; confirmedBookings: number } = { totalRevenue: 0, confirmedBookings: 0 };

  form = this.fb.group({
    routeId: ['', Validators.required],
    busName: ['', Validators.required],
    boardingPoint: ['', Validators.required],
    dropPoint: ['', Validators.required],
    departureTime: ['', Validators.required],
    durationMinutes: [0, Validators.required],
    seatLayoutType: ['2x2', Validators.required],
    totalSeats: [40, Validators.required],
    basePrice: [0, Validators.required]
  });

  maintenanceForm = this.fb.group({
    busId: ['', Validators.required]
  });

  constructor(
    private fb: FormBuilder,
    private busService: BusService,
    private operatorService: OperatorService,
    private routeService: RouteService
  ) {
    this.routeService.getRoutes().subscribe((routes) => (this.routes = routes));
    this.reloadOperatorData();
  }

  addBus(): void {
    if (this.form.invalid) return;
    this.busService.addBus(this.form.getRawValue()).subscribe();
  }

  disable(): void {
    const busId = this.maintenanceForm.value.busId;
    if (!busId) return;
    this.operatorService.disableBus(busId).subscribe(() => this.reloadOperatorData());
  }

  enable(): void {
    const busId = this.maintenanceForm.value.busId;
    if (!busId) return;
    this.operatorService.enableBus(busId).subscribe(() => this.reloadOperatorData());
  }

  remove(): void {
    const busId = this.maintenanceForm.value.busId;
    if (!busId) return;
    this.operatorService.removeBus(busId).subscribe(() => this.reloadOperatorData());
  }

  private reloadOperatorData(): void {
    this.operatorService.getBookings().subscribe((data) => (this.bookings = data));
    this.operatorService.getRevenue().subscribe((data) => (this.revenue = data));
  }
}
