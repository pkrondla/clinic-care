import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import whatsAppService, {
  WhatsAppSettings,
  CreateOrUpdateWhatsAppSettingsRequest,
  TestWhatsAppConnectionResult,
} from '../../services/whatsAppService';
import { message } from 'antd';

export const whatsAppKeys = {
  all: ['whatsapp'] as const,
  settings: () => [...whatsAppKeys.all, 'settings'] as const,
};

export function useWhatsAppSettings() {
  return useQuery({
    queryKey: whatsAppKeys.settings(),
    queryFn: () => whatsAppService.getSettings(),
    staleTime: 5 * 60 * 1000, // 5 minutes
  });
}

export function useCreateOrUpdateWhatsAppSettings() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: CreateOrUpdateWhatsAppSettingsRequest) =>
      whatsAppService.createOrUpdateSettings(request),
    onSuccess: (data) => {
      queryClient.setQueryData(whatsAppKeys.settings(), data);
      message.success('WhatsApp settings saved successfully');
    },
    onError: (error: any) => {
      message.error(error?.response?.data?.message || 'Failed to save WhatsApp settings');
    },
  });
}

export function useTestWhatsAppConnection() {
  return useMutation({
    mutationFn: () => whatsAppService.testConnection(),
    onSuccess: (data) => {
      if (data.success) {
        message.success(data.message || 'WhatsApp connection test successful!');
      } else {
        message.error(data.errorMessage || data.message || 'WhatsApp connection test failed');
      }
    },
    onError: (error: any) => {
      message.error(error?.response?.data?.message || 'Failed to test WhatsApp connection');
    },
  });
}

