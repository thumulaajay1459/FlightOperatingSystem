# ✈️ Frontend Development Guide — Flight Operation System

> All API calls go through **API Gateway at `http://localhost:5000`**. Never call individual services directly.

---

## 🔑 Base URL

```
http://localhost:5000
```

---

## 🔐 Authentication

### Token Storage
Store tokens in `localStorage` or a state manager after login:
```json
{
  "accessToken": "<jwt_token>",
  "refreshToken": "<refresh_token>",
  "expiresAt": "2025-01-01T00:00:00Z"
}
```

### Authorization Header (for all protected routes)
```
Authorization: Bearer <accessToken>
```

### Token Expiry
- Access token expires in **1 hour**
- Refresh token expires in **7 days**
- Call `/auth/refresh-token` before expiry to get a new access token

---

## 📋 Pages & Their API Calls

---

### 1. Register Page

**POST** `/auth/register`

Request body:
```json
{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john@example.com",
  "password": "Password@123",
  "role": "User"
}
```
- `role` is optional — defaults to `"User"`. Use `"Admin"` only for admin accounts.
- On success: redirect to Login page.

---

### 2. Login Page

**POST** `/auth/login`

Request body:
```json
{
  "email": "john@example.com",
  "password": "Password@123"
}
```

Response:
```json
{
  "accessToken": "eyJhbGci...",
  "refreshToken": "abc123...",
  "expiresAt": "2025-01-01T01:00:00Z"
}
```
- Save both tokens.
- Decode the JWT to get `role` claim → redirect Admin to `/admin/dashboard`, User to `/home`.

---

### 3. Home / Flight Search Page

**GET** `/flights/search?from=Mumbai&to=Delhi&date=2025-06-01`

No auth required.

Response:
```json
{
  "searchCriteria": { "from": "Mumbai", "to": "Delhi", "date": "2025-06-01" },
  "totalFlights": 3,
  "flights": [
    {
      "flightId": 1,
      "flightNumber": "AI-101",
      "origin": { "city": "Mumbai", "code": "BOM", "name": "Chhatrapati Shivaji" },
      "destination": { "city": "Delhi", "code": "DEL", "name": "Indira Gandhi" },
      "departureTime": "2025-06-01T06:00:00",
      "arrivalTime": "2025-06-01T08:00:00",
      "duration": 2.0,
      "aircraft": { "manufacturer": "Boeing", "model": "737", "totalSeats": 180 },
      "pricing": {
        "economy": 3500,
        "premiumEconomy": 6000,
        "business": 12000,
        "firstClass": 25000
      },
      "discounts": { "childDiscountPercent": 25, "infantDiscountPercent": 90 },
      "status": "Scheduled"
    }
  ]
}
```

---

### 4. All Flights Page (Browse)

**GET** `/flights?page=1&pageSize=20`

No auth required.

Response:
```json
{
  "data": [ /* array of FlightSummaryDto */ ],
  "pagination": {
    "currentPage": 1,
    "pageSize": 20,
    "totalCount": 100,
    "totalPages": 5
  }
}
```

---

### 5. Flight Detail Page

**GET** `/flights/{id}`

No auth required.

---

### 6. Seat Map Page

**GET** `/seats/flight/{flightId}/seat-map`

No auth required. Shows seat layout for a flight.

**GET** `/flights/available-seats/{id}`

Response:
```json
{ "flightId": 1, "totalSeats": 180, "bookedSeats": 45, "availableSeats": 135 }
```

---

### 7. Price Calculator (before booking)

**POST** `/flights/calculate-price`

No auth required.

Request body:
```json
{
  "flightId": 1,
  "returnFlightId": null,
  "passengers": [
    { "passengerType": "Adult", "seatClass": "Economy", "seatNumber": "12A" },
    { "passengerType": "Child", "seatClass": "Economy", "seatNumber": "12B" }
  ]
}
```
- `passengerType`: `"Adult"` | `"Child"` | `"Infant"`
- `seatClass`: `"Economy"` | `"PremiumEconomy"` | `"Business"` | `"FirstClass"`

---

### 8. User Profile Page

#### Get Profile
**GET** `/profile` *(requires auth)*

Response:
```json
{
  "userId": 1,
  "fullName": "John Doe",
  "email": "john@example.com",
  "passportNumber": "A1234567",
  "gender": "Male",
  "age": 30,
  "nationality": "Indian",
  "dateOfBirth": "1994-05-15"
}
```

#### Create Profile (first time)
**POST** `/profile` *(requires auth)*

Request body:
```json
{
  "passportNumber": "A1234567",
  "gender": "Male",
  "age": 30,
  "nationality": "Indian",
  "dateOfBirth": "1994-05-15"
}
```

#### Update Profile
**PUT** `/profile` *(requires auth)*

Same body as Create.

---

### 9. Create Booking Page

**POST** `/bookings/create` *(requires auth)*

