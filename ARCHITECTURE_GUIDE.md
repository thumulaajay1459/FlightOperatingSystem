# 🏗️ Flight Operation System - Architecture Guide

## 📊 System Architecture

### **Microservices Overview**

```
┌─────────────────────────────────────────────────────────────────┐
│                         EXTERNAL USERS                           │
│                    (Web/Mobile/Postman)                          │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             ↓
┌─────────────────────────────────────────────────────────────────┐
│                    API GATEWAY (Port 5000)                       │
│  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  │
│  • Single entry point for all requests                          │
│  • JWT token validation                                         │
│  • Role-based authorization (Admin/User)                        │
│  • Request routing via Ocelot                                   │
│  • Rate limiting & load balancing                               │
└────────────────────────────┬────────────────────────────────────┘
                             │
        ┌────────────────────┼────────────────────┬────────────────┐
        │                    │                    │                │
        ↓                    ↓                    ↓                ↓
┌──────────────┐    ┌──────────────┐    ┌──────────────┐  ┌──────────────┐
│ UserService  │    │FlightService │    │BookingService│  │PaymentService│
│ (Port 5001)  │    │ (Port 5003)  │    │ (Port 5004)  │  │ (Port 5006)  │
│              │    │              │    │              │  │              │
│ ✅ Auth      │    │ ✅ Flights   │    │ ✅ Bookings  │  │ ✅ Payments  │
│ ✅ Login     │    │ ✅ Aircraft  │    │ ✅ Tickets   │  │ ✅ Razorpay  │
│ ✅ Register  │    │ ✅ Airports  │    │ ✅ PDF Gen   │  │ ✅ Refunds   │
│ ✅ JWT       │    │ ✅ Routes    │    │              │  │              │
│ ✅ Profiles  │    │              │    │              │  │              │
│              │    │              │    │              │  │              │
│ UserServiceDB│    │FlightServiceDB│   │BookingServiceDB│ │PaymentServiceDB│
└──────────────┘    └──────────────┘    └──────┬───────┘  └──────────────┘
                                               │
                                               ↓
                                    ┌──────────────────┐
                                    │NotificationService│
                                    │   (Port 5005)    │
                                    │                  │
                                    │ ✅ Email Sending │
                                    │ ✅ PDF Attach    │
                                    │ ✅ SMTP          │
                                    │                  │
                                    │ NotificationDB   │
                                    └──────────────────┘
```

---

## 🎯 Service Responsibilities

### **1. API Gateway (Port 5000)**

**Technology:** ASP.NET Core + Ocelot

**Responsibilities:**
- ✅ Single entry point for all external requests
- ✅ JWT token validation (signature, expiration, claims)
- ✅ Role-based authorization enforcement
- ✅ Request routing to microservices
- ✅ CORS handling
- ✅ Load balancing (if configured)

**Does NOT:**
- ❌ Check username/password
- ❌ Access any database
- ❌ Create JWT tokens
- ❌ Contain business logic

**Key Configuration:**
- `ocelot.json` - Route definitions
- `appsettings.json` - JWT validation settings

---

### **2. UserService (Port 5001)**

**Technology:** ASP.NET Core + Entity Framework + SQL Server

**Responsibilities:**
- ✅ **Authentication** - Login, Register, JWT token creation
- ✅ **User Management** - CRUD operations on users
- ✅ **Profile Management** - User travel profiles
- ✅ **Password Security** - Hashing and verification
- ✅ **Role Management** - Admin/User roles

**Database:** UserServiceDB
- Users table (ID, Email, PasswordHash, Role)
- UserProfiles table (UserId, PassportNumber, Gender, Age, etc.)

**Key Endpoints:**
- `POST /api/User` - Register new user
- `POST /api/User/Login` - Login and get JWT token
- `GET /api/User` - Get all users (Admin only)
- `GET /api/UserProfile` - Get user profile
- `POST /api/UserProfile` - Create profile
- `PUT /api/UserProfile` - Update profile

**Why UserService Handles Auth:**
- User data and authentication are tightly coupled
- Simpler architecture for this scale
- Industry standard for small-to-medium systems
- Reduces network hops and latency

---

### **3. FlightService (Port 5003)**

**Technology:** ASP.NET Core + Entity Framework + SQL Server

