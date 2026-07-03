import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { clinicMedicineService, type ClinicMedicine, type CreateClinicMedicineRequest, type UpdateClinicMedicineRequest, type AddFromGlobalRequest } from '@core/services/clinicMedicineService'
import toast from 'react-hot-toast'

export const clinicMedicineKeys = {
  all: ['clinic-medicines'] as const,
  lists: () => [...clinicMedicineKeys.all, 'list'] as const,
  list: (params?: { searchTerm?: string; BranchId?: number; isActive?: boolean }) => [...clinicMedicineKeys.lists(), params] as const,
  details: () => [...clinicMedicineKeys.all, 'detail'] as const,
  detail: (id: number) => [...clinicMedicineKeys.details(), id] as const,
  search: (searchTerm?: string) => [...clinicMedicineKeys.all, 'search', searchTerm] as const
}

export const useClinicMedicines = (params?: { searchTerm?: string; BranchId?: number; isActive?: boolean }) => {
  return useQuery({
    queryKey: clinicMedicineKeys.list(params),
    queryFn: () => clinicMedicineService.getAll(params)
  })
}

export const useClinicMedicine = (id: number) => {
  return useQuery({
    queryKey: clinicMedicineKeys.detail(id),
    queryFn: () => clinicMedicineService.getById(id),
    enabled: !!id && id > 0
  })
}

export const useSearchClinicMedicines = (searchTerm?: string) => {
  return useQuery({
    queryKey: clinicMedicineKeys.search(searchTerm),
    queryFn: () => clinicMedicineService.search(searchTerm),
    enabled: !!searchTerm && searchTerm.length >= 2
  })
}

export const useCreateClinicMedicine = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (data: CreateClinicMedicineRequest) => clinicMedicineService.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: clinicMedicineKeys.lists() })
      toast.success('Clinic medicine created successfully')
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.errors?.[0] || error.response?.data?.message || 'Failed to create clinic medicine')
    }
  })
}

export const useAddClinicMedicineFromGlobal = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (data: AddFromGlobalRequest) => clinicMedicineService.addFromGlobal(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: clinicMedicineKeys.lists() })
      toast.success('Medicine added from global catalog successfully')
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.errors?.[0] || error.response?.data?.message || 'Failed to add medicine from global catalog')
    }
  })
}

export const useUpdateClinicMedicine = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, data }: { id: number; data: UpdateClinicMedicineRequest }) =>
      clinicMedicineService.update(id, data),
    onSuccess: (updatedMedicine) => {
      queryClient.setQueryData(clinicMedicineKeys.detail(updatedMedicine.id), updatedMedicine)
      queryClient.invalidateQueries({ queryKey: clinicMedicineKeys.lists() })
      toast.success('Clinic medicine updated successfully')
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.errors?.[0] || error.response?.data?.message || 'Failed to update clinic medicine')
    }
  })
}

export const useDeleteClinicMedicine = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: number) => clinicMedicineService.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: clinicMedicineKeys.lists() })
      toast.success('Clinic medicine deleted successfully')
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.errors?.[0] || error.response?.data?.message || 'Failed to delete clinic medicine')
    }
  })
}

