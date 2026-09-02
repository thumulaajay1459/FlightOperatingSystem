-- ============================================
-- Add Test Data: BOM (Mumbai) to BLR (Bangalore) Route
-- ============================================

-- Step 1: Check if airports exist
SELECT * FROM Airports WHERE Code IN ('BOM', 'BLR');

-- Step 2: Add airports if they don't exist
IF NOT EXISTS (SELECT 1 FROM Airports WHERE Code = 'BOM')
BEGIN
    INSERT INTO Airports (Code, Name, City, Country)
    VALUES ('BOM', 'Chhatrapati Shivaji Maharaj International Airport', 'Mumbai', 'India');
END

IF NOT EXISTS (SELECT 1 FROM Airports WHERE Code = 'BLR')
BEGIN
    INSERT INTO Airports (Code, Name, City, Country)
    VALUES ('BLR', 'Kempegowda International Airport', 'Bangalore', 'India');
END

-- Step 3: Get airport IDs
DECLARE @BomId INT = (SELECT AirportId FROM Airports WHERE Code = 'BOM');
DECLARE @BlrId INT = (SELECT AirportId FROM Airports WHERE Code = 'BLR');

-- Step 4: Add route if it doesn't exist
IF NOT EXISTS (SELECT 1 FROM Routes WHERE OriginAirportId = @BomId AND DestinationAirportId = @BlrId)
BEGIN
    INSERT INTO Routes (OriginAirportId, DestinationAirportId, DistanceKm)
    VALUES (@BomId, @BlrId, 840);
END

-- Step 5: Get route ID
DECLARE @RouteId INT = (SELECT RouteId FROM Routes WHERE OriginAirportId = @BomId AND DestinationAirportId = @BlrId);

-- Step 6: Check if aircraft exists
IF NOT EXISTS (SELECT 1 FROM Aircraft)
BEGIN
    INSERT INTO Aircraft (Manufacturer, Model, TotalSeats, EconomySeats, PremiumEconomySeats, BusinessSeats, FirstClassSeats)
    VALUES 
    ('Boeing', '737-800', 180, 150, 20, 8, 2),
    ('Airbus', 'A320', 180, 150, 20, 8, 2);
END

-- Step 7: Get aircraft ID
DECLARE @AircraftId INT = (SELECT TOP 1 AircraftId FROM Aircraft);

-- Step 8: Add flights for May 2026
DECLARE @FlightDate DATE = '2026-05-14';
DECLARE @Counter INT = 0;

WHILE @Counter < 7  -- Add flights for 7 days
BEGIN
    -- Morning Flight (6:00 AM)
    INSERT INTO Flights (FlightNumber, AircraftId, RouteId, DepartureTime, ArrivalTime, Status, 
                        BasePrice, EconomyPrice, PremiumEconomyPrice, BusinessPrice, FirstClassPrice,
                        ChildDiscountPercent, InfantDiscountPercent)
    VALUES (
        'AI-' + CAST(800 + @Counter AS VARCHAR),
        @AircraftId,
        @RouteId,
        DATEADD(DAY, @Counter, CAST(@FlightDate AS DATETIME)) + CAST('06:00:00' AS DATETIME),
        DATEADD(DAY, @Counter, CAST(@FlightDate AS DATETIME)) + CAST('07:30:00' AS DATETIME),
        'Scheduled',
        4500, 4500, 7000, 12000, 20000,
        25, 90
    );

    -- Afternoon Flight (2:00 PM)
    INSERT INTO Flights (FlightNumber, AircraftId, RouteId, DepartureTime, ArrivalTime, Status, 
                        BasePrice, EconomyPrice, PremiumEconomyPrice, BusinessPrice, FirstClassPrice,
                        ChildDiscountPercent, InfantDiscountPercent)
    VALUES (
        'SG-' + CAST(900 + @Counter AS VARCHAR),
        @AircraftId,
        @RouteId,
        DATEADD(DAY, @Counter, CAST(@FlightDate AS DATETIME)) + CAST('14:00:00' AS DATETIME),
        DATEADD(DAY, @Counter, CAST(@FlightDate AS DATETIME)) + CAST('15:30:00' AS DATETIME),
        'Scheduled',
        5000, 5000, 7500, 13000, 22000,
        25, 90
    );

    -- Evening Flight (6:00 PM)
    INSERT INTO Flights (FlightNumber, AircraftId, RouteId, DepartureTime, ArrivalTime, Status, 
                        BasePrice, EconomyPrice, PremiumEconomyPrice, BusinessPrice, FirstClassPrice,
                        ChildDiscountPercent, InfantDiscountPercent)
    VALUES (
        '6E-' + CAST(1000 + @Counter AS VARCHAR),
        @AircraftId,
        @RouteId,
        DATEADD(DAY, @Counter, CAST(@FlightDate AS DATETIME)) + CAST('18:00:00' AS DATETIME),
        DATEADD(DAY, @Counter, CAST(@FlightDate AS DATETIME)) + CAST('19:30:00' AS DATETIME),
        'Scheduled',
        4800, 4800, 7200, 12500, 21000,
        25, 90
    );

    SET @Counter = @Counter + 1;
END

-- Step 9: Verify data
SELECT 
    f.FlightNumber,
    f.DepartureTime,
    f.ArrivalTime,
    f.EconomyPrice,
    o.Code + ' - ' + o.City AS Origin,
    d.Code + ' - ' + d.City AS Destination
FROM Flights f
JOIN Routes r ON f.RouteId = r.RouteId
JOIN Airports o ON r.OriginAirportId = o.AirportId
JOIN Airports d ON r.DestinationAirportId = d.AirportId
WHERE o.Code = 'BOM' AND d.Code = 'BLR'
ORDER BY f.DepartureTime;

PRINT 'Test data added successfully!';
PRINT 'Added 21 flights (3 per day for 7 days) from BOM to BLR starting 2026-05-14';
