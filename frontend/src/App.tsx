import { Toaster } from 'react-hot-toast'
import { QueryProvider, AntdProvider } from './core/providers'
import { GlobalApp } from './apps/global/GlobalApp'
import { TenantApp } from './apps/tenant/TenantApp'
import { ErrorBoundary } from './components/shared/ErrorBoundary'

function App() {
  // Check if we're on the global domain or a tenant subdomain
  const isGlobalDomain = !window.location.hostname.includes('.');

  return (
    <ErrorBoundary>
      <QueryProvider>
        <AntdProvider>
          {isGlobalDomain ? (
            <ErrorBoundary key="global">
              <GlobalApp />
            </ErrorBoundary>
          ) : (
            <ErrorBoundary key="tenant">
              <TenantApp />
            </ErrorBoundary>
          )}
          
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