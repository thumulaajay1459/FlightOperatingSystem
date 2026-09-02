import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient, HttpClientModule, HttpHeaders } from '@angular/common/http';
import { Router } from '@angular/router';

declare var Razorpay: any;

@Component({
  selector: 'app-payment',
  imports: [CommonModule, HttpClientModule],
  templateUrl: './payment.component.html',
  styleUrl: './payment.component.css',
  standalone: true
})
export class PaymentComponent implements OnInit {
  bookingId: string | null = null;
  amount: number = 0;
  bookingDetails: any = null;
  isLoading = false;
  errorMessage = '';
  paymentInProgress = false;
  paymentSuccess = false;
  private apiUrl = 'http://localhost:5000';
  
  constructor(private http: HttpClient, private router: Router) {}
  
  ngOnInit() {
    console.log('Payment component initialized');
    
    this.bookingId = localStorage.getItem('bookingId');
    const amountStr = localStorage.getItem('paymentAmount');
    this.amount = amountStr ? parseFloat(amountStr) : 0;
    
    console.log('Retrieved from localStorage:', {
      bookingId: this.bookingId,
      amount: this.amount
    });
    
    if (!this.bookingId || !this.amount) {
      console.error('Missing booking data, redirecting to dashboard');
      alert('Booking information not found. Redirecting to dashboard.');
      this.router.navigate(['/dashboard']);
      return;
    }
    
    this.loadBookingDetails();
    this.loadRazorpayScript();
  }
  
  getHeaders() {
    const token = localStorage.getItem('accessToken');
    return new HttpHeaders({ 'Authorization': `Bearer ${token}` });
  }
  
  loadBookingDetails() {
    this.isLoading = true;
    console.log('Loading booking details for:', this.bookingId);
    
    this.http.get<any>(`${this.apiUrl}/bookings/${this.bookingId}`, { headers: this.getHeaders() }).subscribe({
      next: (data) => {
        console.log('Booking details loaded:', data);
        this.bookingDetails = data;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Failed to load booking details:', error);
        // Create minimal booking details to allow payment
        this.bookingDetails = {
          flightId: 0,
          flightNumber: 'N/A',
          origin: 'N/A',
          destination: 'N/A',
          departureTime: new Date().toISOString(),
          userId: '0',
          passengers: [{ fullName: 'Guest', seatNumber: 'N/A' }],
          userEmail: 'guest@example.com',
          userPhone: '0000000000'
        };
        this.errorMessage = 'Could not load full booking details, but you can still proceed with payment.';
        this.isLoading = false;
      }
    });
  }
  
  loadRazorpayScript() {
    const script = document.createElement('script');
    script.src = 'https://checkout.razorpay.com/v1/checkout.js';
    script.async = true;
    document.body.appendChild(script);
  }
  
  initiatePayment() {
    if (!this.bookingDetails) {
      this.errorMessage = 'Booking details not loaded';
      return;
    }
    
    console.log('Initiating payment for booking:', this.bookingDetails);
    
    this.paymentInProgress = true;
    this.errorMessage = '';
    
    // Calculate breakdown
    const baseFare = this.amount * 0.85; // 85% base fare
    const taxAmount = this.amount * 0.12; // 12% tax
    const serviceFee = this.amount * 0.03; // 3% service fee
    
    const payload = {
      bookingId: this.bookingId,
      flightId: this.bookingDetails.flightId?.toString() || '0',
      flightNumber: this.bookingDetails.flightNumber || 'N/A',
      origin: this.bookingDetails.origin || 'N/A',
      destination: this.bookingDetails.destination || 'N/A',
      travelDate: this.bookingDetails.departureTime || new Date().toISOString(),
      passengerId: this.bookingDetails.userId || '0',
      passengerName: this.bookingDetails.passengers?.[0]?.fullName || 'Guest',
      email: this.bookingDetails.userEmail || 'guest@example.com',
      phone: this.bookingDetails.userPhone || '0000000000',
      seatNumber: this.bookingDetails.passengers?.[0]?.seatNumber || 'N/A',
      cabinClass: 'Economy',
      baseFare: baseFare,
      taxAmount: taxAmount,
      serviceFee: serviceFee,
      currency: 'INR'
    };
    
    console.log('Payment payload:', payload);
    
    this.http.post<any>(`${this.apiUrl}/payments/create-order`, payload, { headers: this.getHeaders() }).subscribe({
      next: (response) => {
        console.log('Payment order created:', response);
        this.openRazorpayCheckout(response);
      },
      error: (error) => {
        console.error('Payment order creation failed:', error);
        this.errorMessage = error.error?.message || 'Failed to create payment order';
        this.paymentInProgress = false;
      }
    });
  }
  
