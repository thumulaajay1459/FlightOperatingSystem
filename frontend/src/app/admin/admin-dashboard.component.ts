import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpClientModule, HttpHeaders } from '@angular/common/http';
import { Router } from '@angular/router';
import { FormValidators } from '../validators/form-validators';

@Component({
  selector: 'app-admin-dashboard',
  imports: [CommonModule, FormsModule, HttpClientModule],
  templateUrl: './admin-dashboard.component.html',
  styleUrl: './admin-dashboard.component.css',
  standalone: true
})
export class AdminDashboardComponent implements OnInit {
  activeTab = 'dashboard';
  admin: any = null;
  stats: any = null;
  users: any[] = [];
  bookings: any[] = [];
  flights: any[] = [];
  aircraft: any[] = [];
  airports: any[] = [];
  routes: any[] = [];
  errorMessage = '';
  successMessage = '';
  isLoading = false;
  minDateTime = '';
  
  // Pagination
  currentPage = 1;
  pageSize = 15;
  totalPages = 1;
  totalCount = 0;
  
  aircraftForm = { model: '', manufacturer: '', totalSeats: 0, economySeats: 0, businessSeats: 0, firstClassSeats: 0 };
  airportForm = { code: '', name: '', city: '', country: '' };
  routeForm = { originAirportId: 0, destinationAirportId: 0, distanceKm: 0 };
  flightForm = { routeId: 0, aircraftId: 0, flightNumber: '', departureTime: '', arrivalTime: '', economyPrice: 0, businessPrice: 0, firstClassPrice: 0, status: 'Scheduled' };
  airlineForm = { code: '', name: '', country: '' };
  flightUpdateForm = { flightId: 0, status: '', comments: '', delayedMinutes: 0 };
  
  airlineErrors: any = {};
  airlines: { code: string; name: string; }[] = [];
  selectedAirline = '';
  flightNumberSuffix = '';
  
  editMode = false;
  editId = 0;
  
  aircraftErrors: any = {};
  airportErrors: any = {};
  routeErrors: any = {};
  flightErrors: any = {};
  
  private apiUrl = 'http://localhost:5000';
  
  constructor(private http: HttpClient, private router: Router) {}
  
  ngOnInit() {
    console.log('Admin Dashboard initialized with pageSize:', this.pageSize);
    this.setMinDateTime();
    this.checkAdminAccess();
    this.loadAdminData();
    this.loadDashboardStats();
    this.loadAirports();
    this.loadAircraft();
  }
  
  setMinDateTime() {
    const now = new Date();
    now.setMinutes(now.getMinutes() - now.getTimezoneOffset());
    this.minDateTime = now.toISOString().slice(0, 16);
  }
  
  getHeaders() {
    const token = localStorage.getItem('accessToken');
    return new HttpHeaders({ 'Authorization': `Bearer ${token}` });
  }
  
  checkAdminAccess() {
    const token = localStorage.getItem('accessToken');
    if (!token) {
      this.router.navigate(['/login']);
      return;
    }
    const payload = JSON.parse(atob(token.split('.')[1]));
    const role = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
    if (role !== 'Admin') {
      this.router.navigate(['/dashboard']);
    }
  }
  
  loadAdminData() {
    this.http.get<any>(`${this.apiUrl}/auth/me`, { headers: this.getHeaders() }).subscribe({
      next: (data) => this.admin = data,
      error: () => this.router.navigate(['/login'])
    });
  }
  
  loadDashboardStats() {
    this.http.get<any>(`${this.apiUrl}/admin/dashboard/stats`, { headers: this.getHeaders() }).subscribe({
      next: (data) => this.stats = data,
      error: () => {}
    });
  }
  
  loadUsers() {
    this.http.get<any>(`${this.apiUrl}/admin/users?page=${this.currentPage}&pageSize=${this.pageSize}`, { headers: this.getHeaders() }).subscribe({
      next: (data) => {
        if (data.data) {
          this.users = data.data;
          this.totalPages = data.pagination.totalPages;
          this.totalCount = data.pagination.totalCount;
        } else {
          this.users = Array.isArray(data) ? data : [];
        }
      },
      error: () => this.errorMessage = 'Failed to load users'
    });
  }
  
