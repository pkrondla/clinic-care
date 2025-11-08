import React from 'react';
import { Routes, Route, Navigate } from 'react-router-dom';
import { ConfigProvider } from 'antd';

// Types
import { UserRole } from '@core/types/auth';

// Layout
import { GlobalLayout } from './components/layout/GlobalLayout';

// Pages
import { DashboardPage } from './pages/dashboard/DashboardPage';
import { OrganizationsPage } from './pages/organizations/OrganizationsPage';
import { SubscriptionsPage } from './pages/subscriptions/SubscriptionsPage';
import { GlobalMedicinesPage } from './pages/medicines/GlobalMedicinesPage';

// Components
import { PrivateRoute } from '@shared/PrivateRoute';

export const GlobalApp: React.FC = () => {
  return (
    <ConfigProvider>
      <GlobalLayout>
        <Routes>
          <Route
            path="/"
            element={
              <PrivateRoute
                requiredRoles={[UserRole.SuperAdmin, UserRole.SystemAdmin]}
                redirectTo="/login"
              >
                <Navigate to="/dashboard" replace />
              </PrivateRoute>
            }
          />
          
          <Route
            path="dashboard"
            element={
              <PrivateRoute
                requiredRoles={[UserRole.SuperAdmin, UserRole.SystemAdmin]}
                redirectTo="/login"
              >
                <DashboardPage />
              </PrivateRoute>
            }
          />
          
          <Route
            path="organizations/*"
            element={
              <PrivateRoute
                requiredRoles={[UserRole.SuperAdmin]}
                redirectTo="/login"
              >
                <OrganizationsPage />
              </PrivateRoute>
            }
          />
          
          <Route
            path="subscriptions/*"
            element={
              <PrivateRoute
                requiredRoles={[UserRole.SuperAdmin]}
                redirectTo="/login"
              >
                <SubscriptionsPage />
              </PrivateRoute>
            }
          />
          
          <Route
            path="medicines/*"
            element={
              <PrivateRoute
                requiredRoles={[UserRole.SuperAdmin, UserRole.SystemAdmin]}
                redirectTo="/login"
              >
                <GlobalMedicinesPage />
              </PrivateRoute>
            }
          />
        </Routes>
      </GlobalLayout>
    </ConfigProvider>
  );
};