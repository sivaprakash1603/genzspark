 Authentication

   1 curl -X POST "https://localhost:5001/api/auth/register/passenger" -H "Content-Type:
     application/json" -d '{"username": "johndoe", "email": "john@example.com", "password":
     "Password123!"}'

   1 curl -X POST "https://localhost:5001/api/auth/register/operator" -H "Content-Type:
     application/json" -d '{"username": "fastbus", "email": "contact@fastbus.com", "password":
     "OperatorPass123!", "vehicleNumber": "KA-01-HH-1234"}'

   1 curl -X POST "https://localhost:5001/api/auth/login" -H "Content-Type: application/json" -d
     '{"username": "johndoe", "password": "Password123!"}'

  Public Search
   1 curl -X GET "https://localhost:5001/api/routes"

   1 curl -X GET "https://localhost:5001/api/buses/search?source=NewYork&destination=Boston"

   1 curl -X GET "https://localhost:5001/api/buses/REPLACE_WITH_BUS_ID/seats"

  Passenger Actions (Include Header: Authorization: Bearer <TOKEN>)

   1 curl -X POST "https://localhost:5001/api/bookings/initiate" -H "Content-Type:
     application/json" -d '{"busId": "REPLACE_WITH_BUS_ID", "seatIds": ["REPLACE_WITH_SEAT_ID"]}'

   1 curl -X GET "https://localhost:5001/api/bookings/my"

   1 curl -X POST "https://localhost:5001/api/payments/process" -H "Content-Type:
     application/json" -d '{"bookingId": "REPLACE_WITH_ID", "transactionId":
     "550e8400-e29b-41d4-a716-446655440000", "isSuccess": true, "cardNumber":
     "4111222233334444"}'

  Operator Actions (Include Header: Authorization: Bearer <TOKEN>)

   1 curl -X POST "https://localhost:5001/api/operator/buses" -H "Content-Type: application/json"
     -d '{"routeId": "REPLACE_WITH_ID", "busName": "Express 101", "boardingPoint": "Central",
     "dropPoint": "Downtown", "departureTime": "2026-05-01T10:00:00Z", "durationMinutes": 240,
     "seatLayoutType": "2x2", "totalSeats": 40, "basePrice": 25.50}'

   1 curl -X GET "https://localhost:5001/api/operator/bookings"

   1 curl -X GET "https://localhost:5001/api/operator/revenue"

  Admin Actions (Include Header: Authorization: Bearer <TOKEN>)

   1 curl -X POST "https://localhost:5001/api/admin/sources" -H "Content-Type: application/json"
     -d '{"name": "London"}'

   1 curl -X POST "https://localhost:5001/api/admin/destinations" -H "Content-Type:
     application/json" -d '{"name": "Paris"}'

   1 curl -X POST "https://localhost:5001/api/admin/routes" -H "Content-Type: application/json"
     -d '{"sourceId": "REPLACE_ID", "destinationId": "REPLACE_ID"}'

   1 curl -X POST "https://localhost:5001/api/admin/operators/REPLACE_ID/approve"

   1 curl -X GET "https://localhost:5001/api/admin/users"

   1 curl -X GET "https://localhost:5001/api/admin/bookings"