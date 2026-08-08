import { GLOBAL_API_URL } from '../config'

export type TrialRequestPayload = {
  fullName: string
  email: string
  phone?: string
  clinicName: string
  city?: string
  patientsPerWeek?: string
  message?: string
  source?: string
  consent: boolean
}

export type RegisterTrialPayload = {
  clinicName: string
  contactName: string
  email: string
  phone?: string
  subdomain?: string
  city?: string
  address?: string
  consent: boolean
}

export type ApiEnvelope<T> = {
  success: boolean
  data?: T
  message?: string
  errors?: string[]
}

async function postJson<T>(path: string, body: unknown): Promise<ApiEnvelope<T>> {
  const response = await fetch(`${GLOBAL_API_URL}${path}`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', Accept: 'application/json' },
    body: JSON.stringify(body),
  })

  const payload = (await response.json().catch(() => ({}))) as ApiEnvelope<T>

  if (!response.ok) {
    return {
      success: false,
      errors: payload.errors?.length
        ? payload.errors
        : [payload.message || `Request failed (${response.status})`],
    }
  }

  return {
    success: true,
    data: payload.data,
    message: payload.message,
    errors: payload.errors,
  }
}

export function submitTrialRequest(payload: TrialRequestPayload) {
  return postJson<{ id: number }>('/api/public/trial-requests', {
    fullName: payload.fullName,
    email: payload.email,
    phone: payload.phone,
    clinicName: payload.clinicName,
    city: payload.city,
    patientsPerWeek: payload.patientsPerWeek,
    message: payload.message,
    source: payload.source || 'website',
  })
}

export function registerTrial(payload: RegisterTrialPayload) {
  return postJson<{
    id: number
    name: string
    subdomain: string
    contactEmail: string
    trialEndDate?: string
  }>('/api/public/register', {
    clinicName: payload.clinicName,
    contactName: payload.contactName,
    email: payload.email,
    phone: payload.phone,
    subdomain: payload.subdomain,
    city: payload.city,
    address: payload.address,
  })
}
