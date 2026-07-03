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
          // Invalidate queue queries to refresh data
          const { organizationId, BranchId, doctorId } = data
          
          // Invalidate all queue-related queries
          queryClient.invalidateQueries({ queryKey: ['queues'] })
          queryClient.invalidateQueries({ queryKey: ['public-queues'] })
          queryClient.invalidateQueries({ queryKey: ['queue'] })
          queryClient.invalidateQueries({ queryKey: ['public-queue'] })
          
          // Also invalidate appointments
          queryClient.invalidateQueries({ queryKey: appointmentKeys.lists() })
          
          if (doctorId) {
            queryClient.invalidateQueries({ queryKey: appointmentKeys.queue(doctorId, BranchId, new Date().toISOString().split('T')[0]) })
          }
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
            queryKey: appointmentKeys.queue(data.doctorId, data.BranchId, data.date) 
          })
          
          toast.success(`New appointment: ${data.patientName}`)
        })

        connection.on('PatientCalled', (data) => {
          // Update queue when patient is called
          queryClient.invalidateQueries({ 
            queryKey: appointmentKeys.queue(data.doctorId, data.BranchId, data.date) 
          })
          
          toast.success(`${data.patientName} called for consultation (Token: ${data.tokenNumber})`)
        })

        connection.on('AppointmentCancelled', (data) => {
          queryClient.invalidateQueries({ queryKey: appointmentKeys.lists() })
          queryClient.invalidateQueries({ 
            queryKey: appointmentKeys.queue(data.doctorId, data.BranchId, data.date) 
          })
          
          toast(`Appointment cancelled: ${data.patientName}`, { icon: 'ℹ️' })
        })

        // User join/leave events (informational, no action needed)
        connection.on('UserJoined', (userName) => {
          // User joined the queue group - no action needed, just acknowledge
        })

        connection.on('UserLeft', (userName) => {
          // User left the queue group - no action needed, just acknowledge
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
  const joinDoctorQueue = async (doctorId: number, BranchId: number) => {
    if (state.connection && state.isConnected) {
      try {
        await state.connection.invoke('JoinDoctorQueue', doctorId, BranchId)
        console.log(`Joined queue for doctor ${doctorId} at clinic ${BranchId}`)
      } catch (error) {
        console.error('Failed to join doctor queue:', error)
      }
    }
  }

  // Leave doctor queue
  const leaveDoctorQueue = async (doctorId: number, BranchId: number) => {
    if (state.connection && state.isConnected) {
      try {
        // Check if connection is still active before invoking
        if (state.connection.state === 'Connected') {
          await state.connection.invoke('LeaveDoctorQueue', doctorId, BranchId)
          console.log(`Left queue for doctor ${doctorId} at clinic ${BranchId}`)
        }
      } catch (error: any) {
        // Don't log errors if connection is already closed - this is expected during cleanup
        if (error?.message?.includes('connection being closed') || 
            error?.message?.includes('canceled') ||
            state.connection?.state !== 'Connected') {
          // Connection already closed, which is fine during cleanup
          return
        }
        console.error('Failed to leave doctor queue:', error)
      }
    }
  }

  // Join clinic updates
  const JoinBranchUpdates = async (BranchId: number) => {
    if (state.connection && state.isConnected) {
      try {
        if (state.connection.state === 'Connected') {
          await state.connection.invoke('JoinBranchUpdates', BranchId)
          console.log(`Joined clinic updates for clinic ${BranchId}`)
        }
      } catch (error: any) {
        // Don't log errors if connection is already closed
        if (error?.message?.includes('connection being closed') || 
            error?.message?.includes('canceled') ||
            state.connection?.state !== 'Connected') {
          return
        }
        console.error('Failed to join clinic updates:', error)
      }
    }
  }

  return {
    ...state,
    joinDoctorQueue,
    leaveDoctorQueue,
    JoinBranchUpdates
  }
}

// Hook for doctor queue real-time updates
export const useDoctorQueueUpdates = (doctorId: number, BranchId: number) => {
  const { joinDoctorQueue, leaveDoctorQueue, isConnected, connection } = useSignalR()

  useEffect(() => {
    if (isConnected && doctorId && BranchId) {
      joinDoctorQueue(doctorId, BranchId)
      
      return () => {
        // Only try to leave if connection is still active
        if (connection && connection.state === 'Connected') {
          leaveDoctorQueue(doctorId, BranchId).catch(() => {
            // Silently handle errors during cleanup - connection may already be closed
          })
        }
      }
    }
  }, [isConnected, doctorId, BranchId, joinDoctorQueue, leaveDoctorQueue, connection])
}

// Hook for clinic-wide updates
export const useClinicUpdates = (BranchId: number) => {
  const { JoinBranchUpdates, isConnected } = useSignalR()

  useEffect(() => {
    if (isConnected && BranchId) {
      JoinBranchUpdates(BranchId)
    }
  }, [isConnected, BranchId, JoinBranchUpdates])
}
