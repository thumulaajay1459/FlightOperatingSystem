# 🎨 Frontend Architecture Guide - Flight Operation System

Complete guide for building the Angular frontend for the Flight Operation System microservices.

---

## 📋 Table of Contents

1. [Overview](#overview)
2. [Technology Stack](#technology-stack)
3. [Project Structure](#project-structure)
4. [Architecture Patterns](#architecture-patterns)
5. [Module Organization](#module-organization)
6. [Component Structure](#component-structure)
7. [Service Layer](#service-layer)
8. [State Management](#state-management)
9. [Routing & Navigation](#routing--navigation)
10. [Authentication & Authorization](#authentication--authorization)
11. [API Integration](#api-integration)
12. [Forms & Validation](#forms--validation)
13. [UI/UX Guidelines](#uiux-guidelines)
14. [Best Practices](#best-practices)

---

## 🎯 Overview

### **Architecture Type**
- **Pattern:** Component-Based Architecture
- **Framework:** Angular 17+ (Standalone Components)
- **State:** Service-based state management
- **Styling:** SCSS with modular approach

### **Key Principles**
- ✅ Separation of concerns
- ✅ Reusable components
- ✅ Reactive programming (RxJS)
- ✅ Type safety (TypeScript)
- ✅ Lazy loading for performance
- ✅ Responsive design (mobile-first)

---

## 🛠️ Technology Stack

```json
{
  "framework": "Angular 17+",
  "language": "TypeScript 5+",
  "styling": "SCSS/Tailwind CSS",
  "http": "HttpClient (Angular)",
  "routing": "Angular Router",
  "forms": "Reactive Forms",
  "state": "Services + RxJS",
  "ui": "Angular Material / PrimeNG",
  "icons": "Font Awesome / Material Icons",
  "pdf": "ngx-extended-pdf-viewer",
  "charts": "Chart.js / ngx-charts"
}
```

---

## 📁 Project Structure

```
Frontend/
├── src/
│   ├── app/
│   │   ├── core/                    # Singleton services, guards, interceptors
│   │   │   ├── guards/
│   │   │   │   ├── auth.guard.ts
│   │   │   │   └── role.guard.ts
│   │   │   ├── interceptors/
│   │   │   │   ├── auth.interceptor.ts
│   │   │   │   └── error.interceptor.ts
│   │   │   ├── services/
│   │   │   │   ├── auth.service.ts
│   │   │   │   ├── token.service.ts
│   │   │   │   └── storage.service.ts
│   │   │   └── models/
│   │   │       ├── user.model.ts
│   │   │       └── auth.model.ts
│   │   │
│   │   ├── shared/                  # Shared components, directives, pipes
│   │   │   ├── components/
│   │   │   │   ├── navbar/
│   │   │   │   ├── footer/
│   │   │   │   ├── loader/
│   │   │   │   └── alert/
│   │   │   ├── directives/
│   │   │   │   └── role-access.directive.ts
│   │   │   ├── pipes/
│   │   │   │   ├── date-format.pipe.ts
│   │   │   │   └── currency.pipe.ts
│   │   │   └── models/
│   │   │       └── common.model.ts
│   │   │
│   │   ├── features/                # Feature modules
│   │   │   ├── auth/
│   │   │   │   ├── components/
│   │   │   │   │   ├── login/
│   │   │   │   │   ├── register/
│   │   │   │   │   └── forgot-password/
│   │   │   │   ├── services/
│   │   │   │   │   └── auth-api.service.ts
│   │   │   │   └── auth.routes.ts
│   │   │   │
│   │   │   ├── flights/
│   │   │   │   ├── components/
│   │   │   │   │   ├── flight-list/
│   │   │   │   │   ├── flight-search/
│   │   │   │   │   ├── flight-details/
│   │   │   │   │   └── seat-selection/
│   │   │   │   ├── services/
│   │   │   │   │   └── flight.service.ts
│   │   │   │   ├── models/
│   │   │   │   │   └── flight.model.ts
│   │   │   │   └── flights.routes.ts
│   │   │   │
│   │   │   ├── bookings/
│   │   │   │   ├── components/
│   │   │   │   │   ├── booking-form/
│   │   │   │   │   ├── booking-list/
│   │   │   │   │   ├── booking-details/
│   │   │   │   │   └── ticket-download/
│   │   │   │   ├── services/
│   │   │   │   │   └── booking.service.ts
│   │   │   │   ├── models/
│   │   │   │   │   └── booking.model.ts
│   │   │   │   └── bookings.routes.ts
│   │   │   │
│   │   │   ├── profile/
│   │   │   │   ├── components/
│   │   │   │   │   ├── user-profile/
│   │   │   │   │   └── edit-profile/
│   │   │   │   ├── services/
│   │   │   │   │   └── profile.service.ts
│   │   │   │   └── profile.routes.ts
│   │   │   │
│   │   │   └── admin/
│   │   │       ├── components/
│   │   │       │   ├── dashboard/
│   │   │       │   ├── manage-flights/
│   │   │       │   ├── manage-aircraft/
│   │   │       │   ├── manage-airports/
│   │   │       │   └── manage-users/
│   │   │       ├── services/
│   │   │       │   └── admin.service.ts
│   │   │       └── admin.routes.ts
│   │   │
│   │   ├── layouts/                 # Layout components
│   │   │   ├── main-layout/
│   │   │   ├── auth-layout/
│   │   │   └── admin-layout/
│   │   │
│   │   ├── app.component.ts
│   │   ├── app.config.ts
│   │   └── app.routes.ts
│   │
│   ├── assets/
│   │   ├── images/
│   │   ├── icons/
│   │   └── styles/
│   │       ├── _variables.scss
│   │       ├── _mixins.scss
│   │       └── _themes.scss
│   │
│   ├── environments/
│   │   ├── environment.ts
│   │   └── environment.prod.ts
│   │
│   ├── index.html
│   ├── main.ts
│   └── styles.scss
│
├── angular.json
├── package.json
├── tsconfig.json
└── README.md
```

---

## 🏗️ Architecture Patterns

### **1. Feature-Based Architecture**

Organize by features, not by file types:

```
✅ Good:
features/
  ├── flights/
  │   ├── components/
  │   ├── services/
  │   └── models/

❌ Bad:
components/
  ├── flight-list/
  ├── booking-form/
services/
  ├── flight.service.ts
  ├── booking.service.ts
```

### **2. Smart vs Presentational Components**

**Smart Components (Containers):**
- Handle business logic
- Communicate with services
- Manage state
- Example: `FlightListComponent`

**Presentational Components (Dumb):**
- Receive data via @Input()
- Emit events via @Output()
- No service dependencies
- Example: `FlightCardComponent`

### **3. Service Layer Pattern**

```typescript
// API Service - HTTP calls
export class FlightApiService {
  getFlights() { /* HTTP call */ }
}

// Business Service - Logic + State
export class FlightService {
  constructor(private api: FlightApiService) {}
  
  flights$ = new BehaviorSubject<Flight[]>([]);
  
  loadFlights() {
    this.api.getFlights().subscribe(data => {
      this.flights$.next(data);
    });
  }
}
```

---

## 📦 Module Organization

### **Core Module** (Singleton Services)

```typescript
// core/services/auth.service.ts
@Injectable({ providedIn: 'root' })
export class AuthService {
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  currentUser$ = this.currentUserSubject.asObservable();
  
  login(credentials: LoginDto): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${API_URL}/auth/login`, credentials)
      .pipe(
        tap(response => {
          this.tokenService.setToken(response.token);
          this.currentUserSubject.next(response.user);
        })
      );
  }
  
  logout(): void {
    this.tokenService.clearToken();
    this.currentUserSubject.next(null);
    this.router.navigate(['/login']);
  }
  
  isAuthenticated(): boolean {
    return !!this.tokenService.getToken();
  }
  
  hasRole(role: string): boolean {
    const user = this.currentUserSubject.value;
    return user?.role === role;
  }
}
```

### **Shared Module** (Reusable Components)

```typescript
// shared/components/navbar/navbar.component.ts
@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.scss']
})
export class NavbarComponent {
  currentUser$ = this.authService.currentUser$;
  
  constructor(private authService: AuthService) {}
  
  logout(): void {
    this.authService.logout();
  }
}
```

---

## 🧩 Component Structure

### **Component Template**

```typescript
// features/flights/components/flight-list/flight-list.component.ts
@Component({
  selector: 'app-flight-list',
  standalone: true,
  imports: [CommonModule, FlightCardComponent, LoaderComponent],
  templateUrl: './flight-list.component.html',
  styleUrls: ['./flight-list.component.scss']
})
export class FlightListComponent implements OnInit, OnDestroy {
  flights$ = this.flightService.flights$;
  loading$ = this.flightService.loading$;
  error$ = this.flightService.error$;
  
  private destroy$ = new Subject<void>();
  
  constructor(private flightService: FlightService) {}
  
  ngOnInit(): void {
    this.loadFlights();
  }
  
  loadFlights(): void {
    this.flightService.loadFlights()
      .pipe(takeUntil(this.destroy$))
      .subscribe();
  }
  
  onFlightSelect(flight: Flight): void {
    this.router.navigate(['/flights', flight.id]);
  }
  
  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
```

### **Component HTML**

```html
<!-- flight-list.component.html -->
<div class="flight-list-container">
  <h2>Available Flights</h2>
  
  <!-- Loading State -->
  <app-loader *ngIf="loading$ | async"></app-loader>
  
  <!-- Error State -->
  <app-alert 
    *ngIf="error$ | async as error" 
    [type]="'error'" 
    [message]="error">
  </app-alert>
  
  <!-- Success State -->
  <div class="flights-grid" *ngIf="flights$ | async as flights">
    <app-flight-card 
      *ngFor="let flight of flights"
      [flight]="flight"
      (select)="onFlightSelect($event)">
    </app-flight-card>
  </div>
  
  <!-- Empty State -->
  <div class="empty-state" *ngIf="(flights$ | async)?.length === 0">
    <p>No flights available</p>
  </div>
</div>
```

---

## 🔧 Service Layer

### **HTTP Service Example**

```typescript
// features/flights/services/flight.service.ts
@Injectable({ providedIn: 'root' })
export class FlightService {
  private apiUrl = `${environment.apiUrl}/flights`;
  
  private flightsSubject = new BehaviorSubject<Flight[]>([]);
  private loadingSubject = new BehaviorSubject<boolean>(false);
  private errorSubject = new BehaviorSubject<string | null>(null);
  
  flights$ = this.flightsSubject.asObservable();
  loading$ = this.loadingSubject.asObservable();
  error$ = this.errorSubject.asObservable();
  
  constructor(private http: HttpClient) {}
  
  loadFlights(): Observable<Flight[]> {
    this.loadingSubject.next(true);
    this.errorSubject.next(null);
    
    return this.http.get<Flight[]>(this.apiUrl).pipe(
      tap(flights => {
        this.flightsSubject.next(flights);
        this.loadingSubject.next(false);
      }),
      catchError(error => {
        this.errorSubject.next('Failed to load flights');
        this.loadingSubject.next(false);
        return throwError(() => error);
      })
    );
  }
  
  searchFlights(params: FlightSearchParams): Observable<Flight[]> {
    return this.http.get<Flight[]>(`${this.apiUrl}/search`, { params });
  }
  
  getFlightById(id: number): Observable<Flight> {
    return this.http.get<Flight>(`${this.apiUrl}/${id}`);
  }
}
```

---

## 🔐 Authentication & Authorization

### **Auth Guard**

```typescript
// core/guards/auth.guard.ts
export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  
  if (authService.isAuthenticated()) {
    return true;
  }
  
  router.navigate(['/login'], { queryParams: { returnUrl: state.url } });
  return false;
};
```

### **Role Guard**

```typescript
// core/guards/role.guard.ts
export const roleGuard = (allowedRoles: string[]): CanActivateFn => {
  return (route, state) => {
    const authService = inject(AuthService);
    const router = inject(Router);
    
    const user = authService.getCurrentUser();
    
    if (user && allowedRoles.includes(user.role)) {
      return true;
    }
    
    router.navigate(['/unauthorized']);
    return false;
  };
};
```

### **Auth Interceptor**

```typescript
// core/interceptors/auth.interceptor.ts
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const tokenService = inject(TokenService);
  const token = tokenService.getToken();
  
  if (token) {
    req = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }
  
  return next(req);
};
```

### **Error Interceptor**

```typescript
// core/interceptors/error.interceptor.ts
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  
  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401) {
        authService.logout();
        router.navigate(['/login']);
      }
      
      if (error.status === 403) {
        router.navigate(['/unauthorized']);
      }
      
      return throwError(() => error);
    })
  );
};
```

---

## 🛣️ Routing & Navigation

### **App Routes**

```typescript
// app.routes.ts
export const routes: Routes = [
  {
    path: '',
    component: MainLayoutComponent,
    children: [
      { path: '', redirectTo: 'home', pathMatch: 'full' },
      { path: 'home', component: HomeComponent },
      {
        path: 'flights',
        loadChildren: () => import('./features/flights/flights.routes')
      },
      {
        path: 'bookings',
        loadChildren: () => import('./features/bookings/bookings.routes'),
        canActivate: [authGuard]
      },
      {
        path: 'profile',
        loadChildren: () => import('./features/profile/profile.routes'),
        canActivate: [authGuard]
      }
    ]
  },
  {
    path: 'admin',
    component: AdminLayoutComponent,
    loadChildren: () => import('./features/admin/admin.routes'),
    canActivate: [authGuard, roleGuard(['Admin'])]
  },
  {
    path: 'auth',
    component: AuthLayoutComponent,
    loadChildren: () => import('./features/auth/auth.routes')
  },
  { path: '**', component: NotFoundComponent }
];
```

### **Feature Routes**

```typescript
// features/flights/flights.routes.ts
export default [
  { path: '', component: FlightListComponent },
  { path: 'search', component: FlightSearchComponent },
  { path: ':id', component: FlightDetailsComponent }
] as Routes;
```

---

## 📝 Forms & Validation

### **Reactive Forms**

```typescript
// features/bookings/components/booking-form/booking-form.component.ts
@Component({
  selector: 'app-booking-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './booking-form.component.html'
})
export class BookingFormComponent implements OnInit {
  bookingForm!: FormGroup;
  
  constructor(
    private fb: FormBuilder,
    private bookingService: BookingService
  ) {}
  
  ngOnInit(): void {
    this.initForm();
  }
  
  initForm(): void {
    this.bookingForm = this.fb.group({
      flightId: ['', Validators.required],
      passengers: this.fb.array([this.createPassengerForm()]),
      contactEmail: ['', [Validators.required, Validators.email]],
      contactPhone: ['', [Validators.required, Validators.pattern(/^\d{10}$/)]]
    });
  }
  
  createPassengerForm(): FormGroup {
    return this.fb.group({
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      dateOfBirth: ['', Validators.required],
      gender: ['', Validators.required],
      seatNumber: ['', Validators.required]
    });
  }
  
  get passengers(): FormArray {
    return this.bookingForm.get('passengers') as FormArray;
  }
  
  addPassenger(): void {
    this.passengers.push(this.createPassengerForm());
  }
  
  removePassenger(index: number): void {
    this.passengers.removeAt(index);
  }
  
  onSubmit(): void {
    if (this.bookingForm.valid) {
      this.bookingService.createBooking(this.bookingForm.value)
        .subscribe({
          next: (response) => {
            // Handle success
          },
          error: (error) => {
            // Handle error
          }
        });
    }
  }
}
```

---

## 🎨 UI/UX Guidelines

### **Responsive Design**

```scss
// styles/_breakpoints.scss
$breakpoints: (
  'mobile': 320px,
  'tablet': 768px,
  'desktop': 1024px,
  'wide': 1440px
);

@mixin respond-to($breakpoint) {
  @media (min-width: map-get($breakpoints, $breakpoint)) {
    @content;
  }
}

// Usage
.flight-card {
  width: 100%;
  
  @include respond-to('tablet') {
    width: 50%;
  }
  
  @include respond-to('desktop') {
    width: 33.33%;
  }
}
```

### **Component Styling**

```scss
// flight-card.component.scss
.flight-card {
  border: 1px solid #e0e0e0;
  border-radius: 8px;
  padding: 1.5rem;
  transition: all 0.3s ease;
  
  &:hover {
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
    transform: translateY(-2px);
  }
  
  &__header {
    display: flex;
    justify-content: space-between;
    margin-bottom: 1rem;
  }
  
  &__price {
    font-size: 1.5rem;
    font-weight: bold;
    color: #2196f3;
  }
}
```

---

## 🔄 API Integration

### **Environment Configuration**

```typescript
// environments/environment.ts
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000',
  endpoints: {
    auth: '/auth',
    flights: '/flights',
    bookings: '/bookings',
    profile: '/profile',
    admin: '/admin'
  }
};
```

### **API Service Base**

```typescript
// core/services/api.service.ts
@Injectable({ providedIn: 'root' })
export class ApiService {
  private baseUrl = environment.apiUrl;
  
  constructor(private http: HttpClient) {}
  
  get<T>(endpoint: string, params?: any): Observable<T> {
    return this.http.get<T>(`${this.baseUrl}${endpoint}`, { params });
  }
  
  post<T>(endpoint: string, body: any): Observable<T> {
    return this.http.post<T>(`${this.baseUrl}${endpoint}`, body);
  }
  
  put<T>(endpoint: string, body: any): Observable<T> {
    return this.http.put<T>(`${this.baseUrl}${endpoint}`, body);
  }
  
  delete<T>(endpoint: string): Observable<T> {
    return this.http.delete<T>(`${this.baseUrl}${endpoint}`);
  }
}
```

---

## ✅ Best Practices

### **1. TypeScript Models**

```typescript
// models/flight.model.ts
export interface Flight {
  id: number;
  flightNumber: string;
  origin: Airport;
  destination: Airport;
  departureTime: Date;
  arrivalTime: Date;
  price: number;
  availableSeats: number;
  aircraft: Aircraft;
}

export interface Airport {
  id: number;
  code: string;
  name: string;
  city: string;
  country: string;
}
```

### **2. RxJS Best Practices**

```typescript
// Always unsubscribe
private destroy$ = new Subject<void>();

ngOnInit() {
  this.service.data$
    .pipe(takeUntil(this.destroy$))
    .subscribe();
}

ngOnDestroy() {
  this.destroy$.next();
  this.destroy$.complete();
}

// Use async pipe in templates (auto-unsubscribe)
<div *ngIf="data$ | async as data">{{ data }}</div>
```

### **3. Error Handling**

```typescript
loadData(): void {
  this.service.getData()
    .pipe(
      catchError(error => {
        this.errorMessage = 'Failed to load data';
        console.error(error);
        return of([]);
      })
    )
    .subscribe(data => this.data = data);
}
```

### **4. Performance Optimization**

```typescript
// Use OnPush change detection
@Component({
  changeDetection: ChangeDetectionStrategy.OnPush
})

// Use trackBy in *ngFor
<div *ngFor="let item of items; trackBy: trackById">

trackById(index: number, item: any): number {
  return item.id;
}

// Lazy load modules
loadChildren: () => import('./feature/feature.routes')
```

---

## 🚀 Getting Started

### **1. Install Dependencies**

```bash
cd Frontend
npm install
```

### **2. Configure Environment**

```typescript
// environments/environment.ts
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000'
};
```

### **3. Run Development Server**

```bash
ng serve
# Navigate to http://localhost:4200
```

### **4. Build for Production**

```bash
ng build --configuration production
```

---

## 📚 Key Pages to Implement

### **Public Pages:**
1. Home/Landing Page
2. Flight Search
3. Flight List
4. Flight Details
5. Login
6. Register

### **Authenticated Pages:**
7. User Dashboard
8. My Bookings
9. Booking Details
10. Create Booking
11. User Profile
12. Download Ticket

### **Admin Pages:**
13. Admin Dashboard
14. Manage Flights
15. Manage Aircraft
16. Manage Airports
17. Manage Routes
18. Manage Users
19. View All Bookings

---

## 🎯 Implementation Checklist

- [ ] Setup Angular project with standalone components
- [ ] Configure routing with lazy loading
- [ ] Implement authentication flow (login/register)
- [ ] Create auth guard and role guard
- [ ] Setup HTTP interceptors (auth + error)
- [ ] Create shared components (navbar, footer, loader)
- [ ] Implement flight search and listing
- [ ] Create booking flow with seat selection
- [ ] Implement user profile management
- [ ] Create admin dashboard and management pages
- [ ] Add form validation
- [ ] Implement error handling
- [ ] Add loading states
- [ ] Make responsive design
- [ ] Add PDF download functionality
- [ ] Implement notifications/toasts
- [ ] Add unit tests
- [ ] Optimize performance
- [ ] Build and deploy

---

## 📖 Additional Resources

- **Angular Docs:** https://angular.dev
- **RxJS Docs:** https://rxjs.dev
- **Angular Material:** https://material.angular.io
- **PrimeNG:** https://primeng.org

---

**Last Updated:** December 2024
**Version:** 1.0
