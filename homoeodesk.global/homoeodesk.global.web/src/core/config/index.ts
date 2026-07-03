import { getApiBaseUrl, getApiUrl, getSignalRUrl, getEnvironment, isNotificationsEnabled, isSignalREnabled, getDevSubdomain } from './runtime';

// Export runtime configuration values
export const API_BASE_URL = getApiBaseUrl();
export const API_URL = getApiUrl();
export const SIGNALR_URL = getSignalRUrl();
export const ENVIRONMENT = getEnvironment();
export const ENABLE_NOTIFICATIONS = isNotificationsEnabled();
export const ENABLE_SIGNALR = isSignalREnabled();
export const DEV_SUBDOMAIN = getDevSubdomain();