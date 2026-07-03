import { useQuery } from '@tanstack/react-query'
import { branchService } from '../../services/branchService'
import type { Branch } from '../../services/branchService'

export const branchKeys = {
  all: ['branches'] as const,
  lists: () => [...branchKeys.all, 'list'] as const,
  list: () => [...branchKeys.lists()] as const,
  details: () => [...branchKeys.all, 'detail'] as const,
  detail: (id: number) => [...branchKeys.details(), id] as const,
}

export const useBranches = () => {
  return useQuery<Branch[]>({
    queryKey: branchKeys.list(),
    queryFn: () => branchService.getAll(),
    staleTime: 5 * 60 * 1000,
  })
}

/** @deprecated Use useBranches */
export const useClinics = useBranches
