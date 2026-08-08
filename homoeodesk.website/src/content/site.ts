export const navLinks = [
  { to: '/features', label: 'Features' },
  { to: '/pricing', label: 'Pricing' },
  { to: '/demo', label: 'Demo' },
  { to: '/about', label: 'About' },
] as const

export const heroCopy = {
  brand: 'HomoeoDesk',
  headline: 'Clinic software built for homoeopathy.',
  lead:
    'Run appointments, consultations, prescriptions, inventory, and billing in one calm workspace — designed for how homoeopathy clinics actually work.',
  primaryCta: 'Start free trial',
  secondaryCta: 'Try demo clinic',
}

export const workflowSteps = [
  {
    title: 'Front desk greets the day',
    body: 'Appointments and a live queue keep reception, doctors, and patients aligned.',
  },
  {
    title: 'Doctor consults with clarity',
    body: 'Capture complaints, findings, and a homoeopathy-native prescription without fighting generic EMR forms.',
  },
  {
    title: 'Dispense and settle',
    body: 'Stock, dispensing, invoices, and payments close the visit — with notifications when it matters.',
  },
] as const

export const pillars = [
  {
    title: 'Patient care',
    body: 'Consultations, follow-ups, teleconsult, and prescriptions shaped for globules, liquids, tonics, and more.',
  },
  {
    title: 'Front desk & ops',
    body: 'Booking, schedules, live queues, multi-branch access, and role-based work for Admin, Doctor, and Staff.',
  },
  {
    title: 'Pharmacy & stock',
    body: 'Clinic medicines from a shared catalog, inventory, purchase orders, and dispensing tied to prescriptions.',
  },
  {
    title: 'Billing & insight',
    body: 'Invoices, payments, and clear reports on collections, inventory, and patient activity.',
  },
] as const

export const teamRoles = [
  {
    role: 'Clinic owner',
    body: 'See the whole practice — branches, team access, collections, and subscription health.',
  },
  {
    role: 'Doctor',
    body: 'Focus on the consult: history, examination, diagnosis, and a prescription that speaks homoeopathy.',
  },
  {
    role: 'Front desk / staff',
    body: 'Manage the queue, appointments, dispensing, and billing without hopping between tools.',
  },
] as const

export const demoAccounts = [
  { role: 'Admin', email: 'admin@demo.com', note: 'Full clinic configuration' },
  { role: 'Doctor', email: 'doctor@demo.com', note: 'Consultations & prescriptions' },
  { role: 'Reception', email: 'reception@demo.com', note: 'Appointments, queue, billing' },
] as const
