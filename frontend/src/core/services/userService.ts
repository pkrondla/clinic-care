import { api } from './apiClient'

export interface User {
  id: number
  email: string
  firstName: string
  lastName: string
  fullName: string
  phone: string
  role: string
  organizationId: number
  organizationName: string
  selectedClinicId?: number
  selectedClinicName?: string
  isActive: boolean
  createdAt: string
  lastLoginAt?: string
  clinicAccess: ClinicAccess[]
  doctorProfile?: DoctorProfile
}

export interface ClinicAccess {
  clinicId: number
  clinicName: string
  clinicCode: string
  canAccess: boolean
}

export interface DoctorProfile {
  id: number
  qualification: string
  specialization: string
  registrationNumber: string
  experienceYears: number
  consultationFeeInPerson: number
  consultationFeeTele: number
  followupFeeInPerson: number
  followupFeeTele: number
  isActive: boolean
}

export interface UserFilters {
  searchTerm?: string
  role?: string
  clinicId?: number
  isActive?: boolean
}

export interface CreateUserRequest {
  email: string
  password: string
  firstName: string
  lastName: string
  phone?: string
  role: string
  clinicIds?: number[]
  registrationNumber?: string
  qualification?: string
  specialization?: string
  experienceYears?: number
  consultationFeeInPerson?: number
  consultationFeeTele?: number
  followupFeeInPerson?: number
  followupFeeTele?: number
}

export interface UpdateUserRequest {
  email: string
  password?: string
  firstName: string
  lastName: string
  phone?: string
  role: string
  isActive: boolean
  clinicIds?: number[]
  registrationNumber?: string
  qualification?: string
  specialization?: string
  experienceYears?: number
  consultationFeeInPerson?: number
  consultationFeeTele?: number
  followupFeeInPerson?: number
  followupFeeTele?: number
}

class UserService {
  async getUsers(filters?: UserFilters): Promise<User[]> {
    const params = new URLSearchParams()
    if (filters?.searchTerm) params.append('searchTerm', filters.searchTerm)
    if (filters?.role) params.append('role', filters.role)
    if (filters?.clinicId) params.append('clinicId', filters.clinicId.toString())
    if (filters?.isActive !== undefined) params.append('isActive', filters.isActive.toString())

    const response = await api.get<User[]>(`/users?${params.toString()}`)
    return response.data
  }

  async getUser(id: number): Promise<User> {
    const response = await api.get<User>(`/users/${id}`)
    return response.data
  }

  async createUser(user: CreateUserRequest): Promise<User> {
    const response = await api.post<User>('/users', user)
    return response.data
  }

  async updateUser(id: number, user: UpdateUserRequest): Promise<User> {
    const response = await api.put<User>(`/users/${id}`, user)
    return response.data
  }

  async deleteUser(id: number): Promise<boolean> {
    const response = await api.delete<boolean>(`/users/${id}`)
    return response.data
  }

  async assignClinicAccess(userId: number, clinicIds: number[]): Promise<boolean> {
    const response = await api.post<boolean>(`/users/${userId}/clinic-access`, { clinicIds })
    return response.data
  }
}

export const userService = new UserService()

