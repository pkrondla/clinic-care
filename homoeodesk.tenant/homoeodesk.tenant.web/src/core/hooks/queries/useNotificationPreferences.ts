import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import notificationPreferencesService, {
  NotificationPreference,
  UpdateNotificationPreferencesRequest,
} from '../../services/notificationPreferencesService';
import { message } from 'antd';

export const notificationPreferencesKeys = {
  all: ['notificationPreferences'] as const,
  list: () => [...notificationPreferencesKeys.all, 'list'] as const,
};

export function useNotificationPreferences() {
  return useQuery({
    queryKey: notificationPreferencesKeys.list(),
    queryFn: () => notificationPreferencesService.getPreferences(),
    staleTime: 5 * 60 * 1000, // 5 minutes
  });
}

export function useUpdateNotificationPreferences() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: UpdateNotificationPreferencesRequest) =>
      notificationPreferencesService.updatePreferences(request),
    onSuccess: (data) => {
      queryClient.setQueryData(notificationPreferencesKeys.list(), data);
      message.success('Notification preferences updated successfully');
    },
    onError: (error: any) => {
      message.error(error?.response?.data?.message || 'Failed to update notification preferences');
    },
  });
}

