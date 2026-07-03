import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import doctorAvailabilityService, {
  type GetDoctorAvailabilityParams,
  type CreateDoctorAvailabilityRequest,
  type UpdateDoctorAvailabilityRequest,
} from '@core/services/doctorAvailabilityService';
import { message } from 'antd';

export function useDoctorAvailability(params: GetDoctorAvailabilityParams = {}) {
  return useQuery({
    queryKey: ['doctor-availability', params],
    queryFn: () => doctorAvailabilityService.getAll(params),
  });
}

export function useCreateDoctorAvailability() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (request: CreateDoctorAvailabilityRequest) =>
      doctorAvailabilityService.create(request),
    onSuccess: () => {
      message.success('Doctor availability created successfully');
      queryClient.invalidateQueries({ queryKey: ['doctor-availability'] });
    },
    onError: (error: any) => {
      message.error(error?.response?.data?.errors?.[0] || 'Failed to create doctor availability');
    },
  });
}

export function useUpdateDoctorAvailability() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, ...request }: UpdateDoctorAvailabilityRequest) =>
      doctorAvailabilityService.update(id, request),
    onSuccess: () => {
      message.success('Doctor availability updated successfully');
      queryClient.invalidateQueries({ queryKey: ['doctor-availability'] });
    },
    onError: (error: any) => {
      message.error(error?.response?.data?.errors?.[0] || 'Failed to update doctor availability');
    },
  });
}

export function useDeleteDoctorAvailability() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => doctorAvailabilityService.delete(id),
    onSuccess: () => {
      message.success('Doctor availability deleted successfully');
      queryClient.invalidateQueries({ queryKey: ['doctor-availability'] });
    },
    onError: (error: any) => {
      message.error(error?.response?.data?.errors?.[0] || 'Failed to delete doctor availability');
    },
  });
}

