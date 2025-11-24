import apiClient, { api } from './apiClient';

export interface StockAuditItem {
  inventoryId: number;
  physicalStock: number;
  notes?: string;
}

export interface PerformStockAuditRequest {
  clinicId: number;
  auditItems: StockAuditItem[];
  notes?: string;
}

export interface StockAuditResult {
  clinicId: number;
  clinicName: string;
  auditDate: string;
  totalItemsAudited: number;
  itemsWithVariance: number;
  items: StockAuditItemResult[];
  notes?: string;
}

export interface StockAuditItemResult {
  inventoryId: number;
  medicineId: number;
  medicineName: string;
  systemStock: number;
  physicalStock: number;
  variance: number;
  stockAdjusted: boolean;
  notes?: string;
}

export interface StockAuditHistory {
  id: number;
  clinicId: number;
  clinicName: string;
  medicineId: number;
  medicineName: string;
  systemStock: number;
  physicalStock: number;
  variance: number;
  auditDate: string;
  auditedByUserId?: number;
  auditedByUserName?: string;
  notes?: string;
  reference: string;
}

export interface GetStockAuditHistoryParams {
  clinicId?: number;
  startDate?: string;
  endDate?: string;
}

const stockAuditService = {
  /**
   * Perform a stock audit
   */
  async performStockAudit(request: PerformStockAuditRequest): Promise<StockAuditResult> {
    const response = await api.post<{ data: StockAuditResult }>('/stock-audit', request);
    return response.data.data;
  },

  /**
   * Get stock audit history
   */
  async getStockAuditHistory(params: GetStockAuditHistoryParams = {}): Promise<StockAuditHistory[]> {
    const response = await api.get<{ data: StockAuditHistory[] }>('/stock-audit/history', {
      params,
    });
    return response.data.data;
  },
};

export default stockAuditService;

