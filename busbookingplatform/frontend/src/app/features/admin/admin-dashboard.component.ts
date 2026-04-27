import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
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
    MatInputModule,
    MatIconModule
  ],
  template: `
    <div class="page-shell">
      <div class="hero-panel" style="padding: 32px; text-align: left; margin-bottom: 24px;">
        <div class="eyebrow">Administration</div>
        <h1 style="font-size: 2.2rem; margin-bottom: 8px;">Workspace Overview</h1>
        <p>Manage operators, configure routes, and monitor platform data.</p>
      </div>

      <div style="padding: 0; overflow: hidden;">
        <div class="auth-tabs" style="margin-bottom: 24px; max-width: 600px;">
          <div class="auth-tab" [class.active]="activeTab === 'overview'" (click)="activeTab = 'overview'">Business Overview</div>
          <div class="auth-tab" [class.active]="activeTab === 'operators'" (click)="activeTab = 'operators'">Operators</div>
          <div class="auth-tab" [class.active]="activeTab === 'schedules'" (click)="activeTab = 'schedules'">Schedules</div>
          <div class="auth-tab" [class.active]="activeTab === 'routes'" (click)="activeTab = 'routes'">Routes</div>
          <div class="auth-tab" [class.active]="activeTab === 'data'" (click)="activeTab = 'data'">Logs & Registry</div>
        </div>

        <!-- Tab 0: Business Overview -->
        <div *ngIf="activeTab === 'overview'">
          <div class="surface-section" style="padding: 24px; display: flex; flex-direction: column; gap: 32px;">
            
            <!-- Stats Cards -->
            <div style="display: grid; grid-template-columns: repeat(auto-fit, minmax(240px, 1fr)); gap: 20px;">
              <div class="apple-card" style="padding: 24px; border-left: 4px solid var(--success);">
                <div class="muted" style="font-size: 0.8rem; margin-bottom: 8px;">TOTAL REVENUE</div>
                <div style="font-size: 1.8rem; font-weight: 700; color: var(--text-main);">₹{{ stats?.totalRevenue | number:'1.0-0' }}</div>
                <div style="font-size: 0.75rem; color: var(--success); margin-top: 8px;">↑ 12% from last month</div>
              </div>
              <div class="apple-card" style="padding: 24px; border-left: 4px solid var(--danger);">
                <div class="muted" style="font-size: 0.8rem; margin-bottom: 8px;">TOTAL REFUNDS</div>
                <div style="font-size: 1.8rem; font-weight: 700; color: var(--text-main);">₹{{ stats?.totalRefunds | number:'1.0-0' }}</div>
                <div style="font-size: 0.75rem; color: var(--text-muted); margin-top: 8px;">{{ ((stats?.totalRefunds / stats?.totalRevenue) * 100) | number:'1.1-1' }}% refund rate</div>
              </div>
              <div class="apple-card" style="padding: 24px; border-left: 4px solid var(--apple-blue);">
                <div class="muted" style="font-size: 0.8rem; margin-bottom: 8px;">NET REVENUE</div>
                <div style="font-size: 1.8rem; font-weight: 700; color: var(--apple-blue);">₹{{ stats?.netRevenue | number:'1.0-0' }}</div>
                <div style="font-size: 0.75rem; color: var(--text-muted); margin-top: 8px;">Platform earnings</div>
              </div>
              <div class="apple-card" style="padding: 24px; border-left: 4px solid var(--warning);">
                <div class="muted" style="font-size: 0.8rem; margin-bottom: 8px;">ACTIVE FLEET</div>
                <div style="font-size: 1.8rem; font-weight: 700; color: var(--text-main);">{{ stats?.activeBuses }}</div>
                <div style="font-size: 0.75rem; color: var(--text-muted); margin-top: 8px;">Approved & active buses</div>
              </div>
            </div>

            <div style="display: grid; grid-template-columns: 2fr 1fr; gap: 32px;">
              <!-- Top Routes -->
              <div class="apple-card" style="padding: 24px;">
                <h3 style="margin-bottom: 20px;">Top Performing Routes</h3>
                <table style="width: 100%; border-collapse: collapse;">
                  <thead style="text-align: left; border-bottom: 1px solid var(--border);">
                    <tr>
                      <th style="padding: 12px 0; font-size: 0.8rem; color: var(--text-muted);">ROUTE</th>
                      <th style="padding: 12px 0; font-size: 0.8rem; color: var(--text-muted); text-align: right;">BOOKINGS</th>
                      <th style="padding: 12px 0; font-size: 0.8rem; color: var(--text-muted); text-align: right;">REVENUE</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr *ngFor="let r of stats?.topRoutes" style="border-bottom: 1px solid var(--bg-soft);">
                      <td style="padding: 16px 0; font-weight: 600;">{{ r.routeName }}</td>
                      <td style="padding: 16px 0; text-align: right;">{{ r.bookingCount }}</td>
                      <td style="padding: 16px 0; text-align: right; color: var(--success); font-weight: 600;">₹{{ r.revenue | number:'1.0-0' }}</td>
                    </tr>
                  </tbody>
                </table>
              </div>

              <!-- Quick Stats -->
              <div style="display: flex; flex-direction: column; gap: 20px;">
                 <div class="apple-card" style="padding: 20px; background: var(--bg-soft);">
                    <div class="muted" style="font-size: 0.75rem; margin-bottom: 4px;">TOTAL USERS</div>
                    <div style="font-size: 1.4rem; font-weight: 700;">{{ stats?.totalUsers }}</div>
                 </div>
                 <div class="apple-card" style="padding: 20px; background: var(--bg-soft);">
                    <div class="muted" style="font-size: 0.75rem; margin-bottom: 4px;">TOTAL OPERATORS</div>
                    <div style="font-size: 1.4rem; font-weight: 700;">{{ stats?.totalOperators }}</div>
                 </div>
                 <div class="apple-card" style="padding: 20px; background: var(--bg-soft);">
                    <div class="muted" style="font-size: 0.75rem; margin-bottom: 4px;">TOTAL BOOKINGS</div>
                    <div style="font-size: 1.4rem; font-weight: 700;">{{ stats?.totalBookings }}</div>
                 </div>
              </div>
            </div>

          </div>
        </div>

        <!-- Tab 1: Pending Operators -->
        <div *ngIf="activeTab === 'operators'">
          <div class="surface-section" style="padding: 24px;">
            <div *ngIf="pendingOperators.length === 0" class="empty-state">
              <div style="font-size: 2.5rem; margin-bottom: 16px; opacity: 0.5;">📋</div>
              <h3 style="margin-bottom: 8px;">All caught up!</h3>
              <p>No pending operator requests at the moment.</p>
            </div>

            <div class="results-grid" *ngIf="pendingOperators.length > 0">
              <article class="surface-section" *ngFor="let op of pendingOperators" style="border: 1px solid var(--border); box-shadow: none;">
                <div style="display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 16px;">
                  <div>
                    <h3 style="margin-bottom: 4px;">{{ op.username }}</h3>
                    <div class="muted">{{ op.email }}</div>
                  </div>
                  <span class="status-pill" style="background: rgba(217, 119, 6, 0.1); color: var(--warning); border-color: rgba(217, 119, 6, 0.2);">Pending</span>
                </div>

                <div class="actions" style="display: grid; grid-template-columns: 1fr 1fr;">
                  <button class="solid-button" style="height: 44px; font-size: 0.95rem; width: 100%;" (click)="approve(op.operatorUserId)">Approve</button>
                  <button class="ghost-button" style="height: 44px; border-color: var(--danger); color: var(--danger); width: 100%;" (click)="reject(op.operatorUserId)">Reject</button>
                </div>
              </article>
            </div>
          </div>
        </div>

        <!-- Tab 1.5: Pending Schedules -->
        <div *ngIf="activeTab === 'schedules'">
          <div class="surface-section" style="padding: 24px;">
            <div *ngIf="pendingBuses.length === 0" class="empty-state">
              <div style="font-size: 2.5rem; margin-bottom: 16px; opacity: 0.5;">🚌</div>
              <h3 style="margin-bottom: 8px;">All caught up!</h3>
              <p>No pending bus schedule requests.</p>
            </div>

            <div class="results-grid" *ngIf="pendingBuses.length > 0">
              <article class="surface-section" *ngFor="let bus of pendingBuses" style="border: 1px solid var(--border); box-shadow: none;">
                <div style="display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 16px;">
                  <div>
                    <h3 style="margin-bottom: 4px;">{{ bus.source }} ➔ {{ bus.destination }}</h3>
                    <div class="muted">{{ bus.busName }}</div>
                  </div>
                  <span class="status-pill" style="background: rgba(217, 119, 6, 0.1); color: var(--warning); border-color: rgba(217, 119, 6, 0.2);">Pending</span>
                </div>
                
                <div style="margin-bottom: 24px; background: var(--bg-soft); padding: 12px; border-radius: 8px; display: grid; grid-template-columns: 1fr 1fr 1fr; gap: 12px;">
                  <div>
                    <div class="muted" style="font-size: 0.75rem; text-transform: uppercase; letter-spacing: 0.05em; margin-bottom: 4px;">Departure</div>
                    <div style="font-weight: 600; color: var(--text-main);">{{ bus.departureTime }}</div>
                  </div>
                  <div>
                    <div class="muted" style="font-size: 0.75rem; text-transform: uppercase; letter-spacing: 0.05em; margin-bottom: 4px;">Duration</div>
                    <div style="font-weight: 600; color: var(--text-main);">{{ formatDuration(bus.durationMinutes) }}</div>
                  </div>
                  <div>
                    <div class="muted" style="font-size: 0.75rem; text-transform: uppercase; letter-spacing: 0.05em; margin-bottom: 4px;">Price</div>
                    <div style="font-weight: 600; color: var(--text-main);">₹{{ bus.totalPrice }}</div>
                  </div>
                </div>

                <div class="actions" style="display: grid; grid-template-columns: 1fr 1fr;">
                  <button class="solid-button" style="height: 44px; font-size: 0.95rem; width: 100%;" (click)="approveBus(bus.id)">Approve</button>
                  <button class="ghost-button" style="height: 44px; border-color: var(--danger); color: var(--danger); width: 100%;" (click)="rejectBus(bus.id)">Reject</button>
                </div>
              </article>
            </div>

            <!-- All Schedules Table -->
            <div style="margin-top: 40px;">
              <h3 style="margin-bottom: 20px;">All Bus Schedules</h3>
              <div class="surface-section" style="border: 1px solid var(--border); box-shadow: none; padding: 0; overflow: hidden;">
                <table style="width: 100%; border-collapse: collapse; font-size: 0.9rem; text-align: left;">
                  <thead style="background: var(--bg-soft); border-bottom: 1px solid var(--border);">
                    <tr>
                      <th style="padding: 12px 16px;">Bus/Operator</th>
                      <th style="padding: 12px 16px;">Route</th>
                      <th style="padding: 12px 16px;">Departure</th>
                      <th style="padding: 12px 16px; text-align: right;">Price</th>
                      <th style="padding: 12px 16px; text-align: center;">Status</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr *ngFor="let b of buses" style="border-bottom: 1px solid var(--border);">
                      <td style="padding: 12px 16px;">
                        <div style="font-weight: 600;">{{ b.busName }}</div>
                        <div style="font-size: 0.75rem; color: var(--text-muted);">{{ b.id.substring(0, 8) }}</div>
                      </td>
                      <td style="padding: 12px 16px;">{{ b.source }} ➔ {{ b.destination }}</td>
                      <td style="padding: 12px 16px;">{{ b.departureTime }}</td>
                      <td style="padding: 12px 16px; text-align: right; font-weight: 600;">₹{{ b.totalPrice }}</td>
                      <td style="padding: 12px 16px; text-align: center;">
                        <span class="status-pill" 
                          [style.background]="b.approvalStatus === 'Approved' ? 'rgba(22, 163, 74, 0.1)' : (b.approvalStatus === 'Rejected' ? 'rgba(220, 38, 38, 0.1)' : 'rgba(217, 119, 6, 0.1)')"
                          [style.color]="b.approvalStatus === 'Approved' ? 'var(--success)' : (b.approvalStatus === 'Rejected' ? 'var(--danger)' : 'var(--warning)')">
                          {{ b.approvalStatus }}
                        </span>
                      </td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>
          </div>
        </div>

        <!-- Tab 2: Routes -->
        <div *ngIf="activeTab === 'routes'">
          <div class="surface-section" style="padding: 24px; display: flex; flex-direction: column; gap: 32px;">
            
            <!-- Create Quick Route -->
            <div class="surface-section" style="border: 1px solid var(--border); box-shadow: none; background: rgba(0, 140, 255, 0.02);">
              <h3 style="margin-bottom: 20px;">Create Route Connection</h3>
              <form [formGroup]="quickRouteForm" (ngSubmit)="createQuickRoute()">
                <div style="display: grid; grid-template-columns: 1fr 1fr auto; gap: 16px; align-items: end;">
                  <div>
                    <label class="search-field-label">SOURCE CITY</label>
                    <input class="search-input" formControlName="sourceName" placeholder="e.g. Bangalore" style="height: 48px;" />
                  </div>
                  <div>
                    <label class="search-field-label">DESTINATION CITY</label>
                    <input class="search-input" formControlName="destinationName" placeholder="e.g. Hyderabad" style="height: 48px;" />
                  </div>
                  <div>
                    <button class="solid-button" type="submit" [disabled]="quickRouteForm.invalid" style="height: 48px; padding: 0 32px;">Connect Route</button>
                  </div>
                </div>
              </form>
            </div>

            <!-- Existing Routes -->
            <div>
              <h3 style="margin-bottom: 16px;">Active Routes</h3>
              <div *ngIf="routes.length === 0" class="empty-state" style="padding: 32px;">
                No active routes in the network.
              </div>
              <div class="results-grid" *ngIf="routes.length > 0">
                <div class="surface-section" *ngFor="let r of routes" style="border: 1px solid var(--border); box-shadow: none; padding: 20px;">
                  <div class="route-row" style="margin: 0;">
                    <div>
                      <h4 style="font-size: 1.1rem; margin-bottom: 4px;">{{ r.source }}</h4>
                    </div>
                    <div class="route-divider"></div>
                    <div style="text-align: right;">
                      <h4 style="font-size: 1.1rem; margin-bottom: 4px;">{{ r.destination }}</h4>
                    </div>
                  </div>
                  <div style="margin-top: 16px; font-family: ui-monospace, monospace; font-size: 0.75rem; color: var(--text-muted); text-align: center; background: var(--bg-soft); padding: 6px; border-radius: 6px;">
                    {{ r.id }}
                  </div>
                </div>
              </div>
            </div>

          </div>
        </div>

        <!-- Tab 3: System Data -->
        <div *ngIf="activeTab === 'data'">
          <div class="surface-section" style="padding: 24px; display: flex; flex-direction: column; gap: 32px;">
            
            <!-- Email Logs Table -->
            <div>
              <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px;">
                <h3 style="margin: 0;">System Email Logs</h3>
                <span style="font-size: 0.75rem; color: var(--text-muted);">Showing last 100 entries</span>
              </div>
              <div class="surface-section" style="border: 1px solid var(--border); box-shadow: none; padding: 0; overflow: hidden;">
                <table style="width: 100%; border-collapse: collapse; font-size: 0.9rem; text-align: left;">
                  <thead style="background: var(--bg-soft); border-bottom: 1px solid var(--border);">
                    <tr>
                      <th style="padding: 12px 16px;">Recipient</th>
                      <th style="padding: 12px 16px;">Subject</th>
                      <th style="padding: 12px 16px;">Template</th>
                      <th style="padding: 12px 16px;">Status</th>
                      <th style="padding: 12px 16px;">Time</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr *ngFor="let log of systemLogs" style="border-bottom: 1px solid var(--border);">
                      <td style="padding: 12px 16px; font-weight: 500;">{{ log.toEmail }}</td>
                      <td style="padding: 12px 16px; color: var(--text-muted);">{{ log.subject }}</td>
                      <td style="padding: 12px 16px;">
                        <span style="font-family: monospace; font-size: 0.8rem; background: #f1f3f5; padding: 2px 6px; border-radius: 4px;">{{ log.templateKey }}</span>
                      </td>
                      <td style="padding: 12px 16px;">
                        <span class="status-pill" [style.background]="log.status === 'Sent' ? 'rgba(16, 185, 129, 0.1)' : 'rgba(239, 68, 68, 0.1)'" 
                              [style.color]="log.status === 'Sent' ? '#10b981' : '#ef4444'">
                          {{ log.status }}
                        </span>
                      </td>
                      <td style="padding: 12px 16px; color: var(--text-muted); font-size: 0.8rem;">{{ log.createdAt | date:'short' }}</td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>

            <!-- Users Registry -->
            <div>
              <h3 style="margin-bottom: 20px;">Platform Users</h3>
              <div class="surface-section" style="border: 1px solid var(--border); box-shadow: none; padding: 0; overflow: hidden;">
                <table style="width: 100%; border-collapse: collapse; font-size: 0.9rem; text-align: left;">
                  <thead style="background: var(--bg-soft); border-bottom: 1px solid var(--border);">
                    <tr>
                      <th style="padding: 12px 16px;">User</th>
                      <th style="padding: 12px 16px;">Email</th>
                      <th style="padding: 12px 16px;">Role</th>
                      <th style="padding: 12px 16px;">Joined</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr *ngFor="let u of users" style="border-bottom: 1px solid var(--border);">
                      <td style="padding: 12px 16px; font-weight: 500;">{{ u.username }}</td>
                      <td style="padding: 12px 16px; color: var(--text-muted);">{{ u.email }}</td>
                      <td style="padding: 12px 16px;">
                        <span class="status-pill" style="background: rgba(0, 140, 255, 0.1); color: var(--apple-blue);">{{ u.role }}</span>
                      </td>
                      <td style="padding: 12px 16px; color: var(--text-muted); font-size: 0.8rem;">{{ u.createdAt | date:'mediumDate' }}</td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>

          </div>
        </div>
      </div>
    </div>
  `
})
export class AdminDashboardComponent implements OnInit {
  activeTab = 'overview';
  pendingOperators: any[] = [];
  pendingBuses: any[] = [];
  users: any[] = [];
  buses: any[] = [];
  bookings: any[] = [];
  routes: any[] = [];
  systemLogs: any[] = [];
  stats: any = null;
  operatorColumns = ['username', 'email', 'action'];
  routeColumns = ['id', 'source', 'destination'];

