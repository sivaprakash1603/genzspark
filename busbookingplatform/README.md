# Bus Booking System

This repository contains a complete starter implementation for a bus booking platform.

- Frontend: Angular (standalone components + Angular Material)
- Backend: ASP.NET Core Web API (.NET 8)
- Database: PostgreSQL
- Auth: JWT
- Notifications: SMTP

## Architecture

- Frontend SPA in `frontend`
- Backend API in `backend/BusBooking.Api`
- Layered backend structure:
  - Controllers
  - Application (DTOs, interfaces, services)
  - Domain (entities, enums)
  - Infrastructure (persistence, repositories, jwt, smtp, bootstrap)

## Backend Structure

- `Controllers`: auth, admin, operator, buses, booking, payment, routes, email
- `Application/DTOs`: request/response DTO contracts
- `Application/Interfaces`: service and repository contracts
- `Application/Services`: business logic and rule enforcement
- `Domain/Entities`: EF Core entities
- `Infrastructure/Persistence`: `AppDbContext`
- `Infrastructure/Security`: JWT token generation
- `Infrastructure/Email`: SMTP sender and options
- `Infrastructure/Bootstrap`: env-based admin auto-creation and role seeding

## Frontend Structure

- `src/app/core`: guards, interceptor, services, models
- `src/app/features/auth`: login and registration
- `src/app/features/passenger`: search, seat selection, payment, booking history
- `src/app/features/operator`: operator dashboard
- `src/app/features/admin`: admin dashboard

## Database Schema

- See `docs/database-schema.sql`
- Mandatory scope baseline and acceptance checklist: `docs/mandatory-requirements-baseline.md`
- Key tables included:
  - users
  - roles
  - operator_profiles
  - routes
  - buses
  - seats
  - bookings
  - booking_seats
  - payments
  - email_logs

## Business Rules Implemented

- Username/password login
- Password hashing
- Roles: Passenger, Operator, Admin
- Admin auto-created from environment variables
- Admin can approve/reject/enable/disable operators
- Disabled operators cannot login/manage buses
- Admin can approve/reject buses
- Only approved buses are returned in passenger search
- Platform fee applied in backend when operator creates bus
- Fare calculation uses `total_price = base_price + platform_fee`
- All payment attempts are stored including failures
- Booking confirmation only after successful payment
- Booking cancellation rule: blocked if departure is within 2 hours
- SMTP email triggers for registration, approvals/rejections, enable/disable, booking and payment events

## API Summary

### Auth
- `POST /api/auth/register/passenger`
- `POST /api/auth/register/operator`
- `POST /api/auth/login`

### Admin
- `GET /api/admin/operators/pending`
- `POST /api/admin/operators/{operatorId}/approve`
- `POST /api/admin/operators/{operatorId}/reject`
- `POST /api/admin/operators/{operatorId}/enable`
- `POST /api/admin/operators/{operatorId}/disable`
- `POST /api/admin/buses/{busId}/approve`
- `POST /api/admin/buses/{busId}/reject`
- `POST /api/admin/sources`
- `POST /api/admin/destinations`
- `POST /api/admin/routes`
- `GET /api/admin/users`
- `GET /api/admin/buses`
- `GET /api/admin/bookings`

### Operator
- `POST /api/operator/buses`
- `DELETE /api/operator/buses/{busId}`
- `POST /api/operator/buses/{busId}/disable-temporary`
- `POST /api/operator/buses/{busId}/enable-temporary`
- `GET /api/operator/bookings`
- `GET /api/operator/revenue`

### Passenger / Booking / Payment
- `GET /api/buses/search?source=&destination=`
- `GET /api/buses/{busId}/seats`
- `POST /api/bookings/initiate`
- `GET /api/bookings/my`
- `POST /api/bookings/{bookingId}/cancel`
- `POST /api/payments/process`

### Routes and Email
- `GET /api/routes`
- `POST /api/email/test?to=`
- `GET /api/email/logs`

## Setup Steps

## 1) PostgreSQL

1. Install PostgreSQL 15+.
2. Create database `busbooking`.
3. Create a DB user and grant privileges.
4. Set `CONNECTIONSTRINGS__DEFAULTCONNECTION`.

## 2) Backend (.NET 8)

1. Install .NET SDK 8.
2. Open terminal in `backend/BusBooking.Api`.
3. Restore packages: `dotnet restore`.
4. Set environment variables from `.env.example`.
5. Start API: `dotnet run`.

Notes:
- On startup, API applies migrations and seeds roles.
- Admin user is auto-created from `ADMIN_USERNAME`, `ADMIN_PASSWORD`, `ADMIN_EMAIL`.

## 3) Frontend (Angular)

1. Install Node.js LTS.
2. Open terminal in `frontend`.
3. Install dependencies: `npm install`.
4. Run UI: `npm start`.
5. Ensure `src/environments/environment.ts` points to backend URL.

## 4) SMTP

1. Provide SMTP settings in environment variables.
2. Use `/api/email/test?to=your@email` to validate.
3. Check `/api/email/logs` for send status.

## Notes

- This is a complete starter scaffold and may be extended with:
  - refresh tokens
  - background queue for emails
  - stronger validation and exception middleware
  - integration and unit tests
