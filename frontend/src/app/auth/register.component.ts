import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpClientModule } from '@angular/common/http';
import { Router } from '@angular/router';
import { FormValidators } from '../validators/form-validators';

@Component({
  selector: 'app-register',
  imports: [CommonModule, FormsModule, HttpClientModule],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css',
  standalone: true
})
export class RegisterComponent {
  registerData = {
    firstName: '',
    lastName: '',
    email: '',
    password: '',
    confirmPassword: '',
    role: 'User'
  };
  
  showPassword = false;
  showConfirmPassword = false;
  errorMessage = '';
  successMessage = '';
  isLoading = false;
  fieldErrors: any = {};
  
  private apiUrl = 'http://localhost:5000';
  
  constructor(private http: HttpClient, private router: Router) {}
  
  togglePasswordVisibility() {
    this.showPassword = !this.showPassword;
  }
  
  toggleConfirmPasswordVisibility() {
    this.showConfirmPassword = !this.showConfirmPassword;
  }
  
  validateField(field: string) {
    const data = this.registerData as any;
    switch(field) {
      case 'firstName':
        this.fieldErrors.firstName = FormValidators.validateFirstName(data.firstName);
        break;
      case 'lastName':
        this.fieldErrors.lastName = FormValidators.validateLastName(data.lastName);
        break;
      case 'email':
        this.fieldErrors.email = FormValidators.validateEmail(data.email);
        break;
      case 'password':
        this.fieldErrors.password = FormValidators.validatePassword(data.password);
        break;
      case 'confirmPassword':
        this.fieldErrors.confirmPassword = data.password !== data.confirmPassword ? 'Passwords do not match' : null;
        break;
    }
  }

  validateForm(): boolean {
    this.fieldErrors = {};
    this.fieldErrors.firstName = FormValidators.validateFirstName(this.registerData.firstName);
    this.fieldErrors.lastName = FormValidators.validateLastName(this.registerData.lastName);
    this.fieldErrors.email = FormValidators.validateEmail(this.registerData.email);
    this.fieldErrors.password = FormValidators.validatePassword(this.registerData.password);
    this.fieldErrors.confirmPassword = this.registerData.password !== this.registerData.confirmPassword ? 'Passwords do not match' : null;
    
    return !Object.values(this.fieldErrors).some(error => error !== null);
  }

  register() {
    this.errorMessage = '';
    this.successMessage = '';
    
    if (!this.validateForm()) {
      this.errorMessage = 'Please fix all validation errors';
      return;
    }
    
    this.isLoading = true;
    
    const payload = {
      firstName: this.registerData.firstName,
      lastName: this.registerData.lastName,
      email: this.registerData.email,
      password: this.registerData.password,
      role: this.registerData.role
    };
    
    this.http.post(`${this.apiUrl}/auth/register`, payload).subscribe({
      next: () => {
        this.successMessage = 'Registration successful! Redirecting to login...';
        setTimeout(() => {
          this.router.navigate(['/login']);
        }, 2000);
      },
      error: (error) => {
        this.errorMessage = error.error || 'Registration failed. Please try again.';
        this.isLoading = false;
      }
    });
  }
  
  goToLogin() {
    this.router.navigate(['/login']);
  }

  goToChat() {
    this.router.navigate(['/chat']);
  }
}
