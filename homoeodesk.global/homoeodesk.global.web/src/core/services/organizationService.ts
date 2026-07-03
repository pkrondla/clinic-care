import apiClient from './apiClient'

export interface Organization {
  id: number
  name: string
  subdomain: string
  databaseName: string
  contactEmail: string
  contactPhone?: string
  address?: string
  subscriptionStatus: string
  trialEndDate?: string
  isActive: boolean
  createdAt: string
}

export interface CreateOrganizationRequest {
  name: string
  subdomain?: string
  contactEmail: string
  contactPhone?: string
  address?: string
  createDatabase?: boolean
}

export interface UpdateOrganizationRequest {
  name: string
  contactEmail: string
  contactPhone?: string
  address?: string
  isActive?: boolean
}

export const organizationService = {
  getAll: async (): Promise<Organization[]> => {
    const response = await apiClient.get('/global/organizations')
    return response.data.data
  },

  getById: async (id: number): Promise<Organization> => {
    const response = await apiClient.get(`/global/organizations/${id}`)
    return response.data.data
  },

  getBySubdomain: async (subdomain: string): Promise<Organization> => {
    const response = await apiClient.get(`/global/organizations/subdomain/${subdomain}`)
    return response.data.data
  },

  create: async (data: CreateOrganizationRequest): Promise<Organization> => {
    const response = await apiClient.post('/global/organizations', data)
    return response.data.data
  },

  update: async (id: number, data: UpdateOrganizationRequest): Promise<Organization> => {
    const response = await apiClient.put(`/global/organizations/${id}`, data)
    return response.data.data
  }
}