#### Option A — Use saved profile as passenger (single passenger)
```json
{
  "flightId": 1,
  "bookingType": "OneWay",
  "totalAmount": 4130,
  "useProfileAsPassenger": true,
  "seatNumber": "12A"
}
```

#### Option B — Manual passenger details
```json
{
  "flightId": 1,
  "returnFlightId": null,
  "bookingType": "OneWay",
  "totalAmount": 4130,
  "useProfileAsPassenger": false,
  "passengers": [
    {
      "fullName": "John Doe",
      "age": 30,
      "gender": "Male",
      "passengerType": "Adult",
      "passportNumber": "A1234567",
      "seatNumber": "12A"
    }
  ]
}
```

- `bookingType`: `"OneWay"` | `"RoundTrip"` | `"MultiCity"`
- For round trip, provide `returnFlightId`
- `totalAmount` should come from the price calculator response

Response: Booking object with `id` and `bookingReference`.

---

### 10. My Bookings Page

**GET** `/bookings/my-bookings` *(requires auth)*

Returns list of user's bookings.

---

### 11. Booking Detail Page

**GET** `/bookings/{id}` *(requires auth)*

---

### 12. Ticket Page

**GET** `/bookings/{id}/ticket` *(requires auth)*

Returns ticket details (JSON).

---

### 13. Download PDF Ticket

**GET** `/bookings/{id}/download-ticket` *(requires auth)*

Returns a PDF file (`application/pdf`). In Angular:
```typescript
this.http.get(`/bookings/${id}/download-ticket`, { responseType: 'blob' }).subscribe(blob => {
  const url = window.URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = `ticket-${id}.pdf`;
  a.click();
});
```

---

### 14. Resend Ticket Email

**POST** `/bookings/{id}/resend-ticket` *(requires auth)*

No body needed. Sends ticket PDF to user's email.

---

### 15. Cancel Booking

**POST** `/bookings/cancel?id={bookingId}` *(requires auth)*

No body needed.

---

### 16. Payment Flow

#### Step 1 — Create Payment Order
**POST** `/payments/create-order` *(requires auth)*

```json
{
  "bookingId": "BK-2025-001",
  "flightId": "1",
  "flightNumber": "AI-101",
  "origin": "Mumbai",
  "destination": "Delhi",
  "travelDate": "2025-06-01T06:00:00",
  "passengerId": "1",
  "passengerName": "John Doe",
  "email": "john@example.com",
  "phone": "9876543210",
  "seatNumber": "12A",
  "cabinClass": "Economy",
  "baseFare": 3500,
  "taxAmount": 630,
  "serviceFee": 0,
  "currency": "INR"
}
```

Response:
```json
{
  "paymentId": "uuid-here",
  "razorpayOrderId": "order_xxx",
  "totalAmount": 4130,
  "currency": "INR",
  "key": "rzp_test_xxx"
}
```

#### Step 2 — Open Razorpay Checkout (Frontend)
Use the `key`, `razorpayOrderId`, and `totalAmount` from above to open Razorpay's JS SDK.

#### Step 3 — Verify Payment
**POST** `/payments/verify` *(requires auth)*

```json
{
  "paymentId": "uuid-here",
  "razorpayOrderId": "order_xxx",
  "razorpayPaymentId": "pay_xxx",
  "razorpaySignature": "signature_xxx"
}
```

---

### 17. View Payment

**GET** `/payments/{paymentId}` *(requires auth)*

**GET** `/payments/booking/{bookingId}` *(requires auth)*

---

### 18. Change Password

**POST** `/auth/change-password` *(requires auth)*

```json
{
  "currentPassword": "OldPass@123",
  "newPassword": "NewPass@456"
}
```

---

### 19. Forgot / Reset Password

**POST** `/auth/forgot-password`
```json
{ "email": "john@example.com" }
```

**POST** `/auth/reset-password`
```json
{
  "email": "john@example.com",
  "token": "<reset_token>",
  "newPassword": "NewPass@456"
}
```

---

### 20. Refresh Token

**POST** `/auth/refresh-token`
```json
{ "refreshToken": "<refresh_token>" }
```

Response: Same as login response (new `accessToken` + `refreshToken`).

---

## 🛡️ Admin Pages

All admin routes require `Authorization: Bearer <token>` where the token has `role: "Admin"`.

---

### Admin Dashboard Stats

**GET** `/admin/dashboard/stats` *(Admin only)*

---

### Manage Users

**GET** `/admin/users?page=1&pageSize=50` *(Admin only)*

Response:
```json
{
  "data": [{ "id": 1, "firstName": "John", "lastName": "Doe", "email": "...", "role": "User" }],
  "pagination": { "currentPage": 1, "pageSize": 50, "totalCount": 200, "totalPages": 4 }
}
```

**PUT** `/users/{id}` *(requires auth — own account or Admin)*
```json
{ "firstName": "John", "lastName": "Doe", "email": "john@example.com" }
```

**DELETE** `/users/{id}` *(requires auth — own account or Admin)*

---

### Manage Aircraft

