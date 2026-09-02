import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpClientModule } from '@angular/common/http';

@Component({
  selector: 'app-root',
  imports: [CommonModule, FormsModule, HttpClientModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css',
  standalone: true
})
export class AppComponent {
  tripType: 'OneWay' | 'RoundTrip' = 'OneWay';
  
  searchData = {
    origin: '',
    destination: '',
    departureDate: '',
    returnDate: ''
  };
  
  passengers = {
    adults: 1,
    children: 0,
    infants: 0
  };
  
  flights: any[] = [];
  returnFlights: any[] = [];
  showResults = false;
  isLoading = false;
  errorMessage = '';
  
  private apiUrl = 'http://localhost:5000';
  
  constructor(private http: HttpClient) {}
  
  searchFlights() {
    this.isLoading = true;
    this.errorMessage = '';
    this.showResults = true;
    this.flights = [];
    this.returnFlights = [];
    
    const url = `${this.apiUrl}/flights/search?from=${this.searchData.origin}&to=${this.searchData.destination}&date=${this.searchData.departureDate}`;
    
    this.http.get<any[]>(url).subscribe({
      next: (data) => {
        this.flights = data;
        
        if (this.tripType === 'RoundTrip' && this.searchData.returnDate) {
          this.searchReturnFlights();
        } else {
          this.isLoading = false;
        }
      },
      error: (error) => {
        this.errorMessage = 'Error loading flights. Please make sure the backend is running.';
        this.isLoading = false;
      }
    });
  }
  
  searchReturnFlights() {
    const url = `${this.apiUrl}/flights/search?from=${this.searchData.destination}&to=${this.searchData.origin}&date=${this.searchData.returnDate}`;
    
    this.http.get<any[]>(url).subscribe({
      next: (data) => {
        this.returnFlights = data;
        this.isLoading = false;
      },
      error: (error) => {
        this.errorMessage = 'Error loading return flights.';
        this.isLoading = false;
      }
    });
  }
  
  getTotalPassengers(): number {
    return this.passengers.adults + this.passengers.children + this.passengers.infants;
  }
  
  goToLogin() {
    window.location.href = '/login';
  }

  goToChat() {
    window.location.href = '/chat';
  }
}
