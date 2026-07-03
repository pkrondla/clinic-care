import apiClient from './apiClient';

export interface EmailSettings {
  id: number;
  organizationId: number;
  isEnabled: boolean;
  smtpServer?: string;
  smtpPort?: number;
  useSsl: boolean;
  useTls: boolean;
  smtpUsername?: string;
  smtpPassword?: string;
  fromEmail?: string;
  fromName?: string;
  replyToEmail?: string;
  timeoutSeconds?: number;
}

export interface CreateOrUpdateEmailSettingsRequest {
  isEnabled: boolean;
  smtpServer?: string;
  smtpPort?: number;
  useSsl: boolean;
  useTls: boolean;
  smtpUsername?: string;
  smtpPassword?: string;
  fromEmail?: string;
  fromName?: string;
  replyToEmail?: string;
  timeoutSeconds?: number;
}

const emailService = {
  async getSettings(): Promise<EmailSettings | null> {
    const response = await apiClient.get<{ success: boolean; data: EmailSettings }>(
      '/settings/email'
    );
    return response.data?.data || null;
  },

  async createOrUpdateSettings(request: CreateOrUpdateEmailSettingsRequest): Promise<EmailSettings> {
    const response = await apiClient.post<{ success: boolean; data: EmailSettings; message: string }>(
      '/settings/email',
      request
    );
    return response.data.data;
  },
};

export default emailService;