**GET** `/admin/aircraft` *(Admin only)*

**POST** `/admin/aircraft` *(Admin only)*
```json
{
  "manufacturer": "Boeing",
  "model": "737",
  "totalSeats": 180
}
```

**GET** `/admin/aircraft/{id}` *(Admin only)*

**PUT** `/admin/aircraft/{id}` *(Admin only)*

**DELETE** `/admin/aircraft/{id}` *(Admin only)*

---

### Manage Airports

**GET** `/admin/airports` *(Admin only)*

**POST** `/admin/airports` *(Admin only)*
```json
{
  "name": "Chhatrapati Shivaji International Airport",
  "code": "BOM",
  "city": "Mumbai",
  "country": "India"
}
```

**GET** `/admin/airports/{id}` *(Admin only)*

**PUT** `/admin/airports/{id}` *(Admin only)*

**DELETE** `/admin/airports/{id}` *(Admin only)*

---

### Manage Routes

**GET** `/admin/routes` *(Admin only)*

**POST** `/admin/routes` *(Admin only)*
```json
{
  "originAirportId": 1,
  "destinationAirportId": 2,
  "distanceKm": 1400
}
```

**GET** `/admin/routes/{id}` *(Admin only)*

**PUT** `/admin/routes/{id}` *(Admin only)*

**DELETE** `/admin/routes/{id}` *(Admin only)*

---

### Manage Flights

**POST** `/admin/flights` *(Admin only)*
```json
{
  "flightNumber": "AI-101",
  "aircraftId": 1,
  "routeId": 1,
  "departureTime": "2025-06-01T06:00:00",
  "arrivalTime": "2025-06-01T08:00:00",
  "status": "Scheduled"
}
```

**PUT** `/admin/flights/{id}` *(Admin only)* — same body as above

**DELETE** `/admin/flights/{id}` *(Admin only)*

**PUT** `/flights/{id}/pricing` *(Admin only)*
```json
{
  "basePrice": 3000,
  "economyPrice": 3500,
  "premiumEconomyPrice": 6000,
  "businessPrice": 12000,
  "firstClassPrice": 25000,
  "childDiscountPercent": 25,
  "infantDiscountPercent": 90
}
```

---

### Admin — All Bookings

**GET** `/bookings/all?page=1&pageSize=50` *(Admin only)*

**GET** `/bookings/by-flight/{flightId}` *(Admin only)*

---

### Admin — Reports

**GET** `/admin/reports/revenue` *(Admin only)*

**GET** `/admin/reports/popular-routes` *(Admin only)*

---

### Admin — Refund

**POST** `/admin/payments/refund` *(Admin only)*
```json
{
  "paymentId": "uuid-here",
  "reason": "Flight cancelled"
}
```

---

### Admin — All Payments

**GET** `/payments/all` *(Admin only)*

---

## 🗺️ Suggested Page Structure (Angular)

```
/login                    → Login Page
/register                 → Register Page
/home                     → Flight Search
/flights                  → All Flights
/flights/:id              → Flight Detail + Seat Map
/booking/create/:flightId → Create Booking
/booking/my-bookings      → My Bookings
/booking/:id              → Booking Detail
/booking/:id/ticket       → View Ticket
/profile                  → User Profile
/payment/:bookingId       → Payment Page (Razorpay)

/admin/dashboard          → Admin Dashboard
/admin/users              → Manage Users
/admin/aircraft           → Manage Aircraft
/admin/airports           → Manage Airports
/admin/routes             → Manage Routes
/admin/flights            → Manage Flights
/admin/bookings           → All Bookings
/admin/payments           → All Payments
/admin/reports            → Revenue & Route Reports
```

---

## ⚡ Typical User Journey (Flow)

```
Register → Login → Search Flights → View Seat Map
→ Calculate Price → Create Booking → Payment (Razorpay)
→ Verify Payment → Download PDF Ticket / Receive Email
```

---

## 🔒 Route Guards (Angular)

| Guard | Condition | Redirect |
|-------|-----------|----------|
| `AuthGuard` | `accessToken` exists | `/login` |
| `AdminGuard` | JWT role == `"Admin"` | `/home` |
| `GuestGuard` | Not logged in | `/home` |

Decode JWT role:
```typescript
import { jwtDecode } from 'jwt-decode';
const decoded: any = jwtDecode(token);
const role = decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
```

---

## 🌐 Angular HTTP Interceptor (Auto-attach token)

```typescript
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const token = localStorage.getItem('accessToken');
  if (token) {
    req = req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
  }
  return next(req);
};
```

---

## ❗ Common Error Responses

| Status | Meaning |
|--------|---------|
| `400` | Bad request / validation error |
| `401` | Not authenticated (missing/expired token) |
| `403` | Forbidden (wrong role) |
| `404` | Resource not found |
| `409` | Conflict (e.g., email already exists) |
| `500` | Server error |

---

**Last Updated:** Based on actual backend code — ocelot.json + all controllers
