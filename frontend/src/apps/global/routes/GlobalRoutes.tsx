import { Routes, Route, Navigate } from 'react-router-dom'
import { GlobalDashboardPage as DashboardPage } from '../pages/dashboard/GlobalDashboardPage'
import { OrganizationsPage } from '../pages/organizations/OrganizationsPage'
import { GlobalMedicinesPage } from '../pages/medicines/GlobalMedicinesPage'
import { GlobalLoginPage } from '../pages/auth/GlobalLoginPage'
import { GlobalLayout } from '../components/layout/GlobalLayout'
import { useAuth } from '@core/stores/authStore'
import { UserRole } from '@core/types/auth'

// Protected Global Route Component
const ProtectedGlobalRoute = ({ children }: { children: React.ReactNode }) => {
  const { isAuthenticated, user } = useAuth()

  console.log('ProtectedGlobalRoute: Checking auth', { isAuthenticated, user: user?.email, role: user?.role })

  if (!isAuthenticated) {
    console.log('ProtectedGlobalRoute: Not authenticated, redirecting to /login')
    return <Navigate to="/login" replace />
  }

  // Only super admin can access global routes
  const isSuperAdmin = user?.role === UserRole.SuperAdmin
  
  if (!isSuperAdmin) {
    console.log('ProtectedGlobalRoute: Not SuperAdmin', { 
      role: user?.role, 
      roleType: typeof user?.role,
      expected: UserRole.SuperAdmin 
    })
    return <Navigate to="/" replace />
  }

  console.log('ProtectedGlobalRoute: Access granted, rendering children')
  return <GlobalLayout>{children}</GlobalLayout>
}

export const GlobalRoutes = () => {
  return (
    <Routes>
      {/* Login - Public route */}
      <Route
        path="/login"
        element={<GlobalLoginPage />}
      />

      {/* Dashboard */}
      <Route
        path="/"
        element={
          <ProtectedGlobalRoute>
            <DashboardPage />
          </ProtectedGlobalRoute>
        }
      />

      <Route
        path="/dashboard"
        element={
          <ProtectedGlobalRoute>
            <DashboardPage />
          </ProtectedGlobalRoute>
        }
      />

      {/* Organizations */}
      <Route
        path="/organizations"
        element={
          <ProtectedGlobalRoute>
            <OrganizationsPage />
          </ProtectedGlobalRoute>
        }
      />

      {/* Global Medicines */}
      <Route
        path="/medicines"
        element={
          <ProtectedGlobalRoute>
            <GlobalMedicinesPage />
          </ProtectedGlobalRoute>
        }
      />

      {/* Catch all - redirect to dashboard */}
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  )
}
