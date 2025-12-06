# Doctor Schedule & Clinic Operating Hours - Phase 2 Frontend Implementation

## ✅ Completed

### 1. Frontend Service Types Updated
- ✅ `clinicService.ts` - Added `OperatingHoursType` enum and operating hours fields
- ✅ `doctorService.ts` - Added `baseClinicId` and `baseClinicName` fields
- ✅ `doctorAvailabilityService.ts` - Added `AvailabilityType` enum, `endDate`, `notes` fields

## 🎨 Frontend UI Implementation Guide

### Phase 2A: Clinic Management UI - Operating Hours Configuration

**File to Update:** `frontend/src/apps/tenant/pages/clinics/ClinicsPage.tsx` or create new form component

**Features to Add:**
1. **Operating Hours Type Selector**
   - Radio buttons: Single Shift / Split Shift
   - Show/hide relevant time fields based on selection

2. **Time Fields (conditionally displayed)**
   - **Single Shift**: Full Day Start Time, Full Day End Time
   - **Split Shift**: 
     - Morning: Start Time, End Time
     - Evening: Start Time, End Time

3. **Validation**
   - End time must be after start time
   - For split shift: Evening start must be after morning end
   - Default values: Single Shift (10:00 AM - 5:00 PM)

**UI Mockup:**
```
Operating Hours Type: ○ Single Shift  ⦿ Split Shift

Morning Shift:
  Start Time: [10:00 AM] ▼   End Time: [02:00 PM] ▼

Evening Shift:
  Start Time: [05:00 PM] ▼   End Time: [08:00 PM] ▼
```

### Phase 2B: Doctor Management UI - Base Clinic Assignment

**File to Update:** `frontend/src/apps/tenant/pages/doctors/DoctorsPage.tsx` or doctor form component

**Features to Add:**
1. **Base Clinic Selector**
   - Dropdown with list of clinics
   - Label: "Primary/Base Clinic"
   - Help text: "The main clinic where this doctor works. The doctor will automatically follow this clinic's operating hours."

2. **Display in Doctor List**
   - Add "Base Clinic" column showing clinic name

**UI Mockup:**
```
Base Clinic: [Select Clinic...] ▼
ℹ️ The doctor will automatically be available at this clinic during its operating hours.
```

### Phase 2C: Doctor Schedule Page - Complete Redesign

**File to Update:** `frontend/src/apps/tenant/pages/doctors/DoctorSchedulePage.tsx`

**Major Changes Required:**

#### 1. Page Layout
```
┌─────────────────────────────────────────────────────────┐
│  Doctor Schedule Management                              │
│  [+ Add Exception] [+ Mark Leave] [View: Calendar/List] │
├─────────────────────────────────────────────────────────┤
│  Filters: [Doctor ▼] [Date Range Picker]                │
├─────────────────────────────────────────────────────────┤
│                                                           │
│  📅 Schedule Calendar / List View                        │
│                                                           │
│  • Default Schedule (from base clinic)                   │
│  • Exceptions & Leaves highlighted                       │
│                                                           │
└───────────────────────────────────────────────────────────┘
```

#### 2. Default Schedule Display
```typescript
// Show doctor's base clinic hours
const renderDefaultSchedule = (doctor: Doctor, baseClinic: Clinic) => {
  if (baseClinic.operatingHoursType === OperatingHoursType.SingleShift) {
    return `${doctor.doctorName} is available at ${baseClinic.name} 
            from ${baseClinic.fullDayStartTime} to ${baseClinic.fullDayEndTime}`;
  } else {
    return `${doctor.doctorName} is available at ${baseClinic.name}:
            Morning: ${baseClinic.morningStartTime} - ${baseClinic.morningEndTime}
            Evening: ${baseClinic.eveningStartTime} - ${baseClinic.eveningEndTime}`;
  }
};
```

#### 3. Add Exception Modal
```
┌─────────────────────────────────────────────────┐
│  Add Schedule Exception                          │
├─────────────────────────────────────────────────┤
│  Doctor: [Dr. John Smith]                       │
│  Exception Type: ⦿ Different Clinic             │
│                  ○ Modified Hours               │
│                  ○ Leave                        │
│                                                  │
│  [Date Picker] or [Date Range for Leave]       │
│  Clinic: [Select Clinic...] ▼                  │
│  Time: [09:00 AM] to [01:00 PM]                │
│  Notes: [Optional notes...]                     │
│                                                  │
│  [Cancel] [Save]                                │
└─────────────────────────────────────────────────┘
```

#### 4. Availability Type UI Components

**Different Clinic Exception:**
```tsx
<Form.Item label="Exception Type" name="availabilityType">
  <Radio.Group>
    <Radio value={AvailabilityType.DifferentClinic}>
      <Space direction="vertical">
        <Text strong>Different Clinic</Text>
        <Text type="secondary">Doctor will be at another clinic</Text>
      </Space>
    </Radio>
    <Radio value={AvailabilityType.ModifiedHours}>
      <Space direction="vertical">
        <Text strong>Modified Hours</Text>
        <Text type="secondary">Arriving late or leaving early</Text>
      </Space>
    </Radio>
    <Radio value={AvailabilityType.Leave}>
      <Space direction="vertical">
        <Text strong>Leave</Text>
        <Text type="secondary">Doctor is not available</Text>
      </Space>
    </Radio>
  </Radio.Group>
</Form.Item>
```

