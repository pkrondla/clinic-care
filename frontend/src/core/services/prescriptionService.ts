import { api } from './apiClient'
import apiClient from './apiClient'

export interface PrescriptionMedicine {
  medicineId: number
  medicineName: string
  dispensingForm: number // 1 = Globules, 2 = Tablets, 3 = Packet
  dosage: string // Auto-set based on dispensingForm
  frequency: string // e.g., "Daily 3 times", "Weekly once"
  duration: string // e.g., "4 weeks"
  timing: string // e.g., "Before food", "Before brushing"
  containerSize?: number // Only for Globules: 1, 2, or 3 dram
  quantity?: number // Required for all forms (prescribed quantity for patient)
  dispensedQuantity?: number // Internal: quantity dispensed from inventory (auto-calculated)
  instructions?: string
}

export interface Prescription {
  id: number
  prescriptionNumber: string
  consultationId: number
  patientId: number
  patientName: string
  doctorId: number
  doctorName: string
  prescriptionDate: string
  medicines: PrescriptionMedicine[]
  notes?: string
  createdAt: string
  hasInvoice?: boolean
  invoiceId?: number
}

export interface CreatePrescriptionRequest {
  consultationId: number
  patientId: number
  doctorId: number
  medicines: PrescriptionMedicine[]
  notes?: string
}

export interface GetPrescriptionsParams {
  clinicId?: number
  doctorId?: number
  patientId?: number
  startDate?: string
  endDate?: string
}

export const prescriptionService = {
  getAll: async (params?: GetPrescriptionsParams): Promise<Prescription[]> => {
    const response = await api.get<Prescription[]>('/prescriptions', { params })
    // Backend returns { success: true, data: Prescription[] }
    // api.get returns ApiResponse<T> which extracts response.data from axios
    // So response is { success: true, data: Prescription[] } (as ApiResponse<Prescription[]>)
    // response.data should be Prescription[] (the array)
    return response.data || []
  },

  getById: async (id: number): Promise<Prescription> => {
    const response = await api.get<Prescription>(`/prescriptions/${id}`)
    // Backend returns { success: true, data: Prescription }
    // api.get returns ApiResponse<T> which extracts response.data from axios
    // So response is { success: true, data: Prescription } (as ApiResponse<Prescription>)
    // response.data should be Prescription (the object)
    if (response.data) {
      return response.data
    }
    // Fallback: check if data is nested
    throw new Error('Failed to get prescription: No data returned')
  },

  getByPatient: async (patientId: number): Promise<Prescription[]> => {
    const response = await api.get<Prescription[]>(`/prescriptions/patient/${patientId}`)
    return response.data || []
  },

  create: async (data: CreatePrescriptionRequest): Promise<Prescription> => {
    const response = await api.post<Prescription>('/prescriptions', data)
    // Backend returns { success: true, data: Prescription, message: "..." }
    // response.data should be Prescription (the object)
    if (response.data) {
      return response.data
    }
    throw new Error('Failed to create prescription: No data returned')
  },

  downloadPdf: async (id: number, includeMedicineNames: boolean = true): Promise<Blob> => {
    // For blob responses, we need to use apiClient directly (axios instance)
    const response = await apiClient.get(`/prescriptions/${id}/pdf`, {
      params: { includeMedicineNames },
      responseType: 'blob',
    })
    return response.data
  }
}

