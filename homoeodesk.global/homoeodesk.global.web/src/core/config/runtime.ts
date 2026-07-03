// Runtime configuration loader
// Loads configuration from window.__HOMOEODESK_CONFIG__ (set in public/config.js)

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
    __HOMOEODESK_CONFIG__?: RuntimeConfig;
  }
}

export function getRuntimeConfig(): RuntimeConfig {
  return window.__HOMOEODESK_CONFIG__ || {};
}

export function getApiBaseUrl(): string {
  const runtime = getRuntimeConfig();

  return runtime.API_BASE_URL
    || import.meta.env.VITE_API_BASE_URL
    || import.meta.env.VITE_API_URL
    || 'http://localhost:7100';
}

export function getApiUrl(): string {
  const runtime = getRuntimeConfig();

  return runtime.API_URL
    || import.meta.env.VITE_API_BASE_URL
    || import.meta.env.VITE_API_URL
    || 'http://localhost:7100/api';
}

export function getSignalRUrl(): string {
  const runtime = getRuntimeConfig();

  return runtime.SIGNALR_URL
    || import.meta.env.VITE_SIGNALR_URL
    || getApiBaseUrl();
}

export function getEnvironment(): string {
  const runtime = getRuntimeConfig();

  return runtime.ENVIRONMENT
    || import.meta.env.MODE
    || 'development';
}

export function isNotificationsEnabled(): boolean {
  const runtime = getRuntimeConfig();

  if (runtime.ENABLE_NOTIFICATIONS !== undefined) {
    return runtime.ENABLE_NOTIFICATIONS;
  }

  return import.meta.env.VITE_ENABLE_NOTIFICATIONS === 'true';
}

export function isSignalREnabled(): boolean {
  const runtime = getRuntimeConfig();

  if (runtime.ENABLE_SIGNALR !== undefined) {
    return runtime.ENABLE_SIGNALR;
  }

  return import.meta.env.VITE_ENABLE_SIGNALR !== 'false';
}

export function getDevSubdomain(): string {
  const runtime = getRuntimeConfig();

  return runtime.DEV_SUBDOMAIN
    || import.meta.env.VITE_DEV_SUBDOMAIN
    || 'demo';
}
