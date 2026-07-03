import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { globalApi } from '@core/services/globalApi'
import type { Organization, CreateOrganizationDto } from '@core/types/organization'

export function useOrganizations() {
  return useQuery({
    queryKey: ['organizations'],
    queryFn: async () => {
      const { data } = await globalApi.organizations.getAll()
      return data
    }
  })
}

export function useOrganization(id: string) {
  return useQuery({
    queryKey: ['organizations', id],
    queryFn: async () => {
      const { data } = await globalApi.organizations.getById(id)
      return data
    },
    enabled: !!id
  })
}

export function useCreateOrganization() {
  const queryClient = useQueryClient()
  
  return useMutation({
    mutationFn: (data: CreateOrganizationDto) => 
      globalApi.organizations.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['organizations'] })
    }
  })
}

export function useUpdateOrganization() {
  const queryClient = useQueryClient()
  
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: Partial<CreateOrganizationDto> }) =>
      globalApi.organizations.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['organizations'] })
    }
  })
}

export function useDeleteOrganization() {
  const queryClient = useQueryClient()
  
  return useMutation({
    mutationFn: (id: string) => 
      globalApi.organizations.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['organizations'] })
    }
  })
}