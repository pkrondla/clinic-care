import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import queueService, {
  GetAllQueuesParams,
  GetQueueParams,
  BookAppointmentRequest,
  DoctorQueueDto,
} from '@core/services/queueService';
import { toast } from 'react-hot-toast';

/**
 * Hook to get all doctor queues (authenticated)
 */
export function useQueues(params: GetAllQueuesParams = {}) {
  return useQuery({
    queryKey: ['queues', params],
    queryFn: () => queueService.getAllQueues(params),
    staleTime: 5000, // 5 seconds - queues update frequently
    refetchInterval: 10000, // Auto-refetch every 10 seconds
  });
}

/**
 * Hook to get a specific doctor's queue (authenticated)
 */
export function useQueue(params: GetQueueParams) {
  return useQuery({
    queryKey: ['queue', params],
    queryFn: () => queueService.getQueue(params),
    staleTime: 5000,
    refetchInterval: 10000,
  });
}

/**
 * Hook to get all doctor queues (public - no auth)
 */
export function usePublicQueues(params: Omit<GetAllQueuesParams, 'includePatientDetails'> = {}) {
  return useQuery({
    queryKey: ['public-queues', params],
    queryFn: () => queueService.getPublicQueues(params),
    staleTime: 5000,
    refetchInterval: 10000,
  });
}

/**
 * Hook to get a specific doctor's queue (public - no auth)
 */
export function usePublicQueue(
  doctorId: number,
  params: Omit<GetQueueParams, 'doctorId' | 'includePatientDetails'> = {}
) {
  return useQuery({
    queryKey: ['public-queue', doctorId, params],
    queryFn: () => queueService.getPublicQueue(doctorId, params),
    staleTime: 5000,
    refetchInterval: 10000,
  });
}

/**
 * Hook for patient self-booking
 */
export function useBookAppointment() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: BookAppointmentRequest) => queueService.bookAppointment(request),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['queues'] });
      queryClient.invalidateQueries({ queryKey: ['public-queues'] });
      queryClient.invalidateQueries({ queryKey: ['appointments'] });
      toast.success(`Appointment booked! Your token number is ${data.tokenNumber}`);
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || 'Failed to book appointment');
    },
  });
}

/**
 * Hook to start an appointment (doctor)
 */
export function useStartAppointment() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (appointmentId: number) => queueService.startAppointment(appointmentId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['queues'] });
      queryClient.invalidateQueries({ queryKey: ['public-queues'] });
      queryClient.invalidateQueries({ queryKey: ['appointments'] });
      toast.success('Appointment started');
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || 'Failed to start appointment');
    },
  });
}

/**
 * Hook to complete an appointment (doctor)
 */
export function useCompleteAppointment() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (appointmentId: number) => queueService.completeAppointment(appointmentId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['queues'] });
      queryClient.invalidateQueries({ queryKey: ['public-queues'] });
      queryClient.invalidateQueries({ queryKey: ['appointments'] });
      toast.success('Appointment completed');
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || 'Failed to complete appointment');
    },
  });
}

