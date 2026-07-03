import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { message } from 'antd'
import { patientService } from '../../services/patientService'
import type { 
  CreatePatientRequest, 
  UpdatePatientRequest, 
  PatientFilters,
  PatientSearchRequest 
} from '../../types/patient'

// Query keys
export const patientKeys = {
  all: ['patients'] as const,
  lists: () => [...patientKeys.all, 'list'] as const,
  list: (filters: PatientFilters) => [...patientKeys.lists(), filters] as const,
  details: () => [...patientKeys.all, 'detail'] as const,
  detail: (id: number) => [...patientKeys.details(), id] as const,
  searches: () => [...patientKeys.all, 'search'] as const,
  search: (request: PatientSearchRequest) => [...patientKeys.searches(), request] as const,
}

// Get all patients with filtering and pagination
export const usePatients = (filters: PatientFilters = {}) => {
  return useQuery({
    queryKey: patientKeys.list(filters),
    queryFn: () => patientService.getPatients(filters),
    staleTime: 5 * 60 * 1000, // 5 minutes
  })
}

// Search patients for quick lookup
export const useSearchPatients = (request: PatientSearchRequest) => {
  return useQuery({
    queryKey: patientKeys.search(request),
    queryFn: () => patientService.searchPatients(request),
    enabled: !!request.searchTerm && request.searchTerm.length >= 2,
    staleTime: 2 * 60 * 1000, // 2 minutes
  })
}

// Get specific patient by ID
export const usePatient = (id: number) => {
  return useQuery({
    queryKey: patientKeys.detail(id),
    queryFn: () => patientService.getPatient(id),
    enabled: !!id,
    staleTime: 5 * 60 * 1000, // 5 minutes
  })
}

// Create patient mutation
export const useCreatePatient = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (patient: CreatePatientRequest) => patientService.createPatient(patient),
    onSuccess: () => {
      // Invalidate and refetch patient lists
      queryClient.invalidateQueries({ queryKey: patientKeys.lists() })
      queryClient.invalidateQueries({ queryKey: patientKeys.searches() })
      
      message.success('Patient created successfully')
    },
    onError: (error: any) => {
      // Debug: log the full error response
      console.error('Create patient error:', error.response?.data)
      
      // Backend returns errors in { message: string, errors: string[] } format
      const responseData = error.response?.data
      const errors = responseData?.errors
      let errorMessage = 'Failed to create patient'
      
      if (errors && Array.isArray(errors) && errors.length > 0) {
        errorMessage = errors.join(', ')
      } else if (responseData?.message) {
        errorMessage = responseData.message
      } else if (error.message) {
        errorMessage = error.message
      }
      
      message.error(errorMessage)
    },
  })
}

// Update patient mutation
export const useUpdatePatient = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, patient }: { id: number; patient: UpdatePatientRequest }) => 
      patientService.updatePatient(id, patient),
    onSuccess: (data, variables) => {
      // Update the specific patient in cache
      queryClient.setQueryData(patientKeys.detail(variables.id), data)
      
      // Invalidate and refetch patient lists
      queryClient.invalidateQueries({ queryKey: patientKeys.lists() })
      queryClient.invalidateQueries({ queryKey: patientKeys.searches() })
      
      message.success('Patient updated successfully')
    },
    onError: (error: any) => {
      const errors = error.response?.data?.errors
      const errorMessage = errors?.length > 0 
        ? errors.join(', ') 
        : (error.response?.data?.message || 'Failed to update patient')
      message.error(errorMessage)
    },
  })
}

// Delete patient mutation
export const useDeletePatient = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: number) => patientService.deletePatient(id),
    onSuccess: (_, id) => {
      // Remove the patient from cache
      queryClient.removeQueries({ queryKey: patientKeys.detail(id) })
      
      // Invalidate and refetch patient lists
      queryClient.invalidateQueries({ queryKey: patientKeys.lists() })
      queryClient.invalidateQueries({ queryKey: patientKeys.searches() })
      
      message.success('Patient deleted successfully')
    },
    onError: (error: any) => {
      const errors = error.response?.data?.errors
      const errorMessage = errors?.length > 0 
        ? errors.join(', ') 
        : (error.response?.data?.message || 'Failed to delete patient')
      message.error(errorMessage)
    },
  })
}
