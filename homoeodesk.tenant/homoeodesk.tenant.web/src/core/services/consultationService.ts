import { api } from './apiClient'

export interface ConsultationPhoto {
  id: number
  consultationId: number
  photoUrl: string
  description?: string
  displayOrder: number
  createdAt: string
}

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
  hasPrescription?: boolean
  prescriptionId?: number
  photos?: ConsultationPhoto[]
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
  BranchId?: number
  doctorId?: number
  patientId?: number
  startDate?: string
  endDate?: string
}

export const consultationService = {
  getAll: async (params?: GetConsultationsParams): Promise<Consultation[]> => {
    const response = await api.get<Consultation[]>('/consultations', { params })
    // Backend returns { success: true, data: Consultation[] }
    // api.get returns ApiResponse<T> which extracts response.data from axios
    // So response is { success: true, data: Consultation[] } (as ApiResponse<Consultation[]>)
    // response.data should be Consultation[] (the array)
    console.log('Consultations API Response:', response)
    console.log('Response.data:', response.data)
    console.log('Response.data type:', typeof response.data, Array.isArray(response.data))
    
    if (Array.isArray(response.data)) {
      console.log('Returning consultations from response.data:', response.data.length)
      return response.data
    }
    // Fallback: check if data is nested
    const nestedData = (response as any).data?.data
    if (Array.isArray(nestedData)) {
      console.log('Returning consultations from nested data:', nestedData.length)
      return nestedData
    }
    console.log('No consultations found, returning empty array')
    return []
  },

  getById: async (id: number): Promise<Consultation> => {
    const response = await api.get<Consultation>(`/consultations/${id}`)
    // Backend returns { success: true, data: Consultation }
    // api.get returns ApiResponse<T> which extracts response.data from axios
    // So response is { success: true, data: Consultation } (as ApiResponse<Consultation>)
    // response.data should be Consultation (the object)
    if (response.data) {
      return response.data
    }
    // Fallback: check if data is nested
    const nestedData = (response as any).data?.data
    if (nestedData) {
      return nestedData
    }
    throw new Error('Consultation not found')
  },

  getByPatient: async (patientId: number): Promise<Consultation[]> => {
    const response = await api.get<Consultation[]>(`/consultations/patient/${patientId}`)
    // Backend returns { success: true, data: Consultation[] }
    if (Array.isArray(response.data)) {
      return response.data
    }
    const nestedData = (response as any).data?.data
    if (Array.isArray(nestedData)) {
      return nestedData
    }
    return []
  },

  create: async (data: CreateConsultationRequest): Promise<Consultation> => {
    const response = await api.post<Consultation>('/consultations', data)
    // Backend returns { success: true, data: Consultation }
    return response.data || (response as any).data?.data
  },

  update: async (id: number, data: UpdateConsultationRequest): Promise<Consultation> => {
    const response = await api.put<Consultation>(`/consultations/${id}`, data)
    // Backend returns { success: true, data: Consultation }
    return response.data || (response as any).data?.data
  },

  addPhoto: async (consultationId: number, photoUrl: string, description?: string): Promise<ConsultationPhoto> => {
    const response = await api.post<ConsultationPhoto>(`/consultations/${consultationId}/photos`, {
      consultationId,
      photoUrl,
      description
    })
    return response.data || (response as any).data?.data
  },

  deletePhoto: async (photoId: number): Promise<boolean> => {
    const response = await api.delete<boolean>(`/consultations/photos/${photoId}`)
    return response.data ?? true
  }
}

