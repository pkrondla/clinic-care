export enum UserRole {
  SuperAdmin = 1,
  Admin = 2,
  Doctor = 3,
  Staff = 4,
  Patient = 5
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

export interface AuthState {
  user: User | null
  token: string | null
  selectedClinic: Clinic | null
  availableClinics: Clinic[]
  isAuthenticated: boolean
}

export interface RefreshTokenRequest {
  refreshToken: string
}

export interface RefreshTokenResponse {
  accessToken: string
  expiresAt: string
}
