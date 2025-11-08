import React from 'react';
import { Navigate, useLocation } from 'react-router-dom';

interface PrivateRouteProps {
  children: React.ReactNode;
  requiredRole?: string[];
}

export const PrivateRoute: React.FC<PrivateRouteProps> = ({ 
  children, 
  requiredRole 
}) => {
  const location = useLocation();
  const isGlobalApp = !window.location.hostname.includes('.');
  
  // Check if we're in global or tenant app
  const token = localStorage.getItem(isGlobalApp ? 'global_token' : 'tenant_token');
  const user = JSON.parse(localStorage.getItem(isGlobalApp ? 'global_user' : 'tenant_user') || '{}');
  
  if (!token) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  if (requiredRole && !requiredRole.includes(user.role)) {
    return <Navigate to="/unauthorized" replace />;
  }

  return <>{children}</>;
};