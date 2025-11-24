import apiClient, { api } from './apiClient';

export interface PurchaseOrder {
  id: number;
  clinicId: number;
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
  clinicId: number;
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
  clinicId?: number;
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
    const response = await api.get<{ data: PurchaseOrder[] }>('/purchase-orders', {
      params,
    });
    return response.data.data;
  },

  /**
   * Get a specific purchase order by ID
   */
  async getPurchaseOrder(id: number): Promise<PurchaseOrder> {
    const response = await api.get<{ data: PurchaseOrder }>(`/purchase-orders/${id}`);
    return response.data.data;
  },

  /**
   * Create a new purchase order
   */
  async createPurchaseOrder(request: CreatePurchaseOrderRequest): Promise<PurchaseOrder> {
    const response = await api.post<{ data: PurchaseOrder }>('/purchase-orders', request);
    return response.data.data;
  },

  /**
   * Approve a purchase order
   */
  async approvePurchaseOrder(id: number): Promise<PurchaseOrder> {
    const response = await api.post<{ data: PurchaseOrder }>(
      `/purchase-orders/${id}/approve`,
      {}
    );
    return response.data.data;
  },

  /**
   * Receive items from a purchase order
   */
  async receivePurchaseOrder(request: ReceivePurchaseOrderRequest): Promise<PurchaseOrder> {
    const response = await api.post<{ data: PurchaseOrder }>(
      `/purchase-orders/${request.id}/receive`,
      request
    );
    if (!response.data?.data) {
      throw new Error('Failed to receive purchase order');
    }
    return response.data.data;
  },

  /**
   * Cancel a purchase order
   */
  async cancelPurchaseOrder(id: number, reason?: string): Promise<PurchaseOrder> {
    const response = await api.post<{ data: PurchaseOrder }>(
      `/purchase-orders/${id}/cancel`,
      { reason }
    );
    if (!response.data?.data) {
      throw new Error('Failed to cancel purchase order');
    }
    return response.data.data;
  },
};

export default purchaseOrderService;

