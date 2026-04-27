export interface AuthResponse {
  token: string;
  username: string;
  role: string;
}

export interface BusSearchResult {
  busId: string;
  busName: string;
  source: string;
  destination: string;
  boardingPoint: string;
  dropPoint: string;
  departureTime: string;
  arrivalTime: string;
  durationMinutes: number;
  seatLayoutType: string;
  totalPrice: number;
  totalSeats: number;
  availableSeats: number;
}

export interface PassengerDetails {
  seatId: string;
  name: string;
  age: number;
  gender: string;
}

export interface BookingResponse {
  bookingId: string;
  bookingStatus: string;
  totalAmount: number;
  bookedAt: string;
  journeyDate: string;
  busName: string;
  source: string;
  destination: string;
}

export interface PaymentResult {
  paymentId: string;
  transactionId: string;
  paymentStatus: string;
}

export interface SeatResponse {
  seatId: string;
  seatNumber: string;
  isBooked: boolean;
  isLocked: boolean;
  lockedBy: string | null;
}
