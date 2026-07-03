import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { consultationService, type GetConsultationsParams, type UpdateConsultationRequest } from '../../services/consultationService'
import toast from 'react-hot-toast'

// Query keys
export const consultationKeys = {
  all: ['consultations'] as const,
  lists: () => [...consultationKeys.all, 'list'] as const,
  list: (params?: GetConsultationsParams) => [...consultationKeys.lists(), params] as const,
  details: () => [...consultationKeys.all, 'detail'] as const,
  detail: (id: number) => [...consultationKeys.details(), id] as const,
  patient: (patientId: number) => [...consultationKeys.all, 'patient', patientId] as const
}

// Get consultations with filters
export const useConsultations = (params?: GetConsultationsParams) => {
  return useQuery({
    queryKey: consultationKeys.list(params),
    queryFn: () => consultationService.getAll(params),
    enabled: !!params?.BranchId || !params?.BranchId // Enable if BranchId is provided or not required
  })
}

// Get consultation by ID
export const useConsultation = (id: number) => {
  return useQuery({
    queryKey: consultationKeys.detail(id),
    queryFn: () => consultationService.getById(id),
    enabled: !!id && id > 0
  })
}

// Get patient consultations
export const usePatientConsultations = (patientId: number) => {
  return useQuery({
    queryKey: consultationKeys.patient(patientId),
    queryFn: () => consultationService.getByPatient(patientId),
    enabled: !!patientId && patientId > 0
  })
}

// Update consultation mutation
export const useUpdateConsultation = () => {
  const queryClient = useQueryClient()
  
  return useMutation({
    mutationFn: ({ id, data }: { id: number; data: UpdateConsultationRequest }) =>
      consultationService.update(id, data),
    onSuccess: (updatedConsultation) => {
      queryClient.setQueryData(
        consultationKeys.detail(updatedConsultation.id),
        updatedConsultation
      )
      
      queryClient.invalidateQueries({ queryKey: consultationKeys.lists() })
      
      toast.success('Consultation updated successfully')
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || error.response?.data?.errors?.[0] || 'Failed to update consultation')
    }
  })
}

