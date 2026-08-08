/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_DEMO_TENANT_URL?: string
  readonly VITE_GLOBAL_API_URL?: string
  readonly VITE_CONTACT_EMAIL?: string
  readonly VITE_BOOKING_URL?: string
  readonly VITE_ANALYTICS_ID?: string
  readonly VITE_SITE_URL?: string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}
