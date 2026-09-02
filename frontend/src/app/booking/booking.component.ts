import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpClientModule, HttpHeaders } from '@angular/common/http';
import { Router } from '@angular/router';
import { FormValidators } from '../validators/form-validators';

@Component({
  selector: 'app-booking',
  imports: [CommonModule, FormsModule, HttpClientModule],
  templateUrl: './booking.component.html',
  styleUrl: './booking.component.css',
  standalone: true
})
export class BookingComponent implements OnInit {
  bookingData: any = null;
  flight: any = null;
  returnFlight: any = null;
  tripType = 'OneWay';
  passengers: any[] = [];
  seatMap: any[] = [];
  returnSeatMap: any[] = [];
  groupedSeatMap: any = {};
  groupedReturnSeatMap: any = {};
  selectedSeats: any = {};
  totalPrice = 0;
  errorMessage = '';
  isLoading = false;
  userProfile: any = null;
  user: any = null;
  showSeatSelectionModal = false;
  showConfirmationModal = false;
  termsAccepted = false;
  passengerErrors: any[] = [];
  private apiUrl = 'http://localhost:5000';
  
  constructor(private http: HttpClient, private router: Router) {}
  
  ngOnInit() {
    const data = localStorage.getItem('bookingData');
    if (!data) {
      this.router.navigate(['/dashboard']);
      return;
    }
    this.bookingData = JSON.parse(data);
    console.log('Raw booking data from localStorage:', this.bookingData);
    
    this.flight = this.bookingData.flight;
    this.returnFlight = this.bookingData.returnFlight;
    this.tripType = this.bookingData.tripType;
    
    console.log('Flight before mapping:', JSON.stringify(this.flight, null, 2));
    
    // Map flight data to expected format
    if (this.flight) {
      this.flight.id = this.flight.flightId || this.flight.id;
      const pricing = this.flight.pricing || this.flight.Pricing;
      console.log('Pricing object found:', pricing);
      this.flight.price = this.flight.economyPrice || this.flight.price || pricing?.economy || pricing?.Economy || 0;
      console.log('Mapped flight price:', this.flight.price);
      this.flight.originAirportCode = this.flight.origin?.code || this.flight.originCode || this.flight.originAirportCode || '';
      this.flight.destinationAirportCode = this.flight.destination?.code || this.flight.destinationCode || this.flight.destinationAirportCode || '';
    }
    if (this.returnFlight) {
      this.returnFlight.id = this.returnFlight.flightId || this.returnFlight.id;
      const returnPricing = this.returnFlight.pricing || this.returnFlight.Pricing;
      this.returnFlight.price = this.returnFlight.economyPrice || this.returnFlight.price || returnPricing?.economy || returnPricing?.Economy || 0;
      this.returnFlight.originAirportCode = this.returnFlight.origin?.code || this.returnFlight.originCode || this.returnFlight.originAirportCode || '';
      this.returnFlight.destinationAirportCode = this.returnFlight.destination?.code || this.returnFlight.destinationCode || this.returnFlight.destinationAirportCode || '';
    }
    
    console.log('Flight data mapped:', {
      outbound: { id: this.flight?.id, price: this.flight?.price, fullData: this.flight },
      return: this.returnFlight ? { id: this.returnFlight.id, price: this.returnFlight.price } : null
    });
    
    this.initializePassengers();
    this.loadUserProfile();
    this.loadSeatMap();
    if (this.returnFlight) this.loadReturnSeatMap();
    this.calculatePrice();
  }
  
  getHeaders() {
    const token = localStorage.getItem('accessToken');
    return new HttpHeaders({ 'Authorization': `Bearer ${token}` });
  }
  
  initializePassengers() {
    const { adults, children, infants } = this.bookingData.passengers;
    this.passengers = [];
    this.passengerErrors = [];
    for (let i = 0; i < adults; i++) {
      this.passengers.push({ 
        fullName: '', 
        age: 18, 
        gender: 'Male', 
        passengerType: 'Adult', 
        passportNumber: '', 
        seatNumber: '', 
        returnSeatNumber: '',
        seatCharge: 0,
        returnSeatCharge: 0
      });
      this.passengerErrors.push({});
    }
    for (let i = 0; i < children; i++) {
      this.passengers.push({ 
        fullName: '', 
        age: 8, 
        gender: 'Male', 
        passengerType: 'Child', 
        passportNumber: '', 
        seatNumber: '', 
        returnSeatNumber: '',
        seatCharge: 0,
        returnSeatCharge: 0
      });
      this.passengerErrors.push({});
    }
    for (let i = 0; i < infants; i++) {
      this.passengers.push({ 
        fullName: '', 
        age: 1, 
        gender: 'Male', 
        passengerType: 'Infant', 
        passportNumber: '', 
        seatNumber: '', 
        returnSeatNumber: '',
        seatCharge: 0,
        returnSeatCharge: 0
      });
      this.passengerErrors.push({});
    }
  }
  
