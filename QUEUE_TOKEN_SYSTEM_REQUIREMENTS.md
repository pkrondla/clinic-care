# Queue/Token System Requirements
## Appointment Booking & Queue System

**Date:** November 23, 2025  
**Clarification:** Patients can book appointments through multiple channels. All appointments form a single queue per doctor with sequential token numbers.

---

## 🎯 Core Concept

- **Multiple Booking Channels:** Patients can book via walk-in, telephone, or website
- **Unified Queue:** All appointments for a doctor fall into a single queue
- **Sequential Tokens:** Token numbers are generated sequentially per doctor
- **Public Queue View:** All patients can view all doctor queues
- **Privacy:** Patients see only token numbers; Staff see patient names and mobile numbers
- **Queue Processing:** Doctor processes patients in token order

---

## 📋 User Flows

### 1. **Appointment Booking Methods**

**Walk-In (Staff Creates):**
1. Patient arrives at clinic
2. Staff searches/registers patient
3. Staff selects doctor
4. System auto-generates next sequential token number for that doctor
5. Patient receives token number
6. Appointment added to doctor's queue

**Telephone (Staff Creates):**
1. Patient calls clinic
2. Staff searches/registers patient
3. Staff selects doctor
4. System auto-generates next sequential token number
5. Staff informs patient of token number
6. Appointment added to doctor's queue
7. Patient should arrive at clinic before their token is called

**Website (Patient Creates):**
1. Patient visits website/app
2. Patient logs in (or registers)
3. Patient selects doctor
4. System auto-generates next sequential token number
5. Patient sees their token number
6. Appointment added to doctor's queue
7. Patient can view queue status

### 2. **Patient Queue View (Public)**

1. Patient visits queue page (mobile-friendly or website)
2. Patient can view ALL doctor queues
3. For each doctor queue, patient sees:
   - Token numbers only (privacy - no patient names)
   - Current token being served (highlighted)
   - All waiting tokens
   - Status indicators (Waiting, In Progress, Completed)
4. If patient is logged in:
   - Their own token is highlighted
   - Their position in queue shown
   - Estimated wait time
5. Real-time updates via SignalR
6. Alert when their token is next

### 3. **Staff Queue Management**

1. Staff logs in and selects clinic
2. Staff sees all doctor queues
3. For each queue, staff sees:
   - All tokens in sequential order
   - Patient name and mobile number (full details)
   - Current token being served
   - Status of each token
4. Staff can:
   - Add new appointments (walk-in or phone)
   - View patient details
   - Update appointment status
   - Cancel appointments

### 4. **Doctor Queue Processing**

1. Doctor logs in and selects clinic
2. Doctor sees their own queue
3. Queue shows:
   - All tokens in sequential order
   - Current token (highlighted)
   - Patient name and mobile number
   - Status of each token
4. Doctor clicks "Start" on next token → Status changes to "In Progress"
5. Doctor completes consultation → Status changes to "Completed"
6. Next token automatically highlighted

---

## 🔧 Technical Requirements

### Backend

1. **Token Number Generation Service**
   - Generate next sequential token number per doctor
   - Single queue per doctor (all appointments)
   - Handle concurrent requests (thread-safe)
   - Format: Sequential number (1, 2, 3, ...)
   - **Note:** Clarify if tokens reset daily or are continuous

2. **Queue Queries**
   - Get queue by doctor (all appointments in queue)
   - Get all doctor queues (for public view)
   - Get current token being served per doctor
   - Get patient's position in queue
   - Get queue statistics (wait times, etc.)

3. **Real-Time Updates (SignalR)**
   - Queue position updates
   - Token status changes
   - New tokens added
   - Current token changes
   - Broadcast to all connected clients

4. **Public Queue View Endpoint**
   - Public endpoint: Show all doctor queues with token numbers only
   - Authenticated endpoint: Show patient's own token details
   - Staff/Doctor endpoint: Show full patient details (name, mobile)

5. **Appointment Booking Endpoints**
   - Staff booking (walk-in/phone)
   - Patient self-booking (website)
   - Auto-generate token on booking

### Frontend

1. **Staff Queue Management**
   - View all doctor queues
   - Add appointment form (walk-in/phone)
   - Patient search/registration
   - Doctor selection
   - Auto-generated token display
   - Full patient details (name, mobile) visible
   - Queue management actions

2. **Doctor Queue View**
   - Doctor's own queue
   - Token list in sequential order
   - Patient name and mobile visible
   - Start/Complete buttons
   - Current token highlighted

3. **Patient Queue View (Public)**
   - View ALL doctor queues
   - Mobile-responsive design
   - Token numbers only (privacy)
   - Current serving token highlighted
   - Real-time updates
   - If logged in: Personal token highlighted with position

