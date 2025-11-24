import api from './apiClient';
import type { Result } from './apiClient';

export interface CollectionReportDto {
  startDate: string;
  endDate: string;
  totalCollection: number;
  totalPending: number;
  totalInvoices: number;
  paidInvoices: number;
  pendingInvoices: number;
  items: CollectionReportItemDto[];
  paymentMethodBreakdown: PaymentMethodBreakdownDto[];
  dailyCollections: DailyCollectionDto[];
}

export interface CollectionReportItemDto {
  groupKey: string;
  totalAmount: number;
  paidAmount: number;
  balanceAmount: number;
  invoiceCount: number;
}

export interface PaymentMethodBreakdownDto {
  paymentMethod: string;
  amount: number;
  count: number;
  percentage: number;
}

export interface DailyCollectionDto {
  date: string;
  collection: number;
  invoiceCount: number;
}

export interface GetCollectionReportParams {
  clinicId?: number;
  doctorId?: number;
  startDate: string;
  endDate: string;
  groupBy?: 'day' | 'week' | 'month' | 'clinic' | 'doctor' | 'paymentMethod';
}

export interface PatientReportDto {
  patientId?: number;
  patientName?: string;
  patientCode?: string;
  startDate?: string;
  endDate?: string;
  totalVisits: number;
  totalConsultations: number;
  totalPrescriptions: number;
  totalInvoices: number;
  totalAmountPaid: number;
  totalAmountPending: number;
  visitHistory: PatientVisitDto[];
  treatmentSummary: TreatmentSummaryDto[];
  medicationHistory: MedicationHistoryDto[];
  paymentHistory: PaymentHistoryDto[];
}

export interface PatientVisitDto {
  visitDate: string;
  clinicName: string;
  doctorName: string;
  appointmentType: string;
  status: string;
  consultationId?: number;
  prescriptionId?: number;
  invoiceId?: number;
}

export interface TreatmentSummaryDto {
  consultationDate: string;
  doctorName: string;
  diagnosis: string;
  treatmentPlan: string;
  notes: string;
}

export interface MedicationHistoryDto {
  prescriptionDate: string;
  prescriptionNumber: string;
  doctorName: string;
  medicineCount: number;
  status: string;
  medications: MedicationItemDto[];
}

export interface MedicationItemDto {
  medicineName: string;
  dosage: string;
  frequency: string;
  duration: number;
  durationUnit: string;
  instructions: string;
}

export interface PaymentHistoryDto {
  invoiceDate: string;
  invoiceNumber: string;
  totalAmount: number;
  paidAmount: number;
  balanceAmount: number;
  paymentMethod: string;
  status: string;
  paymentDate?: string;
}

export interface GetPatientReportParams {
  patientId?: number;
  clinicId?: number;
  doctorId?: number;
  startDate?: string;
  endDate?: string;
}

export interface InventoryReportDto {
  generatedAt: string;
  totalInventoryValue: number;
  totalMedicines: number;
  lowStockItems: number;
  outOfStockItems: number;
  combinedInventory: CombinedInventoryItemDto[];
  clinicInventory: ClinicInventoryDto[];
  lowStockAlerts: LowStockAlertDto[];
  stockMovements: StockMovementDto[];
}

export interface CombinedInventoryItemDto {
  medicineId: number;
  medicineName: string;
  medicineCode: string;
  unit: string;
  totalQuantity: number;
  availableQuantity: number;
  reservedQuantity: number;
  averagePrice: number;
  totalValue: number;
  clinicCount: number;
  clinicStocks: ClinicStockDto[];
}

export interface ClinicStockDto {
  clinicId: number;
  clinicName: string;
  quantity: number;
  availableQuantity: number;
  reservedQuantity: number;
  unitPrice: number;
  totalValue: number;
  reorderLevel: number;
  isLowStock: boolean;
}

export interface ClinicInventoryDto {
  clinicId: number;
  clinicName: string;
  medicineCount: number;
  totalValue: number;
  lowStockCount: number;
  outOfStockCount: number;
  stocks: ClinicStockDto[];
}

export interface LowStockAlertDto {
  clinicId: number;
  clinicName: string;
  medicineId: number;
  medicineName: string;
  medicineCode: string;
  currentStock: number;
  reorderLevel: number;
  requiredQuantity: number;
  unit: string;
}

export interface StockMovementDto {
  date: string;
  transactionType: string;
  clinicName: string;
  medicineName: string;
  quantity: number;
  unit: string;
  unitPrice: number;
  totalValue: number;
  reference: string;
}

export interface GetInventoryReportParams {
  clinicId?: number;
  medicineId?: number;
  lowStockOnly?: boolean;
  groupBy?: 'clinic' | 'medicine' | 'category';
}

const reportsService = {
  /**
   * Get collection report
   */
  async getCollectionReport(params: GetCollectionReportParams): Promise<CollectionReportDto> {
    const response = await api.get<Result<CollectionReportDto>>('/reports/collection', { params });
    if (!response.data?.data) {
      throw new Error('Failed to fetch collection report');
    }
    return response.data.data;
  },

  /**
   * Get patient report
   */
  async getPatientReport(params: GetPatientReportParams): Promise<PatientReportDto> {
    const response = await api.get<Result<PatientReportDto>>('/reports/patient', { params });
    if (!response.data?.data) {
      throw new Error('Failed to fetch patient report');
    }
    return response.data.data;
  },

  /**
   * Get inventory report
   */
  async getInventoryReport(params: GetInventoryReportParams = {}): Promise<InventoryReportDto> {
    const response = await api.get<Result<InventoryReportDto>>('/reports/inventory', { params });
    if (!response.data?.data) {
      throw new Error('Failed to fetch inventory report');
    }
    return response.data.data;
  },
};

export default reportsService;

