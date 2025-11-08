export enum AppointmentType {
  InPerson = 1,
  Teleconsultation = 2
}

export enum AppointmentStatus {
  Scheduled = 1,
  InProgress = 2,
  Completed = 3,
  Cancelled = 4
}

export interface AppointmentPatient {
  id: number
  name: string
  patientCode: string
  age: number
  gender: string
  phone?: string
  email?: string
  bloodGroup?: string
  address?: string
  emergencyContact?: string
  medicalHistory?: string
}

export interface Doctor {
  id: number
  name: string
  qualification: string
  specialization?: string
  registrationNumber: string
  experienceYears: number
  consultationFeeInPerson: number
  consultationFeeTele: number
  followupFeeInPerson: number
  followupFeeTele: number
}

export interface Appointment {
  id: number
  tokenNumber: number
  appointmentDate: string
  type: AppointmentType
  status: AppointmentStatus
  notes?: string
  doctor: Doctor
  patient: AppointmentPatient
  clinic: {
    id: number
    name: string
    code: string
  }
  consultation?: {
    id: number
    chiefComplaint: string
    diagnosis: string
    consultationDate: string
  }
}

export interface QueueItem {
  id: number
  tokenNumber: number
  status: AppointmentStatus
  type: AppointmentType
  patient: {
    id: number
    name: string
    patientCode: string
  }
  queuePosition: number
}

export interface AppointmentFilters {
  clinicId?: number
  doctorId?: number
  date?: string
  status?: AppointmentStatus
  type?: AppointmentType
}

export interface CreateAppointmentRequest {
  clinicId: number
  doctorId: number
  patientId: number
  appointmentDate: string
  type: AppointmentType
  notes?: string
}

export interface UpdateAppointmentStatusRequest {
  status: AppointmentStatus
  notes?: string
}
