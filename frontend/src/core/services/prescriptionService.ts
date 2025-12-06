import apiClient from './apiClient'

export interface PrescriptionMedicine {
  medicineId: number
  medicineName: string
  dosage: string
  frequency: string
  duration: number
  quantity: number
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
    const response = await apiClient.get('/prescriptions', { params })
    return response.data.data || []
  },

  getById: async (id: number): Promise<Prescription> => {
    const response = await apiClient.get(`/prescriptions/${id}`)
    return response.data.data
  },

  getByPatient: async (patientId: number): Promise<Prescription[]> => {
    const response = await apiClient.get(`/prescriptions/patient/${patientId}`)
    return response.data.data
  },

  create: async (data: CreatePrescriptionRequest): Promise<Prescription> => {
    const response = await apiClient.post('/prescriptions', data)
    return response.data.data
  },

  downloadPdf: async (id: number, includeMedicineNames: boolean = true): Promise<Blob> => {
    const response = await apiClient.get(`/prescriptions/${id}/pdf`, {
      params: { includeMedicineNames },
      responseType: 'blob',
    })
    return response.data
  }
}

