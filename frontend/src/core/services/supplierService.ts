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
    const response = await api.get<Supplier[] | { data: Supplier[] }>('/suppliers', {
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
   * Get a specific supplier by ID
   */
  async getSupplier(id: number): Promise<Supplier> {
    const response = await api.get<Supplier | { data: Supplier }>(`/suppliers/${id}`);
    
    // Backend may return object directly or wrapped in ApiResponse
    if (response && typeof response === 'object' && 'id' in response) {
      return response as Supplier;
    }
    // Fallback: if wrapped in ApiResponse, extract data
    if (response && typeof response === 'object' && 'data' in response) {
      return (response as { data: Supplier }).data;
    }
    throw new Error('Invalid supplier response format');
  },

  /**
   * Create a new supplier
   */
  async createSupplier(request: CreateSupplierRequest): Promise<Supplier> {
    const response = await api.post<Supplier | { data: Supplier }>('/suppliers', request);
    
    // Backend may return object directly or wrapped in ApiResponse
    if (response && typeof response === 'object' && 'id' in response) {
      return response as Supplier;
    }
    // Fallback: if wrapped in ApiResponse, extract data
    if (response && typeof response === 'object' && 'data' in response) {
      return (response as { data: Supplier }).data;
    }
    throw new Error('Invalid supplier response format');
  },

  /**
   * Update an existing supplier
   */
  async updateSupplier(request: UpdateSupplierRequest): Promise<Supplier> {
    const response = await api.put<Supplier | { data: Supplier }>(
      `/suppliers/${request.id}`,
      request
    );
    
    // Backend may return object directly or wrapped in ApiResponse
    if (response && typeof response === 'object' && 'id' in response) {
      return response as Supplier;
    }
    // Fallback: if wrapped in ApiResponse, extract data
    if (response && typeof response === 'object' && 'data' in response) {
      return (response as { data: Supplier }).data;
    }
    throw new Error('Invalid supplier response format');
  },

  /**
   * Delete a supplier
   */
  async deleteSupplier(id: number): Promise<void> {
    await api.delete(`/suppliers/${id}`);
  },
};

export default supplierService;

