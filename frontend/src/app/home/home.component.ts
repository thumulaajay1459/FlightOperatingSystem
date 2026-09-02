import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpClientModule } from '@angular/common/http';
import { Router } from '@angular/router';

@Component({
  selector: 'app-home',
  imports: [CommonModule, FormsModule, HttpClientModule],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css',
  standalone: true
})
export class HomeComponent implements OnInit {
  tripType = 'OneWay';
  searchData = { origin: '', destination: '', departureDate: '', returnDate: '' };
  passengers = { adults: 1, children: 0, infants: 0 };
  flights: any[] = [];
  returnFlights: any[] = [];
  showResults = false;
  isLoading = false;
  errorMessage = '';
  minDate = '';
  airports: any[] = [];
  private apiUrl = 'http://localhost:5000';
  
  constructor(private http: HttpClient, private router: Router) {}
  
  ngOnInit() {
    this.setMinDate();
    this.loadAirports();
  }
  
  setMinDate() {
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    this.minDate = today.toISOString().split('T')[0];
  }
  
  loadAirports() {
    this.http.get<any>(`${this.apiUrl}/airports?paginate=false`).subscribe({
      next: (data) => {
        const uniqueAirports = data.reduce((acc: any[], current: any) => {
          const exists = acc.find(item => item.city === current.city && item.code === current.code);
          if (!exists) {
            acc.push(current);
          }
          return acc;
        }, []);
        this.airports = uniqueAirports.sort((a: any, b: any) => a.city.localeCompare(b.city));
      },
      error: (err) => {
        console.error('Error loading airports:', err);
      }
    });
  }
  
  searchFlights() {
    this.errorMessage = '';
    
    if (this.searchData.origin === this.searchData.destination) {
      this.errorMessage = 'Source and destination cannot be the same. Please select different locations.';
      return;
    }
    
    if (!this.searchData.origin || !this.searchData.destination) {
      this.errorMessage = 'Please select both origin and destination.';
      return;
    }
    
    this.isLoading = true;
    this.showResults = true;
    this.flights = [];
    this.returnFlights = [];
    
    let url = `${this.apiUrl}/flights/search-flights?from=${encodeURIComponent(this.searchData.origin)}&to=${encodeURIComponent(this.searchData.destination)}`;
    if (this.searchData.departureDate) {
      url += `&date=${this.searchData.departureDate}`;
    }
    
    this.http.get<any>(url).subscribe({
      next: (data) => {
        this.flights = data.outbound?.flights || data.flights || [];
        
        this.flights = this.flights.map((f: any) => ({
          ...f,
          economyPrice: f.pricing?.Economy || f.pricing?.economy || f.economyPrice || 0
        }));
        
        if (this.flights.length === 0) {
          const dateMsg = this.searchData.departureDate ? ` on ${this.searchData.departureDate}` : '';
          this.errorMessage = `No flights found from ${this.searchData.origin} to ${this.searchData.destination}${dateMsg}. Please try a different route or date.`;
        }
        
        if (this.tripType === 'RoundTrip' && this.searchData.returnDate) {
          this.searchReturnFlights();
        } else {
          this.isLoading = false;
        }
      },
      error: (err) => {
        this.errorMessage = err.error?.error || 'Error loading flights. Please try again.';
        this.isLoading = false;
      }
    });
  }
  
  searchReturnFlights() {
    let url = `${this.apiUrl}/flights/search-flights?from=${encodeURIComponent(this.searchData.destination)}&to=${encodeURIComponent(this.searchData.origin)}`;
    if (this.searchData.returnDate) {
      url += `&date=${this.searchData.returnDate}`;
    }
    
    this.http.get<any>(url).subscribe({
      next: (data) => {
        this.returnFlights = data.outbound?.flights || data.flights || [];
        
        this.returnFlights = this.returnFlights.map((f: any) => ({
          ...f,
          economyPrice: f.pricing?.Economy || f.pricing?.economy || f.economyPrice || 0
        }));
        
        this.isLoading = false;
      },
      error: (err) => {
        this.errorMessage = 'Error loading return flights.';
        this.isLoading = false;
      }
    });
  }
  
  getTotalPassengers(): number {
    return this.passengers.adults + this.passengers.children + this.passengers.infants;
  }
  
  calculateDuration(departure: string, arrival: string): string {
    const dep = new Date(departure);
    const arr = new Date(arrival);
    const diff = arr.getTime() - dep.getTime();
    const hours = Math.floor(diff / (1000 * 60 * 60));
    const minutes = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60));
    return `${hours}h ${minutes}m`;
  }
  
  goToChat() {
    this.router.navigate(['/chat']);
  }
  
  goToLogin() {
    this.router.navigate(['/login']);
  }
}
