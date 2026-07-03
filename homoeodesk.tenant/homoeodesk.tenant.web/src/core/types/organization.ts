export interface Organization {
  id: number
  name: string
  subdomain: string
  contactEmail: string
  contactPhone?: string
  isActive: boolean
  createdAt: string
  updatedAt: string
  subscription: {
    id: number
    name: string
    price: number
  }
}

export interface CreateOrganizationDto {
  name: string
  subdomain: string
  contactEmail: string
  contactPhone?: string
  subscriptionPlanId: number
}

export interface SubscriptionPlan {
  id: number
  name: string
  description: string
  price: number
  features: string[]
  isActive: boolean
}