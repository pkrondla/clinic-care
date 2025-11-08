import React from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ConfigProvider } from 'antd';

// Layout
import TenantLayout from './components/layout/TenantLayout';

// Pages
import Dashboard from './pages/Dashboard';
import Appointments from './pages/Appointments';
import Patients from './pages/Patients';
import Queue from './pages/Queue';
import Login from './pages/auth/Login';

// Components
import { PrivateRoute } from '../../components/auth/PrivateRoute';

const queryClient = new QueryClient();

export const TenantApp: React.FC = () => {
  return (
    <QueryClientProvider client={queryClient}>
      <ConfigProvider>
        <BrowserRouter>
          <Routes>
            <Route path="/login" element={<Login />} />
            
            <Route
              path="/"
              element={
                <PrivateRoute>
                  <TenantLayout />
                </PrivateRoute>
              }
            >
              <Route index element={<Navigate to="/dashboard" replace />} />
              <Route path="dashboard" element={<Dashboard />} />
              <Route 
                path="appointments" 
                element={
                  <PrivateRoute requiredRole={['Doctor', 'Reception']}>
                    <Appointments />
                  </PrivateRoute>
                } 
              />
              <Route 
                path="patients" 
                element={
                  <PrivateRoute requiredRole={['Doctor', 'Reception']}>
                    <Patients />
                  </PrivateRoute>
                } 
              />
              <Route 
                path="queue" 
                element={
                  <PrivateRoute requiredRole={['Doctor', 'Reception']}>
                    <Queue />
                  </PrivateRoute>
                } 
              />
            </Route>
          </Routes>
        </BrowserRouter>
      </ConfigProvider>
    </QueryClientProvider>
  );
};