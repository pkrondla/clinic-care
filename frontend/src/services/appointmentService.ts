import { api } from './apiClient'
import type { 
  Appointment, 
  QueueItem, 
  AppointmentFilters,
  CreateAppointmentRequest,
  UpdateAppointmentStatusRequest
} from '../types/index'

export const appointmentService = {
  // Get appointments with filters
  getAppointments: async (filters: AppointmentFilters = {}): Promise<Appointment[]> => {
    const params = new URLSearchParams()
    
    Object.entries(filters).forEach(([key, value]) => {
      if (value !== undefined && value !== null) {
        params.append(key, value.toString())
      }
    })
    
    return api.get<Appointment[]>(`/appointments?${params.toString()}`)
  },

  // Get appointment by ID
  getAppointment: async (id: number): Promise<Appointment> => {
    return api.get<Appointment>(`/appointments/${id}`)
  },

  // Create new appointment
  createAppointment: async (appointment: CreateAppointmentRequest): Promise<Appointment> => {
    return api.post<Appointment>('/appointments', appointment)
  },

  // Update appointment status
  updateAppointmentStatus: async (
    id: number, 
    update: UpdateAppointmentStatusRequest
  ): Promise<Appointment> => {
    return api.patch<Appointment>(`/appointments/${id}/status`, update)
  },

  // Cancel appointment
  cancelAppointment: async (id: number, reason?: string): Promise<Appointment> => {
    return api.patch<Appointment>(`/appointments/${id}/cancel`, { reason })
  },

  // Get doctor's queue for specific clinic and date
  getDoctorQueue: async (
    doctorId: number, 
    clinicId: number, 
    date?: string
  ): Promise<QueueItem[]> => {
    const params = new URLSearchParams()
    if (date) params.append('date', date)
    
    return api.get<QueueItem[]>(
      `/appointments/queue/${doctorId}/${clinicId}?${params.toString()}`
    )
  },

  // Get next patient in queue
  getNextPatient: async (doctorId: number, clinicId: number): Promise<QueueItem | null> => {
    return api.get<QueueItem | null>(`/appointments/queue/${doctorId}/${clinicId}/next`)
  },

  // Call next patient (update status to in-progress)
  callNextPatient: async (doctorId: number, clinicId: number): Promise<Appointment> => {
    return api.post<Appointment>(`/appointments/queue/${doctorId}/${clinicId}/call-next`)
  },

  // Complete appointment (mark as completed)
  completeAppointment: async (id: number): Promise<Appointment> => {
    return api.patch<Appointment>(`/appointments/${id}/complete`)
  },

  // Get patient's upcoming appointments
  getPatientAppointments: async (patientId: number): Promise<Appointment[]> => {
    return api.get<Appointment[]>(`/appointments/patient/${patientId}`)
  },

  // Get doctor's appointments for date range
  getDoctorAppointments: async (
    doctorId: number, 
    startDate: string, 
    endDate: string
  ): Promise<Appointment[]> => {
    return api.get<Appointment[]>(
      `/appointments/doctor/${doctorId}?startDate=${startDate}&endDate=${endDate}`
    )
  },

  // Get clinic appointments for today
  getTodayAppointments: async (clinicId: number): Promise<Appointment[]> => {
    return api.get<Appointment[]>(`/appointments/clinic/${clinicId}/today`)
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
    
    return api.get<Appointment[]>(`/appointments/search?${params.toString()}`)
  },

  // Get appointment statistics
  getAppointmentStats: async (clinicId?: number, doctorId?: number) => {
    const params = new URLSearchParams()
    if (clinicId) params.append('clinicId', clinicId.toString())
    if (doctorId) params.append('doctorId', doctorId.toString())
    
    return api.get(`/appointments/stats?${params.toString()}`)
  }
}
