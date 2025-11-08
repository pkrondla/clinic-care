import { api } from './apiClient'
import type { 
  LoginRequest, 
  LoginResponse, 
  RefreshTokenRequest, 
  RefreshTokenResponse,
  User 
} from '../types/auth'

export const authService = {
  // Login
  login: async (credentials: LoginRequest): Promise<LoginResponse> => {
    const response = await api.post<{message: string, data: LoginResponse}>('/Auth/login', credentials)
    return response.data
  },

  // Logout
  logout: async (): Promise<void> => {
    return api.post('/Auth/logout')
  },

  // Refresh token
  refreshToken: async (request: RefreshTokenRequest): Promise<RefreshTokenResponse> => {
    return api.post<RefreshTokenResponse>('/Auth/refresh', request)
  },

  // Get current user info
  getCurrentUser: async (): Promise<User> => {
    return api.get<User>('/Auth/me')
  },

  // Validate token
  validateToken: async (token: string): Promise<boolean> => {
    try {
      await api.post('/Auth/validate', { token })
      return true
    } catch {
      return false
    }
  },

  // Switch clinic (update user context)
  switchClinic: async (clinicId: number): Promise<User> => {
    return api.post<User>('/Auth/switch-clinic', { clinicId })
  }
}
