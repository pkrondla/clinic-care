import apiClient, { api } from './apiClient';

export interface Supplier {
  id: number;
  name: string;
  contactPerson: string;
  email: string;
  phone: string;
  alternatePhone?: string;
  address: string;
  city?: string;
  state?: string;
  pinCode?: string;
  gstNumber?: string;
  panNumber?: string;
  bankName?: string;
  bankAccountNumber?: string;
  ifscCode?: string;
  notes?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateSupplierRequest {
  name: string;
  contactPerson: string;
  email: string;
  phone: string;
  alternatePhone?: string;
  address: string;
  city?: string;
  state?: string;
  pinCode?: string;
  gstNumber?: string;
  panNumber?: string;
  bankName?: string;
  bankAccountNumber?: string;
  ifscCode?: string;
  notes?: string;
}

export interface UpdateSupplierRequest extends CreateSupplierRequest {
  id: number;
  isActive: boolean;
}

export interface GetSuppliersParams {
  searchTerm?: string;
  isActive?: boolean;
}

const supplierService = {
  /**
   * Get list of suppliers
   */
  async getSuppliers(params: GetSuppliersParams = {}): Promise<Supplier[]> {
    const response = await api.get<{ data: Supplier[] }>('/suppliers', {
      params,
    });
    return response.data.data;
  },

  /**
   * Get a specific supplier by ID
   */
  async getSupplier(id: number): Promise<Supplier> {
    const response = await api.get<{ data: Supplier }>(`/suppliers/${id}`);
    return response.data.data;
  },

  /**
   * Create a new supplier
   */
  async createSupplier(request: CreateSupplierRequest): Promise<Supplier> {
    const response = await api.post<{ data: Supplier }>('/suppliers', request);
    return response.data.data;
  },

  /**
   * Update an existing supplier
   */
  async updateSupplier(request: UpdateSupplierRequest): Promise<Supplier> {
    const response = await api.put<{ data: Supplier }>(
      `/suppliers/${request.id}`,
      request
    );
    return response.data.data;
  },

  /**
   * Delete a supplier
   */
  async deleteSupplier(id: number): Promise<void> {
    await api.delete(`/suppliers/${id}`);
  },
};

export default supplierService;

