# 🔄 UPDATED API ENDPOINTS REFERENCE

## Quick Reference for All Working Endpoints

---

## 🌐 PUBLIC ENDPOINTS (No Authentication Required)

### Authentication
```http
POST /auth/register          # Register new user
POST /auth/login             # Login and get JWT token
POST /auth/forgot-password   # Request password reset
POST /auth/reset-password    # Reset password with token
```

### Airports (NEW - Public Access)
```http
GET /airports               # Get all airports
GET /airports/{id}          # Get airport by ID
```

### Routes (NEW - Public Access)
```http
GET /routes                 # Get all routes
GET /routes/{id}            # Get route by ID
```

### Flights
```http
GET /flights                           # Get all flights (paginated)
GET /flights/{id}                      # Get flight by ID
GET /flights/search                    # Advanced search (multiple filters)
GET /flights/search-flights            # User-friendly search (city + date)
GET /flights/available-seats/{id}      # Get available seats for flight
GET /flights/by-route                  # Get flights by route
POST /flights/calculate-price          # Calculate booking price
```

### Seats
```http
GET /seats/flight/{flightId}/seat-map  # Get seat map for flight
```

---

## 🔐 AUTHENTICATED ENDPOINTS (Requires JWT Token)

### User Profile
```http
GET /auth/me                # Get current user info
POST /auth/change-password  # Change password
POST /auth/refresh-token    # Refresh JWT token

GET /profile                # Get user profile
POST /profile               # Create user profile
PUT /profile                # Update user profile
DELETE /profile             # Delete user profile
```

### Bookings
```http
POST /bookings/create                    # Create new booking
GET /bookings/my-bookings                # Get user's bookings
POST /bookings/cancel                    # Cancel booking
GET /bookings/{id}                       # Get booking details
PUT /bookings/{id}                       # Update booking
GET /bookings/{id}/ticket                # Get ticket details
GET /bookings/{id}/download-ticket       # Download PDF ticket
POST /bookings/{id}/resend-ticket        # Resend ticket email
```

### Seats (Authenticated)
```http
POST /seats/block           # Block seats temporarily
POST /seats/release         # Release blocked seats
```

### Payments
```http
POST /payments/create-order              # Create payment order
POST /payments/verify                    # Verify payment
GET /payments/booking/{bookingId}        # Get payment by booking
GET /payments/user/{userId}              # Get user's payments
GET /payments/{paymentId}                # Get payment details
```

---

## 👑 ADMIN ENDPOINTS (Requires Admin Role)

### Dashboard & Reports
```http
GET /admin/dashboard/stats              # Get dashboard statistics
GET /admin/reports/revenue              # Get revenue report
GET /admin/reports/popular-routes       # Get popular routes report
```

### User Management
```http
GET /admin/users                        # Get all users (paginated)
PUT /admin/users/{id}                   # Update user
DELETE /admin/users/{id}                # Delete user
```

### Aircraft Management
```http
GET /admin/aircraft                     # Get all aircraft
GET /admin/aircraft/{id}                # Get aircraft by ID
POST /admin/aircraft                    # Create aircraft
PUT /admin/aircraft/{id}                # Update aircraft
DELETE /admin/aircraft/{id}             # Delete aircraft
```

### Airport Management
```http
GET /admin/airports                     # Get all airports (admin view)
GET /admin/airports/{id}                # Get airport by ID
POST /admin/airports                    # Create airport
PUT /admin/airports/{id}                # Update airport
DELETE /admin/airports/{id}             # Delete airport
```

### Route Management
```http
GET /admin/routes                       # Get all routes (admin view)
GET /admin/routes/{id}                  # Get route by ID
POST /admin/routes                      # Create route
PUT /admin/routes/{id}                  # Update route
DELETE /admin/routes/{id}               # Delete route
```

### Flight Management
```http
POST /admin/flights                     # Create flight
PUT /admin/flights/{id}                 # Update flight
DELETE /admin/flights/{id}              # Delete flight
PUT /flights/{id}/pricing               # Update flight pricing
POST /seats/bulk-create                 # Bulk create seats for flight
```

### Booking Management
```http
GET /bookings/all                       # Get all bookings (admin)
GET /bookings/by-flight/{flightId}      # Get bookings by flight
```

### Payment Management
```http
GET /payments/all                       # Get all payments
POST /admin/payments/refund             # Process refund
```

---

## 📝 REQUEST EXAMPLES

### 1. Search Flights (User-Friendly)
```http
GET /flights/search-flights?from=Mumbai&to=Delhi&date=2024-12-25

Response:
{
  "searchCriteria": {
    "from": "Mumbai",
    "to": "Delhi",
    "date": "2024-12-25"
  },
  "totalFlights": 5,
  "flights": [
    {
      "flightId": 1,
      "flightNumber": "AI-101",
      "origin": { "city": "Mumbai", "code": "BOM", "name": "..." },
      "destination": { "city": "Delhi", "code": "DEL", "name": "..." },
      "departureTime": "2024-12-25T10:00:00",
      "arrivalTime": "2024-12-25T12:30:00",
      "duration": 2.5,
      "aircraft": { "manufacturer": "Boeing", "model": "737", "totalSeats": 180 },
      "pricing": {
        "economy": 3500,
        "premiumEconomy": 6000,
        "business": 12000,
        "firstClass": 25000
      },
      "status": "Scheduled"
    }
  ]
}
```

