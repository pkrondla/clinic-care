import { BaseEntity, BaseFilter } from './common'

export interface Patient extends BaseEntity {
  id: number
  userId: number
  patientCode: string
  email: string
  firstName: string
  lastName: string
  fullName: string
  phone: string
  dateOfBirth: string
  age: number
  gender: string
  bloodGroup: string
  address: string
  emergencyContact: string
  medicalHistory: string
  photoUrl?: string
  totalAppointments: number
  totalConsultations: number
  lastVisitDate?: string
}

export interface PatientDetail extends Patient {
  completedAppointments: number
  cancelledAppointments: number
  firstVisitDate?: string
  recentAppointments: RecentAppointment[]
  recentConsultations: RecentConsultation[]
}

export interface RecentAppointment {
  id: number
  tokenNumber: number
  appointmentDate: string
  type: string
  status: string
  doctorName: string
  clinicName: string
  notes?: string
}

export interface RecentConsultation {
  id: number
  consultationDate: string
  chiefComplaint: string
  diagnosis: string
  doctorName: string
  clinicName: string
  hasPrescription: boolean
}

export interface PatientSearch {
  id: number
  patientCode: string
  fullName: string
  email: string
  phone: string
  age: number
  gender: string
  bloodGroup: string
  lastVisitDate?: string
}

export interface CreatePatientRequest {
  email: string
  firstName: string
  lastName: string
  phone: string
  dateOfBirth: string
  gender: string
  bloodGroup: string
  address: string
  emergencyContact: string
  medicalHistory: string
  photoUrl?: string
  password: string
}

export interface UpdatePatientRequest {
  email: string
  firstName: string
  lastName: string
  phone: string
  dateOfBirth: string
  gender: string
  bloodGroup: string
  address: string
  emergencyContact: string
  medicalHistory: string
  photoUrl?: string
}

export interface PatientFilters extends BaseFilter {
  gender?: string
  bloodGroup?: string
  minAge?: number
  maxAge?: number
}

export interface PatientSearchRequest {
  searchTerm: string
  limit?: number
}

// Gender options
export const GENDER_OPTIONS = [
  { label: 'Male', value: 'Male' },
  { label: 'Female', value: 'Female' },
  { label: 'Other', value: 'Other' }
] as const

// Blood group options
export const BLOOD_GROUP_OPTIONS = [
  { label: 'A+', value: 'A+' },
  { label: 'A-', value: 'A-' },
  { label: 'B+', value: 'B+' },
  { label: 'B-', value: 'B-' },
  { label: 'AB+', value: 'AB+' },
  { label: 'AB-', value: 'AB-' },
  { label: 'O+', value: 'O+' },
  { label: 'O-', value: 'O-' }
] as const

// Age range options
export const AGE_RANGE_OPTIONS = [
  { label: '0-18', value: { min: 0, max: 18 } },
  { label: '19-30', value: { min: 19, max: 30 } },
  { label: '31-50', value: { min: 31, max: 50 } },
  { label: '51-70', value: { min: 51, max: 70 } },
  { label: '70+', value: { min: 70, max: 100 } }
] as const

