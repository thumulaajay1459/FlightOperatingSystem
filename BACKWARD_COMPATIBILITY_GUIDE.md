# Backward Compatibility Guide

## Issue: Data Not Showing After Pagination

If your frontend is not showing data after implementing pagination, it's because the response format has changed.

---

## Quick Fix Options

### Option 1: Disable Pagination (Temporary)

Add `?paginate=false` to your requests to get the old format:

```bash
# Old format (array)
GET /api/Airports?paginate=false
GET /api/Aircraft?paginate=false
GET /api/Routes?paginate=false
GET /api/booking/my-bookings?paginate=false
```

**Response (Old Format):**
```json
[
  { "airportId": 1, "code": "BOM", "name": "Mumbai" },
  { "airportId": 2, "code": "DEL", "name": "Delhi" }
]
```

---

### Option 2: Update Frontend to Use New Format (Recommended)

Update your frontend code to access `response.data`:

**Before:**
```javascript
const airports = await fetch('/api/Airports').then(r => r.json());
// airports is an array
```

**After:**
```javascript
const response = await fetch('/api/Airports?page=1&pageSize=10').then(r => r.json());
const airports = response.data;  // Access the data property
const pagination = response.pagination;  // Access pagination info
```

---

## Endpoints with Backward Compatibility

The following endpoints support `?paginate=false`:

### FlightService
- `GET /api/Airports?paginate=false`
- `GET /api/Airports/public?paginate=false`
- `GET /api/Aircraft?paginate=false`
- `GET /api/Routes?paginate=false`

### BookingService
- `GET /api/booking/my-bookings?paginate=false`

---

## Frontend Update Examples

### JavaScript/Fetch
```javascript
// Option 1: Disable pagination
const airports = await fetch('/api/Airports?paginate=false').then(r => r.json());

// Option 2: Use pagination (recommended)
const response = await fetch('/api/Airports?page=1&pageSize=10').then(r => r.json());
const airports = response.data;
```

### Axios
```javascript
// Option 1: Disable pagination
const { data: airports } = await axios.get('/api/Airports?paginate=false');

// Option 2: Use pagination (recommended)
const { data: response } = await axios.get('/api/Airports?page=1&pageSize=10');
const airports = response.data;
```

### React Example
```jsx
const [airports, setAirports] = useState([]);
const [pagination, setPagination] = useState({});

// Option 1: Disable pagination
useEffect(() => {
  fetch('/api/Airports?paginate=false')
    .then(res => res.json())
    .then(data => setAirports(data));
}, []);

// Option 2: Use pagination (recommended)
useEffect(() => {
  fetch('/api/Airports?page=1&pageSize=10')
    .then(res => res.json())
    .then(response => {
      setAirports(response.data);
      setPagination(response.pagination);
    });
}, []);
```

### Angular Example
```typescript
// Option 1: Disable pagination
this.http.get<Airport[]>('/api/Airports?paginate=false')
  .subscribe(airports => this.airports = airports);

// Option 2: Use pagination (recommended)
this.http.get<PaginatedResponse<Airport>>('/api/Airports?page=1&pageSize=10')
  .subscribe(response => {
    this.airports = response.data;
    this.pagination = response.pagination;
  });
```

---

## Testing Both Formats

### Test Old Format (No Pagination)
```bash
curl "http://localhost:5003/api/Airports?paginate=false"
```

**Response:**
```json
[
  { "airportId": 1, "code": "BOM", "name": "Mumbai", "city": "Mumbai", "country": "India" },
  { "airportId": 2, "code": "DEL", "name": "Delhi", "city": "Delhi", "country": "India" }
]
```

### Test New Format (With Pagination)
```bash
curl "http://localhost:5003/api/Airports?page=1&pageSize=10"
```

**Response:**
```json
{
  "data": [
    { "airportId": 1, "code": "BOM", "name": "Mumbai", "city": "Mumbai", "country": "India" },
    { "airportId": 2, "code": "DEL", "name": "Delhi", "city": "Delhi", "country": "India" }
  ],
  "pagination": {
    "currentPage": 1,
    "pageSize": 10,
    "totalCount": 2,
    "totalPages": 1
  }
}
```

---

## Migration Strategy

### Phase 1: Use Backward Compatibility (Now)
```javascript
// Keep using old format temporarily
fetch('/api/Airports?paginate=false')
```

### Phase 2: Update Frontend Gradually
```javascript
// Update one component at a time
const response = await fetch('/api/Airports?page=1&pageSize=10');
const airports = response.data;
```

### Phase 3: Remove Backward Compatibility (Future)
Once all frontends are updated, remove the `paginate=false` option.

---

## Common Issues & Solutions

### Issue 1: Empty Array Showing
**Cause:** Frontend expecting array but getting object with `data` property

**Solution:**
```javascript
// Change this:
const airports = response;

// To this:
const airports = response.data;
```

### Issue 2: "Cannot read property 'map' of undefined"
**Cause:** Trying to map over `response` instead of `response.data`

**Solution:**
```javascript
// Change this:
{response.map(item => ...)}

// To this:
{response.data.map(item => ...)}
```

### Issue 3: Pagination Controls Not Working
**Cause:** Not accessing pagination metadata

**Solution:**
```javascript
const response = await fetch('/api/Airports?page=1&pageSize=10');
const { data, pagination } = await response.json();

// Now you can use:
// pagination.currentPage
// pagination.totalPages
// pagination.totalCount
```

---

## Recommended Approach

1. **Immediate Fix:** Add `?paginate=false` to all affected API calls
2. **Update Frontend:** Modify code to use `response.data` and `response.pagination`
3. **Test Thoroughly:** Ensure all features work with new format
4. **Remove Fallback:** Once confident, remove `?paginate=false` usage

---

## Example: Complete Frontend Update

### Before (Not Working)
```javascript
async function loadAirports() {
  const airports = await fetch('/api/Airports').then(r => r.json());
  displayAirports(airports);
}
```

### After (Working with Pagination)
```javascript
async function loadAirports(page = 1, pageSize = 10) {
  const response = await fetch(`/api/Airports?page=${page}&pageSize=${pageSize}`)
    .then(r => r.json());
  
  displayAirports(response.data);
  displayPagination(response.pagination);
}

function displayPagination(pagination) {
  document.getElementById('page-info').textContent = 
    `Page ${pagination.currentPage} of ${pagination.totalPages}`;
}
```

---

## Need Help?

- See **PAGINATION_GUIDE.md** for complete documentation
- See **PAGINATION_TESTS.http** for API examples
- Check console for response format

---

**Remember:** The `paginate=false` option is for backward compatibility only. Update your frontend to use the new paginated format for better performance!
