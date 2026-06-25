import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import smsService, {
  SmsSettings,
  CreateOrUpdateSmsSettingsRequest,
} from '../../services/smsService';
import { message } from 'antd';

export const smsSettingsKeys = {
  all: ['smsSettings'] as const,
  settings: () => [...smsSettingsKeys.all, 'settings'] as const,
};

export function useSmsSettings() {
  return useQuery({
    queryKey: smsSettingsKeys.settings(),
    queryFn: () => smsService.getSettings(),
    staleTime: 5 * 60 * 1000, // 5 minutes
  });
}

export function useCreateOrUpdateSmsSettings() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: CreateOrUpdateSmsSettingsRequest) =>
      smsService.createOrUpdateSettings(request),
    onSuccess: (data) => {
      queryClient.setQueryData(smsSettingsKeys.settings(), data);
      message.success('SMS settings saved successfully');
    },
    onError: (error: any) => {
      message.error(error?.response?.data?.message || 'Failed to save SMS settings');
    },
  });
}

