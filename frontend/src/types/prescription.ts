import { BaseEntity } from './common'
import { Patient } from './patient'

export interface Prescription extends BaseEntity {
  patientId: number
  patient?: Patient
  doctorId: number
  doctorName: string
  prescriptionDate: string
  diagnosis: string
  medicines: PrescriptionMedicine[]
  notes?: string
  followUpDate?: string
}

export interface PrescriptionMedicine {
  medicineId: number
  medicineName: string
  dosage: string
  frequency: string
  duration: string
  notes?: string
}

export interface PrescriptionFilters {
  search?: string
  page?: number
  pageSize?: number
  sortBy?: keyof Prescription
  sortOrder?: 'asc' | 'desc'
  patientId?: number
  doctorId?: number
  startDate?: string
  endDate?: string
}