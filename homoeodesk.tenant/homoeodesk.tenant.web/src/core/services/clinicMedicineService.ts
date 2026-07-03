import { api } from './apiClient'

export interface ClinicMedicine {
  id: number
  BranchId: number
  clinicName: string
  globalMedicineId?: number
  name: string
  genericName: string
  manufacturer: string
  type: string
  potency: string
  purchasePrice: number
  sellingPrice: number
  description: string
  isActive: boolean
  createdAt: string
  updatedAt: string
}

export interface ClinicMedicineSearch {
  id: number
  name: string
  genericName: string
  manufacturer: string
  type: string
  potency: string
}

export interface CreateClinicMedicineRequest {
  BranchId: number
  globalMedicineId?: number
  name: string
  genericName: string
  type: string
  potency: string
  manufacturer: string
  purchasePrice: number
  sellingPrice: number
  description?: string
}

export interface UpdateClinicMedicineRequest {
  name?: string
  genericName?: string
  type?: string
  potency?: string
  manufacturer?: string
  purchasePrice?: number
  sellingPrice?: number
  description?: string
  isActive?: boolean
}

export interface AddFromGlobalRequest {
  globalMedicineId: number
  BranchId: number
  purchasePrice?: number
  sellingPrice?: number
}

export const clinicMedicineService = {
  getAll: async (params?: { searchTerm?: string; BranchId?: number; isActive?: boolean }): Promise<ClinicMedicine[]> => {
    // Build params object, only including defined values
    // When isActive is undefined, don't include it so backend receives null (shows all)
    const queryParams: Record<string, string | number | boolean> = {}
    if (params?.searchTerm) queryParams.searchTerm = params.searchTerm
    if (params?.BranchId !== undefined) queryParams.BranchId = params.BranchId
    if (params?.isActive !== undefined) queryParams.isActive = params.isActive
    
    const response = await api.get<ClinicMedicine[]>(
      `/clinic-medicines`,
      { params: queryParams }
    )
    return response.data || []
  },

  getById: async (id: number): Promise<ClinicMedicine> => {
    const response = await api.get<ClinicMedicine>(
      `/clinic-medicines/${id}`
    )
    return response.data!
  },

  search: async (searchTerm?: string): Promise<ClinicMedicineSearch[]> => {
    const response = await api.get<ClinicMedicineSearch[]>(
      `/clinic-medicines/search`,
      { params: { searchTerm } }
    )
    return response.data || []
  },

  create: async (data: CreateClinicMedicineRequest): Promise<ClinicMedicine> => {
    const response = await api.post<ClinicMedicine>(
      `/clinic-medicines`,
      data
    )
    return response.data!
  },

  addFromGlobal: async (data: AddFromGlobalRequest): Promise<ClinicMedicine> => {
    const response = await api.post<ClinicMedicine>(
      `/clinic-medicines/from-global`,
      data
    )
    return response.data!
  },

  update: async (id: number, data: UpdateClinicMedicineRequest): Promise<ClinicMedicine> => {
    const response = await api.put<ClinicMedicine>(
      `/clinic-medicines/${id}`,
      data
    )
    return response.data!
  },

  delete: async (id: number): Promise<void> => {
    await api.delete(`/clinic-medicines/${id}`)
  }
}