**Responsibilities:**
- ✅ Flight schedule management
- ✅ Aircraft management
- ✅ Airport management
- ✅ Route management
- ✅ Flight search functionality

**Database:** FlightServiceDB
- Flights table
- Aircraft table
- Airports table
- Routes table

**Key Endpoints:**
- `GET /api/ScheduledFlights` - View all flights (Public)
- `GET /api/ScheduledFlights/search` - Search flights (Public)
- `POST /api/ScheduledFlights` - Add flight (Admin only)
- `PUT /api/ScheduledFlights/{id}` - Update flight (Admin only)
- `DELETE /api/ScheduledFlights/{id}` - Delete flight (Admin only)
- `GET /api/Aircraft` - View aircraft (Admin only)
- `POST /api/Aircraft` - Add aircraft (Admin only)
- `GET /api/Airports` - View airports (Admin only)
- `POST /api/Airports` - Add airport (Admin only)
- `GET /api/Routes` - View routes (Admin only)
- `POST /api/Routes` - Add route (Admin only)

---

### **4. BookingService (Port 5004)**

**Technology:** ASP.NET Core + Entity Framework + SQL Server + QuestPDF

**Responsibilities:**
- ✅ Booking creation and management
- ✅ Seat locking mechanism
- ✅ PDF ticket generation
- ✅ Integration with FlightService
- ✅ Integration with UserService
- ✅ Integration with NotificationService

**Database:** BookingServiceDB
- Bookings table
- Passengers table

**Key Endpoints:**
- `POST /api/booking/create` - Create booking
- `GET /api/booking/my-bookings` - View my bookings
- `GET /api/booking/{id}` - View booking details
- `GET /api/booking/{id}/ticket` - View ticket
- `GET /api/booking/{id}/download-ticket` - Download PDF ticket
- `POST /api/booking/cancel` - Cancel booking

**Service-to-Service Communication:**
- Calls FlightService to validate flights
- Calls UserService to get user profiles
- Calls NotificationService to send emails

---

### **5. NotificationService (Port 5005)**

**Technology:** ASP.NET Core + MailKit + SQL Server

**Responsibilities:**
- ✅ Email sending via SMTP
- ✅ PDF attachment handling
- ✅ Notification history tracking
- ✅ Email template management

**Database:** NotificationDB
- Notifications table (tracking sent emails)

**Key Endpoints:**
- `POST /api/notifications/send-ticket-with-pdf` - Send email with PDF
- `GET /api/notifications/history` - View notification history

**SMTP Configuration:**
- Gmail SMTP (smtp.gmail.com:587)
- Configured in appsettings.json

---

### **6. PaymentService (Port 5006)**

**Technology:** ASP.NET Core + Entity Framework + SQL Server + Razorpay

**Responsibilities:**
- ✅ Payment order creation
- ✅ Payment verification
- ✅ Razorpay integration
- ✅ Payment history tracking
- ✅ Refund processing (Admin only)

**Database:** PaymentServiceDB
- Payments table (PaymentId, BookingId, Amount, Status, RazorpayOrderId, etc.)

**Key Endpoints:**
- `POST /api/payments/create-order` - Create payment order
- `POST /api/payments/verify` - Verify payment
- `GET /api/payments/{paymentId}` - Get payment by ID
- `GET /api/payments/booking/{bookingId}` - Get payments by booking
- `POST /api/payments/refund` - Refund payment (Admin only)

**Payment Flow:**
1. User creates booking
2. BookingService calls PaymentService to create order
3. User completes payment via Razorpay
4. Frontend calls PaymentService to verify payment
5. On successful payment, NotificationService sends email with ticket

**Razorpay Integration:**
- Test Mode: Uses Razorpay test keys
- Production: Requires live Razorpay keys
- Signature verification for security

---

## 🔐 Authentication & Authorization Flow

### **1. User Registration**

```
User → API Gateway → UserService
                         ↓
                    1. Validate email uniqueness
                    2. Hash password (bcrypt/PBKDF2)
                    3. Store in database
                    4. Return success
```

### **2. User Login**

```
User → API Gateway → UserService
                         ↓
                    1. Find user by email
                    2. Verify password hash
                    3. Create JWT token with claims:
                       - NameIdentifier (user ID)
                       - Email
                       - Role (Admin/User)
                    4. Return JWT token
```

### **3. Authenticated Request**

