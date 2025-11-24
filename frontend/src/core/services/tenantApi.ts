import axios from 'axios';
import { API_BASE_URL } from '@core/config/index';
import type { Patient, PatientFilters } from '../types/patient';
import type { Appointment, AppointmentFilters } from '../types/appointment';
import type { Clinic, ClinicFilters } from '../types/clinic';
import type { Prescription } from '../types/prescription';
import type { Medicine } from '../types/medicine';
import type { ApiResponse, PaginatedResponse } from '../types/common';
import type { User } from '../types/auth';

const createTenantApiClient = () => {
  const tenantAxios = axios.create({
    baseURL: API_BASE_URL,
    headers: {
      'Content-Type': 'application/json'
    }
  });

  tenantAxios.interceptors.request.use((config) => {
    const token = localStorage.getItem('tenant_token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    
    // Get tenant from subdomain
    const hostname = window.location.hostname;
    const subdomain = hostname.split('.')[0];
    if (subdomain !== 'www' && subdomain !== window.location.host) {
      config.headers['X-Tenant-Subdomain'] = subdomain;
    }
    
    return config;
  });

  return {
    // Clinic Management
    clinics: {
      getAll: (filters?: ClinicFilters) => 
        tenantAxios.get<PaginatedResponse<Clinic>>('/clinics', { params: filters }),
      getById: (id: number) => 
        tenantAxios.get<ApiResponse<Clinic>>(`/clinics/${id}`),
      create: (data: Partial<Clinic>) => 
        tenantAxios.post<ApiResponse<Clinic>>('/clinics', data),
      update: (id: number, data: Partial<Clinic>) => 
        tenantAxios.put<ApiResponse<Clinic>>(`/clinics/${id}`, data),
      delete: (id: number) => 
        tenantAxios.delete<ApiResponse<void>>(`/clinics/${id}`)
    },

    // Appointment Management
    appointments: {
      getAll: (filters?: AppointmentFilters) => 
        tenantAxios.get<PaginatedResponse<Appointment>>('/appointments', { params: filters }),
      getByDoctor: (doctorId: number, filters?: AppointmentFilters) => 
        tenantAxios.get<PaginatedResponse<Appointment>>(`/appointments/doctor/${doctorId}`, { params: filters }),
      getByPatient: (patientId: number, filters?: AppointmentFilters) => 
        tenantAxios.get<PaginatedResponse<Appointment>>(`/appointments/patient/${patientId}`, { params: filters }),
      getById: (id: number) => 
        tenantAxios.get<ApiResponse<Appointment>>(`/appointments/${id}`),
      create: (data: Partial<Appointment>) => 
        tenantAxios.post<ApiResponse<Appointment>>('/appointments', data),
      update: (id: number, data: Partial<Appointment>) => 
        tenantAxios.put<ApiResponse<Appointment>>(`/appointments/${id}`, data),
      delete: (id: number) => 
        tenantAxios.delete<ApiResponse<void>>(`/appointments/${id}`),
      getQueue: (doctorId: number, date: string) => 
        tenantAxios.get<ApiResponse<Appointment[]>>(`/appointments/queue/${doctorId}`, { params: { date } })
    },

    // Patient Management
    patients: {
      getAll: (filters?: PatientFilters) => 
        tenantAxios.get<PaginatedResponse<Patient>>('/patients', { params: filters }),
      getById: (id: number) => 
        tenantAxios.get<ApiResponse<Patient>>(`/patients/${id}`),
      create: (data: Partial<Patient>) => 
        tenantAxios.post<ApiResponse<Patient>>('/patients', data),
      update: (id: number, data: Partial<Patient>) => 
        tenantAxios.put<ApiResponse<Patient>>(`/patients/${id}`, data),
      delete: (id: number) => 
        tenantAxios.delete<ApiResponse<void>>(`/patients/${id}`)
    },

    // Prescription Management
    prescriptions: {
      getAll: (filters?: { patientId?: number }) => 
        tenantAxios.get<PaginatedResponse<Prescription>>('/prescriptions', { params: filters }),
      getById: (id: number) => 
        tenantAxios.get<ApiResponse<Prescription>>(`/prescriptions/${id}`),
      create: (data: Partial<Prescription>) => 
        tenantAxios.post<ApiResponse<Prescription>>('/prescriptions', data),
      update: (id: number, data: Partial<Prescription>) => 
        tenantAxios.put<ApiResponse<Prescription>>(`/prescriptions/${id}`, data),
      delete: (id: number) => 
        tenantAxios.delete<ApiResponse<void>>(`/prescriptions/${id}`)
    },

    // Tenant Auth
    auth: {
      login: (credentials: { email: string; password: string }) => 
        tenantAxios.post<ApiResponse<{ token: string; user: User }>>('/auth/tenant/login', credentials),
      logout: () => {
        localStorage.removeItem('tenant_token');
        localStorage.removeItem('tenant_user');
      },
      getProfile: () => 
        tenantAxios.get<ApiResponse<User>>('/auth/tenant/profile'),
      updateProfile: (data: Partial<User>) => 
        tenantAxios.put<ApiResponse<User>>('/auth/tenant/profile', data)
    },

    // Medicine Management 
    medicines: {
      getAll: () => tenantAxios.get<PaginatedResponse<Medicine>>('/medicines'),
      getById: (id: number) => tenantAxios.get<ApiResponse<Medicine>>(`/medicines/${id}`),
      create: (data: Partial<Medicine>) => tenantAxios.post<ApiResponse<Medicine>>('/medicines', data),
      update: (id: number, data: Partial<Medicine>) => tenantAxios.put<ApiResponse<Medicine>>(`/medicines/${id}`, data),
      delete: (id: number) => tenantAxios.delete<ApiResponse<void>>(`/medicines/${id}`)
    }
  };
};

// Export singleton instance
export const tenantApi = createTenantApiClient();