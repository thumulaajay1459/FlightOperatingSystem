-- Run this in SQL Server Management Studio or via EF Core migration

-- Index on Airport search fields
CREATE NONCLUSTERED INDEX IX_Airports_Search 
ON Airports (Code, City, Name) 
INCLUDE (Country);

-- Index on Flight search by route and date
CREATE NONCLUSTERED INDEX IX_Flights_RouteDate 
ON Flights (RouteId, DepartureTime, Status) 
INCLUDE (FlightNumber, ArrivalTime, EconomyPrice);

-- Index on Route origin/destination lookup
CREATE NONCLUSTERED INDEX IX_Routes_OriginDestination 
ON Routes (OriginAirportId, DestinationAirportId);

-- Index on FlightSeat for availability checks
CREATE NONCLUSTERED INDEX IX_FlightSeats_Availability 
ON FlightSeats (FlightId, Status, BlockedUntil);
