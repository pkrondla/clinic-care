import { api } from './apiClient'
import type { 
  Subscription, 
  CreateSubscriptionRequest, 
  UpdateSubscriptionRequest,
  SubscriptionFilters
} from '../types/subscription'
import type { ApiResponse } from '../types/common'

export const subscriptionService = {
  // Get all subscriptions
  getSubscriptions: async (filters?: SubscriptionFilters): Promise<Subscription[]> => {
    const params = new URLSearchParams()
    
    if (filters) {
      Object.entries(filters).forEach(([key, value]) => {
        if (value !== undefined && value !== null) {
          params.append(key, value.toString())
        }
      })
    }
    
    const response = await api.get<Subscription[]>(`/subscriptions?${params.toString()}`)
    return response.data || []
  },

  // Get subscription by ID
  getSubscription: async (id: number): Promise<Subscription> => {
    const response = await api.get<Subscription>(`/subscriptions/${id}`)
    return response.data as Subscription
  },

  // Create new subscription
  createSubscription: async (data: CreateSubscriptionRequest): Promise<Subscription> => {
    const response = await api.post<Subscription>('/subscriptions', data)
    return response.data as Subscription
  },

  // Update subscription
  updateSubscription: async (id: number, data: UpdateSubscriptionRequest): Promise<Subscription> => {
    const response = await api.put<Subscription>(`/subscriptions/${id}`, data)
    return response.data as Subscription
  },

  // Cancel subscription
  cancelSubscription: async (id: number, reason?: string): Promise<void> => {
    await api.post(`/subscriptions/${id}/cancel`, { reason })
  },

  // Get organization's subscription
  getOrganizationSubscription: async (organizationId: number): Promise<Subscription> => {
    const response = await api.get<Subscription>(`/subscriptions/organization/${organizationId}`)
    return response.data as Subscription
  }
}