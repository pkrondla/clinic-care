import api from './tenantApi';

export interface NotificationPreference {
  id: number;
  notificationType: number;
  notificationTypeName: string;
  enableWhatsApp: boolean;
  enableEmail: boolean;
  enableSMS: boolean;
  template?: string;
  isActive: boolean;
}

export interface UpdateNotificationPreferencesRequest {
  preferences: {
    notificationType: number;
    enableWhatsApp: boolean;
    enableEmail: boolean;
    enableSMS: boolean;
    template?: string;
    isActive: boolean;
  }[];
}

const notificationPreferencesService = {
  async getPreferences(): Promise<NotificationPreference[]> {
    const response = await api.get<{ success: boolean; data: NotificationPreference[] }>(
      '/settings/notifications'
    );
    return response.data?.data || [];
  },

  async updatePreferences(request: UpdateNotificationPreferencesRequest): Promise<NotificationPreference[]> {
    const response = await api.put<{ success: boolean; data: NotificationPreference[]; message: string }>(
      '/settings/notifications',
      request
    );
    return response.data.data;
  },
};

export default notificationPreferencesService;

