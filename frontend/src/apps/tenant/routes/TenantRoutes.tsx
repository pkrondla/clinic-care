import { Routes, Route, Navigate } from 'react-router-dom'
import { AppLayout } from '../components/layout/AppLayout'
import { DashboardPage } from '../pages/dashboard/DashboardPage'
import { PatientsListPage } from '../pages/patients/PatientsListPage'
import { PatientDetailPage } from '../pages/patients/PatientDetailPage'
import { PatientFormPage } from '../pages/patients/PatientFormPage'
import { AppointmentsPage } from '../pages/appointments/AppointmentsPage'
import { AppointmentDetailPage } from '../pages/appointments/AppointmentDetailPage'
import { AppointmentFormPage } from '../pages/appointments/AppointmentFormPage'
import { QueuePage } from '../pages/appointments/QueuePage'
import { StaffQueuePage } from '../pages/queue/StaffQueuePage'
import { DoctorQueuePage } from '../pages/queue/DoctorQueuePage'
import { PublicQueuePage } from '../pages/queue/PublicQueuePage'
import { BookAppointmentPage } from '../pages/queue/BookAppointmentPage'
import { ClinicsPage } from '../pages/clinics/ClinicsPage'
import { ConsultationFormPage } from '../pages/consultations/ConsultationFormPage'
import { ConsultationsPage } from '../pages/consultations/ConsultationsPage'
import { ConsultationDetailPage } from '../pages/consultations/ConsultationDetailPage'
import { PrescriptionFormPage } from '../pages/prescriptions/PrescriptionFormPage'
import { PrescriptionsPage } from '../pages/prescriptions/PrescriptionsPage'
import { PrescriptionDetailPage } from '../pages/prescriptions/PrescriptionDetailPage'
import { InventoryPage } from '../pages/inventory/InventoryPage'
import { ClinicMedicinesPage } from '../pages/medicines/ClinicMedicinesPage'
import { UsersPage } from '../pages/users/UsersPage'
import { ReportsPage } from '../pages/reports/ReportsPage'
import { InvoicesPage } from '../pages/invoices/InvoicesPage'
import { InvoiceDetailPage } from '../pages/invoices/InvoiceDetailPage'
import { InvoiceFormPage } from '../pages/invoices/InvoiceFormPage'
import { SuppliersPage } from '../pages/suppliers/SuppliersPage'
import { PurchaseOrdersPage } from '../pages/purchase-orders/PurchaseOrdersPage'
import { PurchaseOrderDetailPage } from '../pages/purchase-orders/PurchaseOrderDetailPage'
import { PurchaseOrderFormPage } from '../pages/purchase-orders/PurchaseOrderFormPage'
import { StockAuditPage } from '../pages/inventory/StockAuditPage'
import { DoctorSchedulePage } from '../pages/doctors/DoctorSchedulePage'
import { LoginPage } from '../pages/auth/LoginPage'
import { ProfilePage } from '../pages/profile/ProfilePage'
import { SettingsPage } from '../pages/settings/SettingsPage'
import { useAuth } from '@core/stores/authStore'
import { UserRole } from '@core/types/auth'

// Protected Tenant Route Component
const ProtectedTenantRoute = ({ 
  children, 
  allowedRoles 
}: { 
  children: React.ReactNode
  allowedRoles?: UserRole[]
}) => {
  const { isAuthenticated, user } = useAuth()

  console.log('ProtectedTenantRoute: Checking auth', { isAuthenticated, user: user?.email, role: user?.role })

  if (!isAuthenticated) {
    console.log('ProtectedTenantRoute: Not authenticated, redirecting to login')
    return <Navigate to="/login" replace />
  }

  // Global admin should be redirected to global app
  if (user?.role === UserRole.SuperAdmin) {
    return <Navigate to="/organizations" replace />
  }

  // Check role-based access
  if (allowedRoles && user?.role && !allowedRoles.includes(user.role)) {
    return <Navigate to="/" replace />
  }

  return <>{children}</>
}

