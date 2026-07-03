import apiClient from './apiClient';

export interface WhatsAppSettings {
  id: number;
  organizationId: number;
  isEnabled: boolean;
  provider: 'Meta' | 'Twilio' | 'Dialog360';
  phoneNumberId?: string;
  businessAccountId?: string;
  apiVersion?: string;
  fromPhoneNumber?: string;
  webhookUrl?: string;
  webhookVerifyToken?: string;
}

export interface CreateOrUpdateWhatsAppSettingsRequest {
  isEnabled: boolean;
  provider: 'Meta' | 'Twilio' | 'Dialog360';
  phoneNumberId?: string;
  businessAccountId?: string;
  accessToken?: string;
  apiKey?: string;
  apiSecret?: string;
  webhookUrl?: string;
  webhookSecret?: string;
  webhookVerifyToken?: string;
  apiVersion?: string;
  fromPhoneNumber?: string;
}

export interface TestWhatsAppConnectionResult {
  success: boolean;
  message?: string;
  errorMessage?: string;
}

const whatsAppService = {
  async getSettings(): Promise<WhatsAppSettings | null> {
    const response = await apiClient.get<{ success: boolean; data: WhatsAppSettings }>('/settings/whatsapp');
    return response.data?.data || null;
  },

  async createOrUpdateSettings(request: CreateOrUpdateWhatsAppSettingsRequest): Promise<WhatsAppSettings> {
    const response = await apiClient.post<{ success: boolean; data: WhatsAppSettings; message: string }>(
      '/settings/whatsapp',
      request
    );
    return response.data.data;
  },

  async testConnection(): Promise<TestWhatsAppConnectionResult> {
    const response = await apiClient.post<{ success: boolean; data: TestWhatsAppConnectionResult; message: string }>(
      '/settings/whatsapp/test',
      {}
    );
    return response.data.data;
  },
};

export default whatsAppService;
