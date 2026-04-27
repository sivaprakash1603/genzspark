# Roles and Permissions Matrix

This matrix defines the authorization baseline for mandatory scope.

## 1. Roles

- Guest (not logged in)
- Passenger
- Operator
- Admin

## 2. Capability Matrix

| Capability | Guest | Passenger | Operator | Admin |
|---|---|---|---|---|
| Search buses by source/destination/date | Yes | Yes | Yes | Yes |
| Fuzzy location suggestions | Yes | Yes | Yes | Yes |
| View seat map | Yes | Yes | Yes | Yes |
| Lock/unlock seats | No | Yes | No | No |
| Initiate booking | No | Yes | No | No |
| Process payment (dummy) | No | Yes | No | No |
| Download ticket | No | Yes (own bookings) | No | Yes (audit only, optional internal) |
| View own booking history | No | Yes | No | No |
| Cancel own booking | No | Yes | No | No |
| Register passenger | Yes | No | No | No |
| Register operator | Yes | No | No | No |
| Login | Yes | Yes | Yes | Yes |
| Add bus | No | No | Yes (pending admin approval) | No |
| Disable/enable own bus temporarily | No | No | Yes | No |
| Remove own bus permanently | No | No | Yes | No |
| View bookings for own buses | No | No | Yes | No |
| View own revenue | No | No | Yes | No |
| Approve/reject operators | No | No | No | Yes |
| Enable/disable operator | No | No | No | Yes |
| Approve/reject buses | No | No | No | Yes |
| Manage sources/destinations | No | No | No | Yes |
| Create/manage routes | No | No | No | Yes |
| View platform bookings/analytics | No | No | No | Yes |
| View platform revenue | No | No | No | Yes |
| Configure platform fee | No | No | No | Yes |

## 3. API Authorization Matrix

| Endpoint | Access |
|---|---|
| POST /api/auth/register/passenger | Public |
| POST /api/auth/register/operator | Public |
| POST /api/auth/login | Public |
| GET /api/buses/search | Public |
| GET /api/buses/{busId}/seats | Public |
| GET /api/routes | Public |
| POST /api/bookings/initiate | Passenger |
| GET /api/bookings/my | Passenger |
| POST /api/bookings/{bookingId}/cancel | Passenger |
| POST /api/payments/process | Passenger |
| POST /api/operator/buses | Operator |
| DELETE /api/operator/buses/{busId} | Operator |
| POST /api/operator/buses/{busId}/disable-temporary | Operator |
| POST /api/operator/buses/{busId}/enable-temporary | Operator |
| GET /api/operator/bookings | Operator |
| GET /api/operator/revenue | Operator |
| GET /api/admin/operators/pending | Admin |
| POST /api/admin/operators/{operatorId}/approve | Admin |
| POST /api/admin/operators/{operatorId}/reject | Admin |
| POST /api/admin/operators/{operatorId}/enable | Admin |
| POST /api/admin/operators/{operatorId}/disable | Admin |
| POST /api/admin/buses/{busId}/approve | Admin |
| POST /api/admin/buses/{busId}/reject | Admin |
| POST /api/admin/sources | Admin |
| POST /api/admin/destinations | Admin |
| POST /api/admin/routes | Admin |
| GET /api/admin/users | Admin |
| GET /api/admin/buses | Admin |
| GET /api/admin/bookings | Admin |
| POST /api/email/test | Admin |
| GET /api/email/logs | Admin |

## 4. Security Rules

- All non-public endpoints require JWT bearer token.
- Role claims must be checked with [Authorize(Roles = "...")] on controllers/actions.
- Ownership checks are mandatory for Passenger and Operator resources:
  - Passenger can only access/cancel own bookings.
  - Operator can only mutate/view own buses and own booking/revenue data.
- Admin override does not bypass data integrity rules (approval status, seat lock rules, cancellation policies).

## 5. Todo-2 Deliverable

Todo 2 is complete when:

- Role names are fixed and used consistently across backend/frontend.
- Public vs protected endpoint access is documented and enforced.
- Ownership constraints are explicitly defined for Passenger and Operator flows.
- This file is kept as the reference for auth checks during implementation.
