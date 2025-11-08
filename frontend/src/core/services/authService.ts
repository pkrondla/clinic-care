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
    const response = await api.post<LoginResponse>('/Auth/login', credentials)
    return response.data as LoginResponse
  },

  // Logout
  logout: async (): Promise<void> => {
    await api.post('/Auth/logout')
  },

  // Refresh token
  refreshToken: async (request: RefreshTokenRequest): Promise<RefreshTokenResponse> => {
    const response = await api.post<RefreshTokenResponse>('/Auth/refresh', request)
    return response.data as RefreshTokenResponse
  },

  // Get current user info
  getCurrentUser: async (): Promise<User> => {
    const response = await api.get<User>('/Auth/me')
    return response.data as User
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
    const response = await api.post<User>('/Auth/switch-clinic', { clinicId })
    return response.data as User
  }
}
