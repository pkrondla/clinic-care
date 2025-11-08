import axios from 'axios';
import { API_BASE_URL } from '@core/config';
import type { Organization, CreateOrganizationDto, SubscriptionPlan } from '../types/organization';
import type { Medicine } from '../types/medicine';
import type { SystemStats } from '../types/statistics';

const globalApiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json'
  }
});

globalApiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem('global_token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
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
    getAll: () => globalApiClient.get<Organization[]>('/organizations'),
    getById: (id: string) => globalApiClient.get<Organization>(`/organizations/${id}`),
    create: (data: CreateOrganizationDto) => globalApiClient.post<Organization>('/organizations', data),
    update: (id: string, data: Partial<CreateOrganizationDto>) => globalApiClient.put<Organization>(`/organizations/${id}`, data),
    delete: (id: string) => globalApiClient.delete(`/organizations/${id}`)
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
    getAll: () => globalApiClient.get<Medicine[]>('/medicines'),
    getById: (id: string) => globalApiClient.get<Medicine>(`/medicines/${id}`),
    create: (data: Partial<Medicine>) => globalApiClient.post<Medicine>('/medicines', data),
    update: (id: string, data: Partial<Medicine>) => globalApiClient.put<Medicine>(`/medicines/${id}`, data),
    delete: (id: string) => globalApiClient.delete(`/medicines/${id}`)
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

  // System Stats
  stats: {
    getSystemStats: () => globalApiClient.get<SystemStats>('/stats/system')
  }
};