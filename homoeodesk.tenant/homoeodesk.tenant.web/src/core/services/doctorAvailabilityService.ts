import api from './apiClient';
import type { Result } from './apiClient';

export enum AvailabilityType {
  Regular = 0,
  DifferentBranch = 1,
  Leave = 2,
  ModifiedHours = 3,
}

export interface DoctorAvailability {
  id: number;
  doctorId: number;
  doctorName: string;
  BranchId: number;
  clinicName: string;
  availableDate: string;
  endDate?: string;
  startTime: string;
  endTime: string;
  availabilityType: AvailabilityType;
  notes?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface GetDoctorAvailabilityParams {
  doctorId?: number;
  BranchId?: number;
  startDate?: string;
  endDate?: string;
}

export interface CreateDoctorAvailabilityRequest {
  doctorId: number;
  BranchId: number;
  availableDate: string;
  endDate?: string; // For leave ranges
  startTime: string;
  endTime: string;
  availabilityType?: AvailabilityType;
  notes?: string;
}

export interface UpdateDoctorAvailabilityRequest {
  id: number;
  availableDate: string;
  endDate?: string;
  startTime: string;
  endTime: string;
  availabilityType: AvailabilityType;
  notes?: string;
  isActive?: boolean;
}

const doctorAvailabilityService = {
  /**
   * Get doctor availability schedule
   */
  async getAll(params: GetDoctorAvailabilityParams = {}): Promise<DoctorAvailability[]> {
    const response = await api.get<Result<DoctorAvailability[]>>('/doctors/availability', { params });
    if (!response.data?.data) {
      throw new Error('Failed to fetch doctor availability');
    }
    return response.data.data;
  },

  /**
   * Create doctor availability
   */
  async create(request: CreateDoctorAvailabilityRequest): Promise<DoctorAvailability> {
    const response = await api.post<Result<DoctorAvailability>>('/doctors/availability', request);
    if (!response.data?.data) {
      throw new Error('Failed to create doctor availability');
    }
    return response.data.data;
  },

  /**
   * Update doctor availability
   */
  async update(id: number, request: UpdateDoctorAvailabilityRequest): Promise<DoctorAvailability> {
    const response = await api.put<Result<DoctorAvailability>>(`/doctors/availability/${id}`, request);
    if (!response.data?.data) {
      throw new Error('Failed to update doctor availability');
    }
    return response.data.data;
  },

  /**
   * Delete doctor availability
   */
  async delete(id: number): Promise<void> {
    const response = await api.delete<Result<void>>(`/doctors/availability/${id}`);
    if (!response.data?.succeeded) {
      throw new Error('Failed to delete doctor availability');
    }
  },
};

export default doctorAvailabilityService;