```
User → API Gateway
         ↓
    1. Extract JWT from Authorization header
    2. Validate token signature
    3. Check token expiration
    4. Extract claims (user ID, role)
    5. Check route requirements:
       - Authentication required? ✅
       - Admin role required? Check claim
    6. Forward to appropriate service
         ↓
    Service receives validated request
```

---

## 🚀 Complete User Journey

### **Scenario: User Books a Flight**

**Step 1: Register**
```http
POST http://localhost:5000/auth/register
{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john@example.com",
  "password": "SecurePass123",
  "role": "User"
}
```

**Step 2: Login**
```http
POST http://localhost:5000/auth/login
{
  "email": "john@example.com",
  "password": "SecurePass123"
}

Response: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

**Step 3: Create Profile**
```http
POST http://localhost:5000/profile
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6...
{
  "passportNumber": "P123456",
  "gender": "Male",
  "age": 30,
  "nationality": "USA",
  "dateOfBirth": "1994-01-01"
}
```

**Step 4: Search Flights**
```http
GET http://localhost:5000/flights/search?departure=2024-12-25
```

**Step 5: Create Payment Order**
```http
POST http://localhost:5000/payments/create-order
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6...
{
  "bookingId": "BK-20241225-ABC123",
  "flightId": "1",
  "flightNumber": "FL001",
  "origin": "Delhi",
  "destination": "Mumbai",
  "travelDate": "2024-12-25",
  "passengerId": "123",
  "passengerName": "John Doe",
  "email": "john@example.com",
  "phone": "+919876543210",
  "seatNumber": "12A",
  "cabinClass": "Economy",
  "baseFare": 250.00,
  "taxAmount": 45.00,
  "serviceFee": 4.99
}
```

**Step 6: Complete Payment (Frontend)**
- User completes payment via Razorpay
- Frontend receives payment details

**Step 7: Verify Payment**
```http
POST http://localhost:5000/payments/verify
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6...
{
  "paymentId": "guid-from-create-order",
  "razorpayOrderId": "order_xyz",
  "razorpayPaymentId": "pay_abc",
  "razorpaySignature": "signature_string"
}
```

**Step 8: Receive Email**
- After successful payment verification
- BookingService triggers NotificationService
- User receives email with PDF ticket

**Step 9: Download Ticket**
```http
POST http://localhost:5000/bookings/create
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6...
{
  "flightId": 1,
  "useProfileAsPassenger": true,
  "seatNumber": "12A",
  "totalAmount": 299.99
}
```

**Step 6: Receive Email**
- BookingService generates PDF ticket
- Sends to NotificationService
- User receives email with PDF attachment

**Step 7: Download Ticket**
```http
GET http://localhost:5000/bookings/1/download-ticket
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6...
```

---

## 🔧 Technology Stack

| Component | Technology |
|-----------|------------|
| **Framework** | ASP.NET Core 9.0 |
| **API Gateway** | Ocelot |
| **Authentication** | JWT Bearer Tokens |
| **ORM** | Entity Framework Core |
| **Database** | SQL Server |
| **PDF Generation** | QuestPDF |
| **Email** | MailKit (SMTP) |
| **Payment Gateway** | Razorpay |
| **Password Hashing** | Custom PasswordHasher |

---

## 📋 Port Configuration

| Service | Port | Purpose |
|---------|------|---------|
| API Gateway | 5000 | Entry point |
| UserService | 5001 | Auth + Users |
| FlightService | 5003 | Flights |
| BookingService | 5004 | Bookings |
| NotificationService | 5005 | Emails |
| PaymentService | 5006 | Payments |

---

## 🎯 Design Decisions

### **Why No Separate AuthService?**

**Decision:** UserService handles both authentication and user management.

**Rationale:**
1. **Tight Coupling** - User data and authentication are inherently related
2. **Simplicity** - Fewer services = easier to maintain
3. **Performance** - No extra network hop for auth operations
4. **Scale Appropriate** - Suitable for small-to-medium systems
5. **Industry Standard** - Common pattern in microservices

**When to Separate:**
- Multiple applications need shared authentication (SSO)
- OAuth/SAML/LDAP integration required
- Multi-factor authentication needed
- Large enterprise with dedicated auth team
- 100+ microservices requiring centralized auth

### **Why API Gateway Validates Tokens?**

**Decision:** API Gateway validates JWT tokens, not individual services.

**Rationale:**
1. **Centralized Security** - Single point for auth enforcement
2. **Performance** - Validate once at gateway, not in every service
3. **Separation of Concerns** - Services focus on business logic
4. **Consistency** - Same validation rules for all services

---

## 🔒 Security Features

### **Implemented:**
- ✅ JWT token-based authentication
- ✅ Password hashing (not plain text)
- ✅ Role-based authorization (Admin/User)
- ✅ Token expiration (1 hour)
- ✅ HTTPS support (configurable)
- ✅ CORS configuration

### **Recommended for Production:**
- ⚠️ Rate limiting
- ⚠️ API key authentication for service-to-service
- ⚠️ Refresh tokens
- ⚠️ Account lockout after failed attempts
- ⚠️ Password complexity requirements
- ⚠️ HTTPS enforcement
- ⚠️ Security headers
- ⚠️ Input validation & sanitization

---

## 📊 Database Schema Overview

### **UserServiceDB**
```sql
Users (ID, FirstName, LastName, Email, PasswordHash, Role)
UserProfiles (UserId, PassportNumber, Gender, Age, Nationality, DateOfBirth)
```

### **FlightServiceDB**
```sql
Flights (FlightId, FlightNumber, AircraftId, RouteId, DepartureTime, ArrivalTime, Status)
Aircraft (AircraftId, Manufacturer, Model, TotalSeats)
Airports (AirportId, Code, Name, City, Country)
Routes (RouteId, OriginAirportId, DestinationAirportId, DistanceKm)
```

### **BookingServiceDB**
```sql
Bookings (Id, UserId, FlightId, BookingReference, BookingStatus, TotalAmount, CreatedAt)
Passengers (Id, BookingId, FullName, Age, Gender, PassportNumber, SeatNumber)
```

### **PaymentServiceDB**
```sql
Payments (PaymentId, BookingId, FlightId, FlightNumber, PassengerName, TotalAmount, Currency, Status, RazorpayOrderId, RazorpayPaymentId, CreatedAt)
```

---

## 🚀 Getting Started

### **Prerequisites:**
- .NET 9.0 SDK
- SQL Server
- Visual Studio / VS Code / Rider

### **Running the System:**

1. **Start all services in order:**
```bash
# Terminal 1 - API Gateway
cd APIGateway
dotnet run

