# 🚀 Flight Search System - Quick Reference

## 📌 New Endpoints

### 1. Airport Autocomplete
```
GET /api/search/airports?q={query}&limit={limit}
```
**Use Case:** User types in search box, show suggestions

**Examples:**
- `/api/search/airports?q=hyd` → Hyderabad airports
- `/api/search/airports?q=del` → Delhi airports
- `/api/search/airports?q=mumb` → Mumbai airports

---

### 2. Smart Flight Search
```
POST /api/search/flights
```

**Request Body:**
```json
{
  "origin": "HYD",              // IATA code or city name
  "destination": "DEL",         // IATA code or city name
  "departureDate": "2024-12-25",
  "returnDate": "2024-12-30",   // Optional (for round-trip)
  "adults": 2,
  "children": 1,
  "infants": 0,
  "cabinClass": "Economy",      // Economy, Business, FirstClass
  "sortBy": "price",            // price, duration, departure
  "maxPrice": 10000,            // Optional
  "timePreference": "morning"   // morning, afternoon, evening
}
```

**Response:**
```json
{
  "metadata": {
    "origin": "Hyderabad (HYD)",
    "destination": "Delhi (DEL)",
    "totalResults": 5
  },
  "outboundFlights": [...],
  "returnFlights": [...],
  "alternatives": {
    "nearbyDates": [...],
    "message": "..."
  }
}
```

---

### 3. Alternate Flights
```
GET /api/search/alternates?originId={id}&destId={id}&date={date}
```
**Use Case:** Show ±3 days options when no flights found

---

## 🎯 Key Features

### ✅ Smart Airport Resolution
- **IATA Codes:** `HYD`, `DEL`, `BOM`
- **City Names:** `Hyderabad`, `Delhi`, `Mumbai`
- **Partial Text:** `hyd`, `del`, `mumb`
- **Fuzzy Matching:** `hyderbad` → `Hyderabad`

### ✅ Flexible Search
- One-way trips
- Round-trip bookings
- Multiple passengers
- Cabin class selection
- Price filtering
- Time preferences

### ✅ Alternate Suggestions
- ±3 days from selected date
- Cheaper flight options
- Clear messaging when no flights

### ✅ Sorting Options
- `price` - Cheapest first
- `duration` - Shortest first
- `departure` - Earliest first

---

## 🔧 Implementation Checklist

- [x] Create DTOs (`FlightSearchDtos.cs`)
- [x] Create Service (`FlightSearchService.cs`)
- [x] Create Controller (`SearchController.cs`)
- [x] Register Service in DI (`Program.cs`)
- [ ] Apply Database Indexes (`DatabaseIndexes.sql`)
- [ ] Update API Gateway (`ocelot.json`)
- [ ] Test Endpoints (`FLIGHT_SEARCH_TESTS.http`)
- [ ] Integrate with Frontend

---

## 📊 Performance Tips

### Current (Database-First)
```csharp
// Good for: 0-100K flights
- SQL Server with indexes
- LINQ queries
- AsNoTracking for reads
```

### Next (Add Caching)
```csharp
// Good for: 100K-500K flights
services.AddMemoryCache();
// Cache popular routes for 5 minutes
```

### Future (Elasticsearch)
```csharp
// Good for: 500K+ flights
- Full-text search
- Advanced fuzzy matching
- Geo-spatial queries
```

---

## 🧪 Testing

### Test Autocomplete
```bash
curl "http://localhost:5003/api/search/airports?q=hyd"
```

### Test Search
```bash
curl -X POST http://localhost:5003/api/search/flights \
  -H "Content-Type: application/json" \
  -d '{
    "origin": "HYD",
    "destination": "DEL",
    "departureDate": "2024-12-25",
    "adults": 1
  }'
```

---

## 🎨 Frontend Integration

### Autocomplete (Debounced)
```typescript
searchAirports(query: string) {
  if (query.length < 2) return;
  
  this.http.get(`/search/airports?q=${query}`)
    .subscribe(airports => this.suggestions = airports);
}
```

### Search Form
```typescript
searchFlights() {
  this.http.post('/search/flights', this.searchForm.value)
    .subscribe(result => {
      this.flights = result.outboundFlights;
      this.alternates = result.alternatives;
    });
}
```

---

## 📈 Scaling Path

1. **Phase 1 (Now):** Database + Indexes
2. **Phase 2:** Add Redis caching
3. **Phase 3:** Introduce Elasticsearch
4. **Phase 4:** Microservices architecture

---

## ✅ Comparison

| Feature | Old System | New System |
|---------|-----------|------------|
| Search Type | GET with query params | POST with JSON body |
| Airport Input | Exact match only | Fuzzy matching |
| Autocomplete | ❌ No | ✅ Yes |
| Alternates | ❌ No | ✅ ±3 days |
| Sorting | Limited | Multiple options |
| Filtering | Basic | Advanced |
| Response | Simple list | Structured with metadata |

---

## 🚀 Next Steps

1. **Apply indexes** → Run `DatabaseIndexes.sql`
2. **Update Gateway** → Add routes to `ocelot.json`
3. **Test** → Use `FLIGHT_SEARCH_TESTS.http`
4. **Frontend** → Integrate autocomplete
5. **Monitor** → Check query performance
6. **Optimize** → Add caching if needed

---

## 📚 Documentation

- **Full Guide:** `FLIGHT_SEARCH_ARCHITECTURE.md`
- **Tests:** `FLIGHT_SEARCH_TESTS.http`
- **This File:** Quick reference

---

**Status:** ✅ Ready for Production
**Last Updated:** December 2024
