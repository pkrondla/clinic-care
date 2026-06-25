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
  prescriptionItems?: PrescriptionItem[];
}

export interface InvoiceItem {
  id: number;
  itemType: string;
  description: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
}

export interface PrescriptionItem {
  id: number;
  medicineName: string;
  dispensingForm: number;
  dosage: string;
  frequency: string;
  duration: string;
  timing?: string;
  quantity?: number;
  containerSize?: number;
  instructions?: string;
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

export interface CreateInvoiceRequest {
  clinicId: number;
  patientId: number;
  consultationId?: number;
  prescriptionId?: number;
  consultationAmount?: number;
  medicineAmount?: number;
  courierCharges?: number;
  items: InvoiceItemRequest[];
  invoiceDate?: string;
}

export interface InvoiceItemRequest {
  itemType: string;
  description: string;
  quantity: number;
  unitPrice: number;
  medicineId?: number; // Optional: Medicine ID for stock reduction
}

export interface UpdateInvoiceRequest {
  id: number;
  clinicId?: number;
  patientId?: number;
  consultationAmount?: number;
  medicineAmount?: number;
  courierCharges?: number;
  items?: InvoiceItemUpdateRequest[];
  invoiceDate?: string;
  status?: number;
}

export interface InvoiceItemUpdateRequest {
  id?: number;
  itemType: string;
  description: string;
  quantity: number;
  unitPrice: number;
}

export interface GetInvoicesParams {
  clinicId?: number;
  patientId?: number;
  status?: number;
  startDate?: string;
  endDate?: string;
}

export interface InvoicePreparation {
  clinicId: number;
  clinicName: string;
  patientId: number;
  patientName: string;
  patientCode: string;
  consultationId: number;
  prescriptionId: number;
  consultationAmount: number;
  medicineAmount: number;
  courierCharges: number;
  items: InvoiceItemRequest[];
}

const invoiceService = {
  /**
   * Get list of invoices
   */
  async getInvoices(params: GetInvoicesParams = {}): Promise<Invoice[]> {
    const response = await api.get<Invoice[]>('/invoices', {
      params,
    });
    return response.data || [];
  },

  /**
   * Get a specific invoice by ID
   */
  async getInvoice(id: number): Promise<Invoice> {
    const response = await api.get<Invoice>(`/invoices/${id}`);
    if (!response.data) {
      throw new Error('Invoice not found');
    }
    return response.data;
  },

  /**
   * Prepare invoice from prescription (get invoice data without creating)
   */
  async prepareInvoiceFromPrescription(
    prescriptionId: number
  ): Promise<InvoicePreparation> {
    const response = await api.get<InvoicePreparation>(
      `/invoices/prepare-from-prescription/${prescriptionId}`
    );
    if (!response.data) {
      throw new Error('Failed to prepare invoice');
    }
    return response.data;
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
    const response = await api.post<{ message?: string; data: Invoice }>(
      `/invoices/${request.invoiceId}/pay`,
      request
    );
    // Backend returns {message: '...', data: Invoice}
    // api.post returns response.data, so response = {message: '...', data: Invoice}
    if (!response.data) {
      throw new Error('Failed to process payment');
    }
    return response.data;
  },

  /**
   * Download invoice as PDF
   */
  async downloadInvoicePdf(id: number): Promise<Blob> {
    // For blob responses, we need to use apiClient directly (axios instance)
    const response = await apiClient.get(`/invoices/${id}/pdf`, {
      responseType: 'blob',
    });
    return response.data;
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

  /**
   * Create a new invoice manually
   */
  async createInvoice(request: CreateInvoiceRequest): Promise<Invoice> {
    const response = await api.post<{ message?: string; data: Invoice }>('/invoices', request);
    // Backend returns {message: '...', data: Invoice}
    // api.post returns response.data, so response = {message: '...', data: Invoice}
    if (!response.data) {
      throw new Error('Failed to create invoice');
    }
    return response.data;
  },

  /**
   * Update an existing invoice
   */
  async updateInvoice(request: UpdateInvoiceRequest): Promise<Invoice> {
    const response = await api.put<{ data: Invoice }>(
      `/invoices/${request.id}`,
      request
    );
    if (!response.data?.data) {
      throw new Error('Failed to update invoice');
    }
    return response.data.data;
  },
};

export default invoiceService;

