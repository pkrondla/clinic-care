export interface AuthState {
  user: User | null
  token: string | null
  isAuthenticated: boolean
  isGlobalSystem: boolean
}

export enum UserRole {
  SuperAdmin = 'SuperAdmin',
  SystemAdmin = 'SystemAdmin',
  Admin = 'OrganizationAdmin',
  OrganizationAdmin = 'OrganizationAdmin',
  Doctor = 'Doctor',
  Reception = 'Reception',
  Staff = 'Reception',
  Pharmacy = 'Pharmacy',
  Patient = 'Patient'
}

export interface User {
  id: number
  email: string
  firstName: string
  lastName: string
  fullName: string
  role: UserRole
  organizationId: number
  organizationName: string
  selectedClinicId?: number
  selectedClinicName?: string
  phone?: string
  availableClinics?: Clinic[]
}

export interface Organization {
  id: number
  name: string
  subdomain: string
  contactEmail: string
  contactPhone?: string
  address?: string
}

export interface Clinic {
  id: number
  name: string
  code: string
  address?: string
  contactPhone?: string
  contactEmail?: string
}

export interface LoginRequest {
  email: string
  password: string
  clinicId?: number
}

export interface LoginResponse {
  accessToken: string
  refreshToken: string
  expiresAt: string
  user: User
  availableClinics: Clinic[]
}

export interface RefreshTokenRequest {
  refreshToken: string
}

export interface RefreshTokenResponse {
  accessToken: string
  expiresAt: string
}
