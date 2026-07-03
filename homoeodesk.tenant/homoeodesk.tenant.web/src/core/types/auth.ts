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

export interface Branch {
  id: number
  name: string
  code: string
  address?: string
  contactPhone?: string
  contactEmail?: string
}

/** @deprecated Use Branch */
export type Clinic = Branch

export interface User {
  id: number
  email: string
  firstName: string
  lastName: string
  fullName: string
  role: UserRole
  organizationId: number
  organizationName: string
  selectedBranchId?: number
  selectedBranchName?: string
  phone?: string
  availableBranches?: Branch[]
}

export interface Organization {
  id: number
  name: string
  subdomain: string
  contactEmail: string
  contactPhone?: string
  address?: string
}

export interface LoginRequest {
  email: string
  password: string
  branchId?: number
}

export interface LoginResponse {
  accessToken: string
  refreshToken: string
  expiresAt: string
  user: User
  availableBranches: Branch[]
}

export interface AuthState {
  user: User | null
  token: string | null
  activeBranch: Branch | null
  authorizedBranches: Branch[]
  isAuthenticated: boolean
  isGlobalSystem: boolean
}

export interface RefreshTokenRequest {
  refreshToken: string
}

export interface RefreshTokenResponse {
  accessToken: string
  expiresAt: string
}
