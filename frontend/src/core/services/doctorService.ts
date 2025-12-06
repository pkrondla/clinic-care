import apiClient from './apiClient';

export interface Doctor {
  id: number;
  userId: number;
  doctorName: string;
  qualification: string;
  specialization: string;
  registrationNumber: string;
  experienceYears: number;
  consultationFeeInPerson: number;
  consultationFeeTele: number;
  followupFeeInPerson: number;
  followupFeeTele: number;
  baseClinicId?: number;
  baseClinicName?: string;
  isActive: boolean;
}

export interface GetDoctorsParams {
  clinicId?: number;
  isActive?: boolean;
}

const doctorService = {
  /**
   * Get list of doctors
   */
  async getDoctors(params: GetDoctorsParams = {}): Promise<Doctor[]> {
    const response = await apiClient.get<{ data: Doctor[] }>('/doctors', {
      params,
    });
    return response.data.data;
  },
};

export default doctorService;

