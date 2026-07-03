import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { tenantApi } from '@core/services/tenantApi'
import type { Medicine, CreateMedicineDto } from '@core/types/medicine'

export function useMedicines() {
  return useQuery({
    queryKey: ['medicines'],
    queryFn: async () => {
      const { data } = await tenantApi.medicines.getAll()
      return data.data
    }
  })
}

export function useMedicine(id: number) {
  return useQuery({
    queryKey: ['medicines', id],
    queryFn: async () => {
      const { data } = await tenantApi.medicines.getById(id)
      return data.data
    },
    enabled: !!id
  })
}

export function useCreateMedicine() {
  const queryClient = useQueryClient()
  
  return useMutation({
    mutationFn: (data: CreateMedicineDto) => 
      tenantApi.medicines.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['medicines'] })
    }
  })
}

export function useUpdateMedicine() {
  const queryClient = useQueryClient()
  
  return useMutation({
    mutationFn: ({ id, data }: { id: number; data: Partial<CreateMedicineDto> }) =>
      tenantApi.medicines.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['medicines'] })
    }
  })
}

export function useDeleteMedicine() {
  const queryClient = useQueryClient()
  
  return useMutation({
    mutationFn: (id: number) => 
      tenantApi.medicines.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['medicines'] })
    }
  })
}