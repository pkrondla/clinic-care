import apiClient from './apiClient'
import type { 
  Patient, 
  PatientDetail, 
  PatientSearch, 
  CreatePatientRequest, 
  UpdatePatientRequest, 
  PatientFilters,
  PatientSearchRequest
} from '../types/patient'
import type { PaginatedResponse } from '../types/common'

export const patientService = {
  // Get all patients with filtering and pagination
  getPatients: async (filters: PatientFilters = {}): Promise<PaginatedResponse<Patient>> => {
    const params = new URLSearchParams()
    
    if (filters.page) params.append('page', filters.page.toString())
    if (filters.pageSize) params.append('pageSize', filters.pageSize.toString())
    if (filters.search) params.append('search', filters.search)
    if (filters.gender) params.append('gender', filters.gender)
    if (filters.bloodGroup) params.append('bloodGroup', filters.bloodGroup)
    if (filters.minAge) params.append('minAge', filters.minAge.toString())
    if (filters.maxAge) params.append('maxAge', filters.maxAge.toString())
    if (filters.sortBy) params.append('sortBy', filters.sortBy)
    if (filters.sortOrder) params.append('sortOrder', filters.sortOrder)

    const response = await apiClient.get(`/patients?${params.toString()}`)
    return response.data.data
  },

  // Search patients for quick lookup
  searchPatients: async (request: PatientSearchRequest): Promise<PatientSearch[]> => {
    const params = new URLSearchParams()
    params.append('searchTerm', request.searchTerm)
    if (request.limit) params.append('limit', request.limit.toString())

    const response = await apiClient.get(`/patients/search?${params.toString()}`)
    return response.data.data
  },

  // Get specific patient by ID
  getPatient: async (id: number): Promise<PatientDetail> => {
    const response = await apiClient.get(`/patients/${id}`)
    return response.data.data
  },

  // Create new patient
  createPatient: async (patient: CreatePatientRequest): Promise<Patient> => {
    const response = await apiClient.post('/patients', patient)
    return response.data.data
  },

  // Update patient
  updatePatient: async (id: number, patient: UpdatePatientRequest): Promise<Patient> => {
    const response = await apiClient.put(`/patients/${id}`, patient)
    return response.data.data
  },

  // Delete patient (soft delete)
  deletePatient: async (id: number): Promise<void> => {
    await apiClient.delete(`/patients/${id}`)
  }
}