  loadUserProfile() {
    this.http.get<any>(`${this.apiUrl}/auth/me`, { headers: this.getHeaders() }).subscribe({
      next: (data) => {
        this.user = data;
      },
      error: () => {}
    });
    
    this.http.get<any>(`${this.apiUrl}/profile`, { headers: this.getHeaders() }).subscribe({
      next: (data) => {
        this.userProfile = data;
      },
      error: () => {}
    });
  }
  
  useMyProfile(passengerIndex: number) {
    if (!this.userProfile && !this.user) {
      this.errorMessage = 'Profile not found. Please complete your profile first.';
      return;
    }
    
    const passenger = this.passengers[passengerIndex];
    
    if (this.userProfile) {
      passenger.fullName = `${this.user?.firstName || ''} ${this.user?.lastName || ''}`.trim();
      passenger.age = this.userProfile.age || passenger.age;
      passenger.gender = this.userProfile.gender || passenger.gender;
      passenger.passportNumber = this.userProfile.passportNumber || '';
    } else if (this.user) {
      passenger.fullName = `${this.user.firstName} ${this.user.lastName}`;
    }
  }
  
  loadSeatMap() {
    this.http.get<any>(`${this.apiUrl}/seats/flight/${this.flight.id}/seat-map`).subscribe({
      next: (data) => {
        this.seatMap = data;
        this.groupedSeatMap = this.groupSeatsByClass(data);
      },
      error: () => this.errorMessage = 'Failed to load seat map'
    });
  }
  
  loadReturnSeatMap() {
    this.http.get<any>(`${this.apiUrl}/seats/flight/${this.returnFlight.id}/seat-map`).subscribe({
      next: (data) => {
        this.returnSeatMap = data;
        this.groupedReturnSeatMap = this.groupSeatsByClass(data);
      },
      error: () => {}
    });
  }
  
  groupSeatsByClass(seats: any[]): any {
    const classOrder = ['FirstClass', 'Business', 'PremiumEconomy', 'Economy'];
    const grouped: any = {};
    
    classOrder.forEach(seatClass => {
      const classSeats = seats.filter(s => s.seatClass === seatClass);
      if (classSeats.length > 0) {
        grouped[seatClass] = this.groupSeatsByRow(classSeats);
      }
    });
    
    return grouped;
  }
  
  groupSeatsByRow(seats: any[]): any[][] {
    const rowMap = new Map<number, any[]>();
    seats.forEach(seat => {
      if (!rowMap.has(seat.row)) {
        rowMap.set(seat.row, []);
      }
      rowMap.get(seat.row)!.push(seat);
    });
    
    // Convert to array and sort by row number
    return Array.from(rowMap.entries())
      .sort((a, b) => a[0] - b[0])
      .map(([_, rowSeats]) => rowSeats.sort((a, b) => a.column.localeCompare(b.column)));
  }
  
  getClassLabel(seatClass: string): string {
    const labels: any = {
      'FirstClass': 'First Class',
      'Business': 'Business Class',
      'PremiumEconomy': 'Premium Economy',
      'Economy': 'Economy Class'
    };
    return labels[seatClass] || seatClass;
  }
  
  getClassKeys(groupedSeats: any): string[] {
    return Object.keys(groupedSeats);
  }
  
  selectSeat(seat: any, passengerIndex: number, isReturn: boolean = false) {
    console.log('Selecting seat:', seat.seatNumber, 'for passenger', passengerIndex, 'isReturn:', isReturn);
    
    if (seat.status !== 'Available') {
      console.log('Seat not available');
      return;
    }
    
    if (!isReturn && this.isSeatSelectedByOther(seat.seatNumber, passengerIndex)) {
      console.log('Seat already selected by another passenger');
      return;
    }
    
    if (isReturn && this.isReturnSeatSelectedByOther(seat.seatNumber, passengerIndex)) {
      console.log('Return seat already selected by another passenger');
      return;
    }
    
    if (isReturn) {
      this.passengers[passengerIndex].returnSeatNumber = seat.seatNumber;
      this.passengers[passengerIndex].returnSeatCharge = seat.extraCharge || 0;
      console.log('Return seat selected:', seat.seatNumber, 'Extra charge:', seat.extraCharge);
    } else {
      this.passengers[passengerIndex].seatNumber = seat.seatNumber;
      this.passengers[passengerIndex].seatCharge = seat.extraCharge || 0;
      console.log('Outbound seat selected:', seat.seatNumber, 'Extra charge:', seat.extraCharge);
    }
    
    this.selectedSeats[passengerIndex] = { seat, isReturn };
    console.log('Updated passengers:', this.passengers);
    
    // Recalculate price with seat charges
    this.calculatePrice();
  }
  