# Terminal 2 - UserService
cd UserService
dotnet run

# Terminal 3 - FlightService
cd FlightService
dotnet run

# Terminal 4 - BookingService
cd BookingService
dotnet run

# Terminal 5 - NotificationService
cd NotificationService
dotnet run

# Terminal 6 - PaymentService
cd PaymentService
dotnet run
```

2. **Verify all services are running:**
- API Gateway: http://localhost:5000
- UserService: http://localhost:5001
- FlightService: http://localhost:5003
- BookingService: http://localhost:5004
- NotificationService: http://localhost:5005
- PaymentService: http://localhost:5006

3. **Test the system:**
```bash
# Register a user
POST http://localhost:5000/auth/register

# Login
POST http://localhost:5000/auth/login

# Use the JWT token for authenticated requests
```

---

## 📚 Additional Documentation

- `QUICK_START_GUIDE.md` - Quick setup instructions
- `ROLE_BASED_ACCESS_GUIDE.md` - Authorization details
- `FLOW_VERIFICATION.md` - Complete flow verification
- `SIMPLE_PDF_EMAIL_FLOW.md` - PDF and email flow

---

## ✅ System Status

**Current Status:** ✅ Fully Functional

**Services:**
- ✅ API Gateway - Working
- ✅ UserService - Working (handles auth + user management)
- ✅ FlightService - Working
- ✅ BookingService - Working
- ✅ NotificationService - Working
- ✅ PaymentService - Working (Razorpay integration)

**Removed:**
- ❌ AuthService - Removed (redundant, UserService handles auth)

---

## 🎓 Learning Outcomes

This architecture demonstrates:
- ✅ Microservices design patterns
- ✅ API Gateway pattern
- ✅ JWT authentication
- ✅ Role-based authorization
- ✅ Service-to-service communication
- ✅ Database per service pattern
- ✅ Asynchronous operations
- ✅ PDF generation
- ✅ Email notifications
- ✅ Payment gateway integration

---

**Last Updated:** December 2024
**Version:** 1.0
**Status:** Production Ready (for case study/demo)
