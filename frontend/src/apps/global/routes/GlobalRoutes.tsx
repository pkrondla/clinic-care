import { Routes, Route, Navigate } from 'react-router-dom'
import { GlobalLayout } from '../components/layout/GlobalLayout'
import { OrganizationsPage } from '../pages/organizations/OrganizationsPage'
import { useAuth } from '../../../stores/authStore'
import { UserRole } from '../../../types/auth'

// Protected Global Route Component
const ProtectedGlobalRoute = ({ children }: { children: React.ReactNode }) => {
  const { isAuthenticated, user } = useAuth()
  
  if (!isAuthenticated) {
    return <Navigate to="/login" replace />
  }

  // Only SuperAdmin can access global routes
  if (user?.role !== UserRole.SuperAdmin) {
    return <Navigate to="/" replace />
  }
  
  return <>{children}</>
}

export const GlobalRoutes = () => {
  return (
    <Routes>
      <Route
        path="/organizations"
        element={
          <ProtectedGlobalRoute>
            <GlobalLayout>
              <OrganizationsPage />
            </GlobalLayout>
          </ProtectedGlobalRoute>
        }
      />

      {/* Add more global routes as needed */}
      <Route path="*" element={<Navigate to="/organizations" replace />} />
    </Routes>
  )
}