import { create } from 'zustand'
import { persist } from 'zustand/middleware'
import type { User, Clinic, AuthState } from '../types/auth'

interface AuthStore extends AuthState {
  // Actions
  login: (user: User, token: string, availableClinics: Clinic[]) => void
  logout: () => void
  selectClinic: (clinic: Clinic) => void
  updateUser: (user: Partial<User>) => void
  setToken: (token: string) => void
  clearAuth: () => void
}

const initialState: AuthState = {
  user: null,
  token: null,
  selectedClinic: null,
  availableClinics: [],
  isAuthenticated: false
}

export const useAuthStore = create<AuthStore>()(
  persist(
    (set, get) => ({
      ...initialState,

      login: (user, token, availableClinics) => {
        console.log('Auth store login called with:', { user, token, availableClinics })
        set({
          user,
          token,
          availableClinics,
          isAuthenticated: true,
          selectedClinic: user?.selectedClinicId 
            ? availableClinics.find(c => c.id === user.selectedClinicId) || null 
            : null
        })
      },

      logout: () => {
        set(initialState)
        // Clear persisted data
        localStorage.removeItem('auth-storage')
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
          set({
            user: { ...user, ...userUpdate }
          })
        }
      },

      setToken: (token) => {
        set({ token })
      },

      clearAuth: () => {
        set(initialState)
      }
    }),
    {
      name: 'auth-storage',
      partialize: (state) => ({
        user: state.user,
        token: state.token,
        selectedClinic: state.selectedClinic,
        availableClinics: state.availableClinics,
        isAuthenticated: state.isAuthenticated
      })
    }
  )
)

// Selectors for easier access
export const useAuth = () => useAuthStore()
export const useUser = () => useAuthStore(state => state.user)
export const useToken = () => useAuthStore(state => state.token)
export const useSelectedClinic = () => useAuthStore(state => state.selectedClinic)
export const useAvailableClinics = () => useAuthStore(state => state.availableClinics)
export const useIsAuthenticated = () => useAuthStore(state => state.isAuthenticated)
