import apiClient from './apiClient'

export enum OperatingHoursType {
  SingleShift = 0,
  SplitShift = 1,
}

export interface Clinic {
  id: number
  organizationId: number
  name: string
  code: string
  address?: string
  phone?: string
  email?: string
  operatingHoursType: OperatingHoursType
  morningStartTime?: string
  morningEndTime?: string
  eveningStartTime?: string
  eveningEndTime?: string
  fullDayStartTime?: string
  fullDayEndTime?: string
  isActive: boolean
  createdAt: string
}

export interface CreateClinicRequest {
  name: string
  code: string
  address?: string
  phone?: string
  email?: string
  operatingHoursType: OperatingHoursType
  morningStartTime?: string
  morningEndTime?: string
  eveningStartTime?: string
  eveningEndTime?: string
  fullDayStartTime?: string
  fullDayEndTime?: string
}

export interface UpdateClinicRequest {
  name: string
  address?: string
  phone?: string
  email?: string
  operatingHoursType?: OperatingHoursType
  morningStartTime?: string
  morningEndTime?: string
  eveningStartTime?: string
  eveningEndTime?: string
  fullDayStartTime?: string
  fullDayEndTime?: string
  isActive?: boolean
}

export const clinicService = {
  getAll: async (): Promise<Clinic[]> => {
    const response = await apiClient.get('/clinics')
    return response.data.data
  },

  getById: async (id: number): Promise<Clinic> => {
    const response = await apiClient.get(`/clinics/${id}`)
    return response.data.data
  },

  create: async (data: CreateClinicRequest): Promise<Clinic> => {
    const response = await apiClient.post('/clinics', data)
    return response.data.data
  },

  update: async (id: number, data: UpdateClinicRequest): Promise<Clinic> => {
    const response = await apiClient.put(`/clinics/${id}`, data)
    return response.data.data
  }
}