  calculatePrice() {
    // Calculate base price from flight pricing
    let basePrice = 0;
    
    // Calculate outbound flight price
    this.passengers.forEach(p => {
      const pricing = this.flight.pricing || this.flight.Pricing;
      const flightPrice = this.flight.price || pricing?.economy || pricing?.Economy || 0;
      if (p.passengerType === 'Adult') {
        basePrice += flightPrice;
      } else if (p.passengerType === 'Child') {
        basePrice += flightPrice * 0.75; // 25% discount for children
      } else if (p.passengerType === 'Infant') {
        basePrice += flightPrice * 0.1; // 90% discount for infants
      }
      
      // Add seat extra charge for outbound
      if (p.seatCharge) {
        basePrice += p.seatCharge;
      }
    });
    
    // Add return flight price if exists
    if (this.returnFlight) {
      this.passengers.forEach(p => {
        const returnPricing = this.returnFlight.pricing || this.returnFlight.Pricing;
        const returnPrice = this.returnFlight.price || returnPricing?.economy || returnPricing?.Economy || 0;
        if (p.passengerType === 'Adult') {
          basePrice += returnPrice;
        } else if (p.passengerType === 'Child') {
          basePrice += returnPrice * 0.75;
        } else if (p.passengerType === 'Infant') {
          basePrice += returnPrice * 0.1;
        }
        
        // Add seat extra charge for return
        if (p.returnSeatCharge) {
          basePrice += p.returnSeatCharge;
        }
      });
    }
    
    this.totalPrice = Math.round(basePrice * 100) / 100; // Round to 2 decimals
    console.log('Total price calculated:', this.totalPrice);
  }
  
  validatePassengerField(index: number, field: string) {
    const passenger = this.passengers[index];
    if (!this.passengerErrors[index]) this.passengerErrors[index] = {};
    
    switch(field) {
      case 'fullName':
        this.passengerErrors[index].fullName = FormValidators.validateFullName(passenger.fullName);
        break;
      case 'age':
        this.passengerErrors[index].age = FormValidators.validateAge(passenger.age);
        break;
      case 'gender':
        this.passengerErrors[index].gender = FormValidators.validateGender(passenger.gender);
        break;
      case 'passportNumber':
        this.passengerErrors[index].passportNumber = FormValidators.validatePassportNumber(passenger.passportNumber);
        break;
    }
  }

  validateAllPassengers(): boolean {
    let isValid = true;
    this.passengers.forEach((passenger, index) => {
      if (!this.passengerErrors[index]) this.passengerErrors[index] = {};
      this.passengerErrors[index].fullName = FormValidators.validateFullName(passenger.fullName);
      this.passengerErrors[index].age = FormValidators.validateAge(passenger.age);
      this.passengerErrors[index].gender = FormValidators.validateGender(passenger.gender);
      this.passengerErrors[index].passportNumber = FormValidators.validatePassportNumber(passenger.passportNumber);
      
      if (Object.values(this.passengerErrors[index]).some(error => error !== null)) {
        isValid = false;
      }
    });
    return isValid;
  }

  proceedToSeatSelection() {
    if (!this.validateAllPassengers()) {
      this.errorMessage = 'Please fix all passenger validation errors';
      return;
    }
    
    if (this.seatMap.length === 0) {
      this.errorMessage = 'Seat map is still loading. Please wait...';
      return;
    }
    
    this.errorMessage = '';
    this.showSeatSelectionModal = true;
    console.log('Seat map loaded:', this.seatMap);
    console.log('Passengers:', this.passengers);
  }
  
  closeSeatSelection() {
    this.showSeatSelectionModal = false;
  }
  
  proceedToConfirmation() {
    if (!this.allSeatsSelected()) {
      this.errorMessage = 'Please select seats for all passengers';
      return;
    }
    this.showSeatSelectionModal = false;
    this.showConfirmationModal = true;
  }
  
  allSeatsSelected(): boolean {
    const outboundSelected = this.passengers.every(p => p.seatNumber);
    if (!this.returnFlight) return outboundSelected;
    return outboundSelected && this.passengers.every(p => p.returnSeatNumber);
  }
  
  isSeatSelectedByOther(seatNumber: string, currentPassengerIndex: number): boolean {
    return this.passengers.some((p, i) => i !== currentPassengerIndex && p.seatNumber === seatNumber);
  }
  
