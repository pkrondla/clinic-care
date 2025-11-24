import { useQuery } from '@tanstack/react-query'
import { clinicService } from '../../services/clinicService'
import type { Clinic } from '../../services/clinicService'

export const clinicKeys = {
  all: ['clinics'] as const,
  lists: () => [...clinicKeys.all, 'list'] as const,
  list: () => [...clinicKeys.lists()] as const,
  details: () => [...clinicKeys.all, 'detail'] as const,
  detail: (id: number) => [...clinicKeys.details(), id] as const,
}

export const useClinics = () => {
  return useQuery({
    queryKey: clinicKeys.list(),
    queryFn: () => clinicService.getAll(),
    staleTime: 5 * 60 * 1000, // 5 minutes
  })
}

