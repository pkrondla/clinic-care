import apiClient, { api } from './apiClient';

export interface PurchaseOrder {
  id: number;
  BranchId: number;
  clinicName: string;
  supplierId: number;
  supplierName: string;
  orderNumber: string;
  orderDate: string;
  expectedDeliveryDate?: string;
  status: number;
  statusText: string;
  totalAmount: number;
  discountAmount?: number;
  taxAmount?: number;
  grandTotal: number;
  notes?: string;
  approvedDate?: string;
  approvedByUserId?: number;
  approvedByUserName?: string;
  orderedDate?: string;
  receivedDate?: string;
  receivedByUserId?: number;
  receivedByUserName?: string;
  items: PurchaseOrderItem[];
}

export interface PurchaseOrderItem {
  id: number;
  medicineId: number;
  medicineName: string;
  quantity: number;
  receivedQuantity?: number;
  unitPrice: number;
  discountAmount?: number;
  totalPrice: number;
  batchNumber?: string;
  expiryDate?: string;
  notes?: string;
}

export interface CreatePurchaseOrderRequest {
  BranchId: number;
  supplierId: number;
  orderDate?: string;
  expectedDeliveryDate?: string;
  discountAmount?: number;
  taxAmount?: number;
  notes?: string;
  items: CreatePurchaseOrderItemRequest[];
}

export interface CreatePurchaseOrderItemRequest {
  medicineId: number;
  quantity: number;
  unitPrice: number;
  discountAmount?: number;
  batchNumber?: string;
  expiryDate?: string;
  notes?: string;
}

export interface ReceivePurchaseOrderRequest {
  id: number;
  receivedItems: ReceivedItemRequest[];
}

export interface ReceivedItemRequest {
  purchaseOrderItemId: number;
  receivedQuantity: number;
  batchNumber?: string;
  expiryDate?: string;
}

export interface GetPurchaseOrdersParams {
  BranchId?: number;
  supplierId?: number;
  status?: number;
  startDate?: string;
  endDate?: string;
}

const purchaseOrderService = {
  /**
   * Get list of purchase orders
   */
  async getPurchaseOrders(params: GetPurchaseOrdersParams = {}): Promise<PurchaseOrder[]> {
    const response = await api.get<PurchaseOrder[] | { data: PurchaseOrder[] }>('/purchase-orders', {
      params,
    });
    
    // Backend may return array directly or wrapped in ApiResponse
    if (Array.isArray(response)) {
      return response;
    }
    // Fallback: if wrapped in ApiResponse, extract data
    if (response && typeof response === 'object' && 'data' in response && Array.isArray(response.data)) {
      return response.data;
    }
    // Default fallback
    return [];
  },

  /**
   * Get a specific purchase order by ID
   */
  async getPurchaseOrder(id: number): Promise<PurchaseOrder> {
    const response = await api.get<PurchaseOrder | { data: PurchaseOrder }>(`/purchase-orders/${id}`);
    
    // Backend may return object directly or wrapped in ApiResponse
    if (response && typeof response === 'object' && 'id' in response) {
      return response as PurchaseOrder;
    }
    // Fallback: if wrapped in ApiResponse, extract data
    if (response && typeof response === 'object' && 'data' in response) {
      return (response as { data: PurchaseOrder }).data;
    }
    throw new Error('Invalid purchase order response format');
  },

  /**
   * Create a new purchase order
   */
  async createPurchaseOrder(request: CreatePurchaseOrderRequest): Promise<PurchaseOrder> {
    const response = await api.post<PurchaseOrder | { data: PurchaseOrder }>('/purchase-orders', request);
    
    // Backend may return object directly or wrapped in ApiResponse
    if (response && typeof response === 'object' && 'id' in response) {
      return response as PurchaseOrder;
    }
    // Fallback: if wrapped in ApiResponse, extract data
    if (response && typeof response === 'object' && 'data' in response) {
      return (response as { data: PurchaseOrder }).data;
    }
    throw new Error('Invalid purchase order response format');
  },

  /**
   * Approve a purchase order
   */
  async approvePurchaseOrder(id: number): Promise<PurchaseOrder> {
    const response = await api.post<PurchaseOrder | { data: PurchaseOrder }>(
      `/purchase-orders/${id}/approve`,
      {}
    );
    
    // Backend may return object directly or wrapped in ApiResponse
    if (response && typeof response === 'object' && 'id' in response) {
      return response as PurchaseOrder;
    }
    // Fallback: if wrapped in ApiResponse, extract data
    if (response && typeof response === 'object' && 'data' in response) {
      return (response as { data: PurchaseOrder }).data;
    }
    throw new Error('Invalid purchase order response format');
  },

  /**
   * Receive items from a purchase order
   */
  async receivePurchaseOrder(request: ReceivePurchaseOrderRequest): Promise<PurchaseOrder> {
    const response = await api.post<PurchaseOrder | { data: PurchaseOrder }>(
      `/purchase-orders/${request.id}/receive`,
      request
    );
    
    // Backend may return object directly or wrapped in ApiResponse
    if (response && typeof response === 'object' && 'id' in response) {
      return response as PurchaseOrder;
    }
    // Fallback: if wrapped in ApiResponse, extract data
    if (response && typeof response === 'object' && 'data' in response) {
      return (response as { data: PurchaseOrder }).data;
    }
    throw new Error('Failed to receive purchase order');
  },

  /**
   * Cancel a purchase order
   */
  async cancelPurchaseOrder(id: number, reason?: string): Promise<PurchaseOrder> {
    const response = await api.post<PurchaseOrder | { data: PurchaseOrder }>(
      `/purchase-orders/${id}/cancel`,
      { reason }
    );
    
    // Backend may return object directly or wrapped in ApiResponse
    if (response && typeof response === 'object' && 'id' in response) {
      return response as PurchaseOrder;
    }
    // Fallback: if wrapped in ApiResponse, extract data
    if (response && typeof response === 'object' && 'data' in response) {
      return (response as { data: PurchaseOrder }).data;
    }
    throw new Error('Failed to cancel purchase order');
  },
};

export default purchaseOrderService;

