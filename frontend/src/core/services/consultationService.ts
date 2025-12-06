import apiClient from './apiClient'

export interface Consultation {
  id: number
  appointmentId: number
  patientId: number
  patientName: string
  doctorId: number
  doctorName: string
  consultationDate: string
  chiefComplaint: string
  symptoms?: string
  examination?: string
  diagnosis?: string
  treatmentPlan?: string
  notes?: string
  consultationFee: number
  createdAt: string
}

export interface CreateConsultationRequest {
  appointmentId: number
  patientId: number
  doctorId: number
  chiefComplaint: string
  symptoms?: string
  examination?: string
  diagnosis?: string
  treatmentPlan?: string
  notes?: string
  consultationFee: number
}

export interface UpdateConsultationRequest {
  chiefComplaint?: string
  symptoms?: string
  examination?: string
  diagnosis?: string
  treatmentPlan?: string
  notes?: string
  consultationFee?: number
}

export interface GetConsultationsParams {
  clinicId?: number
  doctorId?: number
  patientId?: number
  startDate?: string
  endDate?: string
}

export const consultationService = {
  getAll: async (params?: GetConsultationsParams): Promise<Consultation[]> => {
    const response = await apiClient.get('/consultations', { params })
    return response.data.data || []
  },

  getById: async (id: number): Promise<Consultation> => {
    const response = await apiClient.get(`/consultations/${id}`)
    return response.data.data
  },

  getByPatient: async (patientId: number): Promise<Consultation[]> => {
    const response = await apiClient.get(`/consultations/patient/${patientId}`)
    return response.data.data
  },

  create: async (data: CreateConsultationRequest): Promise<Consultation> => {
    const response = await apiClient.post('/consultations', data)
    return response.data.data
  },

  update: async (id: number, data: UpdateConsultationRequest): Promise<Consultation> => {
    const response = await apiClient.put(`/consultations/${id}`, data)
    return response.data.data
  }
}

