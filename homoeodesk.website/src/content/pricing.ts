export const plans = [
  {
    id: 'trial',
    name: 'Trial',
    price: 'Free',
    period: '30 days',
    description: 'Explore HomoeoDesk with your clinic team before you commit.',
    cta: 'Start free trial',
    ctaTo: '/register',
    highlighted: false,
    features: [
      'Full clinic workflow access',
      'Appointments, queue, consultations, Rx',
      'Inventory and billing basics',
      'Email support during trial',
    ],
  },
  {
    id: 'clinic',
    name: 'Clinic',
    price: 'Contact us',
    period: 'monthly',
    description: 'For a single practice ready to run day-to-day on HomoeoDesk.',
    cta: 'Request pricing',
    ctaTo: '/request-trial?plan=clinic',
    highlighted: true,
    features: [
      'Everything in Trial',
      'Production tenant for your clinic',
      'WhatsApp / SMS / email notifications',
      'Reports and multi-user roles',
      'Onboarding assistance',
    ],
  },
  {
    id: 'practice',
    name: 'Practice+',
    price: 'Contact us',
    period: 'monthly',
    description: 'For multi-branch practices that need broader limits and priority help.',
    cta: 'Talk to us',
    ctaTo: '/contact?plan=practice',
    highlighted: false,
    features: [
      'Everything in Clinic',
      'Multi-branch operations',
      'Higher doctor / patient limits',
      'Priority support',
      'Guided migration from spreadsheets or legacy tools',
    ],
  },
] as const

export const pricingFaqs = [
  {
    q: 'How long is the trial?',
    a: 'Trials run for 30 days from activation. You can request an extension while we finish onboarding.',
  },
  {
    q: 'Is the public demo the same as a trial?',
    a: 'The demo clinic is a shared sandbox with sample data. A trial creates a registry entry for your organization so we can provision your dedicated clinic environment.',
  },
  {
    q: 'Do you support WhatsApp notifications?',
    a: 'Yes. HomoeoDesk supports WhatsApp, email, and SMS notification settings for appointments, prescriptions, payments, and follow-ups.',
  },
  {
    q: 'Can we run multiple branches?',
    a: 'Yes. Multi-branch clinics are supported with role-based access for your team.',
  },
  {
    q: 'Who owns our clinic data?',
    a: 'Your clinic data belongs to your organization. See our Privacy Policy and Security page for how we isolate tenants and handle information.',
  },
] as const
