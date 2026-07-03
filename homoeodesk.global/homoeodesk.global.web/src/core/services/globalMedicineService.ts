import apiClient from './apiClient'

export interface GlobalMedicine {
  id: number
  name: string
  genericName: string
  type: string
  potency: string
  manufacturer: string
  price: number
  description?: string
  isActive: boolean
  createdAt: string
}

export interface CreateGlobalMedicineRequest {
  name: string
  genericName: string
  type: string
  potency: string
  manufacturer: string
  price: number
  description?: string
}

export interface UpdateGlobalMedicineRequest {
  name?: string
  genericName?: string
  type?: string
  potency?: string
  manufacturer?: string
  price?: number
  description?: string
  isActive?: boolean
}

export const globalMedicineService = {
  getAll: async (params?: { 
    searchTerm?: string
    type?: string
    manufacturer?: string
  }): Promise<GlobalMedicine[]> => {
    const response = await apiClient.get('/global/medicines', { params })
    return response.data.data
  },

  getById: async (id: number): Promise<GlobalMedicine> => {
    const response = await apiClient.get(`/global/medicines/${id}`)
    return response.data.data
  },

  create: async (data: CreateGlobalMedicineRequest): Promise<GlobalMedicine> => {
    const response = await apiClient.post('/global/medicines', data)
    return response.data.data
  },

  update: async (id: number, data: UpdateGlobalMedicineRequest): Promise<GlobalMedicine> => {
    const response = await apiClient.put(`/global/medicines/${id}`, data)
    return response.data.data
  },

  delete: async (id: number): Promise<void> => {
    await apiClient.delete(`/global/medicines/${id}`)
  }
}

