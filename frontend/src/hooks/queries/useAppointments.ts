import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { appointmentService } from '../../services/appointmentService'
import { useSelectedClinic } from '../../stores/authStore'
import type { 
  AppointmentFilters, 
  UpdateAppointmentStatusRequest 
} from '../../types/index'
import toast from 'react-hot-toast'
import dayjs from 'dayjs'

// Query keys
export const appointmentKeys = {
  all: ['appointments'] as const,
  lists: () => [...appointmentKeys.all, 'list'] as const,
  list: (filters: AppointmentFilters) => [...appointmentKeys.lists(), filters] as const,
  details: () => [...appointmentKeys.all, 'detail'] as const,
  detail: (id: number) => [...appointmentKeys.details(), id] as const,
  queue: (doctorId: number, clinicId: number, date?: string) => 
    [...appointmentKeys.all, 'queue', doctorId, clinicId, date] as const,
  patient: (patientId: number) => [...appointmentKeys.all, 'patient', patientId] as const,
  doctor: (doctorId: number, startDate: string, endDate: string) => 
    [...appointmentKeys.all, 'doctor', doctorId, startDate, endDate] as const,
  today: (clinicId: number) => [...appointmentKeys.all, 'today', clinicId] as const,
  stats: (clinicId?: number, doctorId?: number) => 
    [...appointmentKeys.all, 'stats', clinicId, doctorId] as const
}

// Get appointments with filters
export const useAppointments = (filters: AppointmentFilters = {}) => {
  const selectedClinic = useSelectedClinic()
  
  const finalFilters = {
    ...filters,
    clinicId: filters.clinicId || selectedClinic?.id
  }
  
  return useQuery({
    queryKey: appointmentKeys.list(finalFilters),
    queryFn: () => appointmentService.getAppointments(finalFilters),
    enabled: !!finalFilters.clinicId,
    staleTime: 30 * 1000 // 30 seconds
  })
}

// Get appointment by ID
export const useAppointment = (id: number) => {
  return useQuery({
    queryKey: appointmentKeys.detail(id),
    queryFn: () => appointmentService.getAppointment(id),
    enabled: !!id
  })
}

// Get doctor's queue
export const useDoctorQueue = (
  doctorId: number, 
  clinicId: number, 
  date?: string
) => {
  const targetDate = date || dayjs().format('YYYY-MM-DD')
  
  return useQuery({
    queryKey: appointmentKeys.queue(doctorId, clinicId, targetDate),
    queryFn: () => appointmentService.getDoctorQueue(doctorId, clinicId, targetDate),
    enabled: !!doctorId && !!clinicId,
    refetchInterval: 30 * 1000, // Refresh every 30 seconds
    staleTime: 10 * 1000 // 10 seconds
  })
}

// Get today's appointments for clinic
export const useTodayAppointments = () => {
  const selectedClinic = useSelectedClinic()
  
  return useQuery({
    queryKey: appointmentKeys.today(selectedClinic?.id || 0),
    queryFn: () => appointmentService.getTodayAppointments(selectedClinic!.id),
    enabled: !!selectedClinic,
    refetchInterval: 60 * 1000, // Refresh every minute
    staleTime: 30 * 1000
  })
}

// Get patient appointments
export const usePatientAppointments = (patientId: number) => {
  return useQuery({
    queryKey: appointmentKeys.patient(patientId),
    queryFn: () => appointmentService.getPatientAppointments(patientId),
    enabled: !!patientId
  })
}

// Get doctor appointments for date range
export const useDoctorAppointments = (
  doctorId: number,
  startDate: string,
  endDate: string
) => {
  return useQuery({
    queryKey: appointmentKeys.doctor(doctorId, startDate, endDate),
    queryFn: () => appointmentService.getDoctorAppointments(doctorId, startDate, endDate),
    enabled: !!doctorId && !!startDate && !!endDate
  })
}

// Get appointment statistics
export const useAppointmentStats = (clinicId?: number, doctorId?: number) => {
  return useQuery({
    queryKey: appointmentKeys.stats(clinicId, doctorId),
    queryFn: () => appointmentService.getAppointmentStats(clinicId, doctorId),
    staleTime: 5 * 60 * 1000 // 5 minutes
  })
}

