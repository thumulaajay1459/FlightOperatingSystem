import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpClientModule, HttpHeaders } from '@angular/common/http';
import { Router, ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-user-dashboard',
  imports: [CommonModule, FormsModule, HttpClientModule],
  templateUrl: './user-dashboard.component.html',
  styleUrl: './user-dashboard.component.css',
  standalone: true
})
export class UserDashboardComponent implements OnInit {
  activeTab = 'search';
  user: any = null;
  userProfile: any = null;
  tripType = 'OneWay';
  searchData = { origin: '', destination: '', departureDate: '', returnDate: '' };
  passengers = { adults: 1, children: 0, infants: 0 };
  flights: any[] = [];
  returnFlights: any[] = [];
  showResults = false;
  isLoading = false;
  errorMessage = '';
  successMessage = '';
  airports: any[] = [];
  updateUserData = { firstName: '', lastName: '', email: '' };
  profileData = { passportNumber: '', gender: '', age: 0, nationality: '', dateOfBirth: '' };
  passwordData = { currentPassword: '', newPassword: '', confirmPassword: '' };
  showCurrentPassword = false;
  showNewPassword = false;
  showConfirmPassword = false;
  bookings: any[] = [];
  minDate = '';
  maxDateOfBirth = '';
  private apiUrl = 'http://localhost:5000';
  
  constructor(private http: HttpClient, private router: Router, private route: ActivatedRoute) {}
  
  ngOnInit() {
    this.setMinDate();
    this.setMaxDateOfBirth();
    // Check for tab query parameter
    this.route.queryParams.subscribe(params => {
      if (params['tab']) {
        this.activeTab = params['tab'];
        
        // Show success message if coming from payment
        if (params['tab'] === 'bookings' && params['paymentSuccess']) {
          this.successMessage = '🎉 Payment successful! Your booking is confirmed and ticket has been downloaded.';
          setTimeout(() => this.successMessage = '', 5000);
        }
      }
    });
    
    this.loadUserData();
    this.loadUserProfile();
    this.loadBookings();
    this.loadAirports();
  }
  
  setMinDate() {
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    this.minDate = today.toISOString().split('T')[0];
  }
  
  setMaxDateOfBirth() {
    const today = new Date();
    this.maxDateOfBirth = today.toISOString().split('T')[0];
  }
  
  getHeaders() {
    const token = localStorage.getItem('accessToken');
    return new HttpHeaders({ 'Authorization': `Bearer ${token}` });
  }
  
  loadUserData() {
    this.http.get<any>(`${this.apiUrl}/auth/me`, { headers: this.getHeaders() }).subscribe({
      next: (data) => {
        this.user = data;
        this.updateUserData = { firstName: data.firstName, lastName: data.lastName, email: data.email };
      },
      error: () => this.router.navigate(['/login'])
    });
  }
  
  loadUserProfile() {
    this.http.get<any>(`${this.apiUrl}/profile`, { headers: this.getHeaders() }).subscribe({
      next: (data) => {
        this.userProfile = data;
        this.profileData = {
          passportNumber: data.passportNumber || '',
          gender: data.gender || '',
          age: data.age || 0,
          nationality: data.nationality || '',
          dateOfBirth: data.dateOfBirth ? data.dateOfBirth.split('T')[0] : ''
        };
      },
      error: () => {}
    });
  }
  
  loadBookings() {
    this.http.get<any[]>(`${this.apiUrl}/bookings/my-bookings`, { headers: this.getHeaders() }).subscribe({
      next: (data) => this.bookings = data,
      error: () => {}
    });
  }
  
