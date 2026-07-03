import { create } from 'zustand'
import { persist } from 'zustand/middleware'
import type { User, AuthState } from '../types/auth'
import { UserRole } from '../types/auth'

interface AuthStore extends AuthState {
  login: (user: User, token: string) => void
  logout: () => void
  updateUser: (user: Partial<User>) => void
  setToken: (token: string) => void
  clearAuth: () => void
}

const createInitialState = (): AuthState => ({
  user: null,
  token: null,
  isAuthenticated: false,
  isGlobalSystem: true
})

export const useGlobalAuthStore = create<AuthStore>()(
  persist(
    (set, get) => ({
      ...createInitialState(),

      login: (user, token) => {
        set({
          user,
          token,
          isAuthenticated: true,
          isGlobalSystem: true
        })
      },

      logout: () => {
        set(createInitialState())
        localStorage.removeItem('homoeodesk-global-auth')
      },

      updateUser: (userUpdate) => {
        const { user } = get()
        if (user) {
          set({ user: { ...user, ...userUpdate } })
        }
      },
      setToken: (token) => set({ token }),
      clearAuth: () => set(createInitialState())
    }),
    {
      name: 'homoeodesk-global-auth',
      partialize: (state) => ({
        user: state.user,
        token: state.token,
        isAuthenticated: state.isAuthenticated,
        isGlobalSystem: true
      })
    }
  )
)

export const useAuth = () => useGlobalAuthStore()

export const useUser = () => useAuth().user
export const useToken = () => useAuth().token
export const useIsAuthenticated = () => useAuth().isAuthenticated

export const useLogout = () => useAuth().logout

export const isGlobalUser = (user: User): boolean => {
  return user.role === UserRole.SuperAdmin || user.role === UserRole.SystemAdmin
}
