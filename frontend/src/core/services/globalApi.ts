import axios from 'axios';
import { API_BASE_URL } from '@core/config/index';
import type { Organization, CreateOrganizationDto, SubscriptionPlan } from '../types/organization';
import type { Medicine } from '../types/medicine';
import type { SystemStats } from '../types/statistics';

// Vite proxy is configured to proxy /api to http://localhost:51537
// So we should use relative URLs and let Vite handle the proxying
// This avoids CORS issues and double /api/api/ URLs
const globalApiClient = axios.create({
  baseURL: '', // Use relative URLs - Vite proxy will handle /api
  headers: {
    'Content-Type': 'application/json'
  }
});

globalApiClient.interceptors.request.use((config) => {
  // Get token from Zustand persisted store
  try {
    const stored = localStorage.getItem('global-auth-storage');
    if (stored) {
      const authData = JSON.parse(stored);
      const token = authData?.state?.token;
      if (token) {
        config.headers.Authorization = `Bearer ${token}`;
        console.log('globalApi: Added Authorization header', { hasToken: !!token, url: config.url });
      } else {
        console.warn('globalApi: No token found in global-auth-storage');
      }
    } else {
      console.warn('globalApi: No global-auth-storage found in localStorage');
    }
  } catch (e) {
    console.error('globalApi: Error reading token from localStorage', e);
    // Fallback to old storage key for backward compatibility
    const token = localStorage.getItem('global_token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
      console.log('globalApi: Using fallback token from global_token');
    }
  }
  
  // Global endpoints don't need tenant subdomain header
  // Only add it if explicitly needed (not for /api/global/* endpoints)
  if (import.meta.env.DEV && !config.url?.startsWith('/api/global')) {
    config.headers['X-Tenant-Subdomain'] = 'demo';
  }
  
  return config;
});

export interface GlobalAuthResponse {
  token: string;
  user: {
    id: number;
    email: string;
    name: string;
    role: string;
  };
}

export const globalApi = {
  // Organization Management
  organizations: {
    getAll: () => globalApiClient.get<{ data: Organization[] }>('/api/global/organizations'),
    getById: (id: string) => globalApiClient.get<Organization>(`/api/global/organizations/${id}`),
    create: (data: CreateOrganizationDto) => globalApiClient.post<Organization>('/api/global/organizations', data),
    update: (id: string, data: Partial<CreateOrganizationDto>) => globalApiClient.put<Organization>(`/api/global/organizations/${id}`, data),
    delete: (id: string) => globalApiClient.delete(`/api/global/organizations/${id}`)
  },

  // Subscription Management
  subscriptions: {
    getAll: () => globalApiClient.get<SubscriptionPlan[]>('/subscriptions'),
    getById: (id: string) => globalApiClient.get<SubscriptionPlan>(`/subscriptions/${id}`),
    create: (data: Partial<SubscriptionPlan>) => globalApiClient.post<SubscriptionPlan>('/subscriptions', data),
    update: (id: string, data: Partial<SubscriptionPlan>) => globalApiClient.put<SubscriptionPlan>(`/subscriptions/${id}`, data)
  },

  // Global Medicine Database
  medicines: {
    getAll: () => globalApiClient.get<{ data: Medicine[] }>('/api/global/medicines'),
    getById: (id: string) => globalApiClient.get<Medicine>(`/api/global/medicines/${id}`),
    create: (data: Partial<Medicine>) => globalApiClient.post<Medicine>('/api/global/medicines', data),
    update: (id: string, data: Partial<Medicine>) => globalApiClient.put<Medicine>(`/api/global/medicines/${id}`, data),
    delete: (id: string) => globalApiClient.delete(`/api/global/medicines/${id}`)
  },

  // Global Auth
  auth: {
    login: (credentials: { email: string; password: string }) => 
      globalApiClient.post<GlobalAuthResponse>('/auth/global/login', credentials),
    logout: () => {
      localStorage.removeItem('global_token');
      localStorage.removeItem('global_user');
    }
  },

  // System Stats (placeholder - endpoint needs to be created)
  stats: {
    getSystemStats: () => Promise.resolve({
      data: {
        totalOrganizations: 0,
        activeUsers: 0,
        totalMedicines: 0,
        monthlyRevenue: 0,
        uptime: 0
      }
    })
  }
};