  loadBookings() {
    console.log(`Loading bookings: page=${this.currentPage}, pageSize=${this.pageSize}`);
    this.http.get<any>(`${this.apiUrl}/bookings/all?page=${this.currentPage}&pageSize=${this.pageSize}`, { headers: this.getHeaders() }).subscribe({
      next: (data) => {
        console.log('Bookings response:', data);
        let bookingsData = [];
        if (data.data) {
          bookingsData = data.data;
          this.totalPages = data.pagination.totalPages;
          this.totalCount = data.pagination.totalCount;
          console.log(`Received ${bookingsData.length} bookings, page ${data.pagination.currentPage} of ${data.pagination.totalPages}`);
        } else {
          bookingsData = Array.isArray(data) ? data : [];
        }
        this.bookings = bookingsData.map((b: any) => ({
          id: b.id,
          userId: b.userId,
          flightNumber: b.flightNumber || 'N/A',
          status: b.bookingStatus,
          totalAmount: b.totalAmount,
          bookingDate: b.createdAt
        }));
      },
      error: () => this.errorMessage = 'Failed to load bookings'
    });
  }
  
  loadFlights() {
    this.isLoading = true;
    this.http.get<any>(`${this.apiUrl}/flights?page=${this.currentPage}&pageSize=${this.pageSize}`).subscribe({
      next: (data) => {
        console.log('Admin flights response:', data);
        // Handle both array and paginated response
        if (data.data) {
          this.flights = Array.isArray(data.data) ? data.data : [];
          this.totalPages = data.pagination.totalPages;
          this.totalCount = data.pagination.totalCount;
        } else {
          this.flights = Array.isArray(data) ? data : [];
        }
        console.log('Flights array:', this.flights);
        // Enrich flights with route and aircraft names
        this.flights.forEach(flight => {
          console.log('Processing flight:', flight);
          const route = this.routes.find(r => (r.routeId || r.id) === (flight.routeId || flight.route?.routeId));
          if (route) {
            flight.routeName = `${route.originName} → ${route.destName}`;
          }
          const aircraft = this.aircraft.find(ac => (ac.aircraftId || ac.id) === (flight.aircraftId || flight.aircraft?.aircraftId));
          if (aircraft) {
            flight.aircraftName = `${aircraft.manufacturer} ${aircraft.model}`;
          }
        });
        console.log('Enriched flights:', this.flights);
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading flights:', error);
        this.errorMessage = 'Failed to load flights';
        this.isLoading = false;
      }
    });
  }
  
  loadAircraft() {
    this.http.get<any>(`${this.apiUrl}/admin/aircraft?page=${this.currentPage}&pageSize=${this.pageSize}`, { headers: this.getHeaders() }).subscribe({
      next: (data) => {
        if (data.data) {
          this.aircraft = data.data;
          this.totalPages = data.pagination.totalPages;
          this.totalCount = data.pagination.totalCount;
        } else {
          this.aircraft = Array.isArray(data) ? data : [];
        }
      },
      error: () => this.errorMessage = 'Failed to load aircraft'
    });
  }
  
  loadAirports() {
    this.http.get<any>(`${this.apiUrl}/airports?page=${this.currentPage}&pageSize=${this.pageSize}`, { headers: this.getHeaders() }).subscribe({
      next: (data) => {
        if (data.data) {
          this.airports = data.data;
          this.totalPages = data.pagination.totalPages;
          this.totalCount = data.pagination.totalCount;
        } else {
          this.airports = Array.isArray(data) ? data : [];
        }
      },
      error: () => this.errorMessage = 'Failed to load airports'
    });
  }
  
  loadRoutes() {
    this.http.get<any>(`${this.apiUrl}/routes?page=${this.currentPage}&pageSize=${this.pageSize}`, { headers: this.getHeaders() }).subscribe({
      next: (data) => {
        if (data.data) {
          this.routes = data.data;
          this.totalPages = data.pagination.totalPages;
          this.totalCount = data.pagination.totalCount;
        } else {
          this.routes = Array.isArray(data) ? data : [];
        }
        // Enrich routes with airport names
        this.routes.forEach(route => {
          const origin = this.airports.find(ap => (ap.airportId || ap.id) === route.originAirportId);
          const dest = this.airports.find(ap => (ap.airportId || ap.id) === route.destinationAirportId);
          route.originName = origin ? `${origin.code || origin.airportCode} - ${origin.city}` : `Airport ${route.originAirportId}`;
          route.destName = dest ? `${dest.code || dest.airportCode} - ${dest.city}` : `Airport ${route.destinationAirportId}`;
        });
      },
      error: () => this.errorMessage = 'Failed to load routes'
    });
  }
  