  searchFlights() {
    this.errorMessage = '';
    
    // Validate that origin and destination are different
    if (this.searchData.origin === this.searchData.destination) {
      this.errorMessage = 'Source and destination cannot be the same. Please select different locations.';
      return;
    }
    
    // Validate that origin and destination are selected
    if (!this.searchData.origin || !this.searchData.destination) {
      this.errorMessage = 'Please select both origin and destination.';
      return;
    }
    
    this.isLoading = true;
    this.showResults = true;
    this.flights = [];
    this.returnFlights = [];
    
    console.log('Searching with:', {
      origin: this.searchData.origin,
      destination: this.searchData.destination,
      date: this.searchData.departureDate
    });
    
    // Build URL with optional date parameter
    let url = `${this.apiUrl}/flights/search-flights?from=${encodeURIComponent(this.searchData.origin)}&to=${encodeURIComponent(this.searchData.destination)}`;
    if (this.searchData.departureDate) {
      url += `&date=${this.searchData.departureDate}`;
    }
    
    console.log('Search URL:', url);
    
    this.http.get<any>(url).subscribe({
      next: (data) => {
        console.log('Full search response:', data);
        // Handle the new response structure
        this.flights = data.outbound?.flights || data.flights || [];
        
        // Map pricing to economyPrice for display
        this.flights = this.flights.map((f: any) => ({
          ...f,
          economyPrice: f.pricing?.Economy || f.pricing?.economy || f.economyPrice || 0
        }));
        
        console.log('Flights found:', this.flights.length);
        console.log('Flight details with prices:', this.flights);
        
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
        console.error('Search error:', err);
        this.errorMessage = err.error?.error || 'Error loading flights. Please try again.';
        this.isLoading = false;
      }
    });
  }
  
  selectFlight(flight: any, isReturn: boolean = false) {
    // Ensure price is properly mapped
    const mapFlightData = (f: any) => {
      const pricing = f.pricing || f.Pricing;
      const price = f.economyPrice || pricing?.economy || pricing?.Economy || f.price || 0;
      return {
        ...f,
        price: price,
        economyPrice: price
      };
    };
    
    const bookingData = {
      flight: isReturn ? mapFlightData(this.selectedOutboundFlight) : mapFlightData(flight),
      returnFlight: isReturn ? mapFlightData(flight) : null,
      tripType: this.tripType,
      passengers: this.passengers,
      searchData: this.searchData
    };
    console.log('Booking data with prices:', bookingData);
    localStorage.setItem('bookingData', JSON.stringify(bookingData));
    this.router.navigate(['/booking']);
  }
  
  selectedOutboundFlight: any = null;
  
  selectOutbound(flight: any) {
    if (this.tripType === 'RoundTrip') {
      this.selectedOutboundFlight = flight;
    } else {
      this.selectFlight(flight, false);
    }
  }
  
  searchReturnFlights() {
    let url = `${this.apiUrl}/flights/search-flights?from=${this.searchData.destination}&to=${this.searchData.origin}`;
    if (this.searchData.returnDate) {
      url += `&date=${this.searchData.returnDate}`;
    }
    
    this.http.get<any>(url).subscribe({
      next: (data) => {
        this.returnFlights = data.outbound?.flights || data.flights || [];
        
        // Map pricing to economyPrice for display
        this.returnFlights = this.returnFlights.map((f: any) => ({
          ...f,
          economyPrice: f.pricing?.Economy || f.pricing?.economy || f.economyPrice || 0
        }));
        
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Return flights error:', err);
        this.errorMessage = 'Error loading return flights.';
        this.isLoading = false;
      }
    });
  }
  
  updateUser() {
    this.errorMessage = '';
    this.successMessage = '';
    this.http.put(`${this.apiUrl}/users/${this.user.id}`, this.updateUserData, { headers: this.getHeaders() }).subscribe({
      next: () => {
        this.successMessage = 'User information updated successfully!';
        this.loadUserData();
      },
      error: (error) => this.errorMessage = error.error || 'Failed to update user information.'
    });
  }
  
  updateProfile() {
    this.errorMessage = '';
    this.successMessage = '';
    
    if (this.userProfile) {
      this.errorMessage = 'Profile already exists and cannot be modified.';
      return;
    }
    
    this.createProfile();
  }
  
