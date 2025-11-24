import React, { useMemo } from 'react'
import { BrowserRouter } from 'react-router-dom'
import { Toaster } from 'react-hot-toast'
import { QueryProvider, AntdProvider } from '@core/providers'
import { GlobalApp } from './apps/global/GlobalApp'
import { TenantApp } from './apps/tenant/TenantApp'
import { ErrorBoundary } from '@shared/ErrorBoundary'
import { useGlobalAuthStore, useTenantAuthStore } from './core/stores/authStore'

function App() {
  // Check if we're on the global domain or a tenant subdomain
  // For local development: localhost = global, *.localhost = tenant
  // For production: yourdomain.com = global, *.yourdomain.com = tenant
  const hostname = window.location.hostname
  const parts = hostname.split('.')
  
  // Check for tenant query parameter or path (for local development)
  const urlParams = new URLSearchParams(window.location.search)
  const tenantParam = urlParams.get('tenant')
  const isTenantPath = window.location.pathname.startsWith('/tenant')
  
  // Check auth state to determine if user is logged in as tenant or global
  // Use hooks to subscribe to store changes
  const globalAuth = useGlobalAuthStore()
  const tenantAuth = useTenantAuthStore()
  const isTenantAuthenticated = tenantAuth.isAuthenticated && !tenantAuth.isGlobalSystem
  const isGlobalAuthenticated = globalAuth.isAuthenticated && globalAuth.isGlobalSystem
  
  // Determine if this is a tenant subdomain
  // Allow tenant access via query parameter (?tenant=demo) or path (/tenant/login) for local development
  // Also check auth state - if user is authenticated as tenant, stay in tenant mode
  const isGlobalDomain = useMemo(() => {
    // If explicitly requesting tenant via query param or path, use tenant app
    if (tenantParam || isTenantPath) {
      return false
    }
    
    // If tenant user is authenticated, use tenant app
    if (isTenantAuthenticated) {
      return false
    }
    
    // If global user is authenticated, use global app
    if (isGlobalAuthenticated) {
      return true
    }
    
    // Default: check hostname pattern
    return hostname === 'localhost' || 
           hostname === '127.0.0.1' ||
           parts.length === 2 || // e.g., domain.com
           (parts.length === 3 && parts[0] === 'www') // e.g., www.domain.com
  }, [hostname, parts, tenantParam, isTenantPath, isTenantAuthenticated, isGlobalAuthenticated])

  console.log('App.tsx: Rendering app', { 
    hostname, 
    isGlobalDomain, 
    pathname: window.location.pathname,
    tenantParam,
    isTenantPath,
    isTenantAuthenticated,
    isGlobalAuthenticated
  })

  return (
    <ErrorBoundary>
      <QueryProvider>
        <AntdProvider>
          <BrowserRouter>
            {isGlobalDomain ? (
              <ErrorBoundary key="global">
                <GlobalApp />
              </ErrorBoundary>
            ) : (
              <ErrorBoundary key="tenant">
                <TenantApp />
              </ErrorBoundary>
            )}
          </BrowserRouter>
          
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
    </ErrorBoundary>
  )
}

export default App
