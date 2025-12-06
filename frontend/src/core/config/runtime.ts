// Runtime configuration loader
// Loads configuration from window.__CLINICCARE_CONFIG__ (set in public/config.js)

interface RuntimeConfig {
  API_BASE_URL?: string;
  API_URL?: string;
  SIGNALR_URL?: string;
  ENABLE_NOTIFICATIONS?: boolean;
  ENABLE_SIGNALR?: boolean;
  DEV_SUBDOMAIN?: string;
  ENVIRONMENT?: string;
}

declare global {
  interface Window {
    __CLINICCARE_CONFIG__?: RuntimeConfig;
  }
}

/**
 * Get runtime configuration value
 * Falls back to environment variables or defaults
 */
export function getRuntimeConfig(): RuntimeConfig {
  return window.__CLINICCARE_CONFIG__ || {};
}

/**
 * Get API base URL from runtime config, env vars, or default
 */
export function getApiBaseUrl(): string {
  const runtime = getRuntimeConfig();
  
  // Priority: Runtime config > Environment variable > Default
  return runtime.API_BASE_URL 
    || import.meta.env.VITE_API_BASE_URL 
    || import.meta.env.VITE_API_URL 
    || 'http://localhost:7000';
}

/**
 * Get full API URL (with /api suffix)
 */
export function getApiUrl(): string {
  const runtime = getRuntimeConfig();
  
  return runtime.API_URL 
    || import.meta.env.VITE_API_BASE_URL 
    || import.meta.env.VITE_API_URL 
    || 'http://localhost:7000/api';
}

/**
 * Get SignalR URL
 */
export function getSignalRUrl(): string {
  const runtime = getRuntimeConfig();
  
  return runtime.SIGNALR_URL 
    || import.meta.env.VITE_SIGNALR_URL 
    || getApiBaseUrl();
}

/**
 * Get environment name
 */
export function getEnvironment(): string {
  const runtime = getRuntimeConfig();
  
  return runtime.ENVIRONMENT 
    || import.meta.env.MODE 
    || 'development';
}

/**
 * Check if notifications are enabled
 */
export function isNotificationsEnabled(): boolean {
  const runtime = getRuntimeConfig();
  
  if (runtime.ENABLE_NOTIFICATIONS !== undefined) {
    return runtime.ENABLE_NOTIFICATIONS;
  }
  
  return import.meta.env.VITE_ENABLE_NOTIFICATIONS === 'true';
}

/**
 * Check if SignalR is enabled
 */
export function isSignalREnabled(): boolean {
  const runtime = getRuntimeConfig();
  
  if (runtime.ENABLE_SIGNALR !== undefined) {
    return runtime.ENABLE_SIGNALR;
  }
  
  return import.meta.env.VITE_ENABLE_SIGNALR !== 'false';
}

/**
 * Get development subdomain
 */
export function getDevSubdomain(): string {
  const runtime = getRuntimeConfig();
  
  return runtime.DEV_SUBDOMAIN 
    || import.meta.env.VITE_DEV_SUBDOMAIN 
    || 'demo';
}

