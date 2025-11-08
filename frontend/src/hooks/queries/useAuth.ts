import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { authService } from '../../services/authService'
import { useAuthStore } from '../../stores/authStore'
import toast from 'react-hot-toast'

// Query keys
export const authKeys = {
  all: ['auth'] as const,
  user: () => [...authKeys.all, 'user'] as const,
  validate: (token: string) => [...authKeys.all, 'validate', token] as const
}

// Get current user query
export const useCurrentUser = () => {
  const { token, isAuthenticated } = useAuthStore()
  
  return useQuery({
    queryKey: authKeys.user(),
    queryFn: authService.getCurrentUser,
    enabled: isAuthenticated && !!token,
    staleTime: 5 * 60 * 1000, // 5 minutes
    retry: false
  })
}

// Login mutation
export const useLogin = () => {
  const { login } = useAuthStore()
  const queryClient = useQueryClient()
  
  return useMutation({
    mutationFn: authService.login,
    onSuccess: (data) => {
      console.log('Login response data:', data)
      login(data.user, data.accessToken, data.availableClinics)
      queryClient.setQueryData(authKeys.user(), data.user)
      toast.success('Login successful!')
    },
    onError: (error: any) => {
      console.error('Login error:', error)
      console.error('Error response:', error.response?.data)
      toast.error(error.response?.data?.message || 'Login failed')
    }
  })
}

// Logout mutation
export const useLogout = () => {
  const { logout } = useAuthStore()
  const queryClient = useQueryClient()
  
  return useMutation({
    mutationFn: authService.logout,
    onSuccess: () => {
      logout()
      queryClient.clear()
      toast.success('Logged out successfully')
    },
    onError: () => {
      // Even if logout fails on server, clear local state
      logout()
      queryClient.clear()
    }
  })
}

// Refresh token mutation
export const useRefreshToken = () => {
  const { setToken } = useAuthStore()
  
  return useMutation({
    mutationFn: authService.refreshToken,
    onSuccess: (data) => {
      setToken(data.accessToken)
    },
    onError: () => {
      // If refresh fails, logout user
      useAuthStore.getState().logout()
    }
  })
}

// Switch clinic mutation
export const useSwitchClinic = () => {
  const { updateUser, selectClinic, availableClinics } = useAuthStore()
  const queryClient = useQueryClient()
  
  return useMutation({
    mutationFn: authService.switchClinic,
    onSuccess: (updatedUser) => {
      updateUser(updatedUser)
      const clinic = availableClinics.find(c => c.id === updatedUser.selectedClinicId)
      if (clinic) {
        selectClinic(clinic)
      }
      queryClient.setQueryData(authKeys.user(), updatedUser)
      toast.success('Clinic switched successfully')
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || 'Failed to switch clinic')
    }
  })
}

// Validate token query
export const useValidateToken = (token: string, enabled = true) => {
  return useQuery({
    queryKey: authKeys.validate(token),
    queryFn: () => authService.validateToken(token),
    enabled: enabled && !!token,
    staleTime: Infinity,
    retry: false
  })
}
