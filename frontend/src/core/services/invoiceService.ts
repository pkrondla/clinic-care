import apiClient, { api } from './apiClient';

export interface Invoice {
  id: number;
  clinicId: number;
  clinicName: string;
  patientId: number;
  patientName: string;
  patientCode: string;
  consultationId?: number;
  prescriptionId?: number;
  prescriptionNumber: string;
  invoiceNumber: string;
  consultationAmount: number;
  medicineAmount: number;
  courierCharges: number;
  totalAmount: number;
  paidAmount: number;
  balanceAmount: number;
  status: number;
  statusText: string;
  paymentMethod: string;
  paymentReference: string;
  invoiceDate: string;
  paymentDate?: string;
  courierDocketNumber?: string;
  courierCompany?: string;
  courierDispatchedDate?: string;
  courierTrackingUrl?: string;
  courierStatus?: number;
  courierStatusText?: string;
  items: InvoiceItem[];
}

export interface InvoiceItem {
  id: number;
  itemType: string;
  description: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
}

export interface CreateInvoiceFromPrescriptionRequest {
  prescriptionId: number;
  courierCharges?: number;
}

export interface PayInvoiceRequest {
  invoiceId: number;
  amount: number;
  paymentMethod: string;
  paymentReference?: string;
}

export interface UpdateCourierDocketRequest {
  invoiceId: number;
  courierDocketNumber: string;
  courierCompany: string;
  courierTrackingUrl?: string;
}

export interface GetInvoicesParams {
  clinicId?: number;
  patientId?: number;
  status?: number;
  startDate?: string;
  endDate?: string;
}

const invoiceService = {
  /**
   * Get list of invoices
   */
  async getInvoices(params: GetInvoicesParams = {}): Promise<Invoice[]> {
    const response = await api.get<{ data: Invoice[] }>('/invoices', {
      params,
    });
    return response.data?.data || [];
  },

  /**
   * Get a specific invoice by ID
   */
  async getInvoice(id: number): Promise<Invoice> {
    const response = await api.get<{ data: Invoice }>(`/invoices/${id}`);
    if (!response.data?.data) {
      throw new Error('Invoice not found');
    }
    return response.data.data;
  },

  /**
   * Create invoice from prescription
   */
  async createInvoiceFromPrescription(
    request: CreateInvoiceFromPrescriptionRequest
  ): Promise<Invoice> {
    const response = await api.post<{ data: Invoice }>(
      '/invoices/from-prescription',
      request
    );
    if (!response.data?.data) {
      throw new Error('Failed to create invoice');
    }
    return response.data.data;
  },

  /**
   * Process payment for an invoice
   */
  async payInvoice(request: PayInvoiceRequest): Promise<Invoice> {
    const response = await api.post<{ data: Invoice }>(
      `/invoices/${request.invoiceId}/pay`,
      request
    );
    if (!response.data?.data) {
      throw new Error('Failed to process payment');
    }
    return response.data.data;
  },

  /**
   * Download invoice as PDF
   */
  async downloadInvoicePdf(id: number): Promise<Blob> {
    const response = await api.get(`/invoices/${id}/pdf`, {
      responseType: 'blob',
    });
    if (!response.data) {
      throw new Error('Failed to download invoice PDF');
    }
    return response.data as Blob;
  },

  /**
   * Update courier docket information
   */
  async updateCourierDocket(
    request: UpdateCourierDocketRequest
  ): Promise<Invoice> {
    const response = await api.post<{ data: Invoice }>(
      `/invoices/${request.invoiceId}/courier`,
      request
    );
    if (!response.data?.data) {
      throw new Error('Failed to update courier docket');
    }
    return response.data.data;
  },
};

export default invoiceService;

