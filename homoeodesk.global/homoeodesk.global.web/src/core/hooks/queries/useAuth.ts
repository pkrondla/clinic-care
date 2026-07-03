import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { authService } from '../../services/authService'
import { useAuth } from '../../stores/authStore'
import toast from 'react-hot-toast'
import type { ApiResponse } from '../../types/common'
import type {
  LoginResponse,
  User,
  LoginRequest,
  RefreshTokenRequest,
  RefreshTokenResponse
} from '../../types/auth'

export const authKeys = {
  all: ['auth'] as const,
  user: () => [...authKeys.all, 'user'] as const,
  validate: (token: string) => [...authKeys.all, 'validate', token] as const
}

export const useCurrentUser = () => {
  const auth = useAuth()

  return useQuery<User, { response: { data: ApiResponse } }>({
    queryKey: authKeys.user(),
    queryFn: authService.getCurrentUser,
    enabled: auth.isAuthenticated && !!auth.token,
    staleTime: 5 * 60 * 1000,
    retry: false
  })
}

export const useLogin = () => {
  const auth = useAuth()
  const queryClient = useQueryClient()

  return useMutation<LoginResponse, { response: { data: ApiResponse } }, LoginRequest>({
    mutationFn: authService.login,
    onSuccess: (data) => {
      auth.login(data.user, data.accessToken)
      queryClient.setQueryData(authKeys.user(), data.user)
      toast.success('Login successful!')
    },
    onError: (error: { response: { data: ApiResponse } }) => {
      toast.error(error.response?.data?.message || 'Login failed')
    }
  })
}

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
      auth.logout()
      queryClient.clear()
    }
  })
}

export const useRefreshToken = () => {
  const auth = useAuth()

  return useMutation<RefreshTokenResponse, { response: { data: ApiResponse } }, RefreshTokenRequest>({
    mutationFn: authService.refreshToken,
    onSuccess: (data) => {
      auth.setToken(data.accessToken)
    },
    onError: () => {
      auth.logout()
    }
  })
}

export const useValidateToken = (token: string, enabled = true) => {
  return useQuery<boolean, { response: { data: ApiResponse } }>({
    queryKey: authKeys.validate(token),
    queryFn: () => authService.validateToken(token),
    enabled: enabled && !!token,
    staleTime: Infinity,
    retry: false
  })
}
