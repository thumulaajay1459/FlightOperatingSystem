export class FormValidators {
  static validateFirstName(value: string): string | null {
    if (!value || value.trim() === '') return 'First name is required';
    if (value.length < 2 || value.length > 50) return 'First name must be between 2 and 50 characters';
    if (!/^[a-zA-Z ]+$/.test(value)) return 'First name can only contain letters';
    return null;
  }

  static validateLastName(value: string): string | null {
    if (!value || value.trim() === '') return 'Last name is required';
    if (value.length < 2 || value.length > 50) return 'Last name must be between 2 and 50 characters';
    if (!/^[a-zA-Z ]+$/.test(value)) return 'Last name can only contain letters';
    return null;
  }

  static validateEmail(value: string): string | null {
    if (!value || value.trim() === '') return 'Email is required';
    if (value.length > 100) return 'Email cannot exceed 100 characters';
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value)) return 'Invalid email format';
    return null;
  }

  static validatePassword(value: string): string | null {
    if (!value) return 'Password is required';
    if (value.length < 8) return 'Password must be at least 8 characters';
    if (!/[A-Z]/.test(value)) return 'Password must contain at least one uppercase letter';
    if (!/[a-z]/.test(value)) return 'Password must contain at least one lowercase letter';
    if (!/[0-9]/.test(value)) return 'Password must contain at least one number';
    if (!/[^a-zA-Z0-9]/.test(value)) return 'Password must contain at least one special character';
    return null;
  }

  static validateFullName(value: string): string | null {
    if (!value || value.trim() === '') return 'Full name is required';
    if (value.length < 2 || value.length > 100) return 'Full name must be between 2 and 100 characters';
    if (!/^[a-zA-Z ]+$/.test(value)) return 'Full name can only contain letters and spaces';
    return null;
  }

  static validateAge(value: number): string | null {
    if (value < 0 || value > 120) return 'Age must be between 0 and 120';
    return null;
  }

  static validateGender(value: string): string | null {
    if (!value) return 'Gender is required';
    if (!['Male', 'Female', 'Other'].includes(value)) return 'Gender must be Male, Female, or Other';
    return null;
  }

  static validatePassportNumber(value: string): string | null {
    if (!value || value.trim() === '') return 'Passport number is required';
    if (value.length < 6 || value.length > 20) return 'Passport number must be between 6 and 20 characters';
    if (!/^[A-Z0-9]+$/.test(value)) return 'Passport number must contain only uppercase letters and numbers';
    return null;
  }

  static validateManufacturer(value: string): string | null {
    if (!value || value.trim() === '') return 'Manufacturer is required';
    if (value.length < 2 || value.length > 50) return 'Manufacturer must be between 2 and 50 characters';
    return null;
  }

  static validateModel(value: string): string | null {
    if (!value || value.trim() === '') return 'Model is required';
    if (value.length < 2 || value.length > 50) return 'Model must be between 2 and 50 characters';
    return null;
  }

  static validateTotalSeats(value: number): string | null {
    if (value < 1 || value > 1000) return 'Total seats must be between 1 and 1000';
    return null;
  }

  static validateAirportCode(value: string): string | null {
    if (!value || value.trim() === '') return 'Airport code is required';
    if (value.length !== 3) return 'Airport code must be exactly 3 characters';
    if (!/^[A-Z]+$/.test(value)) return 'Airport code must contain only uppercase letters';
    return null;
  }

  static validateAirportName(value: string): string | null {
    if (!value || value.trim() === '') return 'Airport name is required';
    if (value.length < 3 || value.length > 100) return 'Airport name must be between 3 and 100 characters';
    return null;
  }

  static validateCity(value: string): string | null {
    if (!value || value.trim() === '') return 'City is required';
    if (value.length < 2 || value.length > 50) return 'City must be between 2 and 50 characters';
    return null;
  }

  static validateCountry(value: string): string | null {
    if (!value || value.trim() === '') return 'Country is required';
    if (value.length < 2 || value.length > 50) return 'Country must be between 2 and 50 characters';
    return null;
  }

  static validateAirportId(value: number, fieldName: string): string | null {
    if (value <= 0) return `${fieldName} must be greater than 0`;
    return null;
  }

  static validateDistance(value: number): string | null {
    if (value < 1 || value > 20000) return 'Distance must be between 1 and 20000 km';
    return null;
  }

  static validateFlightNumber(value: string): string | null {
    if (!value || value.trim() === '') return 'Flight number is required';
    if (value.length < 3 || value.length > 10) return 'Flight number must be between 3 and 10 characters';
    if (!/^[A-Z0-9]+$/.test(value)) return 'Flight number must contain only uppercase letters and numbers';
    return null;
  }

  static validatePrice(value: number, fieldName: string): string | null {
    if (value <= 0) return `${fieldName} must be greater than 0`;
    return null;
  }

  static validateDepartureTime(value: string): string | null {
    if (!value) return 'Departure time is required';
    const date = new Date(value);
    if (date <= new Date()) return 'Departure time must be in the future';
    return null;
  }

  static validateArrivalTime(departureTime: string, arrivalTime: string): string | null {
    if (!arrivalTime) return 'Arrival time is required';
    const departure = new Date(departureTime);
    const arrival = new Date(arrivalTime);
    if (arrival <= departure) return 'Arrival time must be after departure time';
    return null;
  }

  static validateNationality(value: string): string | null {
    if (!value || value.trim() === '') return 'Nationality is required';
    if (value.length < 2 || value.length > 50) return 'Nationality must be between 2 and 50 characters';
    return null;
  }
}
