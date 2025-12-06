// Runtime configuration for ClinicCare Frontend
// This file is loaded at runtime and can be modified without rebuilding
// For IIS deployment, update this file with your production API URL

window.__CLINICCARE_CONFIG__ = {
  API_BASE_URL: 'http://localhost:7000',
  API_URL: 'http://localhost:7000/api',
  SIGNALR_URL: 'http://localhost:7000',
  ENABLE_NOTIFICATIONS: true,
  ENABLE_SIGNALR: true,
  DEV_SUBDOMAIN: 'demo',
  ENVIRONMENT: 'development'
};

