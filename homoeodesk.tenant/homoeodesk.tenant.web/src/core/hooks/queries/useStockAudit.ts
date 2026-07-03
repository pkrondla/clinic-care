import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import stockAuditService, {
  PerformStockAuditRequest,
  GetStockAuditHistoryParams,
} from '../../services/stockAuditService';
import { message } from 'antd';

export const stockAuditKeys = {
  all: ['stockAudit'] as const,
  history: () => [...stockAuditKeys.all, 'history'] as const,
  historyList: (params: GetStockAuditHistoryParams) => [...stockAuditKeys.history(), params] as const,
};

export function useStockAuditHistory(params: GetStockAuditHistoryParams = {}) {
  return useQuery({
    queryKey: stockAuditKeys.historyList(params),
    queryFn: () => stockAuditService.getStockAuditHistory(params),
    staleTime: 30 * 1000, // 30 seconds
  });
}

export function usePerformStockAudit() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: PerformStockAuditRequest) =>
      stockAuditService.performStockAudit(request),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: stockAuditKeys.all });
      // Also invalidate inventory queries
      queryClient.invalidateQueries({ queryKey: ['inventory'] });
      message.success(
        `Stock audit completed. ${data.itemsWithVariance} item(s) adjusted out of ${data.totalItemsAudited} audited.`
      );
    },
    onError: (error: any) => {
      message.error(error?.response?.data?.message || 'Failed to perform stock audit');
    },
  });
}

