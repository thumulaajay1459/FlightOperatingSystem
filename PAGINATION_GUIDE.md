# Pagination Implementation Guide

## Overview
Pagination has been implemented across all list endpoints in the Flight Operation System to improve performance and user experience.

---

## Default Pagination Parameters

All paginated endpoints support the following query parameters:

| Parameter | Type | Default | Max | Description |
|-----------|------|---------|-----|-------------|
| `page` | int | 1 | - | Current page number (starts from 1) |
| `pageSize` | int | 10 | 100 | Number of items per page |

---

## Response Format

All paginated endpoints return data in the following format:

```json
{
  "data": [...],
  "pagination": {
    "currentPage": 1,
    "pageSize": 10,
    "totalCount": 150,
    "totalPages": 15
  }
}
```

---

## Paginated Endpoints by Service

### **UserService (Port 5001)**

#### 1. Get All Users (Admin)
```http
GET /api/User?page=1&pageSize=10
Authorization: Bearer {admin_token}
```

**Default:** page=1, pageSize=50, max=100

---

### **FlightService (Port 5003)**

#### 1. Get All Airports (Admin)
```http
GET /api/Airports?page=1&pageSize=10
Authorization: Bearer {admin_token}
```

#### 2. Get Public Airports
```http
GET /api/Airports/public?page=1&pageSize=10
```

#### 3. Get All Aircraft (Admin)
```http
GET /api/Aircraft?page=1&pageSize=10
Authorization: Bearer {admin_token}
```

#### 4. Get All Routes (Admin)
```http
GET /api/Routes?page=1&pageSize=10
Authorization: Bearer {admin_token}
```

#### 5. Get All Flights
```http
GET /api/ScheduledFlights?page=1&pageSize=10
```

**Default:** page=1, pageSize=50, max=100

#### 6. Search Flights
```http
GET /api/ScheduledFlights/search?flightNumber=AI&page=1&pageSize=10
```

#### 7. Get Flights by Route
```http
GET /api/ScheduledFlights/by-route?originId=1&destinationId=2&page=1&pageSize=10
```

---

### **BookingService (Port 5004)**

#### 1. Get My Bookings
```http
GET /api/booking/my-bookings?page=1&pageSize=10
Authorization: Bearer {token}
```

#### 2. Get All Bookings (Admin)
```http
GET /api/booking/all?page=1&pageSize=10
Authorization: Bearer {admin_token}
```

#### 3. Get Bookings by Flight (Admin)
```http
GET /api/booking/by-flight/123?page=1&pageSize=10
Authorization: Bearer {admin_token}
```

---

### **NotificationService (Port 5005)**

#### 1. Get Notification History
```http
GET /api/notifications/history?page=1&pageSize=10
```

#### 2. Get User Notifications
```http
GET /api/notifications/user/{userId}?page=1&pageSize=10
```

---

### **PaymentService (Port 5006)**

#### 1. Get All Payments (Admin)
```http
GET /api/payments/all?page=1&pageSize=10
Authorization: Bearer {admin_token}
```

#### 2. Get User Payments
```http
GET /api/payments/user/{userId}?page=1&pageSize=10
Authorization: Bearer {token}
```

---

### **AIService (Port 5008)**

#### 1. Get Chat Sessions
```http
GET /api/chat/sessions?page=1&pageSize=10
Authorization: Bearer {token}
```

#### 2. Get Chat History
```http
GET /api/chat/sessions/{sessionId}/history?page=1&pageSize=20
Authorization: Bearer {token}
```

**Default:** page=1, pageSize=20, max=100

---

## Usage Examples

### Example 1: Get First Page of Flights
```bash
curl -X GET "http://localhost:5003/api/ScheduledFlights?page=1&pageSize=10"
```

**Response:**
```json
{
  "data": [
    {
      "flightId": 1,
      "flightNumber": "AI101",
      "origin": "Mumbai (BOM)",
      "destination": "Delhi (DEL)",
      ...
    }
  ],
  "pagination": {
    "currentPage": 1,
    "pageSize": 10,
    "totalCount": 45,
    "totalPages": 5
  }
}
```

### Example 2: Get User Bookings with Pagination
```bash
curl -X GET "http://localhost:5004/api/booking/my-bookings?page=2&pageSize=5" \
  -H "Authorization: Bearer {token}"
```

