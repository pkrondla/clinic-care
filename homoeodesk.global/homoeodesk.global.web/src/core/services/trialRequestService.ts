import apiClient from './apiClient'

export interface TrialRequest {
  id: number
  fullName: string
  email: string
  phone?: string
  clinicName: string
  city?: string
  patientsPerWeek?: string
  message?: string
  source: string
  status: string
  createdAt: string
}

export const trialRequestService = {
  getAll: async (status?: string): Promise<TrialRequest[]> => {
    const response = await apiClient.get('/global/trial-requests', {
      params: status ? { status } : undefined,
    })
    return response.data.data
  },

  updateStatus: async (id: number, status: string): Promise<TrialRequest> => {
    const response = await apiClient.patch(`/global/trial-requests/${id}/status`, { status })
    return response.data.data
  },
}
