import type { BaseEntity } from './common'
import type { Organization } from './auth'

export enum SubscriptionPlan {
  Basic = 'Basic',
  Professional = 'Professional',
  Enterprise = 'Enterprise'
}

export enum SubscriptionStatus {
  Active = 'Active',
  Inactive = 'Inactive',
  Cancelled = 'Cancelled',
  Expired = 'Expired',
  Pending = 'Pending'
}

export interface Subscription extends BaseEntity {
  organization: Organization
  plan: SubscriptionPlan
  status: SubscriptionStatus
  startDate: string
  endDate: string
  price: number
  maxClinics: number
  maxUsers: number
  features: string[]
  paymentHistory: {
    id: number
    date: string
    amount: number
    status: 'Paid' | 'Failed' | 'Pending'
    method: string
  }[]
}

export interface CreateSubscriptionRequest {
  organizationId: number
  plan: SubscriptionPlan
  startDate: string
  endDate: string
  price: number
  maxClinics: number
  maxUsers: number
  features: string[]
}

export interface UpdateSubscriptionRequest extends Partial<CreateSubscriptionRequest> {
  status?: SubscriptionStatus
}

export interface SubscriptionFilters {
  organizationId?: number
  plan?: SubscriptionPlan
  status?: SubscriptionStatus
  startDate?: string
  endDate?: string
  search?: string
}