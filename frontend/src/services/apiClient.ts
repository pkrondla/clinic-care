import axios, { AxiosInstance, AxiosRequestConfig, AxiosResponse } from 'axios'
import { useAuthStore } from '../stores/authStore'
import toast from 'react-hot-toast'

// Create axios instance
const apiClient: AxiosInstance = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || '/api',
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json'
  }
})

// Request interceptor to add auth token and tenant info
apiClient.interceptors.request.use(
  (config) => {
    const token = useAuthStore.getState().token
    const selectedClinic = useAuthStore.getState().selectedClinic
    
    console.log('API Request:', config.method?.toUpperCase(), config.url, config.data)
    
    // Add authorization header
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }
    
    // Add tenant subdomain header for development
    if (import.meta.env.DEV) {
      config.headers['X-Tenant-Subdomain'] = 'healthcareplus'
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
      // Unauthorized - clear auth and redirect to login
      useAuthStore.getState().logout()
      toast.error('Session expired. Please login again.')
      window.location.href = '/login'
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
  get: <T = any>(url: string, config?: AxiosRequestConfig): Promise<T> => 
    apiClient.get(url, config).then(response => response.data),
    
  post: <T = any>(url: string, data?: any, config?: AxiosRequestConfig): Promise<T> => 
    apiClient.post(url, data, config).then(response => response.data),
    
  put: <T = any>(url: string, data?: any, config?: AxiosRequestConfig): Promise<T> => 
    apiClient.put(url, data, config).then(response => response.data),
    
  patch: <T = any>(url: string, data?: any, config?: AxiosRequestConfig): Promise<T> => 
    apiClient.patch(url, data, config).then(response => response.data),
    
  delete: <T = any>(url: string, config?: AxiosRequestConfig): Promise<T> => 
    apiClient.delete(url, config).then(response => response.data)
}

export default apiClient
