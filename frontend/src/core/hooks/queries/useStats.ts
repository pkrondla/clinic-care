import { useQuery } from '@tanstack/react-query'
import { globalApi } from '@core/services/globalApi'

export interface SystemStats {
  totalOrganizations: number
  totalClinics: number
  activeSubscriptions: number
  totalUsers: number
}

export function useSystemStats() {
  return useQuery({
    queryKey: ['systemStats'],
    queryFn: async () => {
      const response = await globalApi.stats.getSystemStats()
      return response.data
    }
  })
}