### Example 3: Search Flights with Pagination
```bash
curl -X GET "http://localhost:5003/api/ScheduledFlights/search?origin=BOM&destination=BLR&page=1&pageSize=10"
```

---

## Via API Gateway (Port 5000)

All endpoints are accessible through the API Gateway with the same pagination parameters:

```bash
# Get flights via gateway
GET http://localhost:5000/flights?page=1&pageSize=10

# Get bookings via gateway
GET http://localhost:5000/bookings/my-bookings?page=1&pageSize=10
```

---

## Implementation Details

### Validation Rules
- If `page < 1`, defaults to `1`
- If `pageSize < 1` or `pageSize > maxPageSize`, defaults to service-specific default
- Most services: default=10, max=100
- UserService & FlightService (GetAll): default=50, max=100
- AIService (history): default=20, max=100

### Performance Considerations
1. **Database Queries**: Uses `Skip()` and `Take()` for efficient pagination
2. **Count Queries**: Separate count query for total records
3. **Indexing**: Ensure proper database indexes on frequently queried columns
4. **Caching**: Consider implementing caching for frequently accessed pages

---

## Frontend Integration

### JavaScript/TypeScript Example
```typescript
async function getFlights(page: number = 1, pageSize: number = 10) {
  const response = await fetch(
    `http://localhost:5000/flights?page=${page}&pageSize=${pageSize}`
  );
  const result = await response.json();
  
  return {
    flights: result.data,
    currentPage: result.pagination.currentPage,
    totalPages: result.pagination.totalPages,
    totalCount: result.pagination.totalCount
  };
}
```

### React Example
```jsx
const [page, setPage] = useState(1);
const [flights, setFlights] = useState([]);
const [pagination, setPagination] = useState({});

useEffect(() => {
  fetch(`/api/flights?page=${page}&pageSize=10`)
    .then(res => res.json())
    .then(data => {
      setFlights(data.data);
      setPagination(data.pagination);
    });
}, [page]);

// Render pagination controls
<button onClick={() => setPage(page - 1)} disabled={page === 1}>
  Previous
</button>
<span>Page {pagination.currentPage} of {pagination.totalPages}</span>
<button onClick={() => setPage(page + 1)} disabled={page === pagination.totalPages}>
  Next
</button>
```

---

## Testing Pagination

### Test Case 1: Default Pagination
```http
GET /api/ScheduledFlights
Expected: page=1, pageSize=50
```

### Test Case 2: Custom Page Size
```http
GET /api/ScheduledFlights?pageSize=20
Expected: page=1, pageSize=20
```

### Test Case 3: Invalid Parameters
```http
GET /api/ScheduledFlights?page=-1&pageSize=200
Expected: page=1, pageSize=50 (defaults applied)
```

### Test Case 4: Last Page
```http
GET /api/ScheduledFlights?page=999
Expected: Empty data array, valid pagination metadata
```

---

## Migration Notes

### Breaking Changes
- All list endpoints now return `{ data: [], pagination: {} }` instead of just arrays
- Frontend applications need to access `response.data` instead of `response` directly

### Backward Compatibility
To maintain backward compatibility, you can:
1. Create separate endpoints (e.g., `/api/flights/paginated`)
2. Use query parameter to enable pagination (e.g., `?paginate=true`)
3. Update all clients to use new format (recommended)

---

## Best Practices

1. **Always specify pageSize** in frontend to ensure consistent UX
2. **Cache pagination results** for better performance
3. **Show loading indicators** during page transitions
4. **Validate page numbers** before making requests
5. **Handle empty results** gracefully
6. **Display total count** to users for better context
7. **Use reasonable page sizes** (10-50 items per page)

---

## Future Enhancements

1. **Cursor-based pagination** for real-time data
2. **Infinite scroll** support
3. **Custom sort parameters** with pagination
4. **Page size presets** (10, 25, 50, 100)
5. **Jump to page** functionality
6. **Pagination metadata caching**

---

## Troubleshooting

### Issue: Empty data array
**Solution:** Check if page number exceeds totalPages

### Issue: Slow pagination
**Solution:** Add database indexes on sorted/filtered columns

### Issue: Inconsistent counts
**Solution:** Use transactions or snapshot isolation for count queries

---

## API Gateway Configuration

The API Gateway (Ocelot) automatically forwards pagination parameters to downstream services. No additional configuration needed.

---

**Last Updated:** December 2024  
**Version:** 1.0
