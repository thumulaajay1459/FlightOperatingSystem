const API_BASE_URL = 'http://localhost:5000';

document.getElementById('searchForm').addEventListener('submit', async (e) => {
    e.preventDefault();
    
    const origin = document.getElementById('origin').value.toUpperCase();
    const destination = document.getElementById('destination').value.toUpperCase();
    const date = document.getElementById('date').value;
    
    await searchFlights(origin, destination, date);
});

async function searchFlights(origin, destination, date) {
    const resultsSection = document.getElementById('resultsSection');
    const flightResults = document.getElementById('flightResults');
    
    flightResults.innerHTML = '<p>Searching flights...</p>';
    resultsSection.style.display = 'block';
    
    try {
        const response = await fetch(
            `${API_BASE_URL}/flights/search?origin=${origin}&destination=${destination}&departureDate=${date}`
        );
        
        if (!response.ok) {
            throw new Error('Failed to fetch flights');
        }
        
        const flights = await response.json();
        displayFlights(flights);
        
    } catch (error) {
        flightResults.innerHTML = `
            <div class="error-message">
                Error loading flights. Please make sure the backend is running.
            </div>
        `;
    }
}

function displayFlights(flights) {
    const flightResults = document.getElementById('flightResults');
    
    if (!flights || flights.length === 0) {
        flightResults.innerHTML = '<div class="no-results">No flights found for your search.</div>';
        return;
    }
    
    flightResults.innerHTML = flights.map(flight => `
        <div class="flight-card">
            <div class="flight-info">
                <div class="flight-route">
                    ${flight.originAirportCode} → ${flight.destinationAirportCode}
                </div>
                <div class="flight-details">
                    Flight: ${flight.flightNumber} | 
                    Departure: ${new Date(flight.departureTime).toLocaleString()} | 
                    Arrival: ${new Date(flight.arrivalTime).toLocaleString()} | 
                    Available Seats: ${flight.availableSeats}
                </div>
            </div>
            <div class="flight-price">
                $${flight.price}
            </div>
        </div>
    `).join('');
}