### 2. Advanced Search
```http
GET /flights/search?flightNumber=AI-101&departure=2024-12-25&status=Scheduled

Response: Array of flights matching criteria
```

### 3. Create Flight (Admin)
```http
POST /admin/flights
Authorization: Bearer {admin_token}
Content-Type: application/json

{
  "flightNumber": "AI-101",
  "aircraftId": 1,
  "routeId": 1,
  "departureTime": "2024-12-25T10:00:00",
  "arrivalTime": "2024-12-25T12:30:00",
  "status": "Scheduled",
  "basePrice": 0,
  "economyPrice": 3500,
  "premiumEconomyPrice": 6000,
  "businessPrice": 12000,
  "firstClassPrice": 25000,
  "childDiscountPercent": 25,
  "infantDiscountPercent": 90
}
```

### 4. Create Route (Admin)
```http
POST /admin/routes
Authorization: Bearer {admin_token}
Content-Type: application/json

{
  "originAirportId": 1,
  "destinationAirportId": 2,
  "distanceKm": 1400
}

Response:
{
  "routeId": 1,
  "originAirportId": 1,
  "destinationAirportId": 2,
  "distanceKm": 1400,
  "originAirport": { "code": "BOM", "name": "...", "city": "Mumbai" },
  "destinationAirport": { "code": "DEL", "name": "...", "city": "Delhi" }
}
```

### 5. Create Airport (Admin)
```http
POST /admin/airports
Authorization: Bearer {admin_token}
Content-Type: application/json

{
  "name": "Indira Gandhi International Airport",
  "code": "DEL",
  "city": "Delhi",
  "country": "India"
}
```

### 6. Calculate Price
```http
POST /flights/calculate-price
Content-Type: application/json

{
  "flightId": 1,
  "returnFlightId": 2,
  "passengers": [
    {
      "passengerType": "Adult",
      "seatClass": "Economy",
      "seatNumber": "12A"
    },
    {
      "passengerType": "Child",
      "seatClass": "Economy",
      "seatNumber": "12B"
    }
  ]
}

Response:
{
  "outbound": {
    "baseFare": 6125,
    "taxes": 1102.5,
    "totalAmount": 7227.5,
    "passengerPrices": [...]
  },
  "returnTrip": { ... },
  "totalAmount": 14455
}
```

### 7. Create Booking
```http
POST /bookings/create
Authorization: Bearer {user_token}
Content-Type: application/json

{
  "flightId": 1,
  "returnFlightId": null,
  "passengers": [
    {
      "firstName": "John",
      "lastName": "Doe",
      "email": "john@example.com",
      "phone": "+919876543210",
      "dateOfBirth": "1990-01-01",
      "gender": "Male",
      "passengerType": "Adult",
      "seatClass": "Economy",
      "seatNumber": "12A"
    }
  ],
  "contactEmail": "john@example.com",
  "contactPhone": "+919876543210"
}
```

---

## 🔑 AUTHENTICATION

### Get Token
```http
POST /auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "Password@123"
}

Response:
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "userId": 1,
  "email": "user@example.com",
  "role": "User"
}
```

### Use Token
```http
GET /bookings/my-bookings
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

---

## 📊 RESPONSE FORMATS

### Success Response
```json
{
  "data": { ... },
  "message": "Success"
}
```

### Error Response
```json
{
  "message": "Error description",
  "errors": { ... }
}
```

### Paginated Response
```json
{
  "data": [ ... ],
  "pagination": {
    "currentPage": 1,
    "pageSize": 20,
    "totalCount": 100,
    "totalPages": 5
  }
}
```

---

## 🎯 COMMON QUERY PARAMETERS

- `page` - Page number (default: 1)
- `pageSize` - Items per page (default: 20, max: 100)
- `from` - Origin city/code
- `to` - Destination city/code
- `date` - Date in YYYY-MM-DD format
- `flightNumber` - Flight number filter
- `status` - Flight status filter
- `origin` - Origin filter
- `destination` - Destination filter

---

## ✅ STATUS CODES

- `200 OK` - Success
- `201 Created` - Resource created
- `204 No Content` - Success with no response body
- `400 Bad Request` - Invalid input
- `401 Unauthorized` - Missing or invalid token
- `403 Forbidden` - Insufficient permissions
- `404 Not Found` - Resource not found
- `500 Internal Server Error` - Server error

---

**Last Updated**: December 2024
**Version**: 1.1
**Base URL**: http://localhost:5000