  switchTab(tab: string) {
    this.activeTab = tab;
    this.errorMessage = '';
    this.successMessage = '';
    this.currentPage = 1; // Reset to first page
    this.resetForms();
    if (tab === 'users') this.loadUsers();
    if (tab === 'bookings') this.loadBookings();
    if (tab === 'flights') { 
      this.loadAirports();
      this.loadAircraft();
      setTimeout(() => {
        this.loadRoutes();
        setTimeout(() => this.loadFlights(), 300);
      }, 500);
    }
    if (tab === 'aircraft') this.loadAircraft();
    if (tab === 'airports') this.loadAirports();
    if (tab === 'routes') { 
      this.loadAirports();
      setTimeout(() => this.loadRoutes(), 500);
    }
    if (tab === 'airlines') this.loadAirlines();
  }
  
  resetForms() {
    this.editMode = false;
    this.editId = 0;
    this.aircraftForm = { model: '', manufacturer: '', totalSeats: 0, economySeats: 0, businessSeats: 0, firstClassSeats: 0 };
    this.airportForm = { code: '', name: '', city: '', country: '' };
    this.routeForm = { originAirportId: 0, destinationAirportId: 0, distanceKm: 0 };
    this.flightForm = { routeId: 0, aircraftId: 0, flightNumber: '', departureTime: '', arrivalTime: '', economyPrice: 0, businessPrice: 0, firstClassPrice: 0, status: 'Scheduled' };
    this.airlineForm = { code: '', name: '', country: '' };
    this.flightUpdateForm = { flightId: 0, status: '', comments: '', delayedMinutes: 0 };
    this.selectedAirline = '';
    this.flightNumberSuffix = '';
    this.aircraftErrors = {};
    this.airportErrors = {};
    this.routeErrors = {};
    this.flightErrors = {};
  }
  
  validateAircraftField(field: string) {
    switch(field) {
      case 'manufacturer':
        this.aircraftErrors.manufacturer = FormValidators.validateManufacturer(this.aircraftForm.manufacturer);
        break;
      case 'model':
        this.aircraftErrors.model = FormValidators.validateModel(this.aircraftForm.model);
        break;
      case 'totalSeats':
        this.aircraftErrors.totalSeats = FormValidators.validateTotalSeats(this.aircraftForm.totalSeats);
        break;
    }
  }

  validateAircraftForm(): boolean {
    this.aircraftErrors = {};
    this.aircraftErrors.manufacturer = FormValidators.validateManufacturer(this.aircraftForm.manufacturer);
    this.aircraftErrors.model = FormValidators.validateModel(this.aircraftForm.model);
    this.aircraftErrors.totalSeats = FormValidators.validateTotalSeats(this.aircraftForm.totalSeats);
    return !Object.values(this.aircraftErrors).some(error => error !== null);
  }

  addAircraft() {
    if (!this.validateAircraftForm()) {
      this.errorMessage = 'Please fix all validation errors';
      return;
    }
    this.isLoading = true;
    this.http.post(`${this.apiUrl}/admin/aircraft`, this.aircraftForm, { headers: this.getHeaders() }).subscribe({
      next: () => {
        this.successMessage = 'Aircraft added successfully!';
        this.resetForms();
        this.loadAircraft();
        this.isLoading = false;
      },
      error: (error) => {
        this.errorMessage = error.error || 'Failed to add aircraft';
        this.isLoading = false;
      }
    });
  }
  
  deleteAircraft(id: number) {
    if (confirm('Are you sure you want to delete this aircraft?')) {
      this.http.delete(`${this.apiUrl}/admin/aircraft/${id}`, { headers: this.getHeaders() }).subscribe({
        next: () => {
          this.successMessage = 'Aircraft deleted successfully!';
          this.loadAircraft();
        },
        error: (error) => this.errorMessage = error.error || 'Failed to delete aircraft'
      });
    }
  }
  
  validateAirportField(field: string) {
    switch(field) {
      case 'code':
        this.airportErrors.code = FormValidators.validateAirportCode(this.airportForm.code);
        break;
      case 'name':
        this.airportErrors.name = FormValidators.validateAirportName(this.airportForm.name);
        break;
      case 'city':
        this.airportErrors.city = FormValidators.validateCity(this.airportForm.city);
        break;
      case 'country':
        this.airportErrors.country = FormValidators.validateCountry(this.airportForm.country);
        break;
    }
  }