// Create appointment mutation
export const useCreateAppointment = () => {
  const queryClient = useQueryClient()
  
  return useMutation({
    mutationFn: appointmentService.createAppointment,
    onSuccess: (newAppointment) => {
      // Invalidate related queries
      queryClient.invalidateQueries({ queryKey: appointmentKeys.lists() })
      queryClient.invalidateQueries({ 
        queryKey: appointmentKeys.queue(
          newAppointment.doctor.id, 
          newAppointment.clinic.id,
          newAppointment.appointmentDate
        ) 
      })
      queryClient.invalidateQueries({ queryKey: appointmentKeys.today(newAppointment.clinic.id) })
      
      toast.success('Appointment created successfully')
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || 'Failed to create appointment')
    }
  })
}

// Update appointment status mutation
export const useUpdateAppointmentStatus = () => {
  const queryClient = useQueryClient()
  
  return useMutation({
    mutationFn: ({ id, update }: { id: number; update: UpdateAppointmentStatusRequest }) =>
      appointmentService.updateAppointmentStatus(id, update),
    onSuccess: (updatedAppointment) => {
      // Update specific appointment
      queryClient.setQueryData(
        appointmentKeys.detail(updatedAppointment.id),
        updatedAppointment
      )
      
      // Invalidate lists and queue
      queryClient.invalidateQueries({ queryKey: appointmentKeys.lists() })
      queryClient.invalidateQueries({ 
        queryKey: appointmentKeys.queue(
          updatedAppointment.doctor.id, 
          updatedAppointment.clinic.id
        ) 
      })
      
      toast.success('Appointment status updated')
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || 'Failed to update appointment')
    }
  })
}

// Cancel appointment mutation
export const useCancelAppointment = () => {
  const queryClient = useQueryClient()
  
  return useMutation({
    mutationFn: ({ id, reason }: { id: number; reason?: string }) =>
      appointmentService.cancelAppointment(id, reason),
    onSuccess: (cancelledAppointment) => {
      queryClient.setQueryData(
        appointmentKeys.detail(cancelledAppointment.id),
        cancelledAppointment
      )
      
      queryClient.invalidateQueries({ queryKey: appointmentKeys.lists() })
      queryClient.invalidateQueries({ 
        queryKey: appointmentKeys.queue(
          cancelledAppointment.doctor.id, 
          cancelledAppointment.clinic.id
        ) 
      })
      
      toast.success('Appointment cancelled')
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || 'Failed to cancel appointment')
    }
  })
}

// Call next patient mutation
export const useCallNextPatient = () => {
  const queryClient = useQueryClient()
  
  return useMutation({
    mutationFn: ({ doctorId, clinicId }: { doctorId: number; clinicId: number }) =>
      appointmentService.callNextPatient(doctorId, clinicId),
    onSuccess: (appointment) => {
      queryClient.invalidateQueries({ 
        queryKey: appointmentKeys.queue(appointment.doctor.id, appointment.clinic.id) 
      })
      queryClient.invalidateQueries({ queryKey: appointmentKeys.lists() })
      
      toast.success(`Patient ${appointment.patient.name} called for consultation`)
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || 'Failed to call next patient')
    }
  })
}

// Complete appointment mutation
export const useCompleteAppointment = () => {
  const queryClient = useQueryClient()
  
  return useMutation({
    mutationFn: appointmentService.completeAppointment,
    onSuccess: (completedAppointment) => {
      queryClient.setQueryData(
        appointmentKeys.detail(completedAppointment.id),
        completedAppointment
      )
      
      queryClient.invalidateQueries({ queryKey: appointmentKeys.lists() })
      queryClient.invalidateQueries({ 
        queryKey: appointmentKeys.queue(
          completedAppointment.doctor.id, 
          completedAppointment.clinic.id
        ) 
      })
      
      toast.success('Appointment completed')
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || 'Failed to complete appointment')
    }
  })
}