**Leave Date Range:**
```tsx
{availabilityType === AvailabilityType.Leave && (
  <Form.Item label="Leave Period" required>
    <DatePicker.RangePicker
      style={{ width: '100%' }}
      format="YYYY-MM-DD"
    />
  </Form.Item>
)}
```

#### 5. Schedule Table Columns

```typescript
const columns = [
  {
    title: 'Date',
    dataIndex: 'availableDate',
    render: (date: string, record: DoctorAvailability) => {
      if (record.endDate) {
        return `${dayjs(date).format('MMM DD')} - ${dayjs(record.endDate).format('MMM DD, YYYY')}`;
      }
      return dayjs(date).format('MMM DD, YYYY');
    },
  },
  {
    title: 'Type',
    dataIndex: 'availabilityType',
    render: (type: AvailabilityType) => {
      const config = {
        [AvailabilityType.Regular]: { color: 'green', text: 'Regular' },
        [AvailabilityType.DifferentClinic]: { color: 'blue', text: 'Different Clinic' },
        [AvailabilityType.Leave]: { color: 'red', text: 'Leave' },
        [AvailabilityType.ModifiedHours]: { color: 'orange', text: 'Modified Hours' },
      };
      return <Tag color={config[type].color}>{config[type].text}</Tag>;
    },
  },
  {
    title: 'Clinic',
    dataIndex: 'clinicName',
  },
  {
    title: 'Time',
    render: (_: any, record: DoctorAvailability) => {
      if (record.availabilityType === AvailabilityType.Leave) {
        return <Text type="secondary">Not Available</Text>;
      }
      return `${record.startTime} - ${record.endTime}`;
    },
  },
  {
    title: 'Notes',
    dataIndex: 'notes',
    render: (notes?: string) => notes || '-',
  },
  {
    title: 'Actions',
    render: (_: any, record: DoctorAvailability) => (
      <Space>
        <Button type="link" icon={<EditOutlined />} onClick={() => handleEdit(record)}>
          Edit
        </Button>
        <Popconfirm title="Delete this entry?" onConfirm={() => handleDelete(record.id)}>
          <Button type="link" danger icon={<DeleteOutlined />}>
            Delete
          </Button>
        </Popconfirm>
      </Space>
    ),
  },
];
```

### Phase 2D: Appointment Booking Integration

**File to Update:** `frontend/src/apps/tenant/pages/appointments/AppointmentsPage.tsx`

**Features to Add:**

1. **Available Time Slots Calculation**
```typescript
const calculateAvailableSlots = (
  doctor: Doctor,
  date: string,
  availability: DoctorAvailability[],
  existingAppointments: Appointment[]
) => {
  // 1. Check if doctor is on leave
  const isOnLeave = availability.some(
    a => a.availabilityType === AvailabilityType.Leave &&
         dayjs(date).isBetween(a.availableDate, a.endDate, 'day', '[]')
  );
  if (isOnLeave) return [];

  // 2. Check for different clinic or modified hours exceptions
  const exception = availability.find(
    a => dayjs(a.availableDate).format('YYYY-MM-DD') === date &&
         a.availabilityType !== AvailabilityType.Regular
  );

  // 3. Determine working hours
  let startTime, endTime;
  if (exception) {
    startTime = exception.startTime;
    endTime = exception.endTime;
  } else {
    // Use base clinic hours
    const baseClinic = getBaseClinic(doctor.baseClinicId);
    // Extract clinic operating hours
  }

  // 4. Generate 15-minute slots
  // 5. Filter out booked slots
  // 6. Return available slots
};
```

2. **Visual Indicators**
- Show "Doctor on Leave" if not available
- Show "At [Clinic Name]" if working at different clinic
- Show modified hours in appointment booking form

### Implementation Priority

1. **High Priority** (Core Functionality)
   - ✅ Service types updated
   - ⏳ Doctor Schedule Page redesign
   - ⏳ Availability type selection
   - ⏳ Leave date ranges

2. **Medium Priority** (Enhanced UX)
   - ⏳ Clinic operating hours UI
   - ⏳ Base clinic selector
   - ⏳ Calendar view for schedule

3. **Low Priority** (Integration)
   - ⏳ Appointment booking integration
   - ⏳ Available slots calculation
   - ⏳ Conflict detection

### Testing Checklist

- [ ] Create clinic with single shift hours
- [ ] Create clinic with split shift hours
- [ ] Assign base clinic to doctor
- [ ] View doctor's default schedule
- [ ] Add "Different Clinic" exception
- [ ] Add "Leave" for date range
- [ ] Add "Modified Hours" for specific date
- [ ] Edit/Delete schedule exceptions
- [ ] Book appointment (should respect schedule)

### Helper Components to Create

1. `OperatingHoursForm.tsx` - Reusable operating hours input
2. `AvailabilityTypeSelector.tsx` - Availability type radio group
3. `ScheduleCalendar.tsx` - Calendar view of doctor schedules
4. `TimeSlotPicker.tsx` - Available time slots display

## 🚀 Next Steps

After Phase 2 is complete:
1. **Backend DTOs** - Update backend DTOs to match frontend expectations
2. **API Testing** - Test all endpoints with new fields
3. **Integration Testing** - End-to-end testing of schedule flow
4. **User Documentation** - Create user guide for schedule management


