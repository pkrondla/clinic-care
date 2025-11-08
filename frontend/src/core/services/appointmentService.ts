import { api } from './apiClient'
import type { ApiResponse } from '../types/common'
import type { 
  Appointment, 
  QueueItem, 
  AppointmentFilters,
  CreateAppointmentRequest,
  UpdateAppointmentStatusRequest,
  AppointmentStats
} from '../types/appointment'

export const appointmentService = {
  // Get appointments with filters
  getAppointments: async (filters: AppointmentFilters = {}): Promise<Appointment[]> => {
    const params = new URLSearchParams()
    
    Object.entries(filters).forEach(([key, value]) => {
      if (value !== undefined && value !== null) {
        params.append(key, value.toString())
      }
    })
    
    const response = await api.get<Appointment[]>(`/appointments?${params.toString()}`)
    return response.data || []
  },

  // Get appointment by ID
  getAppointment: async (id: number): Promise<Appointment> => {
    const response = await api.get<Appointment>(`/appointments/${id}`)
    return response.data as Appointment
  },

  // Create new appointment
  createAppointment: async (appointment: CreateAppointmentRequest): Promise<Appointment> => {
    const response = await api.post<Appointment>('/appointments', appointment)
    return response.data as Appointment
  },

  // Update appointment status
  updateAppointmentStatus: async (
    id: number, 
    update: UpdateAppointmentStatusRequest
  ): Promise<Appointment> => {
    const response = await api.patch<Appointment>(`/appointments/${id}/status`, update)
    return response.data as Appointment
  },

  // Cancel appointment
  cancelAppointment: async (id: number, reason?: string): Promise<Appointment> => {
    const response = await api.patch<Appointment>(`/appointments/${id}/cancel`, { reason })
    return response.data as Appointment
  },

  // Get doctor's queue for specific clinic and date
  getDoctorQueue: async (
    doctorId: number, 
    clinicId: number, 
    date?: string
  ): Promise<QueueItem[]> => {
    const params = new URLSearchParams()
    if (date) params.append('date', date)
    
    const response = await api.get<QueueItem[]>(
      `/appointments/queue/${doctorId}/${clinicId}?${params.toString()}`
    )
    return response.data || []
  },

  // Get next patient in queue
  getNextPatient: async (doctorId: number, clinicId: number): Promise<QueueItem | null> => {
    const response = await api.get<QueueItem | null>(`/appointments/queue/${doctorId}/${clinicId}/next`)
    return response.data || null
  },

  // Call next patient (update status to in-progress)
  callNextPatient: async (doctorId: number, clinicId: number): Promise<Appointment> => {
    const response = await api.post<Appointment>(`/appointments/queue/${doctorId}/${clinicId}/call-next`)
    return response.data as Appointment
  },

  // Complete appointment (mark as completed)
  completeAppointment: async (id: number): Promise<Appointment> => {
    const response = await api.patch<Appointment>(`/appointments/${id}/complete`)
    return response.data as Appointment
  },

  // Get patient's upcoming appointments
  getPatientAppointments: async (patientId: number): Promise<Appointment[]> => {
    const response = await api.get<Appointment[]>(`/appointments/patient/${patientId}`)
    return response.data || []
  },

  // Get doctor's appointments for date range
  getDoctorAppointments: async (
    doctorId: number, 
    startDate: string, 
    endDate: string
  ): Promise<Appointment[]> => {
    const response = await api.get<Appointment[]>(
      `/appointments/doctor/${doctorId}?startDate=${startDate}&endDate=${endDate}`
    )
    return response.data || []
  },

  // Get clinic appointments for today
  getTodayAppointments: async (clinicId: number): Promise<Appointment[]> => {
    const response = await api.get<Appointment[]>(`/appointments/clinic/${clinicId}/today`)
    return response.data || []
  },

  // Search appointments
  searchAppointments: async (
    query: string, 
    filters?: AppointmentFilters
  ): Promise<Appointment[]> => {
    const params = new URLSearchParams({ query })
    
    if (filters) {
      Object.entries(filters).forEach(([key, value]) => {
        if (value !== undefined && value !== null) {
          params.append(key, value.toString())
        }
      })
    }
    
    const response = await api.get<Appointment[]>(`/appointments/search?${params.toString()}`)
    return response.data || []
  },

  // Get appointment statistics
  getAppointmentStats: async (clinicId?: number, doctorId?: number): Promise<AppointmentStats> => {
    const params = new URLSearchParams()
    if (clinicId) params.append('clinicId', clinicId.toString())
    if (doctorId) params.append('doctorId', doctorId.toString())
    
    const response = await api.get<AppointmentStats>(`/appointments/stats?${params.toString()}`)
    return response.data as AppointmentStats
  }
}
