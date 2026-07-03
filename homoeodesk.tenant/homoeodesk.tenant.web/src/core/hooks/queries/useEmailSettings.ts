import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import emailService, {
  EmailSettings,
  CreateOrUpdateEmailSettingsRequest,
} from '../../services/emailService';
import { message } from 'antd';

export const emailSettingsKeys = {
  all: ['emailSettings'] as const,
  settings: () => [...emailSettingsKeys.all, 'settings'] as const,
};

export function useEmailSettings() {
  return useQuery({
    queryKey: emailSettingsKeys.settings(),
    queryFn: () => emailService.getSettings(),
    staleTime: 5 * 60 * 1000, // 5 minutes
  });
}

export function useCreateOrUpdateEmailSettings() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: CreateOrUpdateEmailSettingsRequest) =>
      emailService.createOrUpdateSettings(request),
    onSuccess: (data) => {
      queryClient.setQueryData(emailSettingsKeys.settings(), data);
      message.success('Email settings saved successfully');
    },
    onError: (error: any) => {
      message.error(error?.response?.data?.message || 'Failed to save email settings');
    },
  });
}