export const TenantRoutes = () => {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />

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

      {/* Clinics */}
      <Route
        path="/clinics"
        element={
          <ProtectedTenantRoute>
            <AppLayout>
              <ClinicsPage />
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

      {/* Appointments */}
      <Route
        path="/appointments"
        element={
          <ProtectedTenantRoute>
            <AppLayout>
              <AppointmentsPage />
            </AppLayout>
          </ProtectedTenantRoute>
        }
      />

      {/* New Appointment / Book Appointment for Staff */}
      <Route
        path="/appointments/new"
        element={
          <ProtectedTenantRoute>
            <AppLayout>
              <BookAppointmentPage />
            </AppLayout>
          </ProtectedTenantRoute>
        }
      />

      {/* Appointment Detail */}
      <Route
        path="/appointments/:id"
        element={
          <ProtectedTenantRoute>
            <AppLayout>
              <AppointmentDetailPage />
            </AppLayout>
          </ProtectedTenantRoute>
        }
      />

      {/* Edit Appointment */}
      <Route
        path="/appointments/:id/edit"
        element={
          <ProtectedTenantRoute>
            <AppLayout>
              <AppointmentFormPage />
            </AppLayout>
          </ProtectedTenantRoute>
        }
      />

      {/* Queue Management - Role-based routing */}
      <Route
        path="/queue"
        element={
          <ProtectedTenantRoute>
            <AppLayout>
              <QueuePage />
            </AppLayout>
          </ProtectedTenantRoute>
        }
      />

      {/* Staff Queue Management */}
      <Route
        path="/queue/staff"
        element={
          <ProtectedTenantRoute allowedRoles={[UserRole.Admin, UserRole.Staff]}>
            <AppLayout>
              <StaffQueuePage />
            </AppLayout>
          </ProtectedTenantRoute>
        }
      />

      {/* Doctor Queue View */}
      <Route
        path="/queue/doctor"
        element={
          <ProtectedTenantRoute allowedRoles={[UserRole.Doctor]}>
            <AppLayout>
              <DoctorQueuePage />
            </AppLayout>
          </ProtectedTenantRoute>
        }
      />

      {/* Patient Self-Booking */}
      <Route
        path="/book-appointment"
        element={
          <ProtectedTenantRoute allowedRoles={[UserRole.Patient]}>
            <AppLayout>
              <BookAppointmentPage />
            </AppLayout>
          </ProtectedTenantRoute>
        }
      />

      {/* Public Queue View (no auth required) */}
      <Route
        path="/public/queue"
        element={<PublicQueuePage />}
      />

      {/* Consultations */}
      <Route
        path="/consultations"
        element={
          <ProtectedTenantRoute>
            <AppLayout>
              <ConsultationsPage />
            </AppLayout>
          </ProtectedTenantRoute>
        }
      />

      <Route
        path="/consultations/new"
        element={
          <ProtectedTenantRoute>
            <AppLayout>
              <ConsultationFormPage />
            </AppLayout>
          </ProtectedTenantRoute>
        }
      />

      {/* Consultation Detail */}
      <Route
        path="/consultations/:id"
        element={
          <ProtectedTenantRoute>
            <AppLayout>
              <ConsultationDetailPage />
            </AppLayout>
          </ProtectedTenantRoute>
        }
      />

      {/* Edit Consultation */}
      <Route
        path="/consultations/:id/edit"
        element={
          <ProtectedTenantRoute>
            <AppLayout>
              <ConsultationFormPage />
            </AppLayout>
          </ProtectedTenantRoute>
        }
      />

      {/* Prescriptions */}
      <Route
        path="/prescriptions"
        element={
          <ProtectedTenantRoute>
            <AppLayout>
              <PrescriptionsPage />
            </AppLayout>
          </ProtectedTenantRoute>
        }
      />

      <Route
        path="/prescriptions/new"
        element={
          <ProtectedTenantRoute allowedRoles={[UserRole.Doctor, UserRole.Admin]}>
            <AppLayout>
              <PrescriptionFormPage />
            </AppLayout>
          </ProtectedTenantRoute>
        }
      />

      {/* Prescription Edit */}
      <Route
        path="/prescriptions/:id/edit"
        element={
          <ProtectedTenantRoute allowedRoles={[UserRole.Doctor, UserRole.Admin]}>
            <AppLayout>
              <PrescriptionFormPage />
            </AppLayout>
          </ProtectedTenantRoute>
        }
      />

      {/* Prescription Detail */}
      <Route
        path="/prescriptions/:id"
        element={
          <ProtectedTenantRoute>
            <AppLayout>
              <PrescriptionDetailPage />
            </AppLayout>
          </ProtectedTenantRoute>
        }
      />

      {/* Inventory */}
      <Route
        path="/inventory"
        element={
          <ProtectedTenantRoute>
            <AppLayout>
              <InventoryPage />
            </AppLayout>
          </ProtectedTenantRoute>
        }
      />

      {/* Clinic Medicines */}
      <Route
        path="/medicines"
        element={
          <ProtectedTenantRoute allowedRoles={[UserRole.Admin, UserRole.Staff]}>
            <AppLayout>
              <ClinicMedicinesPage />
            </AppLayout>
          </ProtectedTenantRoute>
        }
      />

        {/* Stock Audit */}
        <Route
          path="/inventory/audit"
          element={
            <ProtectedTenantRoute allowedRoles={[UserRole.Admin, UserRole.Staff]}>
              <AppLayout>
                <StockAuditPage />
              </AppLayout>
            </ProtectedTenantRoute>
          }
        />
        {/* Doctor Schedule */}
        <Route
          path="/doctors/schedule"
          element={
            <ProtectedTenantRoute allowedRoles={[UserRole.Admin, UserRole.Doctor]}>
              <AppLayout>
                <DoctorSchedulePage />
              </AppLayout>
            </ProtectedTenantRoute>
          }
        />

      {/* Users Management */}
      <Route
        path="/users"
        element={
          <ProtectedTenantRoute>
            <AppLayout>
              <UsersPage />
            </AppLayout>
          </ProtectedTenantRoute>
        }
      />

      {/* Invoices */}
      <Route
        path="/invoices"
        element={
          <ProtectedTenantRoute>
            <AppLayout>
              <InvoicesPage />
            </AppLayout>
          </ProtectedTenantRoute>
        }
      />

      <Route
        path="/invoices/new"
        element={
          <ProtectedTenantRoute>
            <AppLayout>
              <InvoiceFormPage />
            </AppLayout>
          </ProtectedTenantRoute>
        }
      />

      <Route
        path="/invoices/:id"
        element={
          <ProtectedTenantRoute>
            <AppLayout>
              <InvoiceDetailPage />
            </AppLayout>
          </ProtectedTenantRoute>
        }
      />

      <Route
        path="/invoices/:id/edit"
        element={
          <ProtectedTenantRoute>
            <AppLayout>
              <InvoiceFormPage />
            </AppLayout>
          </ProtectedTenantRoute>
        }
      />

      {/* Reports */}
      <Route
        path="/reports"
        element={
          <ProtectedTenantRoute>
            <AppLayout>
              <ReportsPage />
            </AppLayout>
          </ProtectedTenantRoute>
        }
      />

      {/* Suppliers */}
      <Route
        path="/suppliers"
        element={
          <ProtectedTenantRoute allowedRoles={[UserRole.Admin, UserRole.Staff]}>
            <AppLayout>
              <SuppliersPage />
            </AppLayout>
          </ProtectedTenantRoute>
        }
      />

      {/* Purchase Orders */}
      <Route
        path="/purchase-orders"
        element={
          <ProtectedTenantRoute allowedRoles={[UserRole.Admin, UserRole.Staff]}>
            <AppLayout>
              <PurchaseOrdersPage />
            </AppLayout>
          </ProtectedTenantRoute>
        }
      />

      <Route
        path="/purchase-orders/new"
        element={
          <ProtectedTenantRoute allowedRoles={[UserRole.Admin, UserRole.Staff]}>
            <AppLayout>
              <PurchaseOrderFormPage />
            </AppLayout>
          </ProtectedTenantRoute>
        }
      />

      <Route
        path="/purchase-orders/:id"
        element={
          <ProtectedTenantRoute allowedRoles={[UserRole.Admin, UserRole.Staff]}>
            <AppLayout>
              <PurchaseOrderDetailPage />
            </AppLayout>
          </ProtectedTenantRoute>
        }
      />

      {/* Profile */}
      <Route
        path="/profile"
        element={
          <ProtectedTenantRoute>
            <AppLayout>
              <ProfilePage />
            </AppLayout>
          </ProtectedTenantRoute>
        }
      />

      {/* Settings */}
      <Route
        path="/settings"
        element={
          <ProtectedTenantRoute>
            <AppLayout>
              <SettingsPage />
            </AppLayout>
          </ProtectedTenantRoute>
        }
      />

      {/* Catch all - redirect to dashboard */}
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  )
}
