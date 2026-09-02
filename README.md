# ✈️ Flight Operation System

A microservices-based flight booking system built with ASP.NET Core, featuring JWT authentication, role-based authorization, PDF ticket generation, and email notifications.

---

## 🏗️ Architecture

```
API Gateway (5000)
    ↓
    ├── UserService (5001) - Authentication & User Management
    ├── FlightService (5003) - Flight Operations
    ├── BookingService (5004) - Bookings & Tickets
    ├── NotificationService (5005) - Email Notifications
    ├── PaymentService (5006) - Payment Processing
    └── AIService (5008) - AI Chatbot Assistant
```

---

## 🚀 Quick Start

### **Prerequisites:**
- .NET 9.0 SDK
- SQL Server
- Visual Studio / VS Code / Rider

### **Start Services:**
```bash
# Start each service in a separate terminal
cd APIGateway && dotnet run
cd UserService && dotnet run
cd FlightService && dotnet run
cd BookingService && dotnet run
cd NotificationService && dotnet run
cd PaymentService && dotnet run
cd AIService && dotnet run
```

### **Test:**
```bash
# Register
POST http://localhost:5000/auth/register

# Login
POST http://localhost:5000/auth/login

# Use JWT token for authenticated requests
```

---

## 📚 Documentation

| File | Description |
|------|-------------|
| **[CHANGES_COMPLETE.md](CHANGES_COMPLETE.md)** | ⭐ Start here - Summary of system |
| **[ARCHITECTURE_GUIDE.md](ARCHITECTURE_GUIDE.md)** | Complete architecture documentation |
| **[QUICK_REFERENCE.md](QUICK_REFERENCE.md)** | API endpoints quick reference |
| **[PAGINATION_GUIDE.md](PAGINATION_GUIDE.md)** | Pagination implementation guide |
| **[AUTHSERVICE_REMOVAL_SUMMARY.md](AUTHSERVICE_REMOVAL_SUMMARY.md)** | Authentication flow details |
| **[QUICK_START_GUIDE.md](QUICK_START_GUIDE.md)** | Detailed setup guide |
| **[ROLE_BASED_ACCESS_GUIDE.md](ROLE_BASED_ACCESS_GUIDE.md)** | Authorization details |

---

## 🎯 Features

- ✅ **Microservices Architecture** - 6 independent services
- ✅ **API Gateway** - Centralized entry point with Ocelot
- ✅ **JWT Authentication** - Secure token-based auth
- ✅ **Role-Based Authorization** - Admin and User roles
- ✅ **Flight Management** - CRUD operations for flights, aircraft, airports
- ✅ **Booking System** - Create, view, cancel bookings
- ✅ **PDF Tickets** - Generate professional PDF tickets
- ✅ **Email Notifications** - Send tickets via email with PDF attachment
- ✅ **AI Chatbot** - Intelligent flight booking assistant with RAG
- ✅ **Database per Service** - Independent data storage
- ✅ **Pagination** - Efficient data retrieval for all list endpoints

---

## 🔐 Authentication

### **How It Works:**

1. **UserService** handles authentication:
   - Validates username/password
   - Creates JWT tokens
   - Manages user accounts

2. **API Gateway** validates tokens:
   - Checks token signature
   - Verifies expiration
   - Enforces role-based access

3. **No separate AuthService needed** - UserService handles both auth and user management (appropriate for this scale)

---

## 📊 Services

### **API Gateway (Port 5000)**
- Routes all external requests
- Validates JWT tokens
- Enforces role-based authorization

### **UserService (Port 5001)**
- User registration & login
- JWT token creation
- User profile management
- Password hashing

### **FlightService (Port 5003)**
- Flight schedule management
- Aircraft management
- Airport management
- Route management

### **BookingService (Port 5004)**
- Booking creation
- Seat management
- PDF ticket generation
- Integration with other services

### **NotificationService (Port 5005)**
- Email sending via SMTP
- PDF attachment handling
- Notification tracking

### **PaymentService (Port 5006)**
- Payment order creation
- Payment verification
- Razorpay integration
- Refund processing

### **AIService (Port 5008)**
- AI-powered chatbot using Azure AI (Phi-4)
- RAG (Retrieval-Augmented Generation)
- Real-time data from other services
- Conversation history management
- Intent detection & context-aware responses

---

## 🔧 Technology Stack

- **Framework:** ASP.NET Core 9.0
- **API Gateway:** Ocelot
- **Authentication:** JWT Bearer Tokens
- **ORM:** Entity Framework Core
- **Database:** SQL Server
- **PDF Generation:** QuestPDF
- **Email:** MailKit (SMTP)
- **AI:** Azure AI (Phi-4-mini)
- **RAG:** In-memory vector store

---

## 📋 API Endpoints

### **Public:**
- `POST /auth/register` - Register user
- `POST /auth/login` - Login
- `GET /flights` - View flights
- `GET /flights/search` - Search flights

### **Authenticated:**
- `GET /profile` - Get profile
- `POST /bookings/create` - Create booking
- `GET /bookings/my-bookings` - View bookings
- `GET /bookings/{id}/download-ticket` - Download PDF
- `POST /chat` - Chat with AI assistant
- `GET /chat/sessions` - Get chat sessions
- `GET /chat/sessions/{id}/history` - Get chat history

### **Admin Only:**
- `GET /admin/users` - View all users
- `POST /admin/aircraft` - Add aircraft
- `POST /admin/airports` - Add airport
- `POST /admin/routes` - Add route
- `POST /admin/flights` - Add flight

---

## 🧪 Testing

### **1. Create Admin:**
```http
POST http://localhost:5000/auth/register
{
  "firstName": "Admin",
  "lastName": "User",
  "email": "admin@test.com",
  "password": "Admin@123",
  "role": "Admin"
}
```

### **2. Setup Data:**
```http
# Login as admin
POST http://localhost:5000/auth/login

# Add aircraft, airports, routes, flights
POST http://localhost:5000/admin/aircraft
POST http://localhost:5000/admin/airports
POST http://localhost:5000/admin/routes
POST http://localhost:5000/admin/flights
```

### **3. User Journey:**
```http
# Register user
POST http://localhost:5000/auth/register

# Login
POST http://localhost:5000/auth/login

# Create profile
POST http://localhost:5000/profile

# Search flights
GET http://localhost:5000/flights/search

# Create booking
POST http://localhost:5000/bookings/create

# Download ticket
GET http://localhost:5000/bookings/{id}/download-ticket
```

---

## 🎓 Learning Outcomes

This project demonstrates:
- Microservices architecture patterns
- API Gateway pattern
- JWT authentication & authorization
- Role-based access control
- Service-to-service communication
- Database per service pattern
- PDF generation
- Email notifications
- RESTful API design
- AI integration with RAG
- Conversational AI

---

## 📞 Support

For detailed information, refer to:
- **[ARCHITECTURE_GUIDE.md](ARCHITECTURE_GUIDE.md)** - Complete system overview
- **[QUICK_REFERENCE.md](QUICK_REFERENCE.md)** - API reference
- **[CHANGES_COMPLETE.md](CHANGES_COMPLETE.md)** - System summary

---

## ✅ Status

**System Status:** ✅ Fully Functional

**Documentation:** ✅ Complete

**Ready for:** ✅ Demo / Case Study / Production

---

## 📝 License

This is a case study / learning project.

---

## 👥 Contributors

Developed as a microservices case study project.

---

**Last Updated:** December 2024
**Version:** 1.0
