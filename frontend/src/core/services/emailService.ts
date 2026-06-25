import api from './tenantApi';

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
    const response = await api.get<{ success: boolean; data: EmailSettings }>(
      '/settings/email'
    );
    return response.data?.data || null;
  },

  async createOrUpdateSettings(request: CreateOrUpdateEmailSettingsRequest): Promise<EmailSettings> {
    const response = await api.post<{ success: boolean; data: EmailSettings; message: string }>(
      '/settings/email',
      request
    );
    return response.data.data;
  },
};

export default emailService;

