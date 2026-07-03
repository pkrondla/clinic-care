import { BaseEntity } from './common'

export interface Clinic extends BaseEntity {
  name: string
  address: string
  phone: string
  email: string
  description?: string
  logoUrl?: string
  timing: {
    openTime: string
    closeTime: string
    workingDays: string[]
  }
}

export interface ClinicFilters {
  search?: string
  page?: number
  pageSize?: number
  sortBy?: keyof Clinic
  sortOrder?: 'asc' | 'desc'
  isActive?: boolean
}