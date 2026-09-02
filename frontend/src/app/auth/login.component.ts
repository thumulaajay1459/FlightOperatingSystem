import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpClientModule } from '@angular/common/http';
import { Router } from '@angular/router';
import { FormValidators } from '../validators/form-validators';

@Component({
  selector: 'app-login',
  imports: [CommonModule, FormsModule, HttpClientModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css',
  standalone: true
})
export class LoginComponent {
  loginData = {
    email: '',
    password: ''
  };
  
  showPassword = false;
  errorMessage = '';
  isLoading = false;
  fieldErrors: any = {};
  
  private apiUrl = 'http://localhost:5000';
  
  constructor(private http: HttpClient, private router: Router) {}
  
  togglePasswordVisibility() {
    this.showPassword = !this.showPassword;
  }
  
  validateField(field: string) {
    switch(field) {
      case 'email':
        this.fieldErrors.email = FormValidators.validateEmail(this.loginData.email);
        break;
      case 'password':
        this.fieldErrors.password = !this.loginData.password ? 'Password is required' : null;
        break;
    }
  }

  validateForm(): boolean {
    this.fieldErrors = {};
    this.fieldErrors.email = FormValidators.validateEmail(this.loginData.email);
    this.fieldErrors.password = !this.loginData.password ? 'Password is required' : null;
    return !Object.values(this.fieldErrors).some(error => error !== null);
  }

  login() {
    this.errorMessage = '';
    
    if (!this.validateForm()) {
      this.errorMessage = 'Please fix all validation errors';
      return;
    }
    
    this.isLoading = true;
    
    this.http.post<any>(`${this.apiUrl}/auth/login`, this.loginData).subscribe({
      next: (response) => {
        localStorage.setItem('accessToken', response.accessToken);
        localStorage.setItem('refreshToken', response.refreshToken);
        
        // Decode JWT to get user role
        const token = response.accessToken;
        const payload = JSON.parse(atob(token.split('.')[1]));
        const role = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
        
        if (role === 'Admin') {
          this.router.navigate(['/admin']);
        } else {
          this.router.navigate(['/dashboard']);
        }
      },
      error: (error) => {
        this.errorMessage = error.error || 'Login failed. Please check your credentials.';
        this.isLoading = false;
      }
    });
  }
  
  goToRegister() {
    this.router.navigate(['/register']);
  }

  goToChat() {
    this.router.navigate(['/chat']);
  }
}
