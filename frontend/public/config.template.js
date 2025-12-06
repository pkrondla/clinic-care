// Template configuration file for ClinicCare Frontend
// Copy this file to config.js and update with your production values
// This file is loaded at runtime and can be modified without rebuilding

window.__CLINICCARE_CONFIG__ = {
  // Backend API base URL (without /api suffix)
  API_BASE_URL: 'https://api.yourdomain.com',
  
  // Full API URL (with /api suffix)
  API_URL: 'https://api.yourdomain.com/api',
  
  // SignalR WebSocket URL
  SIGNALR_URL: 'https://api.yourdomain.com',
  
  // Feature flags
  ENABLE_NOTIFICATIONS: true,
  ENABLE_SIGNALR: true,
  
  // Development subdomain (for local testing)
  DEV_SUBDOMAIN: 'demo',
  
  // Environment: 'development', 'staging', 'production'
  ENVIRONMENT: 'production'
};