  isReturnSeatSelectedByOther(seatNumber: string, currentPassengerIndex: number): boolean {
    return this.passengers.some((p, i) => i !== currentPassengerIndex && p.returnSeatNumber === seatNumber);
  }
  
  getSeatTooltip(seat: any, passengerIndex: number): string {
    if (seat.status !== 'Available') return 'Seat unavailable';
    if (this.isSeatSelectedByOther(seat.seatNumber, passengerIndex)) return 'Selected by another passenger';
    
    const extraCharge = seat.extraCharge || 0;
    if (extraCharge > 0) {
      return `Click to select - Extra charge: $${extraCharge}`;
    }
    return 'Click to select - No extra charge';
  }
  
  getReturnSeatTooltip(seat: any, passengerIndex: number): string {
    if (seat.status !== 'Available') return 'Seat unavailable';
    if (this.isReturnSeatSelectedByOther(seat.seatNumber, passengerIndex)) return 'Selected by another passenger';
    
    const extraCharge = seat.extraCharge || 0;
    if (extraCharge > 0) {
      return `Click to select - Extra charge: $${extraCharge}`;
    }
    return 'Click to select - No extra charge';
  }
  
  showConfirmation() {
    this.showConfirmationModal = true;
  }
  
  closeConfirmation() {
    this.showConfirmationModal = false;
    this.termsAccepted = false;
  }
  
  confirmBooking() {
    if (!this.termsAccepted) {
      this.errorMessage = 'Please accept the terms to proceed';
      return;
    }
    this.showConfirmationModal = false;
    this.createBooking();
  }
  
  createBooking() {
    this.isLoading = true;
    this.errorMessage = '';
    const payload: any = {
      flightId: this.flight.id,
      bookingType: this.tripType,
      totalAmount: this.totalPrice,
      passengers: this.passengers
    };
    if (this.returnFlight) payload.returnFlightId = this.returnFlight.id;
    
    console.log('Creating booking with payload:', payload);
    
    this.http.post<any>(`${this.apiUrl}/bookings/create`, payload, { headers: this.getHeaders() }).subscribe({
      next: (data) => {
        console.log('Booking created successfully:', data);
        
        // Handle different response formats
        const bookingId = data.id || data.bookingId || data.Id || data.BookingId;
        
        if (!bookingId) {
          console.error('No booking ID found in response:', data);
          this.errorMessage = 'Booking created but ID not found. Please check your bookings.';
          this.isLoading = false;
          return;
        }
        
        localStorage.setItem('bookingId', bookingId.toString());
        localStorage.setItem('paymentAmount', this.totalPrice.toString());
        
        console.log('Navigating to payment page with:', {
          bookingId: bookingId,
          amount: this.totalPrice
        });
        
        this.router.navigate(['/payment']).then(success => {
          console.log('Navigation to payment:', success ? 'Success' : 'Failed');
        });
      },
      error: (error) => {
        console.error('Booking creation failed:', error);
        this.errorMessage = error.error?.message || error.error || 'Failed to create booking';
        this.isLoading = false;
      }
    });
  }
  
  cancel() {
    localStorage.removeItem('bookingData');
    this.router.navigate(['/dashboard']);
  }

  goToChat() {
    this.router.navigate(['/chat']);
  }

  getOutboundFlightTotal(): number {
    let total = 0;
    const pricing = this.flight.pricing || this.flight.Pricing;
    const flightPrice = this.flight.price || pricing?.economy || pricing?.Economy || 0;
    
    this.passengers.forEach(p => {
      if (p.passengerType === 'Adult') {
        total += flightPrice;
      } else if (p.passengerType === 'Child') {
        total += flightPrice * 0.75;
      } else if (p.passengerType === 'Infant') {
        total += flightPrice * 0.1;
      }
    });
    
    return Math.round(total * 100) / 100;
  }

  getReturnFlightTotal(): number {
    if (!this.returnFlight) return 0;
    
    let total = 0;
    const returnPricing = this.returnFlight.pricing || this.returnFlight.Pricing;
    const returnPrice = this.returnFlight.price || returnPricing?.economy || returnPricing?.Economy || 0;
    
    this.passengers.forEach(p => {
      if (p.passengerType === 'Adult') {
        total += returnPrice;
      } else if (p.passengerType === 'Child') {
        total += returnPrice * 0.75;
      } else if (p.passengerType === 'Infant') {
        total += returnPrice * 0.1;
      }
    });
    
    return Math.round(total * 100) / 100;
  }

  getTotalSeatCharges(): number {
    let total = 0;
    this.passengers.forEach(p => {
      total += (p.seatCharge || 0) + (p.returnSeatCharge || 0);
    });
    return Math.round(total * 100) / 100;
  }
}
