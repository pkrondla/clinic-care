import { BrowserRouter } from 'react-router-dom'
import { Toaster } from 'react-hot-toast'
import { QueryProvider, AntdProvider } from '@core/providers'
import { TenantApp } from './TenantApp'
import { ErrorBoundary } from '@shared/ErrorBoundary'

function App() {
  return (
    <ErrorBoundary>
      <QueryProvider>
        <AntdProvider>
          <BrowserRouter>
            <TenantApp />
          </BrowserRouter>

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
