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
  durationMinutes: number;
  totalPrice: number;
  totalSeats: number;
}

export interface BookingResponse {
  bookingId: string;
  bookingStatus: string;
  totalAmount: number;
  bookedAt: string;
}
