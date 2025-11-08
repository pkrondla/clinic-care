export interface Medicine {
  id: number
  name: string
  genericName: string
  manufacturer: string
  description?: string
  dosageForm: string
  strength: string
  packageSize: string
  price: number
  isActive: boolean
  createdAt: string
  updatedAt: string
}

export interface CreateMedicineDto {
  name: string
  genericName: string
  manufacturer: string
  description?: string
  dosageForm: string
  strength: string
  packageSize: string
  price: number
}