  openRazorpayCheckout(orderData: any) {
    const options = {
      key: orderData.key,
      amount: orderData.totalAmount * 100,
      currency: orderData.currency,
      name: 'Flight Booking System',
      description: `Booking ID: ${this.bookingId}`,
      order_id: orderData.razorpayOrderId,
      handler: (response: any) => {
        this.verifyPayment({
          paymentId: orderData.paymentId,
          razorpayOrderId: response.razorpay_order_id,
          razorpayPaymentId: response.razorpay_payment_id,
          razorpaySignature: response.razorpay_signature
        });
      },
      prefill: {
        name: this.bookingDetails?.passengers?.[0]?.fullName || '',
        email: this.bookingDetails?.userEmail || '',
        contact: this.bookingDetails?.userPhone || ''
      },
      theme: {
        color: '#2563eb'
      },
      modal: {
        ondismiss: () => {
          console.log('Payment modal dismissed/closed by user');
          this.paymentInProgress = false;
          this.errorMessage = 'Payment window closed. Your booking is still pending. You can retry payment anytime from My Bookings section.';
          
          // Scroll to top to show error message
          window.scrollTo({ top: 0, behavior: 'smooth' });
        },
        escape: true,
        backdropclose: false
      }
    };
    
    const razorpay = new Razorpay(options);
    razorpay.on('payment.failed', (response: any) => {
      console.error('Payment failed:', response.error);
      this.handlePaymentFailure(response.error);
    });
    razorpay.open();
  }
  
  verifyPayment(verificationData: any) {
    this.http.post<any>(`${this.apiUrl}/payments/verify`, verificationData, { headers: this.getHeaders() }).subscribe({
      next: (response) => {
        console.log('Payment verified successfully:', response);
        this.paymentInProgress = false;
        this.paymentSuccess = true;
        
        // Download and open PDF in new tab
        this.downloadAndOpenTicket();
        
        // Redirect to bookings after 3 seconds
        setTimeout(() => {
          this.goToBookings();
        }, 3000);
      },
      error: (error) => {
        console.error('Payment verification failed:', error);
        this.paymentInProgress = false;
        this.errorMessage = 'Payment verification failed. Please contact support with your payment details.';
      }
    });
  }
  
  handlePaymentFailure(error: any) {
    this.paymentInProgress = false;
    
    let errorMsg = 'Payment failed. ';
    
    if (error.code === 'BAD_REQUEST_ERROR') {
      errorMsg += 'Invalid payment details. Please try again.';
    } else if (error.code === 'GATEWAY_ERROR') {
      errorMsg += 'Payment gateway error. Please try again later.';
    } else if (error.code === 'SERVER_ERROR') {
      errorMsg += 'Server error. Please try again later.';
    } else if (error.description) {
      errorMsg += error.description;
    } else {
      errorMsg += 'Please try again or contact support.';
    }
    
    errorMsg += ' Your booking is still pending and you can retry payment from My Bookings.';
    
    this.errorMessage = errorMsg;
    
    // Show alert for better visibility
    alert(errorMsg);
  }
  
  downloadAndOpenTicket() {
    if (!this.bookingId) return;
    
    console.log('Downloading ticket for booking:', this.bookingId);
    
    this.http.get(`${this.apiUrl}/bookings/${this.bookingId}/download-ticket`, {
      headers: this.getHeaders(),
      responseType: 'blob'
    }).subscribe({
      next: (blob) => {
        console.log('PDF blob received, opening in new tab');
        
        // Create blob URL and open in new tab
        const url = window.URL.createObjectURL(blob);
        window.open(url, '_blank');
        
        // Also trigger download
        const link = document.createElement('a');
        link.href = url;
        link.download = `FlightTicket_${this.bookingId}.pdf`;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        
        // Clean up after a delay
        setTimeout(() => window.URL.revokeObjectURL(url), 100);
      },
      error: (error) => {
        console.error('Failed to download ticket:', error);
        // Don't show error to user, just log it
      }
    });
  }
  
  goToBookings() {
    localStorage.removeItem('bookingId');
    localStorage.removeItem('paymentAmount');
    localStorage.removeItem('bookingData');
    this.router.navigate(['/dashboard'], { queryParams: { tab: 'bookings', paymentSuccess: 'true' } });
  }
  
  goToDashboard() {
    localStorage.removeItem('bookingId');
    localStorage.removeItem('paymentAmount');
    localStorage.removeItem('bookingData');
    this.router.navigate(['/dashboard']);
  }
}
