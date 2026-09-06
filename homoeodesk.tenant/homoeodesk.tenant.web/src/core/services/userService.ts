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
  SelectedBranchId?: number
  SelectedBranchName?: string
  isActive: boolean
  createdAt: string
  lastLoginAt?: string
  clinicAccess: ClinicAccess[]
  doctorProfile?: DoctorProfile
}

export interface ClinicAccess {
  BranchId: number
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
  BranchId?: number
  isActive?: boolean
}

export interface CreateUserRequest {
  email: string
  password: string
  firstName: string
  lastName: string
  phone?: string
  role: string
  BranchIds?: number[]
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
  BranchIds?: number[]
  registrationNumber?: string
  qualification?: string
  specialization?: string
  experienceYears?: number
  consultationFeeInPerson?: number
  consultationFeeTele?: number
  followupFeeInPerson?: number
  followupFeeTele?: number
}

/** Map frontend role labels to backend UserRole enum names. */
function toBackendRole(role: string): string {
  switch (role) {
    case 'OrganizationAdmin':
    case 'Admin':
      return 'Admin'
    case 'Reception':
    case 'Staff':
    case 'Pharmacy':
      return 'Staff'
    case 'Doctor':
      return 'Doctor'
    case 'Patient':
      return 'Patient'
    case 'SuperAdmin':
      return 'SuperAdmin'
    default:
      return role
  }
}

class UserService {
  async getUsers(filters?: UserFilters): Promise<User[]> {
    const params = new URLSearchParams()
    if (filters?.searchTerm) params.append('searchTerm', filters.searchTerm)
    if (filters?.role) params.append('role', filters.role)
    if (filters?.BranchId) params.append('BranchId', filters.BranchId.toString())
    if (filters?.isActive !== undefined) params.append('isActive', filters.isActive.toString())

    const queryString = params.toString()
    const url = queryString ? `/users?${queryString}` : '/users'
    const response = await api.get<User[]>(url)
    
    // Backend returns array directly, not wrapped in ApiResponse
    // api.get returns response.data which is the array itself
    if (Array.isArray(response)) {
      return response
    }
    // Fallback: if wrapped in ApiResponse, extract data
    if (response && typeof response === 'object' && 'data' in response && Array.isArray(response.data)) {
      return response.data
    }
    // Safety fallback
    return []
  }

  async getUser(id: number): Promise<User> {
    const response = await api.get<User>(`/users/${id}`)
    return response.data
  }

  async createUser(user: CreateUserRequest): Promise<User> {
    const response = await api.post<User>('/users', {
      ...user,
      role: toBackendRole(user.role),
    })
    return response.data
  }

  async updateUser(id: number, user: UpdateUserRequest): Promise<User> {
    const response = await api.put<User>(`/users/${id}`, {
      ...user,
      role: toBackendRole(user.role),
    })
    return response.data
  }

  async deleteUser(id: number): Promise<boolean> {
    const response = await api.delete<boolean>(`/users/${id}`)
    return response.data
  }

  async AssignBranchAccess(userId: number, BranchIds: number[]): Promise<boolean> {
    const response = await api.post<boolean>(`/users/${userId}/clinic-access`, { BranchIds })
    return response.data
  }
}

export const userService = new UserService()

