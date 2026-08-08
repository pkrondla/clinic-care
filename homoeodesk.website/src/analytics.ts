import { ANALYTICS_ID } from './config'

declare global {
  interface Window {
    plausible?: (event: string, options?: { props?: Record<string, string> }) => void
    dataLayer?: unknown[]
    gtag?: (...args: unknown[]) => void
  }
}

export function initAnalytics() {
  if (!ANALYTICS_ID || typeof document === 'undefined') return

  if (ANALYTICS_ID.startsWith('G-') || ANALYTICS_ID.startsWith('GT-')) {
    const script = document.createElement('script')
    script.async = true
    script.src = `https://www.googletagmanager.com/gtag/js?id=${ANALYTICS_ID}`
    document.head.appendChild(script)
    window.dataLayer = window.dataLayer || []
    window.gtag = function gtag(...args: unknown[]) {
      window.dataLayer?.push(args)
    }
    window.gtag('js', new Date())
    window.gtag('config', ANALYTICS_ID)
    return
  }

  const script = document.createElement('script')
  script.defer = true
  script.setAttribute('data-domain', ANALYTICS_ID)
  script.src = 'https://plausible.io/js/script.js'
  document.head.appendChild(script)
}

export function trackEvent(name: string, props?: Record<string, string>) {
  if (typeof window === 'undefined') return

  window.plausible?.(name, props ? { props } : undefined)

  if (window.gtag) {
    window.gtag('event', name, props)
  }

  if (import.meta.env.DEV) {
    console.debug('[analytics]', name, props)
  }
}
