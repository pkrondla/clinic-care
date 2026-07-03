import { create } from 'zustand'
import type { NotificationConfig } from '../types/common'

interface UIState {
  // Layout
  sidebarCollapsed: boolean
  mobileMenuOpen: boolean
  
  // Loading states
  globalLoading: boolean
  
  // Notifications
  notifications: NotificationConfig[]
  
  // Theme
  theme: 'light' | 'dark'
  
  // Modals
  modals: Record<string, boolean>
}

interface UIStore extends UIState {
  // Layout actions
  toggleSidebar: () => void
  setSidebarCollapsed: (collapsed: boolean) => void
  toggleMobileMenu: () => void
  setMobileMenuOpen: (open: boolean) => void
  
  // Loading actions
  setGlobalLoading: (loading: boolean) => void
  
  // Notification actions
  addNotification: (notification: NotificationConfig) => void
  removeNotification: (index: number) => void
  clearNotifications: () => void
  
  // Theme actions
  setTheme: (theme: 'light' | 'dark') => void
  toggleTheme: () => void
  
  // Modal actions
  openModal: (modalId: string) => void
  closeModal: (modalId: string) => void
  toggleModal: (modalId: string) => void
  isModalOpen: (modalId: string) => boolean
}

const initialState: UIState = {
  sidebarCollapsed: false,
  mobileMenuOpen: false,
  globalLoading: false,
  notifications: [],
  theme: 'light',
  modals: {}
}

export const useUIStore = create<UIStore>((set, get) => ({
  ...initialState,

  // Layout actions
  toggleSidebar: () => {
    set(state => ({ sidebarCollapsed: !state.sidebarCollapsed }))
  },

  setSidebarCollapsed: (collapsed) => {
    set({ sidebarCollapsed: collapsed })
  },

  toggleMobileMenu: () => {
    set(state => ({ mobileMenuOpen: !state.mobileMenuOpen }))
  },

  setMobileMenuOpen: (open) => {
    set({ mobileMenuOpen: open })
  },

  // Loading actions
  setGlobalLoading: (loading) => {
    set({ globalLoading: loading })
  },

  // Notification actions
  addNotification: (notification) => {
    set(state => ({
      notifications: [...state.notifications, notification]
    }))
  },

  removeNotification: (index) => {
    set(state => ({
      notifications: state.notifications.filter((_, i) => i !== index)
    }))
  },

  clearNotifications: () => {
    set({ notifications: [] })
  },

  // Theme actions
  setTheme: (theme) => {
    set({ theme })
    document.documentElement.setAttribute('data-theme', theme)
  },

  toggleTheme: () => {
    const { theme, setTheme } = get()
    setTheme(theme === 'light' ? 'dark' : 'light')
  },

  // Modal actions
  openModal: (modalId) => {
    set(state => ({
      modals: { ...state.modals, [modalId]: true }
    }))
  },

  closeModal: (modalId) => {
    set(state => ({
      modals: { ...state.modals, [modalId]: false }
    }))
  },

  toggleModal: (modalId) => {
    const { modals } = get()
    set(state => ({
      modals: { ...state.modals, [modalId]: !modals[modalId] }
    }))
  },

  isModalOpen: (modalId) => {
    const { modals } = get()
    return !!modals[modalId]
  }
}))

// Selectors
export const useSidebar = () => {
  const collapsed = useUIStore(state => state.sidebarCollapsed)
  const toggle = useUIStore(state => state.toggleSidebar)
  const setCollapsed = useUIStore(state => state.setSidebarCollapsed)
  
  return { collapsed, toggle, setCollapsed }
}

export const useMobileMenu = () => {
  const open = useUIStore(state => state.mobileMenuOpen)
  const toggle = useUIStore(state => state.toggleMobileMenu)
  const setOpen = useUIStore(state => state.setMobileMenuOpen)
  
  return { open, toggle, setOpen }
}

export const useTheme = () => {
  const theme = useUIStore(state => state.theme)
  const setTheme = useUIStore(state => state.setTheme)
  const toggleTheme = useUIStore(state => state.toggleTheme)
  
  return { theme, setTheme, toggle: toggleTheme }
}

export const useNotifications = () => {
  const notifications = useUIStore(state => state.notifications)
  const add = useUIStore(state => state.addNotification)
  const remove = useUIStore(state => state.removeNotification)
  const clear = useUIStore(state => state.clearNotifications)
  
  return { notifications, add, remove, clear }
}

export const useModal = (modalId: string) => {
  const isOpen = useUIStore(state => state.isModalOpen(modalId))
  const open = useUIStore(state => state.openModal)
  const close = useUIStore(state => state.closeModal)
  const toggle = useUIStore(state => state.toggleModal)
  
  return { 
    isOpen, 
    open: () => open(modalId), 
    close: () => close(modalId), 
    toggle: () => toggle(modalId) 
  }
}
