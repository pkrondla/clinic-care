import api from './apiClient';
import type { Result } from './apiClient';

export interface DoctorAvailability {
  id: number;
  doctorId: number;
  doctorName: string;
  clinicId: number;
  clinicName: string;
  availableDate: string;
  startTime: string;
  endTime: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface GetDoctorAvailabilityParams {
  doctorId?: number;
  clinicId?: number;
  startDate?: string;
  endDate?: string;
}

export interface CreateDoctorAvailabilityRequest {
  doctorId: number;
  clinicId: number;
  availableDate: string;
  startTime: string;
  endTime: string;
}

export interface UpdateDoctorAvailabilityRequest {
  id: number;
  availableDate: string;
  startTime: string;
  endTime: string;
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

