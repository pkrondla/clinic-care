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
        set({
          user,
          token,
          availableClinics,
          isAuthenticated: true,
          isGlobalSystem: false,
          selectedClinic: user?.selectedClinicId 
            ? availableClinics.find(c => c.id === user.selectedClinicId) || null 
            : null
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
  const globalAuth = useGlobalAuthStore()
  const tenantAuth = useTenantAuthStore()
  const isGlobalDomain = !window.location.hostname.includes('.')
  
  return isGlobalDomain ? globalAuth : tenantAuth
}

// Selectors for easier access
export const useUser = () => useAuth().user
export const useToken = () => useAuth().token
export const useSelectedClinic = () => useAuth().selectedClinic
export const useAvailableClinics = () => useAuth().availableClinics
export const useIsAuthenticated = () => useAuth().isAuthenticated

// Type helpers
export const isGlobalUser = (user: User): boolean => {
  return user.role === UserRole.SuperAdmin || user.role === UserRole.SystemAdmin
}

export const isTenantUser = (user: User): boolean => {
  return [UserRole.Doctor, UserRole.Reception, UserRole.Pharmacy].includes(user.role)
}