4. **Patient Self-Booking (Website)**
   - Patient login/registration
   - Doctor selection
   - Book appointment
   - Receive token number
   - View queue status

---

## 📊 Database Considerations

Current `Appointment` entity can be used, but terminology should be:
- "Appointment" = "Queue Entry" or "Token"
- "AppointmentDate" = "Queue Date" (always today for walk-ins)
- Token numbers are sequential per doctor/clinic/date

**Token Number Generation Logic:**
```csharp
// Pseudo-code - Sequential tokens per doctor
public async Task<int> GetNextTokenNumber(int doctorId, int clinicId, DateOnly date)
{
    // Get max token for this doctor on this date
    // If tokens reset daily, filter by date
    // If tokens are continuous, remove date filter
    var maxToken = await _context.Appointments
        .Where(a => a.DoctorId == doctorId 
                 && a.ClinicId == clinicId 
                 && a.AppointmentDate.Value == date
                 && a.Status != AppointmentStatus.Cancelled)
        .MaxAsync(a => (int?)a.TokenNumber) ?? 0;
    
    return maxToken + 1;
}

// Alternative: Continuous tokens (no daily reset)
public async Task<int> GetNextTokenNumber(int doctorId)
{
    var maxToken = await _context.Appointments
        .Where(a => a.DoctorId == doctorId 
                 && a.Status != AppointmentStatus.Cancelled)
        .MaxAsync(a => (int?)a.TokenNumber) ?? 0;
    
    return maxToken + 1;
}
```

---

## 🎨 UI/UX Requirements

### Staff Interface
- Simple "Add to Queue" button/form
- Quick patient search
- Doctor selection dropdown
- Token number display (large, visible)
- Print token option (optional)

### Doctor Interface
- Queue dashboard showing all tokens
- Current token highlighted
- One-click "Start Consultation"
- One-click "Complete Consultation"
- Patient details on hover/click

### Patient Mobile Interface
- Large token number display
- Current serving token (large)
- "Your turn in X patients" message
- Color-coded status (Waiting = blue, In Progress = orange, Completed = green)
- Auto-refresh every 5-10 seconds OR real-time via SignalR

---

## 🔄 Status Flow

```
Scheduled (Waiting) 
    ↓
In Progress (Doctor started)
    ↓
Completed (Consultation done)
```

**Cancelled** can happen at any time before Completed.

---

## 📱 Queue Page Routes

- **Public Queue View:** `/queue` (shows all doctor queues, token numbers only)
- **Patient Queue View:** `/patient/queue` (authenticated, shows patient's token details)
- **Staff Queue Management:** `/staff/queue` (authenticated, shows all queues with patient details)
- **Doctor Queue View:** `/doctor/queue` (authenticated, shows doctor's own queue)
- **Patient Self-Booking:** `/book-appointment` (authenticated, patient books appointment)

---

## ✅ Implementation Checklist

### Backend
- [ ] Create `TokenNumberService` for sequential token generation per doctor
- [ ] Enhance `CreateAppointmentCommand` to auto-generate token
- [ ] Create `GetQueueQuery` (by doctor - all appointments)
- [ ] Create `GetAllQueuesQuery` (all doctor queues for public view)
- [ ] Create `GetPatientQueuePositionQuery`
- [ ] Create patient self-booking endpoint
- [ ] Enhance SignalR `QueueHub` for real-time updates (broadcast to all)
- [ ] Create public queue view endpoint (token numbers only)
- [ ] Create authenticated queue endpoints (with patient details for staff/doctor)

### Frontend
- [ ] Staff: Queue management dashboard (all doctor queues with patient details)
- [ ] Staff: "Add Appointment" form (walk-in/phone booking)
- [ ] Doctor: Queue view (own queue with patient details)
- [ ] Patient: Public queue view page (all doctor queues, token numbers only)
- [ ] Patient: Self-booking page (website booking)
- [ ] Patient: Authenticated queue view (personal token details)
- [ ] Real-time queue updates (SignalR integration)
- [ ] Token number display component
- [ ] Privacy: Hide patient names in public view

---

## 🚀 Priority

**HIGH** - This is a core feature that needs to be clarified and enhanced before other features.

---

## 🔒 Privacy & Security

- **Public Queue View:** Only token numbers visible (no patient names)
- **Staff/Doctor View:** Full patient details (name, mobile number)
- **Patient Authenticated View:** Can see their own token details and position
- **Queue Updates:** Real-time updates broadcast to all connected clients

## 📝 Notes

- The existing "Appointment" entity and endpoints can be reused
- Token numbers are sequential per doctor
- All appointments for a doctor form a single queue
- Patients can book via walk-in, phone (staff), or website (self)
- Queue visibility is public (all can see all doctor queues)
- Privacy is maintained by showing only token numbers in public view