  validateAirportForm(): boolean {
    this.airportErrors = {};
    this.airportErrors.code = FormValidators.validateAirportCode(this.airportForm.code);
    this.airportErrors.name = FormValidators.validateAirportName(this.airportForm.name);
    this.airportErrors.city = FormValidators.validateCity(this.airportForm.city);
    this.airportErrors.country = FormValidators.validateCountry(this.airportForm.country);
    return !Object.values(this.airportErrors).some(error => error !== null);
  }

  addAirport() {
    if (!this.validateAirportForm()) {
      this.errorMessage = 'Please fix all validation errors';
      return;
    }
    this.isLoading = true;
    this.http.post(`${this.apiUrl}/admin/airports`, this.airportForm, { headers: this.getHeaders() }).subscribe({
      next: () => {
        this.successMessage = 'Airport added successfully!';
        this.resetForms();
        this.loadAirports();
        this.isLoading = false;
      },
      error: (error) => {
        this.errorMessage = error.error || 'Failed to add airport';
        this.isLoading = false;
      }
    });
  }
  
  deleteAirport(id: number) {
    if (confirm('Are you sure you want to delete this airport?')) {
      this.http.delete(`${this.apiUrl}/admin/airports/${id}`, { headers: this.getHeaders() }).subscribe({
        next: () => {
          this.successMessage = 'Airport deleted successfully!';
          this.loadAirports();
        },
        error: (error) => this.errorMessage = error.error || 'Failed to delete airport'
      });
    }
  }
  
  validateRouteField(field: string) {
    switch(field) {
      case 'originAirportId':
        this.routeErrors.originAirportId = FormValidators.validateAirportId(this.routeForm.originAirportId, 'Origin airport');
        break;
      case 'destinationAirportId':
        this.routeErrors.destinationAirportId = FormValidators.validateAirportId(this.routeForm.destinationAirportId, 'Destination airport');
        if (this.routeForm.originAirportId === this.routeForm.destinationAirportId && this.routeForm.destinationAirportId > 0) {
          this.routeErrors.destinationAirportId = 'Destination airport must be different from origin airport';
        }
        break;
      case 'distanceKm':
        this.routeErrors.distanceKm = FormValidators.validateDistance(this.routeForm.distanceKm);
        break;
    }
  }

  validateRouteForm(): boolean {
    this.routeErrors = {};
    this.routeErrors.originAirportId = FormValidators.validateAirportId(this.routeForm.originAirportId, 'Origin airport');
    this.routeErrors.destinationAirportId = FormValidators.validateAirportId(this.routeForm.destinationAirportId, 'Destination airport');
    if (this.routeForm.originAirportId === this.routeForm.destinationAirportId && this.routeForm.destinationAirportId > 0) {
      this.routeErrors.destinationAirportId = 'Destination airport must be different from origin airport';
    }
    this.routeErrors.distanceKm = FormValidators.validateDistance(this.routeForm.distanceKm);
    return !Object.values(this.routeErrors).some(error => error !== null);
  }

  addRoute() {
    if (!this.validateRouteForm()) {
      this.errorMessage = 'Please fix all validation errors';
      return;
    }
    this.isLoading = true;
    this.http.post(`${this.apiUrl}/admin/routes`, this.routeForm, { headers: this.getHeaders() }).subscribe({
      next: () => {
        this.successMessage = 'Route added successfully!';
        this.resetForms();
        setTimeout(() => this.loadRoutes(), 500);
        this.isLoading = false;
      },
      error: (error) => {
        this.errorMessage = error.error || 'Failed to add route';
        this.isLoading = false;
      }
    });
  }
  
  deleteRoute(id: number) {
    if (confirm('Are you sure you want to delete this route?')) {
      this.http.delete(`${this.apiUrl}/admin/routes/${id}`, { headers: this.getHeaders() }).subscribe({
        next: () => {
          this.successMessage = 'Route deleted successfully!';
          this.loadRoutes();
        },
        error: (error) => this.errorMessage = error.error || 'Failed to delete route'
      });
    }
  }
  
