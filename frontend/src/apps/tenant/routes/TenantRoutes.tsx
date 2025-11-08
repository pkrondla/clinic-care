import { Routes, Route, Navigate } from 'react-router-dom'
import { AppLayout } from '../components/layout/AppLayout'
import { DashboardPage } from '../pages/dashboard/DashboardPage'
import { PatientsListPage } from '../pages/patients/PatientsListPage'
import { PatientDetailPage } from '../pages/patients/PatientDetailPage'
import { PatientFormPage } from '../pages/patients/PatientFormPage'
import { useAuth } from '../../../stores/authStore'

// Protected Tenant Route Component
const ProtectedTenantRoute = ({ children }: { children: React.ReactNode }) => {
  const { isAuthenticated, user } = useAuth()
  
  if (!isAuthenticated) {
    return <Navigate to="/login" replace />
  }

  // Global admin should be redirected to global app
  if (user?.role === 'SuperAdmin') {
    return <Navigate to="/organizations" replace />
  }
  
  return <>{children}</>
}

export const TenantRoutes = () => {
  return (
    <Routes>
      {/* Dashboard */}
      <Route
        path="/"
        element={
          <ProtectedTenantRoute>
            <AppLayout>
              <DashboardPage />
            </AppLayout>
          </ProtectedTenantRoute>
        }
      />

      <Route
        path="/dashboard"
        element={
          <ProtectedTenantRoute>
            <AppLayout>
              <DashboardPage />
            </AppLayout>
          </ProtectedTenantRoute>
        }
      />

      {/* Patient Management */}
      <Route
        path="/patients"
        element={
          <ProtectedTenantRoute>
            <AppLayout>
              <PatientsListPage />
            </AppLayout>
          </ProtectedTenantRoute>
        }
      />

      <Route
        path="/patients/new"
        element={
          <ProtectedTenantRoute>
            <AppLayout>
              <PatientFormPage />
            </AppLayout>
          </ProtectedTenantRoute>
        }
      />

      <Route
        path="/patients/:id"
        element={
          <ProtectedTenantRoute>
            <AppLayout>
              <PatientDetailPage />
            </AppLayout>
          </ProtectedTenantRoute>
        }
      />

      <Route
        path="/patients/:id/edit"
        element={
          <ProtectedTenantRoute>
            <AppLayout>
              <PatientFormPage />
            </AppLayout>
          </ProtectedTenantRoute>
        }
      />

      {/* TODO: Add routes for:
       * - Appointments
       * - Queue Management
       * - Prescriptions
       * - Inventory
       * - Reports
       */}

      {/* Catch all - redirect to dashboard */}
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  )
}