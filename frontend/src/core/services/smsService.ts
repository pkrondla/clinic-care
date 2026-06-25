import api from './tenantApi';

export interface SmsSettings {
  id: number;
  organizationId: number;
  isEnabled: boolean;
  provider?: string;
  apiKey?: string;
  apiSecret?: string;
  accountSid?: string;
  authToken?: string;
  fromPhoneNumber?: string;
  senderId?: string;
  apiUrl?: string;
  timeoutSeconds?: number;
}

export interface CreateOrUpdateSmsSettingsRequest {
  isEnabled: boolean;
  provider?: string;
  apiKey?: string;
  apiSecret?: string;
  accountSid?: string;
  authToken?: string;
  fromPhoneNumber?: string;
  senderId?: string;
  apiUrl?: string;
  timeoutSeconds?: number;
}

const smsService = {
  async getSettings(): Promise<SmsSettings | null> {
    const response = await api.get<{ success: boolean; data: SmsSettings }>(
      '/settings/sms'
    );
    return response.data?.data || null;
  },

  async createOrUpdateSettings(request: CreateOrUpdateSmsSettingsRequest): Promise<SmsSettings> {
    const response = await api.post<{ success: boolean; data: SmsSettings; message: string }>(
      '/settings/sms',
      request
    );
    return response.data.data;
  },
};

export default smsService;

