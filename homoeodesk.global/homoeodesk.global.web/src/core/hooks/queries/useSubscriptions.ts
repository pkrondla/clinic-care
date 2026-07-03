import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { subscriptionService } from '@core/services/subscriptionService'
import type { Subscription, CreateSubscriptionRequest, UpdateSubscriptionRequest } from '@core/types/subscription'

export function useSubscriptions() {
  return useQuery({
    queryKey: ['subscriptions'],
    queryFn: () => subscriptionService.getSubscriptions()
  })
}

export function useSubscription(id: number) {
  return useQuery({
    queryKey: ['subscriptions', id],
    queryFn: () => subscriptionService.getSubscription(id),
    enabled: !!id
  })
}

export function useCreateSubscription() {
  const queryClient = useQueryClient()
  
  return useMutation({
    mutationFn: (data: CreateSubscriptionRequest) => 
      subscriptionService.createSubscription(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['subscriptions'] })
    }
  })
}

export function useUpdateSubscription() {
  const queryClient = useQueryClient()
  
  return useMutation({
    mutationFn: ({ id, data }: { id: number; data: UpdateSubscriptionRequest }) =>
      subscriptionService.updateSubscription(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['subscriptions'] })
    }
  })
}

export function useCancelSubscription() {
  const queryClient = useQueryClient()
  
  return useMutation({
    mutationFn: (id: number) => 
      subscriptionService.cancelSubscription(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['subscriptions'] })
    }
  })
}