import { Routes } from '@angular/router';
import { HomeComponent } from './features/home/home.component';
import { LoginComponent } from './features/auth/login.component';
import { SearchBusesComponent } from './features/passenger/search-buses.component';
import { SeatSelectionComponent } from './features/passenger/seat-selection.component';
import { DummyPaymentComponent } from './features/passenger/dummy-payment.component';
import { TicketComponent } from './features/passenger/ticket.component';
import { BookingHistoryComponent } from './features/passenger/booking-history.component';
import { AdminDashboardComponent } from './features/admin/admin-dashboard.component';
import { OperatorDashboardComponent } from './features/operator/operator-dashboard.component';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';

export const appRoutes: Routes = [
  { path: '', pathMatch: 'full', component: HomeComponent },
  { path: 'login', component: LoginComponent },
  { path: 'search', component: SearchBusesComponent },
  { path: 'seat-selection/:busId', component: SeatSelectionComponent, canActivate: [authGuard, roleGuard], data: { roles: ['Passenger'] } },
  { path: 'payment/:bookingId', component: DummyPaymentComponent, canActivate: [authGuard, roleGuard], data: { roles: ['Passenger'] } },
  { path: 'ticket/:bookingId', component: TicketComponent, canActivate: [authGuard, roleGuard], data: { roles: ['Passenger'] } },
  { path: 'bookings', component: BookingHistoryComponent, canActivate: [authGuard, roleGuard], data: { roles: ['Passenger'] } },
  { path: 'admin', component: AdminDashboardComponent, canActivate: [authGuard, roleGuard], data: { roles: ['Admin'] } },
  { path: 'operator', component: OperatorDashboardComponent, canActivate: [authGuard, roleGuard], data: { roles: ['Operator'] } },
  { path: '**', redirectTo: '' }
];
