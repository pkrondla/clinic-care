import apiClient from './apiClient'

export interface InventoryItem {
  id: number
  clinicId: number
  medicineId: number
  medicineName: string
  currentStock: number
  minimumStock: number
  maximumStock: number
  purchasePrice: number
  sellingPrice: number
  expiryDate: string
  batchNumber: string
  isLowStock: boolean
  lastUpdated: string
}

export interface CreateInventoryItemRequest {
  clinicId: number
  medicineId: number
  initialStock: number
  minimumStock: number
  maximumStock: number
  purchasePrice: number
  sellingPrice: number
  expiryDate: string
  batchNumber?: string
}

export interface AdjustStockRequest {
  inventoryId: number
  quantity: number // positive to add, negative to subtract
  transactionType: 'Purchase' | 'Sale' | 'Adjustment' | 'Return' | 'Expired'
  notes?: string
}

export const inventoryService = {
  getAll: async (clinicId?: number): Promise<InventoryItem[]> => {
    const response = await apiClient.get('/inventory-management', {
      params: clinicId ? { clinicId } : undefined
    })
    return response.data.data
  },

  getLowStock: async (clinicId?: number): Promise<InventoryItem[]> => {
    const response = await apiClient.get('/inventory-management/low-stock', {
      params: clinicId ? { clinicId } : undefined
    })
    return response.data.data
  },

  create: async (data: CreateInventoryItemRequest): Promise<InventoryItem> => {
    const response = await apiClient.post('/inventory-management', data)
    return response.data.data
  },

  adjustStock: async (data: AdjustStockRequest): Promise<InventoryItem> => {
    const response = await apiClient.post('/inventory-management/adjust-stock', data)
    return response.data.data
  }
}