  validateFlightField(field: string) {
    switch(field) {
      case 'flightNumber':
        const fullFlightNumber = this.selectedAirline + this.flightNumberSuffix;
        this.flightErrors.flightNumber = FormValidators.validateFlightNumber(fullFlightNumber);
        break;
      case 'aircraftId':
        this.flightErrors.aircraftId = this.flightForm.aircraftId <= 0 ? 'Aircraft is required' : null;
        break;
      case 'routeId':
        this.flightErrors.routeId = this.flightForm.routeId <= 0 ? 'Route is required' : null;
        break;
      case 'departureTime':
        this.flightErrors.departureTime = FormValidators.validateDepartureTime(this.flightForm.departureTime);
        break;
      case 'arrivalTime':
        this.flightErrors.arrivalTime = FormValidators.validateArrivalTime(this.flightForm.departureTime, this.flightForm.arrivalTime);
        break;
      case 'economyPrice':
        this.flightErrors.economyPrice = FormValidators.validatePrice(this.flightForm.economyPrice, 'Economy price');
        break;
      case 'businessPrice':
        this.flightErrors.businessPrice = FormValidators.validatePrice(this.flightForm.businessPrice, 'Business price');
        break;
      case 'firstClassPrice':
        this.flightErrors.firstClassPrice = FormValidators.validatePrice(this.flightForm.firstClassPrice, 'First class price');
        break;
    }
  }

  validateFlightForm(): boolean {
    this.flightErrors = {};
    const fullFlightNumber = this.selectedAirline + this.flightNumberSuffix;
    this.flightErrors.flightNumber = FormValidators.validateFlightNumber(fullFlightNumber);
    this.flightErrors.aircraftId = this.flightForm.aircraftId <= 0 ? 'Aircraft is required' : null;
    this.flightErrors.routeId = this.flightForm.routeId <= 0 ? 'Route is required' : null;
    this.flightErrors.departureTime = FormValidators.validateDepartureTime(this.flightForm.departureTime);
    this.flightErrors.arrivalTime = FormValidators.validateArrivalTime(this.flightForm.departureTime, this.flightForm.arrivalTime);
    this.flightErrors.economyPrice = FormValidators.validatePrice(this.flightForm.economyPrice, 'Economy price');
    this.flightErrors.businessPrice = FormValidators.validatePrice(this.flightForm.businessPrice, 'Business price');
    this.flightErrors.firstClassPrice = FormValidators.validatePrice(this.flightForm.firstClassPrice, 'First class price');
    return !Object.values(this.flightErrors).some(error => error !== null);
  }

  addFlight() {
    if (!this.validateFlightForm()) {
      this.errorMessage = 'Please fix all validation errors';
      return;
    }
    this.isLoading = true;
    this.flightForm.flightNumber = this.selectedAirline + this.flightNumberSuffix;
    
    // First create the flight
    this.http.post<any>(`${this.apiUrl}/admin/flights`, this.flightForm, { headers: this.getHeaders() }).subscribe({
      next: (response) => {
        const flightId = response.flightId;
        
        // Then update pricing
        const pricingData = {
          basePrice: this.flightForm.economyPrice,
          economyPrice: this.flightForm.economyPrice,
          premiumEconomyPrice: this.flightForm.economyPrice * 1.5,
          businessPrice: this.flightForm.businessPrice,
          firstClassPrice: this.flightForm.firstClassPrice,
          childDiscountPercent: 25,
          infantDiscountPercent: 90
        };
        
        this.http.put(`${this.apiUrl}/flights/${flightId}/pricing`, pricingData, { headers: this.getHeaders() }).subscribe({
          next: () => {
            this.successMessage = 'Flight added successfully with pricing!';
            this.resetForms();
            setTimeout(() => {
              this.loadAirports();
              setTimeout(() => {
                this.loadRoutes();
                this.loadFlights();
              }, 500);
            }, 500);
            this.isLoading = false;
          },
          error: (error) => {
            this.successMessage = 'Flight added but pricing update failed. Please update pricing manually.';
            this.isLoading = false;
          }
        });
      },
      error: (error) => {
        this.errorMessage = error.error || 'Failed to add flight';
        this.isLoading = false;
      }
    });
  }
  
  deleteFlight(id: number) {
    if (confirm('Are you sure you want to delete this flight?')) {
      this.http.delete(`${this.apiUrl}/admin/flights/${id}`, { headers: this.getHeaders() }).subscribe({
        next: () => {
          this.successMessage = 'Flight deleted successfully!';
          this.loadFlights();
        },
        error: (error) => this.errorMessage = error.error || 'Failed to delete flight'
      });
    }
  }
  
