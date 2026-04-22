import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { AdminService } from '../../core/services/admin.service';
import { RouteService } from '../../core/services/route.service';

@Component({
  standalone: true,
  selector: 'app-admin-dashboard',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatTabsModule,
    MatTableModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule
  ],
  template: `
    <mat-card class="page-card">
      <h2>Admin Dashboard</h2>
      <mat-tab-group>
        <mat-tab label="Pending Operators">
          <table mat-table [dataSource]="pendingOperators" style="width:100%; margin-top: 10px;">
            <ng-container matColumnDef="username"><th mat-header-cell *matHeaderCellDef>User</th><td mat-cell *matCellDef="let x">{{ x.username }}</td></ng-container>
            <ng-container matColumnDef="email"><th mat-header-cell *matHeaderCellDef>Email</th><td mat-cell *matCellDef="let x">{{ x.email }}</td></ng-container>
            <ng-container matColumnDef="vehicleNumber"><th mat-header-cell *matHeaderCellDef>Vehicle</th><td mat-cell *matCellDef="let x">{{ x.vehicleNumber }}</td></ng-container>
            <ng-container matColumnDef="action">
              <th mat-header-cell *matHeaderCellDef>Action</th>
              <td mat-cell *matCellDef="let x">
                <button mat-button color="primary" (click)="approve(x.operatorUserId)">Approve</button>
                <button mat-button color="warn" (click)="reject(x.operatorUserId)">Reject</button>
              </td>
            </ng-container>
            <tr mat-header-row *matHeaderRowDef="operatorColumns"></tr>
            <tr mat-row *matRowDef="let row; columns: operatorColumns"></tr>
          </table>
        </mat-tab>

        <mat-tab label="Users"><pre>{{ users | json }}</pre></mat-tab>
        <mat-tab label="Buses"><pre>{{ buses | json }}</pre></mat-tab>
        <mat-tab label="Bookings"><pre>{{ bookings | json }}</pre></mat-tab>
        <mat-tab label="Routes">
          <form [formGroup]="sourceForm" class="form-grid" style="margin-top: 10px;">
            <mat-form-field appearance="outline"><mat-label>Source Name</mat-label><input matInput formControlName="name" /></mat-form-field>
            <button mat-raised-button color="primary" (click)="createSource()">Create Source</button>
          </form>

          <form [formGroup]="destinationForm" class="form-grid">
            <mat-form-field appearance="outline"><mat-label>Destination Name</mat-label><input matInput formControlName="name" /></mat-form-field>
            <button mat-raised-button color="primary" (click)="createDestination()">Create Destination</button>
          </form>

          <form [formGroup]="routeForm" class="form-grid">
            <mat-form-field appearance="outline"><mat-label>Source Id</mat-label><input matInput formControlName="sourceId" /></mat-form-field>
            <mat-form-field appearance="outline"><mat-label>Destination Id</mat-label><input matInput formControlName="destinationId" /></mat-form-field>
            <button mat-raised-button color="accent" (click)="createRoute()">Create Route</button>
          </form>

          <pre>{{ routes | json }}</pre>
        </mat-tab>
      </mat-tab-group>
    </mat-card>
  `
})
export class AdminDashboardComponent implements OnInit {
  pendingOperators: any[] = [];
  users: any[] = [];
  buses: any[] = [];
  bookings: any[] = [];
  routes: any[] = [];
  operatorColumns = ['username', 'email', 'vehicleNumber', 'action'];

  sourceForm = this.fb.group({ name: ['', Validators.required] });
  destinationForm = this.fb.group({ name: ['', Validators.required] });
  routeForm = this.fb.group({
    sourceId: ['', Validators.required],
    destinationId: ['', Validators.required]
  });

  constructor(private adminService: AdminService, private routeService: RouteService, private fb: FormBuilder) {}

  ngOnInit(): void {
    this.refresh();
  }

  approve(id: string): void {
    this.adminService.approveOperator(id).subscribe(() => this.refresh());
  }

  reject(id: string): void {
    this.adminService.rejectOperator(id, 'Rejected by admin').subscribe(() => this.refresh());
  }

  createSource(): void {
    if (this.sourceForm.invalid) return;
    this.routeService.createSource(this.sourceForm.value.name ?? '').subscribe(() => this.refresh());
  }

  createDestination(): void {
    if (this.destinationForm.invalid) return;
    this.routeService.createDestination(this.destinationForm.value.name ?? '').subscribe(() => this.refresh());
  }

  createRoute(): void {
    if (this.routeForm.invalid) return;
    this.routeService.createRoute(this.routeForm.value.sourceId ?? '', this.routeForm.value.destinationId ?? '').subscribe(() => this.refresh());
  }

  private refresh(): void {
    this.adminService.getPendingOperators().subscribe((data) => (this.pendingOperators = data));
    this.adminService.getUsers().subscribe((data) => (this.users = data));
    this.adminService.getBuses().subscribe((data) => (this.buses = data));
    this.adminService.getBookings().subscribe((data) => (this.bookings = data));
    this.routeService.getRoutes().subscribe((data) => (this.routes = data));
  }
}
