import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom'
import { Toaster } from 'react-hot-toast'
import { QueryProvider } from './providers/QueryProvider'
import { AntdProvider } from './providers/AntdProvider'
import { AppLayout } from './components/layout/AppLayout'
import { LoginPage } from './pages/auth/LoginPage'
import { DashboardPage } from './pages/dashboard/DashboardPage'
import { PatientsListPage } from './pages/patients/PatientsListPage'
import { PatientDetailPage } from './pages/patients/PatientDetailPage'
import { PatientFormPage } from './pages/patients/PatientFormPage'
import { useAuth } from './stores/authStore'
import { useSignalR } from './hooks/useSignalR'

// Protected Route Component
const ProtectedRoute = ({ children }: { children: React.ReactNode }) => {
  const { isAuthenticated } = useAuth()
  
  if (!isAuthenticated) {
    return <Navigate to="/login" replace />
  }
  
  return <>{children}</>
}

// App Layout Wrapper (with SignalR)
const AppLayoutWrapper = ({ children }: { children: React.ReactNode }) => {
  // Initialize SignalR connection for authenticated users
  useSignalR()
  
  return (
    <AppLayout>
      {children}
    </AppLayout>
  )
}

function App() {
  const { isAuthenticated } = useAuth()

  return (
    <QueryProvider>
      <AntdProvider>
        <Router>
          <Routes>
            {/* Public Routes */}
            <Route 
              path="/login" 
              element={
                isAuthenticated ? <Navigate to="/" replace /> : <LoginPage />
              } 
            />
            
            {/* Protected Routes */}
            <Route
              path="/"
              element={
                <ProtectedRoute>
                  <AppLayoutWrapper>
                    <DashboardPage />
                  </AppLayoutWrapper>
                </ProtectedRoute>
              }
            />
            
            <Route
              path="/dashboard"
              element={
                <ProtectedRoute>
                  <AppLayoutWrapper>
                    <DashboardPage />
                  </AppLayoutWrapper>
                </ProtectedRoute>
              }
            />
            
            {/* Patient Management Routes */}
            <Route
              path="/patients"
              element={
                <ProtectedRoute>
                  <AppLayoutWrapper>
                    <PatientsListPage />
                  </AppLayoutWrapper>
                </ProtectedRoute>
              }
            />
            
            <Route
              path="/patients/new"
              element={
                <ProtectedRoute>
                  <AppLayoutWrapper>
                    <PatientFormPage />
                  </AppLayoutWrapper>
                </ProtectedRoute>
              }
            />
            
            <Route
              path="/patients/:id"
              element={
                <ProtectedRoute>
                  <AppLayoutWrapper>
                    <PatientDetailPage />
                  </AppLayoutWrapper>
                </ProtectedRoute>
              }
            />
            
            <Route
              path="/patients/:id/edit"
              element={
                <ProtectedRoute>
                  <AppLayoutWrapper>
                    <PatientFormPage />
                  </AppLayoutWrapper>
                </ProtectedRoute>
              }
            />
            
            {/* TODO: Add more routes as we build them */}
            {/* 
            <Route
              path="/appointments"
              element={
                <ProtectedRoute>
                  <AppLayoutWrapper>
                    <AppointmentsPage />
                  </AppLayoutWrapper>
                </ProtectedRoute>
              }
            />
            */}
            
            {/* Catch all - redirect to dashboard */}
            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>
        </Router>
        
        {/* Global Toast Notifications */}
        <Toaster
          position="top-right"
          toastOptions={{
            duration: 4000,
            style: {
              background: '#fff',
              color: '#363636',
              boxShadow: '0 4px 12px rgba(0, 0, 0, 0.15)',
              borderRadius: '8px',
              padding: '16px',
              fontSize: '14px'
            },
            success: {
              iconTheme: {
                primary: '#52c41a',
                secondary: '#fff'
              }
            },
            error: {
              iconTheme: {
                primary: '#ff4d4f',
                secondary: '#fff'
              }
            }
          }}
        />
      </AntdProvider>
    </QueryProvider>
  )
}

export default App