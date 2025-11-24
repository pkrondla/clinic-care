// API Configuration
// Note: This file is deprecated - use config/index.ts instead
// Keeping for backward compatibility
export const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || import.meta.env.VITE_API_URL || 'http://localhost:51537/api'
export const API_TIMEOUT = 30000 // 30 seconds

// Authentication
export const AUTH_TOKEN_KEY = 'auth_token'
export const REFRESH_TOKEN_KEY = 'refresh_token'
export const TOKEN_EXPIRY_KEY = 'token_expiry'

// Feature Flags
export const ENABLE_NOTIFICATIONS = import.meta.env.VITE_ENABLE_NOTIFICATIONS === 'true'
export const ENABLE_SIGNALR = import.meta.env.VITE_ENABLE_SIGNALR === 'true'

// SignalR Hub URLs
export const QUEUE_HUB_URL = `${API_BASE_URL}/queueHub`

// Pagination Defaults
export const DEFAULT_PAGE_SIZE = 10
export const PAGE_SIZE_OPTIONS = ['10', '20', '50', '100']