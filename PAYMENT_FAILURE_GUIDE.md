# Payment Failure Handling

## Overview
When users exit without completing payment, the system automatically marks the payment as failed and updates the booking status WITHOUT triggering notification service.

---

## Features

### 1. **Manual Payment Failure**
Users or frontend can explicitly mark a payment as failed when they exit the payment flow.

**Endpoint:** `POST /api/payments/fail`

**Request:**
```json
{
  "paymentId": "guid",
  "reason": "User exited without payment"
}
```

**Response:**
```json
{
  "paymentId": "guid",
  "status": "Failed",
  "failureReason": "User exited without payment",
  ...
}
```

**Side Effects:**
- Payment status → `Failed`
- Booking status → `Failed`
- ❌ NO notification sent

---

### 2. **Automatic Payment Expiry**
A background service runs every 5 minutes to automatically expire pending payments.

**Configuration:** `appsettings.json`
```json
{
  "Payment": {
    "ExpiryMinutes": 15
  }
}
```

**Logic:**
- Checks for payments with status `Pending` or `OrderCreated`
- If payment was created more than 15 minutes ago
- Marks payment as `Failed` with reason: "Payment expired - User did not complete payment"
- Marks booking as `Failed`
- ❌ NO notification sent

---

## Payment & Booking Status Flow

### Success Flow:
```
Payment: Pending → OrderCreated → Success ✓
Booking: Pending → Confirmed ✓
         ↓
    Notification Sent ✅
```

### Failure Flow:
```
Payment: Pending → OrderCreated → Failed ✗
Booking: Pending → Failed ✗
         ↓
    NO Notification ❌
```

---

## Implementation Details

### Payment Service:
1. **FailPaymentAsync()** - Marks payment as failed
2. **FailBookingAsync()** - Calls BookingService to mark booking as failed
3. **ExpirePendingPaymentsAsync()** - Auto-expires old payments

### Booking Service:
1. **POST /api/booking/{id}/confirm-payment** - Confirms booking + sends notification ✅
2. **POST /api/booking/{id}/fail-payment** - Fails booking + NO notification ❌

### Key Difference:
- `confirm-payment` → Sends notification with PDF ticket
- `fail-payment` → Only updates status, no notification

---

## Files Modified:

### PaymentService:
1. **PaymentDTOs.cs** - Added `FailPaymentRequest`
2. **IServices.cs** - Added `FailPaymentAsync()` and `ExpirePendingPaymentsAsync()`
3. **RazorpayPaymentService.cs** - Implemented failure logic + booking integration
4. **PaymentController.cs** - Added `/fail` endpoint
5. **Program.cs** - Registered background service
6. **appsettings.json** - Added expiry configuration

### BookingService:
1. **BookingController.cs** - Added `/fail-payment` endpoint
2. **Booking.cs** - Already has `Failed` status

### New Files:
1. **PaymentExpiryService.cs** - Background service
2. **FailPaymentRequestValidator.cs** - Validation
3. **PAYMENT_FAILURE_TESTS.http** - Test cases
4. **PAYMENT_FAILURE_GUIDE.md** - Documentation

---

## Frontend Integration

### When User Exits Payment Modal:
```typescript
async onPaymentModalClose() {
  if (!this.paymentCompleted) {
    // Mark payment as failed - NO notification will be sent
    await this.http.post('/api/payments/fail', {
      paymentId: this.currentPaymentId,
      reason: 'User exited without payment'
    }).toPromise();
  }
}
```

### On Payment Success:
```typescript
async onPaymentSuccess(response) {
  this.paymentCompleted = true;
  // Verify payment - notification WILL be sent after confirmation
  await this.http.post('/api/payments/verify', {
    paymentId: this.currentPaymentId,
    razorpayOrderId: response.razorpay_order_id,
    razorpayPaymentId: response.razorpay_payment_id,
    razorpaySignature: response.razorpay_signature
  }).toPromise();
}
```

---

## Testing

### Test Scenario 1: Manual Failure
1. Create payment order
2. User closes payment modal
3. Call `/payments/fail` endpoint
4. Verify status is `Failed`

### Test Scenario 2: Auto Expiry
1. Create payment order
2. Wait 15+ minutes
3. Background service runs
4. Verify status is `Failed`

### Test Scenario 3: Cannot Fail Successful Payment
1. Complete payment successfully
2. Try to call `/payments/fail`
3. Should return error: "Cannot fail a successful payment"

---

## Benefits

✅ **Data Integrity** - No orphaned payment records  
✅ **Clear Status** - Easy to track failed payments  
✅ **Automatic Cleanup** - Background service handles expired payments  
✅ **User Experience** - Immediate feedback when exiting  
✅ **Analytics** - Track payment abandonment rates  
✅ **No Spam** - Failed payments don't trigger email notifications  
✅ **Booking Sync** - Payment and booking status stay in sync  

---

## Configuration

**Adjust Expiry Time:**
```json
{
  "Payment": {
    "ExpiryMinutes": 30  // Change to 30 minutes
  }
}
```

**Adjust Background Service Interval:**
Edit `PaymentExpiryService.cs`:
```csharp
await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken); // Run every 10 minutes
```

---

## API Reference

### POST /api/payments/fail
Mark payment as failed when user exits

**Auth:** Required  
**Body:** `FailPaymentRequest`  
**Returns:** `PaymentResponse`  
**Side Effects:**
- Payment status → Failed
- Booking status → Failed
- ❌ NO notification sent

### POST /api/booking/{id}/confirm-payment
Confirm booking after successful payment

**Auth:** Required  
**Returns:** Success message  
**Side Effects:**
- Booking status → Confirmed
- ✅ Notification sent with PDF ticket

### POST /api/booking/{id}/fail-payment
Mark booking as failed (called by PaymentService)

**Auth:** Required  
**Returns:** Success message  
**Side Effects:**
- Booking status → Failed
- ❌ NO notification sent

### GET /api/payments/{paymentId}
Check payment status

**Auth:** Required  
**Returns:** `PaymentResponse`

---

## Monitoring

**Check Failed Payments:**
```sql
SELECT * FROM Payments 
WHERE Status = 'Failed' 
ORDER BY UpdatedAt DESC;
```

**Check Expired Payments:**
```sql
SELECT * FROM Payments 
WHERE Status = 'Failed' 
AND FailureReason LIKE '%expired%'
ORDER BY UpdatedAt DESC;
```

---

**Last Updated:** December 2024  
**Version:** 1.0
