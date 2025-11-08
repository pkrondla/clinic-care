import { UserRole } from './auth'

export interface ApiResponse<T = any> {
  data?: T
  message?: string
  errors?: string[]
  success: boolean
}

export interface PaginatedResponse<T> {
  data: T[]
  total: number
  page: number
  pageSize: number
  hasNext: boolean
  hasPrevious: boolean
}

export interface SelectOption {
  label: string
  value: string | number
  disabled?: boolean
}

export interface TableColumn {
  key: string
  title: string
  dataIndex?: string
  render?: (value: any, record: any) => React.ReactNode
  sorter?: boolean
  width?: number | string
  align?: 'left' | 'center' | 'right'
  fixed?: 'left' | 'right'
}

export interface FormError {
  field: string
  message: string
}

export interface NotificationConfig {
  type: 'success' | 'error' | 'warning' | 'info'
  title: string
  message?: string
  duration?: number
}

export interface MenuItem {
  key: string
  label: string
  icon?: React.ReactNode
  path?: string
  children?: MenuItem[]
  roles?: UserRole[]
}

export type LoadingState = 'idle' | 'loading' | 'success' | 'error'

export interface BaseEntity {
  id: number
  createdAt: string
  updatedAt: string
  isActive: boolean
}

// Generic hook return type for API operations
export interface MutationResult<T = any> {
  mutate: (data: any) => Promise<T>
  isLoading: boolean
  error: Error | null
  isSuccess: boolean
  reset: () => void
}

// Generic filter interface
export interface BaseFilter {
  page?: number
  pageSize?: number
  sortBy?: string
  sortOrder?: 'asc' | 'desc'
  search?: string
}

// Date range filter
export interface DateRangeFilter {
  startDate?: string
  endDate?: string
}