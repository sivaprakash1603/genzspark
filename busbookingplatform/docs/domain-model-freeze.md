# Domain Model and Status Freeze

This document freezes the mandatory domain model and status vocabulary for implementation.

## 1. Core Entities (Mandatory)

- User
- Role
- OperatorProfile
- SourceLocation
- DestinationLocation
- Route
- Bus
- Seat
- Booking
- BookingSeat
- BookingPassenger
- Payment
- EmailLog

## 2. Entity Responsibilities

- User: identity, credential hash, role, active flag
- Role: Passenger / Operator / Admin
- OperatorProfile: operator onboarding, approval and enablement state
- Route: point-to-point source and destination
- Bus: operator-owned service on route with timing and pricing
- Seat: bus seat definition and temporary lock state
- Booking: booking header and lifecycle status
- BookingSeat: seat-to-booking fare records
- BookingPassenger: passenger details per selected seat
- Payment: payment attempt outcome for a booking
- EmailLog: notification audit trail

## 3. Frozen Status Values
Use these exact strings everywhere (DB, backend, frontend filters) until moved to enums/constants.

### Operator ApprovalStatus
- Pending
- Approved
- Rejected

### Bus ApprovalStatus
- Pending
- Approved
- Rejected

### Booking BookingStatus
- PendingPayment
- Confirmed
- Cancelled

### Payment PaymentStatus
- Success
- Failed

### User IsActive
- true: account can authenticate (subject to role-specific checks)
- false: account blocked

### OperatorProfile IsEnabled
- true: approved operator is operational
- false: operator disabled by admin or not yet approved

## 4. Seat Lock Rules

- Lock key: Seat.Id + LockedByUserId + LockedUntil
- Lock duration: configurable 5-10 minutes (current default 5 minutes)
- Locked seat cannot be booked by other users while lock is valid
- Expired lock is treated as unlocked

## 5. Booking Lifecycle

1. PendingPayment: seats selected and booking initiated
2. Confirmed: payment success and ticket available
3. Cancelled: user/admin/operator-impact cancellation

## 6. Mandatory Fields by Flow

### Booking Initiation
- booking.busId
- passengers[] where each item has:
  - seatId
  - name
  - age
  - gender

### Payment
- bookingId
- transactionId
- success/failure outcome
- gateway metadata (dummy allowed)

### Cancellation
- bookingId
- cancellation reason
- cancellation timestamp
- refund calculation output (amount/percentage/message)

## 7. Planned Additions Required by Mandatory Scope
These are required for remaining todos and should be added in migration phase.

- Ticket table/model
- Refund table/model
- PlatformFeeConfig table/model
- Optional: OperatorOfficeLocation mapping for source/destination pickup/drop derivation

## 8. Invariants

- One operator owns many buses.
- One bus belongs to one route.
- One booking belongs to one passenger and one bus.
- One booking has many booking seats and booking passengers.
- Passenger count equals selected seat count.
- Seat can be active/inactive and lockable.
- Only approved + enabled operators can run active buses.
- Search result only includes approved, active, non-temporarily-disabled buses.

## 9. Todo-3 Completion Criteria

Todo 3 is complete when:

- Status vocabulary above is applied consistently in code.
- Required relationships are reflected in entity models.
- Missing mandatory entities are identified and scheduled for migration (Todo 4).
- Backend/frontend use one canonical status set for filtering and UI labels.
