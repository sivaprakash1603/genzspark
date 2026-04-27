# Bus Booking System - Mandatory Requirements Baseline

## 1. Purpose
This document freezes the non-optional scope for the local assignment.

- Included: mandatory features only
- Excluded: optional enhancements (round-trip, coupons, SMS, advanced pricing, cloud)
- Target environment: local machine only

## 2. Stack and Technical Acceptance
The submission is acceptable only if all items are true.

- Frontend is Angular
- Backend is .NET Web API
- Database is PostgreSQL
- App runs locally end-to-end (frontend + backend + DB)
- Passwords are hashed (never stored in plain text)

## 3. Role Matrix (Mandatory)

### Customer / User
- Search buses without login by source, destination, date
- Use fuzzy location suggestions while typing source/destination
- View bus list with bus details, seat availability, and price
- Login required before booking
- Select one or more seats from seat layout
- Enter passenger details per seat: name, age, gender
- Temporary seat lock prevents double booking for lock window
- Complete booking via dummy payment
- Receive booking confirmation email
- Download generated ticket after successful booking
- View booking history (upcoming, completed/past, cancelled)
- Cancel booking based on cancellation policy
- View own profile

### Bus Operator
- Register as operator
- Access granted only after admin approval
- Login only when approved/enabled
- View own buses
- Add bus (pending admin approval before active use)
- Use only admin-created routes
- Temporarily disable bus (maintenance)
- Permanently remove bus
- View bookings on own buses
- View own revenue

### Admin
- Approve/reject operator registrations
- Enable/disable operator
- Approve/reject new buses
- Create/manage sources and destinations
- Create/manage point-to-point routes
- View platform booking details/analytics
- View platform revenue
- Configure platform convenience fee
- On operator disable: cancel impacted future bookings, update statuses, send email notifications

## 4. Booking and Concurrency Acceptance

- Search flow: source + destination + journey date
- Seat map availability shown before booking
- Seat lock duration implemented (5-10 min configurable)
- Concurrent booking attempts cannot book same locked/booked seat
- Multi-seat booking is supported in one transaction
- Passenger details are persisted per seat/passenger
- Payment result persists (dummy payment acceptable)
- Booking confirmation generated only on successful payment
- Ticket is generated and downloadable

## 5. Cancellation and Refund Acceptance

- User cancellation endpoint and UI are available
- Cancellation cutoff rule enforced before departure
- Refund policy logic implemented based on time-to-departure
- Cancellation/refund communication email sent

## 6. Notifications Acceptance

- Email sent for booking confirmation
- Email sent for cancellation/refund communication
- Email sent for operator disablement impact on customers

## 7. Out of Scope (Explicitly Excluded)

- Round-trip booking
- SMS integration
- Dynamic pricing
- Different seat-category pricing
- Multi-stop route engine
- Coupon/recovery suggestions
- Cloud deployment
- MFA and advanced identity hardening

## 8. Definition of Done for This Assignment
The assignment is complete only when all items below are demo-ready locally.

- Customer can search, select seats, pay (dummy), get ticket, view/cancel bookings
- Seat lock prevents simultaneous seat conflicts
- Operator lifecycle works: register -> admin approval -> bus management -> booking/revenue view
- Admin lifecycle works: route/location management, operator/bus approval, disable flow handling
- Required emails are sent
- Backend and frontend run locally against PostgreSQL with documented run steps

## 9. Build Order (Execution Sequence)
Use this order to avoid rework.

1. Roles/permissions matrix and status enums
2. Data model + migrations
3. Auth + hashing + JWT
4. Search + fuzzy locations + seat availability
5. Seat lock and booking workflow
6. Payment + ticket + email
7. Booking history + cancellation/refund
8. Operator workflows
9. Admin workflows (including disable impact)
10. Frontend flows per role
11. End-to-end validation and runbook