  quickRouteForm = this.fb.group({
    sourceName: ['', Validators.required],
    destinationName: ['', Validators.required]
  });

  constructor(private adminService: AdminService, private routeService: RouteService, private fb: FormBuilder) { }

  ngOnInit(): void {
    this.refresh();
  }

  formatDuration(totalMinutes: number): string {
    if (!totalMinutes) return '0h 0m';
    const h = Math.floor(totalMinutes / 60);
    const m = totalMinutes % 60;
    return `${h}h ${m}m`;
  }

  approve(id: string): void {
    this.adminService.approveOperator(id).subscribe(() => this.refresh());
  }

  reject(id: string): void {
    this.adminService.rejectOperator(id, 'Rejected by admin').subscribe(() => this.refresh());
  }

  approveBus(id: string): void {
    this.adminService.approveBus(id).subscribe(() => this.refresh());
  }

  rejectBus(id: string): void {
    this.adminService.rejectBus(id, 'Rejected by admin').subscribe(() => this.refresh());
  }

  createQuickRoute(): void {
    if (this.quickRouteForm.invalid) return;
    this.routeService.createRouteByName(
      this.quickRouteForm.value.sourceName ?? '',
      this.quickRouteForm.value.destinationName ?? ''
    ).subscribe(() => {
      this.quickRouteForm.reset();
      this.refresh();
    });
  }

  private refresh(): void {
    this.adminService.getPendingOperators().subscribe((data) => (this.pendingOperators = data));
    this.adminService.getUsers().subscribe((data) => (this.users = data));
    this.adminService.getBuses().subscribe((data) => {
      this.buses = data;
      this.pendingBuses = data.filter(b => b.approvalStatus === 'Pending');
    });
    this.adminService.getBookings().subscribe((data) => (this.bookings = data));
    this.routeService.getRoutes().subscribe((data) => (this.routes = data));
    this.adminService.getSystemLogs().subscribe((data) => (this.systemLogs = data));
    this.adminService.getStats().subscribe((data) => (this.stats = data));
  }
}
