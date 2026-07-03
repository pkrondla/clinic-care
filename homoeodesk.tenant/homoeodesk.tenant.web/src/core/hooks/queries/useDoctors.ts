import { useQuery } from '@tanstack/react-query';
import doctorService, { GetDoctorsParams, Doctor } from '@core/services/doctorService';

export const doctorKeys = {
  all: ['doctors'] as const,
  lists: () => [...doctorKeys.all, 'list'] as const,
  list: (params?: GetDoctorsParams) => [...doctorKeys.lists(), params] as const,
  details: () => [...doctorKeys.all, 'detail'] as const,
  detail: (id: number) => [...doctorKeys.details(), id] as const,
};

/**
 * Hook to get list of doctors
 */
export function useDoctors(params: GetDoctorsParams = {}) {
  return useQuery({
    queryKey: doctorKeys.list(params),
    queryFn: () => doctorService.getDoctors(params),
    staleTime: 5 * 60 * 1000, // 5 minutes
  });
}

