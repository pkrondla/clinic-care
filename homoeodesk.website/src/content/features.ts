export const featureGroups = [
  {
    id: 'care',
    title: 'Patient care built for homoeopaths',
    lead: 'Clinical workflow that matches how you take a case and write a prescription.',
    items: [
      'Consultation notes: complaint, symptoms, examination, diagnosis, and plan',
      'Homoeopathy prescription forms — globules, tablets, packets, liquids, tonics',
      'Dosage, frequency, duration, timing, and dram sizes where they matter',
      'Follow-up reminders and teleconsultation support',
      'Patient photos and history in one place',
    ],
  },
  {
    id: 'ops',
    title: 'Operations that keep the clinic moving',
    lead: 'Front desk tools for busy mornings and multi-branch practices.',
    items: [
      'Appointments and doctor availability',
      'Live queues for staff, doctors, and public displays',
      'Multi-branch clinics with role-based access',
      'Admin, Doctor, Staff, and Patient experiences',
    ],
  },
  {
    id: 'pharmacy',
    title: 'Pharmacy & inventory without the spreadsheet',
    lead: 'Keep remedies stocked and prescriptions fulfilled from the same system.',
    items: [
      'Clinic medicines linked to a global catalog',
      'Inventory levels, stock audit, and suppliers',
      'Purchase orders for replenishment',
      'Dispensing connected to prescriptions',
    ],
  },
  {
    id: 'billing',
    title: 'Billing, reports, and notifications',
    lead: 'Close the visit cleanly and keep patients informed.',
    items: [
      'Invoices and payments, including PDF invoices',
      'Collections, inventory, and patient activity reports',
      'WhatsApp, email, and SMS notification settings',
      'Preferences for appointments, Rx ready, payments, and follow-ups',
    ],
  },
] as const