  deleteUser(id: number) {
    if (confirm('Are you sure you want to delete this user?')) {
      this.http.delete(`${this.apiUrl}/admin/users/${id}`, { headers: this.getHeaders() }).subscribe({
        next: () => {
          this.successMessage = 'User deleted successfully!';
          this.loadUsers();
        },
        error: (error) => this.errorMessage = error.error || 'Failed to delete user'
      });
    }
  }
  
  
  loadAirlines() {
    this.http.get<any>(`${this.apiUrl}/airlines`).subscribe({
      next: (data) => {
        this.airlines = Array.isArray(data) ? data : [];
      },
      error: () => this.errorMessage = 'Failed to load airlines'
    });
  }
  
  addAirline() {
    this.isLoading = true;
    this.http.post(`${this.apiUrl}/admin/airlines`, this.airlineForm, { headers: this.getHeaders() }).subscribe({
      next: () => {
        this.successMessage = 'Airline added successfully!';
        this.resetForms();
        this.loadAirlines();
        this.isLoading = false;
      },
      error: (error) => {
        this.errorMessage = error.error || 'Failed to add airline';
        this.isLoading = false;
      }
    });
  }
  
  deleteAirline(id: number) {
    if (confirm('Are you sure you want to delete this airline?')) {
      this.http.delete(`${this.apiUrl}/admin/airlines/${id}`, { headers: this.getHeaders() }).subscribe({
        next: () => {
          this.successMessage = 'Airline deleted successfully!';
          this.loadAirlines();
        },
        error: (error) => this.errorMessage = error.error || 'Failed to delete airline'
      });
    }
  }
  
  openFlightUpdateModal(flight: any) {
    this.flightUpdateForm = {
      flightId: flight.flightId || flight.id,
      status: flight.status,
      comments: '',
      delayedMinutes: 0
    };
    this.editMode = true;
  }
  
  updateFlightStatus() {
    if (!this.flightUpdateForm.flightId) return;
    
    this.isLoading = true;
    const flight = this.flights.find(f => (f.flightId || f.id) === this.flightUpdateForm.flightId);
    
    const updateData = {
      flightNumber: flight.flightNumber,
      aircraftId: flight.aircraftId,
      routeId: flight.routeId,
      departureTime: flight.departureTime,
      arrivalTime: flight.arrivalTime,
      status: this.flightUpdateForm.status,
      comments: this.flightUpdateForm.comments,
      delayedMinutes: this.flightUpdateForm.delayedMinutes
    };
    
    this.http.put(`${this.apiUrl}/admin/flights/${this.flightUpdateForm.flightId}`, updateData, { headers: this.getHeaders() }).subscribe({
      next: () => {
        this.successMessage = 'Flight updated successfully! Notifications sent to passengers.';
        this.editMode = false;
        this.loadFlights();
        this.isLoading = false;
      },
      error: (error) => {
        this.errorMessage = error.error || 'Failed to update flight';
        this.isLoading = false;
      }
    });
  }
  
  logout() {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    this.router.navigate(['/login']);
  }
  
  // Pagination methods
  nextPage() {
    if (this.currentPage < this.totalPages) {
      this.currentPage++;
      this.reloadCurrentTab();
    }
  }
  
  previousPage() {
    if (this.currentPage > 1) {
      this.currentPage--;
      this.reloadCurrentTab();
    }
  }
  
  goToPage(page: number) {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.reloadCurrentTab();
    }
  }
  
  reloadCurrentTab() {
    if (this.activeTab === 'users') this.loadUsers();
    if (this.activeTab === 'bookings') this.loadBookings();
    if (this.activeTab === 'flights') this.loadFlights();
    if (this.activeTab === 'aircraft') this.loadAircraft();
    if (this.activeTab === 'airports') this.loadAirports();
    if (this.activeTab === 'routes') this.loadRoutes();
  }
  
  getPageNumbers(): number[] {
    const pages: number[] = [];
    const maxVisible = 5;
    let start = Math.max(1, this.currentPage - Math.floor(maxVisible / 2));
    let end = Math.min(this.totalPages, start + maxVisible - 1);
    
    if (end - start < maxVisible - 1) {
      start = Math.max(1, end - maxVisible + 1);
    }
    
    for (let i = start; i <= end; i++) {
      pages.push(i);
    }
    return pages;
  }
}
