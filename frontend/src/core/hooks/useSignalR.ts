import { useEffect, useRef, useState } from 'react'
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr'
import { useQueryClient } from '@tanstack/react-query'
import { useAuth } from '../stores/authStore'
import { appointmentKeys } from './queries/useAppointments'
import toast from 'react-hot-toast'

interface SignalRState {
  connection: HubConnection | null
  isConnected: boolean
  isConnecting: boolean
  error: string | null
}

export const useSignalR = () => {
  const [state, setState] = useState<SignalRState>({
    connection: null,
    isConnected: false,
    isConnecting: false,
    error: null
  })
  
  const queryClient = useQueryClient()
  const { token, isAuthenticated } = useAuth()
  const connectionRef = useRef<HubConnection | null>(null)

  useEffect(() => {
    if (!isAuthenticated || !token) {
      // Disconnect if not authenticated
      if (connectionRef.current) {
        connectionRef.current.stop()
        connectionRef.current = null
        setState(prev => ({ ...prev, connection: null, isConnected: false }))
      }
      return
    }

    const connect = async () => {
      setState(prev => ({ ...prev, isConnecting: true, error: null }))

      try {
        const connection = new HubConnectionBuilder()
          .withUrl('/queueHub', {
            accessTokenFactory: () => token
          })
          .withAutomaticReconnect([0, 2000, 10000, 30000])
          .configureLogging(LogLevel.Information)
          .build()

        // Event handlers
        connection.on('QueueUpdated', (data) => {
          // Update queue data in cache
          const { doctorId, clinicId, date, queueData } = data
          queryClient.setQueryData(
            appointmentKeys.queue(doctorId, clinicId, date),
            queueData
          )
        })

        connection.on('AppointmentStatusChanged', (data) => {
          // Update appointment status
          const { appointmentId, status, appointment } = data
          
          queryClient.setQueryData(
            appointmentKeys.detail(appointmentId),
            appointment
          )
          
          // Invalidate related queries
          queryClient.invalidateQueries({ queryKey: appointmentKeys.lists() })
          
          toast.success(`Appointment status updated to ${status}`)
        })

        connection.on('NewAppointment', (data) => {
          // Invalidate appointment lists to show new appointment
          queryClient.invalidateQueries({ queryKey: appointmentKeys.lists() })
          queryClient.invalidateQueries({ 
            queryKey: appointmentKeys.queue(data.doctorId, data.clinicId, data.date) 
          })
          
          toast.success(`New appointment: ${data.patientName}`)
        })

        connection.on('PatientCalled', (data) => {
          // Update queue when patient is called
          queryClient.invalidateQueries({ 
            queryKey: appointmentKeys.queue(data.doctorId, data.clinicId, data.date) 
          })
          
          toast.success(`${data.patientName} called for consultation (Token: ${data.tokenNumber})`)
        })

        connection.on('AppointmentCancelled', (data) => {
          queryClient.invalidateQueries({ queryKey: appointmentKeys.lists() })
          queryClient.invalidateQueries({ 
            queryKey: appointmentKeys.queue(data.doctorId, data.clinicId, data.date) 
          })
          
          toast(`Appointment cancelled: ${data.patientName}`, { icon: 'ℹ️' })
        })

        // Connection state handlers
        connection.onreconnecting(() => {
          setState(prev => ({ ...prev, isConnected: false, isConnecting: true }))
          toast.loading('Reconnecting...', { id: 'signalr-reconnect' })
        })

        connection.onreconnected(() => {
          setState(prev => ({ ...prev, isConnected: true, isConnecting: false }))
          toast.success('Reconnected', { id: 'signalr-reconnect' })
        })

        connection.onclose((error) => {
          setState(prev => ({ 
            ...prev, 
            isConnected: false, 
            isConnecting: false,
            error: error?.message || 'Connection closed'
          }))
          toast.error('Connection lost')
        })

        // Start connection
        await connection.start()
        
        connectionRef.current = connection
        setState(prev => ({ 
          ...prev, 
          connection, 
          isConnected: true, 
          isConnecting: false,
          error: null 
        }))

        console.log('SignalR connected successfully')

      } catch (error: any) {
        console.error('SignalR connection failed:', error)
        setState(prev => ({ 
          ...prev, 
          isConnecting: false,
          error: error.message || 'Connection failed'
        }))
        toast.error('Failed to connect to real-time updates')
      }
    }

    connect()

    // Cleanup on unmount
    return () => {
      if (connectionRef.current) {
        connectionRef.current.stop()
        connectionRef.current = null
      }
    }
  }, [isAuthenticated, token, queryClient])

  // Join doctor queue for real-time updates
  const joinDoctorQueue = async (doctorId: number, clinicId: number) => {
    if (state.connection && state.isConnected) {
      try {
        await state.connection.invoke('JoinDoctorQueue', doctorId, clinicId)
        console.log(`Joined queue for doctor ${doctorId} at clinic ${clinicId}`)
      } catch (error) {
        console.error('Failed to join doctor queue:', error)
      }
    }
  }

  // Leave doctor queue
  const leaveDoctorQueue = async (doctorId: number, clinicId: number) => {
    if (state.connection && state.isConnected) {
      try {
        await state.connection.invoke('LeaveDoctorQueue', doctorId, clinicId)
        console.log(`Left queue for doctor ${doctorId} at clinic ${clinicId}`)
      } catch (error) {
        console.error('Failed to leave doctor queue:', error)
      }
    }
  }

  // Join clinic updates
  const joinClinicUpdates = async (clinicId: number) => {
    if (state.connection && state.isConnected) {
      try {
        await state.connection.invoke('JoinClinicUpdates', clinicId)
        console.log(`Joined clinic updates for clinic ${clinicId}`)
      } catch (error) {
        console.error('Failed to join clinic updates:', error)
      }
    }
  }

  return {
    ...state,
    joinDoctorQueue,
    leaveDoctorQueue,
    joinClinicUpdates
  }
}

// Hook for doctor queue real-time updates
export const useDoctorQueueUpdates = (doctorId: number, clinicId: number) => {
  const { joinDoctorQueue, leaveDoctorQueue, isConnected } = useSignalR()

  useEffect(() => {
    if (isConnected && doctorId && clinicId) {
      joinDoctorQueue(doctorId, clinicId)
      
      return () => {
        leaveDoctorQueue(doctorId, clinicId)
      }
    }
  }, [isConnected, doctorId, clinicId, joinDoctorQueue, leaveDoctorQueue])
}

// Hook for clinic-wide updates
export const useClinicUpdates = (clinicId: number) => {
  const { joinClinicUpdates, isConnected } = useSignalR()

  useEffect(() => {
    if (isConnected && clinicId) {
      joinClinicUpdates(clinicId)
    }
  }, [isConnected, clinicId, joinClinicUpdates])
}
