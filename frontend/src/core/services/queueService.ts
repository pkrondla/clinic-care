import apiClient from './apiClient';

export interface QueueTokenDto {
  tokenNumber: number;
  appointmentId: number;
  status: number;
  statusText: string;
  patientId?: number;
  patientName?: string;
  patientMobile?: string;
  patientCode?: string;
  createdAt: string;
}

export interface DoctorQueueDto {
  doctorId: number;
  doctorName: string;
  qualification: string;
  currentToken: number;
  totalTokens: number;
  waitingTokens: number;
  tokens: QueueTokenDto[];
}

export interface GetAllQueuesParams {
  clinicId?: number;
  date?: string;
  includePatientDetails?: boolean;
}

export interface GetQueueParams {
  doctorId: number;
  clinicId?: number;
  date?: string;
  includePatientDetails?: boolean;
}

export interface BookAppointmentRequest {
  clinicId: number;
  doctorId: number;
  appointmentDate: string;
  type?: number;
  notes?: string;
}

const queueService = {
  /**
   * Get all doctor queues (authenticated)
   */
  async getAllQueues(params: GetAllQueuesParams = {}) {
    const response = await apiClient.get<{ data: DoctorQueueDto[] }>('/appointments/queues', {
      params,
    });
    return response.data.data;
  },

  /**
   * Get queue for a specific doctor (authenticated)
   */
  async getQueue(params: GetQueueParams) {
    const { doctorId, ...queryParams } = params;
    const response = await apiClient.get<{ data: DoctorQueueDto }>(
      `/appointments/queues/${doctorId}`,
      {
        params: queryParams,
      }
    );
    return response.data.data;
  },

  /**
   * Get all doctor queues (public - token numbers only)
   */
  async getPublicQueues(params: Omit<GetAllQueuesParams, 'includePatientDetails'> = {}) {
    const response = await apiClient.get<{ data: DoctorQueueDto[] }>('/public/queues', {
      params,
    });
    return response.data.data;
  },

  /**
   * Get queue for a specific doctor (public - token numbers only)
   */
  async getPublicQueue(doctorId: number, params: Omit<GetQueueParams, 'doctorId' | 'includePatientDetails'> = {}) {
    const response = await apiClient.get<{ data: DoctorQueueDto }>(
      `/public/queues/${doctorId}`,
      {
        params,
      }
    );
    return response.data.data;
  },

  /**
   * Patient self-booking
   */
  async bookAppointment(request: BookAppointmentRequest) {
    const response = await apiClient.post<{ data: any; message: string }>(
      '/appointments/book',
      request
    );
    return response.data.data;
  },

  /**
   * Start an appointment (doctor)
   */
  async startAppointment(appointmentId: number) {
    const response = await apiClient.post<{ data: any; message: string }>(
      `/appointments/${appointmentId}/start`
    );
    return response.data.data;
  },

  /**
   * Complete an appointment (doctor)
   */
  async completeAppointment(appointmentId: number) {
    const response = await apiClient.post<{ data: any; message: string }>(
      `/appointments/${appointmentId}/complete`
    );
    return response.data.data;
  },
};

export default queueService;

