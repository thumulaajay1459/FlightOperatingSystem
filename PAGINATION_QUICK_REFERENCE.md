# Pagination Quick Reference Card

## 🚀 Quick Start

### Basic Usage
```http
GET /api/endpoint?page=1&pageSize=10
```

### Response Format
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

## 📋 All Paginated Endpoints

| Service | Endpoint | Default PageSize | Max |
|---------|----------|------------------|-----|
| **UserService** | `GET /api/User` | 50 | 100 |
| **FlightService** | `GET /api/Airports` | 10 | 100 |
| **FlightService** | `GET /api/Airports/public` | 10 | 100 |
| **FlightService** | `GET /api/Aircraft` | 10 | 100 |
| **FlightService** | `GET /api/Routes` | 10 | 100 |
| **FlightService** | `GET /api/ScheduledFlights` | 50 | 100 |
| **FlightService** | `GET /api/ScheduledFlights/search` | 10 | 100 |
| **FlightService** | `GET /api/ScheduledFlights/by-route` | 10 | 100 |
| **BookingService** | `GET /api/booking/my-bookings` | 10 | 100 |
| **BookingService** | `GET /api/booking/all` | 10 | 100 |
| **BookingService** | `GET /api/booking/by-flight/{id}` | 10 | 100 |
| **NotificationService** | `GET /api/notifications/history` | 10 | 100 |
| **NotificationService** | `GET /api/notifications/user/{id}` | 10 | 100 |
| **PaymentService** | `GET /api/payments/all` | 10 | 100 |
| **PaymentService** | `GET /api/payments/user/{id}` | 10 | 100 |
| **AIService** | `GET /api/chat/sessions` | 10 | 100 |
| **AIService** | `GET /api/chat/sessions/{id}/history` | 20 | 100 |

---

## 🔧 Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `page` | int | 1 | Current page number |
| `pageSize` | int | varies | Items per page |

---

## 💡 Examples

### Get First Page
```bash
curl "http://localhost:5003/api/ScheduledFlights?page=1&pageSize=10"
```

### Get Second Page
```bash
curl "http://localhost:5003/api/ScheduledFlights?page=2&pageSize=10"
```

### With Authentication
```bash
curl "http://localhost:5004/api/booking/my-bookings?page=1&pageSize=10" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### Via API Gateway
```bash
curl "http://localhost:5000/flights?page=1&pageSize=10"
```

---

## ⚡ Frontend Integration

### JavaScript
```javascript
const response = await fetch('/api/flights?page=1&pageSize=10');
const { data, pagination } = await response.json();
```

### React Hook
```jsx
const [page, setPage] = useState(1);
const { data, pagination } = useFetch(`/api/flights?page=${page}&pageSize=10`);
```

### Pagination Controls
```jsx
<button onClick={() => setPage(p => p - 1)} disabled={page === 1}>
  Previous
</button>
<span>Page {pagination.currentPage} of {pagination.totalPages}</span>
<button onClick={() => setPage(p => p + 1)} disabled={page === pagination.totalPages}>
  Next
</button>
```

---

## ✅ Validation Rules

- `page < 1` → defaults to `1`
- `pageSize < 1` → defaults to service default
- `pageSize > 100` → defaults to service default
- Invalid page → returns empty array with valid metadata

---

## 🎯 Common Use Cases

### Load More / Infinite Scroll
```javascript
let page = 1;
const loadMore = async () => {
  const response = await fetch(`/api/flights?page=${page++}&pageSize=10`);
  const { data } = await response.json();
  items.push(...data);
};
```

### Page Navigation
```javascript
const goToPage = async (pageNum) => {
  const response = await fetch(`/api/flights?page=${pageNum}&pageSize=10`);
  const { data, pagination } = await response.json();
  setItems(data);
  setCurrentPage(pagination.currentPage);
  setTotalPages(pagination.totalPages);
};
```

### Show All (with limit)
```javascript
// Get maximum allowed items
const response = await fetch('/api/flights?page=1&pageSize=100');
```

---

## 🐛 Troubleshooting

| Issue | Solution |
|-------|----------|
| Empty data array | Check if page > totalPages |
| Slow response | Reduce pageSize or add DB indexes |
| Wrong total count | Ensure filters are applied to count query |
| Missing pagination | Check response.data instead of response |

---

## 📚 Documentation

- **Full Guide:** [PAGINATION_GUIDE.md](PAGINATION_GUIDE.md)
- **Implementation:** [PAGINATION_IMPLEMENTATION_SUMMARY.md](PAGINATION_IMPLEMENTATION_SUMMARY.md)
- **Tests:** [PAGINATION_TESTS.http](PAGINATION_TESTS.http)

---

## 🔗 Related Endpoints

### Non-Paginated (Single Items)
- `GET /api/User/{id}` - Get user by ID
- `GET /api/ScheduledFlights/{id}` - Get flight by ID
- `GET /api/booking/{id}` - Get booking by ID

### Search with Pagination
- `GET /api/ScheduledFlights/search?origin=BOM&page=1&pageSize=10`
- `GET /api/search/flights` (POST) - Returns paginated results

---

**Quick Tip:** Always specify `pageSize` in production for consistent UX!

**Last Updated:** December 2024
