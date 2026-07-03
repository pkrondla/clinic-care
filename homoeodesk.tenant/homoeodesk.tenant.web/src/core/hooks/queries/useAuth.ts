import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { authService } from '../../services/authService'
import { useAuth, useAuthorizedBranches } from '../../stores/authStore'
import toast from 'react-hot-toast'
import type { ApiResponse } from '../../types/common'
import type {
  Clinic,
  LoginResponse,
  User,
  LoginRequest,
  RefreshTokenRequest,
  RefreshTokenResponse
} from '../../types/auth'

// Query keys
export const authKeys = {
  all: ['auth'] as const,
  user: () => [...authKeys.all, 'user'] as const,
  validate: (token: string) => [...authKeys.all, 'validate', token] as const
}

// Get current user query
export const useCurrentUser = () => {
  const auth = useAuth()
  
  return useQuery<User, { response: { data: ApiResponse } }>({
    queryKey: authKeys.user(),
    queryFn: authService.getCurrentUser,
    enabled: auth.isAuthenticated && !!auth.token,
    staleTime: 5 * 60 * 1000, // 5 minutes
    retry: false
  })
}

// Login mutation
export const useLogin = () => {
  const auth = useAuth()
  const queryClient = useQueryClient()
  
  return useMutation<LoginResponse, { response: { data: ApiResponse } }, LoginRequest>({
    mutationFn: authService.login,
    onSuccess: (data) => {
      console.log('Login response data:', data)
      auth.login(data.user, data.accessToken, data.availableBranches)
      queryClient.setQueryData(authKeys.user(), data.user)
      toast.success('Login successful!')
    },
    onError: (error: { response: { data: ApiResponse } }) => {
      console.error('Login error:', error)
      console.error('Error response:', error.response?.data)
      toast.error(error.response?.data?.message || 'Login failed')
    }
  })
}

// Logout mutation
export const useLogout = () => {
  const auth = useAuth()
  const queryClient = useQueryClient()
  
  return useMutation<void, { response: { data: ApiResponse } }>({
    mutationFn: authService.logout,
    onSuccess: () => {
      auth.logout()
      queryClient.clear()
      toast.success('Logged out successfully')
    },
    onError: () => {
      // Even if logout fails on server, clear local state
      auth.logout()
      queryClient.clear()
    }
  })
}

// Refresh token mutation
export const useRefreshToken = () => {
  const auth = useAuth()
  
  return useMutation<RefreshTokenResponse, { response: { data: ApiResponse } }, RefreshTokenRequest>({
    mutationFn: authService.refreshToken,
    onSuccess: (data) => {
      auth.setToken(data.accessToken)
    },
    onError: () => {
      // If refresh fails, logout user
      auth.logout()
    }
  })
}

// Switch clinic mutation
export const useSwitchClinic = () => {
  const auth = useAuth()
  const authorizedBranches = useAuthorizedBranches()
  const queryClient = useQueryClient()
  
  return useMutation<User, { response: { data: ApiResponse } }, number>({
    mutationFn: authService.switchClinic,
    onSuccess: (updatedUser) => {
      auth.updateUser(updatedUser)
      const branch = authorizedBranches.find((c) => c.id === updatedUser.selectedBranchId)
      if (branch) {
        auth.selectBranch(branch)
      }
      queryClient.setQueryData(authKeys.user(), updatedUser)
      toast.success('Branch switched successfully')
    },
    onError: (error: { response: { data: ApiResponse } }) => {
      toast.error(error.response?.data?.message || 'Failed to switch branch')
    }
  })
}

// Validate token query
export const useValidateToken = (token: string, enabled = true) => {
  return useQuery<boolean, { response: { data: ApiResponse } }>({
    queryKey: authKeys.validate(token),
    queryFn: () => authService.validateToken(token),
    enabled: enabled && !!token,
    staleTime: Infinity,
    retry: false
  })
}
