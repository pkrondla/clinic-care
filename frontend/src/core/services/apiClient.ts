import axios, { AxiosInstance, AxiosRequestConfig, AxiosResponse } from 'axios'
import { useGlobalAuthStore, useTenantAuthStore } from '../stores/authStore'
import toast from 'react-hot-toast'
import type { ApiResponse } from '../types/common'

// Helper function to get auth state without using hooks (for use in interceptors)
// This should match the logic in useAuth() hook
const getAuthState = () => {
  const globalAuth = useGlobalAuthStore.getState()
  const tenantAuth = useTenantAuthStore.getState()
  
  // Check for tenant query parameter or path (for local development)
  const urlParams = new URLSearchParams(window.location.search)
  const tenantParam = urlParams.get('tenant')
  const isTenantPath = window.location.pathname.startsWith('/tenant')
  
  // Priority 1: If tenant user is authenticated, use tenant store
  if (tenantAuth.isAuthenticated && !tenantAuth.isGlobalSystem) {
    return tenantAuth
  }
  
  // Priority 2: If global user is authenticated, use global store
  if (globalAuth.isAuthenticated && globalAuth.isGlobalSystem) {
    return globalAuth
  }
  
  // Priority 3: Determine based on domain/params (for unauthenticated users)
  const hostname = window.location.hostname
  const parts = hostname.split('.')
  const isGlobalDomain = (hostname === 'localhost' || 
                         hostname === '127.0.0.1' ||
                         parts.length === 2 || // e.g., domain.com
                         (parts.length === 3 && parts[0] === 'www')) && // e.g., www.domain.com
                         !tenantParam && // Not explicitly requesting tenant
                         !isTenantPath // Not using tenant path
  
  return isGlobalDomain ? globalAuth : tenantAuth
}

import { API_URL } from '@core/config';

// Create axios instance
const apiClient: AxiosInstance = axios.create({
  baseURL: API_URL,
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json'
  }
})

// Request interceptor to add auth token and tenant info
apiClient.interceptors.request.use(
  (config) => {
    // Access store state directly (not using hooks)
    const authState = getAuthState()
    const token = authState.token
    const selectedClinic = authState.selectedClinic
    
    console.log('API Request:', config.method?.toUpperCase(), config.url, {
      hasToken: !!token,
      tokenLength: token?.length,
      isAuthenticated: authState.isAuthenticated,
      user: authState.user?.email
    })
    
    // Add authorization header
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
      console.log('API Request: Added Authorization header')
    } else {
      console.warn('API Request: No token available for request', config.url)
    }
    
    // Add tenant subdomain header for development
    if (import.meta.env.DEV) {
      config.headers['X-Tenant-Subdomain'] = 'demo'
    }
    
    // Add clinic context if available
    if (selectedClinic) {
      config.headers['X-Clinic-Id'] = selectedClinic.id.toString()
    }
    
    return config
  },
  (error) => {
    return Promise.reject(error)
  }
)

// Response interceptor for error handling
apiClient.interceptors.response.use(
  (response: AxiosResponse) => {
    console.log('API Response:', response.status, response.config.url, response.data)
    return response
  },
  (error) => {
    const { response } = error
    
    if (response?.status === 401) {
      // Unauthorized - only redirect if we're not already on login page
      if (!window.location.pathname.includes('/login')) {
        const authState = getAuthState()
        authState.logout()
        toast.error('Session expired. Please login again.')
        window.location.href = '/login'
      }
    } else if (response?.status === 403) {
      // Forbidden
      toast.error('Access denied. You do not have permission to perform this action.')
    } else if (response?.status === 404) {
      // Not found
      toast.error('Resource not found.')
    } else if (response?.status >= 500) {
      // Server error
      toast.error('Server error. Please try again later.')
    } else if (response?.data?.message) {
      // API error with message
      toast.error(response.data.message)
    } else if (error.message) {
      // Network or other error
      toast.error(error.message)
    }
    
    return Promise.reject(error)
  }
)

// Generic API methods
export const api = {
  get: <T>(url: string, config?: AxiosRequestConfig): Promise<ApiResponse<T>> => 
    apiClient.get(url, config).then(response => response.data),
    
  post: <T>(url: string, data?: unknown, config?: AxiosRequestConfig): Promise<ApiResponse<T>> => 
    apiClient.post(url, data, config).then(response => response.data),
    
  put: <T>(url: string, data?: unknown, config?: AxiosRequestConfig): Promise<ApiResponse<T>> => 
    apiClient.put(url, data, config).then(response => response.data),
    
  patch: <T>(url: string, data?: unknown, config?: AxiosRequestConfig): Promise<ApiResponse<T>> => 
    apiClient.patch(url, data, config).then(response => response.data),
    
  delete: <T>(url: string, config?: AxiosRequestConfig): Promise<ApiResponse<T>> => 
    apiClient.delete(url, config).then(response => response.data)
}

export default apiClient
