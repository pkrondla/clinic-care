import apiClient from './apiClient'

export enum OperatingHoursType {
  SingleShift = 0,
  SplitShift = 1,
}

export interface Branch {
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

export interface CreateBranchRequest {
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

export interface UpdateBranchRequest {
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

export const branchService = {
  getAll: async (): Promise<Branch[]> => {
    const response = await apiClient.get('/branches')
    return response.data.data
  },

  getById: async (id: number): Promise<Branch> => {
    const response = await apiClient.get(`/branches/${id}`)
    return response.data.data
  },

  create: async (data: CreateBranchRequest): Promise<Branch> => {
    const response = await apiClient.post('/branches', data)
    return response.data.data
  },

  update: async (id: number, data: UpdateBranchRequest): Promise<Branch> => {
    const response = await apiClient.put(`/branches/${id}`, data)
    return response.data.data
  }
}
