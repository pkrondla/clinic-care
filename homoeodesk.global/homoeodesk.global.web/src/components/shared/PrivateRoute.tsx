import { Navigate } from 'react-router-dom'
import { useAuth } from '@core/stores/authStore'
import type { UserRole } from '@core/types/auth'

interface PrivateRouteProps {
  children: React.ReactNode
  requiredRoles?: UserRole[]
  redirectTo?: string
}

export function PrivateRoute({ children, requiredRoles, redirectTo = '/login' }: PrivateRouteProps) {
  const { user, isAuthenticated } = useAuth()

  if (!isAuthenticated) {
    return <Navigate to={redirectTo} replace />
  }

  if (requiredRoles && user && !requiredRoles.includes(user.role)) {
    return <Navigate to="/unauthorized" replace />
  }

  return <>{children}</>
}