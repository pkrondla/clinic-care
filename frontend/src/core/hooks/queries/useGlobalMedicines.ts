import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { globalApi } from '@core/services/globalApi'
import type { Medicine, CreateMedicineDto } from '@core/types/medicine'

export function useGlobalMedicines() {
  return useQuery({
    queryKey: ['global-medicines'],
    queryFn: async () => {
      const { data } = await globalApi.medicines.getAll()
      return data
    }
  })
}

export function useGlobalMedicine(id: string) {
  return useQuery({
    queryKey: ['global-medicines', id],
    queryFn: async () => {
      const { data } = await globalApi.medicines.getById(id)
      return data
    },
    enabled: !!id
  })
}

export function useCreateGlobalMedicine() {
  const queryClient = useQueryClient()
  
  return useMutation({
    mutationFn: (data: CreateMedicineDto) => 
      globalApi.medicines.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['global-medicines'] })
    }
  })
}

export function useUpdateGlobalMedicine() {
  const queryClient = useQueryClient()
  
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: Partial<CreateMedicineDto> }) =>
      globalApi.medicines.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['global-medicines'] })
    }
  })
}

export function useDeleteGlobalMedicine() {
  const queryClient = useQueryClient()
  
  return useMutation({
    mutationFn: (id: string) => 
      globalApi.medicines.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['global-medicines'] })
    }
  })
}