import { create } from 'zustand'
import { persist } from 'zustand/middleware'
import type { User, Clinic, AuthState } from '../types/auth'
import { UserRole } from '../types/auth'

interface AuthStore extends AuthState {
  // Actions
  login: (user: User, token: string, availableClinics?: Clinic[]) => void
  logout: () => void
  selectClinic: (clinic: Clinic) => void
  updateUser: (user: Partial<User>) => void
  setToken: (token: string) => void
  clearAuth: () => void
}

const createInitialState = (isGlobal: boolean): AuthState => ({
  user: null,
  token: null,
  selectedClinic: null,
  availableClinics: [],
  isAuthenticated: false,
  isGlobalSystem: isGlobal
})

// Global auth store for system administrators
export const useGlobalAuthStore = create<AuthStore>()(
  persist(
    (set, get) => ({
      ...createInitialState(true),

      login: (user, token) => {
        set({
          user,
          token,
          isAuthenticated: true,
          isGlobalSystem: true
        })
      },

      logout: () => {
        set(createInitialState(true))
        localStorage.removeItem('global-auth-storage')
      },

      selectClinic: () => {}, // Not used in global system
      updateUser: (userUpdate) => {
        const { user } = get()
        if (user) {
          set({ user: { ...user, ...userUpdate } })
        }
      },
      setToken: (token) => set({ token }),
      clearAuth: () => set(createInitialState(true))
    }),
    {
      name: 'global-auth-storage',
      partialize: (state) => ({
        user: state.user,
        token: state.token,
        isAuthenticated: state.isAuthenticated,
        isGlobalSystem: true
      })
    }
  )
)

// Tenant auth store for clinic staff
export const useTenantAuthStore = create<AuthStore>()(
  persist(
    (set, get) => ({
      ...createInitialState(false),

      login: (user, token, availableClinics = []) => {
        // Determine selected clinic based on business rules:
        // 1. If user has only one clinic, auto-select it
        // 2. If user has multiple clinics, use saved selectedClinicId if valid
        // 3. Otherwise, leave null (user must select)
        let selectedClinic: Clinic | null = null
        
        if (availableClinics.length === 1) {
          // Auto-select if only one clinic
          selectedClinic = availableClinics[0]
          if (user) {
            user.selectedClinicId = selectedClinic.id
            user.selectedClinicName = selectedClinic.name
          }
        } else if (user?.selectedClinicId) {
          // Use saved clinic if it's still available
          selectedClinic = availableClinics.find(c => c.id === user.selectedClinicId) || null
        }
        
        set({
          user,
          token,
          availableClinics,
          isAuthenticated: true,
          isGlobalSystem: false,
          selectedClinic
        })
      },

      logout: () => {
        set(createInitialState(false))
        localStorage.removeItem('tenant-auth-storage')
      },

      selectClinic: (clinic) => {
        const { user } = get()
        if (user) {
          set({
            selectedClinic: clinic,
            user: {
              ...user,
              selectedClinicId: clinic.id,
              selectedClinicName: clinic.name
            }
          })
          
          // Persist to backend (fire and forget)
          // This will be handled by the component calling the API
        }
      },

      updateUser: (userUpdate) => {
        const { user } = get()
        if (user) {
          set({ user: { ...user, ...userUpdate } })
        }
      },

      setToken: (token) => set({ token }),
      clearAuth: () => set(createInitialState(false))
    }),
    {
      name: 'tenant-auth-storage',
      partialize: (state) => ({
        user: state.user,
        token: state.token,
        selectedClinic: state.selectedClinic,
        availableClinics: state.availableClinics,
        isAuthenticated: state.isAuthenticated,
        isGlobalSystem: false
      })
    }
  )
)

// Custom hook to determine which auth store to use
export const useAuth = () => {
  // Always subscribe to both stores so changes trigger re-renders
  const globalAuth = useGlobalAuthStore()
  const tenantAuth = useTenantAuthStore()
  
  // Check for tenant query parameter or path (for local development)
  const urlParams = new URLSearchParams(window.location.search)
  const tenantParam = urlParams.get('tenant')
  const isTenantPath = window.location.pathname.startsWith('/tenant')
  
  // Priority 1: If tenant user is authenticated, use tenant store
  if (tenantAuth.isAuthenticated && !tenantAuth.isGlobalSystem) {
    return tenantAuth
  }
  
  // Priority 2: If global user is authenticated, use global store
  if (globalAuth.isAuthenticated && globalAuth.isGlobalSystem) {
    return globalAuth
  }
  
  // Priority 3: Determine based on domain/params (for unauthenticated users)
  const hostname = window.location.hostname
  const parts = hostname.split('.')
  const isGlobalDomain = (hostname === 'localhost' || 
                         hostname === '127.0.0.1' ||
                         parts.length === 2 || // e.g., domain.com
                         (parts.length === 3 && parts[0] === 'www')) && // e.g., www.domain.com
                         !tenantParam && // Not explicitly requesting tenant
                         !isTenantPath // Not using tenant path
  
  return isGlobalDomain ? globalAuth : tenantAuth
}

// Selectors for easier access
export const useUser = () => useAuth().user
export const useToken = () => useAuth().token
export const useSelectedClinic = () => useAuth().selectedClinic
export const useAvailableClinics = () => useAuth().availableClinics
export const useIsAuthenticated = () => useAuth().isAuthenticated

// Action hooks
export const useLogin = () => {
  // Return a function that handles login and returns a result
  return async (_email: string, _password: string, _context: 'global' | 'tenant') => {
    try {
      // This would normally call an API service
      // For now, return a success structure that the login page expects
      return {
        success: false,
        error: 'Login functionality needs to be implemented with API service'
      }
    } catch (error) {
      return {
        success: false,
        error: error instanceof Error ? error.message : 'Login failed'
      }
    }
  }
}

export const useLogout = () => useAuth().logout
export const useSelectClinic = () => useAuth().selectClinic

// Type helpers
export const isGlobalUser = (user: User): boolean => {
  return user.role === UserRole.SuperAdmin || user.role === UserRole.SystemAdmin
}

export const isTenantUser = (user: User): boolean => {
  return [UserRole.Doctor, UserRole.Reception, UserRole.Pharmacy].includes(user.role)
}
