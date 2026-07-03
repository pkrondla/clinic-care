import { api } from './apiClient'
import type { ApiResponse } from '../types/common'
import type { 
  Appointment, 
  QueueItem, 
  AppointmentFilters,
  CreateAppointmentRequest,
  UpdateAppointmentStatusRequest
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
    // Backend returns { data: AppointmentDto }
    // api.get returns response.data from axios, which is { data: AppointmentDto }
    // But api.get is typed as ApiResponse<T>, creating a type mismatch
    // At runtime, response is { data: AppointmentDto }, so we access response.data
    const response = await api.get<{ data: Appointment }>(`/appointments/${id}`)
    // Type assertion needed because api.get typing doesn't match runtime structure
    const responseData = (response as any)?.data
    if (!responseData) {
      throw new Error('Appointment not found')
    }
    return responseData
  },

  // Update appointment
  updateAppointment: async (id: number, notes: string): Promise<Appointment> => {
    const response = await api.put<Appointment>(`/appointments/${id}`, {
      id,
      notes
    })
    // Backend returns { message: 'Appointment updated successfully', data: Appointment }
    // api.put returns ApiResponse<T> which has { data?: T, message?: string, ... }
    // So response.data is the Appointment object
    const appointment = response.data || (response as any)?.data?.data
    if (!appointment) {
      throw new Error('Failed to update appointment: No data returned')
    }
    return appointment
  },

  // Create new appointment
  createAppointment: async (appointment: CreateAppointmentRequest): Promise<any> => {
    const response = await api.post<any>('/appointments', appointment)
    return response.data
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
    // Backend uses DELETE /appointments/{id}
    // Note: reason parameter is not currently used by backend, but kept for future use
    await api.delete(`/appointments/${id}`)
    // After cancellation, fetch the updated appointment to return it
    const response = await api.get<Appointment>(`/appointments/${id}`)
    return response.data as Appointment
  },

  // Get doctor's queue for specific clinic and date
  getDoctorQueue: async (
    doctorId: number, 
    BranchId: number, 
    date?: string
  ): Promise<QueueItem[]> => {
    const params = new URLSearchParams()
    if (date) params.append('date', date)
    
    const response = await api.get<QueueItem[]>(
      `/appointments/queue/${doctorId}/${BranchId}?${params.toString()}`
    )
    return response.data || []
  },

  // Get next patient in queue
  getNextPatient: async (doctorId: number, BranchId: number): Promise<QueueItem | null> => {
    const response = await api.get<QueueItem | null>(`/appointments/queue/${doctorId}/${BranchId}/next`)
    return response.data || null
  },

  // Call next patient (update status to in-progress)
  callNextPatient: async (doctorId: number, BranchId: number): Promise<Appointment> => {
    const response = await api.post<Appointment>(`/appointments/queue/${doctorId}/${BranchId}/call-next`)
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
  getTodayAppointments: async (BranchId: number): Promise<Appointment[]> => {
    const response = await api.get<Appointment[]>(`/appointments/clinic/${BranchId}/today`)
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
  getAppointmentStats: async (BranchId?: number, doctorId?: number): Promise<any> => {
    const params = new URLSearchParams()
    if (BranchId) params.append('BranchId', BranchId.toString())
    if (doctorId) params.append('doctorId', doctorId.toString())
    
    const response = await api.get<any>(`/appointments/stats?${params.toString()}`)
    return response.data as any
  }
}
