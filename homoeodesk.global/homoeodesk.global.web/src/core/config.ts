// API Configuration
// Note: This file is deprecated - use config/index.ts instead
// Keeping for backward compatibility
import { API_BASE_URL, API_URL, SIGNALR_URL, ENABLE_NOTIFICATIONS, ENABLE_SIGNALR } from './config/index';

export { API_BASE_URL, API_URL, ENABLE_NOTIFICATIONS, ENABLE_SIGNALR };

export const API_TIMEOUT = 30000 // 30 seconds

// Authentication
export const AUTH_TOKEN_KEY = 'auth_token'
export const REFRESH_TOKEN_KEY = 'refresh_token'
export const TOKEN_EXPIRY_KEY = 'token_expiry'

// SignalR Hub URLs
export const QUEUE_HUB_URL = `${SIGNALR_URL}/queueHub`

// Pagination Defaults
export const DEFAULT_PAGE_SIZE = 10
export const PAGE_SIZE_OPTIONS = ['10', '20', '50', '100']