  createProfile() {
    this.errorMessage = '';
    this.successMessage = '';
    
    if (this.userProfile) {
      this.errorMessage = 'Profile already exists and cannot be modified.';
      return;
    }
    
    const payload = { ...this.profileData, age: Number(this.profileData.age) };
    this.http.post(`${this.apiUrl}/profile`, payload, { headers: this.getHeaders() }).subscribe({
      next: () => {
        this.successMessage = 'Profile created successfully! This information is now locked and cannot be changed.';
        this.loadUserProfile();
      },
      error: (error) => this.errorMessage = error.error || 'Failed to create profile.'
    });
  }
  
  changePassword() {
    this.errorMessage = '';
    this.successMessage = '';
    if (this.passwordData.newPassword !== this.passwordData.confirmPassword) {
      this.errorMessage = 'Passwords do not match!';
      return;
    }
    const payload = { currentPassword: this.passwordData.currentPassword, newPassword: this.passwordData.newPassword };
    this.http.post(`${this.apiUrl}/auth/change-password`, payload, { headers: this.getHeaders() }).subscribe({
      next: () => {
        this.successMessage = 'Password changed successfully!';
        this.passwordData = { currentPassword: '', newPassword: '', confirmPassword: '' };
      },
      error: (error) => this.errorMessage = error.error?.message || 'Failed to change password.'
    });
  }
  
  downloadTicket(bookingId: number) {
    console.log('Downloading ticket for booking:', bookingId);
    
    this.http.get(`${this.apiUrl}/bookings/${bookingId}/download-ticket`, {
      headers: this.getHeaders(),
      responseType: 'blob'
    }).subscribe({
      next: (blob) => {
        console.log('PDF blob received:', blob);
        
        // Create a blob URL and trigger download
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `FlightTicket_${bookingId}.pdf`;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        window.URL.revokeObjectURL(url);
        
        this.successMessage = 'Ticket downloaded successfully!';
        setTimeout(() => this.successMessage = '', 3000);
      },
      error: (error) => {
        console.error('Failed to download ticket:', error);
        this.errorMessage = 'Failed to download ticket. Please try again.';
        setTimeout(() => this.errorMessage = '', 3000);
      }
    });
  }
  
  cancelBooking(bookingId: number) {
    if (confirm('Are you sure you want to cancel this booking?')) {
      this.http.post(`${this.apiUrl}/bookings/cancel`, { bookingId }, { headers: this.getHeaders() }).subscribe({
        next: () => {
          this.successMessage = 'Booking cancelled successfully!';
          this.loadBookings();
        },
        error: (error) => this.errorMessage = error.error || 'Failed to cancel booking.'
      });
    }
  }
  
  logout() {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    this.router.navigate(['/login']);
  }
  
  getTotalPassengers(): number {
    return this.passengers.adults + this.passengers.children + this.passengers.infants;
  }

  goToChat() {
    this.router.navigate(['/chat']);
  }

  loadAirports() {
    this.http.get<any>(`${this.apiUrl}/airports?paginate=false`).subscribe({
      next: (data) => {
        console.log('Airports loaded:', data);
        // Remove duplicates based on city name and code
        const uniqueAirports = data.reduce((acc: any[], current: any) => {
          const exists = acc.find(item => item.city === current.city && item.code === current.code);
          if (!exists) {
            acc.push(current);
          }
          return acc;
        }, []);
        // Sort alphabetically by city
        this.airports = uniqueAirports.sort((a: any, b: any) => a.city.localeCompare(b.city));
        console.log('Unique airports:', this.airports);
      },
      error: (err) => {
        console.error('Error loading airports:', err);
      }
    });
  }
  
  retryPayment(bookingId: number, amount: number) {
    localStorage.setItem('bookingId', bookingId.toString());
    localStorage.setItem('paymentAmount', amount.toString());
    this.router.navigate(['/payment']);
  }
}
