import { create } from 'zustand'
import { persist } from 'zustand/middleware'
import type { User, Branch, AuthState } from '../types/auth'
import { UserRole } from '../types/auth'

interface AuthStore extends AuthState {
  login: (user: User, token: string, authorizedBranches?: Branch[]) => void
  logout: () => void
  selectBranch: (branch: Branch) => void
  updateUser: (user: Partial<User>) => void
  setToken: (token: string) => void
  clearAuth: () => void
}

const createInitialState = (): AuthState => ({
  user: null,
  token: null,
  activeBranch: null,
  authorizedBranches: [],
  isAuthenticated: false,
  isGlobalSystem: false
})

export const useAuthStore = create<AuthStore>()(
  persist(
    (set, get) => ({
      ...createInitialState(),

      login: (user, token, authorizedBranches = []) => {
        let activeBranch: Branch | null = null

        if (authorizedBranches.length === 1) {
          activeBranch = authorizedBranches[0]
          if (user) {
            user.selectedBranchId = activeBranch.id
            user.selectedBranchName = activeBranch.name
          }
        } else if (user?.selectedBranchId) {
          activeBranch = authorizedBranches.find(c => c.id === user.selectedBranchId) || null
        }

        set({
          user,
          token,
          authorizedBranches,
          isAuthenticated: true,
          isGlobalSystem: false,
          activeBranch
        })
      },

      logout: () => {
        set(createInitialState())
        localStorage.removeItem('homoeodesk-tenant-auth')
      },

      selectBranch: (branch) => {
        const { user } = get()
        if (user) {
          set({
            activeBranch: branch,
            user: {
              ...user,
              selectedBranchId: branch.id,
              selectedBranchName: branch.name
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
      clearAuth: () => set(createInitialState())
    }),
    {
      name: 'homoeodesk-tenant-auth',
      partialize: (state) => ({
        user: state.user,
        token: state.token,
        activeBranch: state.activeBranch,
        authorizedBranches: state.authorizedBranches,
        isAuthenticated: state.isAuthenticated,
        isGlobalSystem: false
      })
    }
  )
)

export const useAuth = () => useAuthStore()

export const useUser = () => useAuth().user
export const useToken = () => useAuth().token
export const useIsAuthenticated = () => useAuth().isAuthenticated

export const useActiveBranch = () => useAuth().activeBranch
export const useAuthorizedBranches = () => useAuth().authorizedBranches
export const useSelectBranch = () => useAuth().selectBranch

/** @deprecated Use useActiveBranch */
export const useSelectedBranch = () => useAuth().activeBranch
/** @deprecated Use useAuthorizedBranches */
export const useAvailableBranches = () => useAuth().authorizedBranches
/** @deprecated Use useSelectBranch */
export const useSelectClinic = () => useAuth().selectBranch

/** @deprecated Use useSelectedBranch */
export const useSelectedClinic = () => useAuth().activeBranch
/** @deprecated Use useAvailableBranches */
export const useAvailableClinics = () => useAuth().authorizedBranches

export const useLogout = () => useAuth().logout

/** @deprecated Use useAuthStore */
export const useTenantAuthStore = useAuthStore

export const isTenantUser = (user: User): boolean => {
  return [UserRole.Doctor, UserRole.Reception, UserRole.Pharmacy].includes(user.role)
}
