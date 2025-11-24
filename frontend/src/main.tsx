import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import App from './App.tsx'
import './index.css'

// Ensure we have a root element
const rootElement = document.getElementById('root')
if (!rootElement) {
  throw new Error('Failed to find the root element')
}

console.log('main.tsx: Starting app render', { rootElement: !!rootElement })

createRoot(rootElement).render(
  <StrictMode>
    <App />
  </StrictMode>,
)

console.log('main.tsx: App rendered')