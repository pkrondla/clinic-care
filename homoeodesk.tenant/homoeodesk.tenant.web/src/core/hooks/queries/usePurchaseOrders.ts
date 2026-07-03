import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import purchaseOrderService, {
  GetPurchaseOrdersParams,
  CreatePurchaseOrderRequest,
  ReceivePurchaseOrderRequest,
} from '../../services/purchaseOrderService';
import { message } from 'antd';

export const purchaseOrderKeys = {
  all: ['purchaseOrders'] as const,
  lists: () => [...purchaseOrderKeys.all, 'list'] as const,
  list: (params: GetPurchaseOrdersParams) => [...purchaseOrderKeys.lists(), params] as const,
  details: () => [...purchaseOrderKeys.all, 'detail'] as const,
  detail: (id: number) => [...purchaseOrderKeys.details(), id] as const,
};

export function usePurchaseOrders(params: GetPurchaseOrdersParams = {}) {
  return useQuery({
    queryKey: purchaseOrderKeys.list(params),
    queryFn: () => purchaseOrderService.getPurchaseOrders(params),
    staleTime: 30 * 1000, // 30 seconds
  });
}

export function usePurchaseOrder(id: number) {
  return useQuery({
    queryKey: purchaseOrderKeys.detail(id),
    queryFn: () => purchaseOrderService.getPurchaseOrder(id),
    enabled: !!id,
    staleTime: 30 * 1000,
  });
}

export function useCreatePurchaseOrder() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: CreatePurchaseOrderRequest) =>
      purchaseOrderService.createPurchaseOrder(request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: purchaseOrderKeys.all });
      message.success('Purchase order created successfully');
    },
    onError: (error: any) => {
      message.error(error?.response?.data?.message || 'Failed to create purchase order');
    },
  });
}

export function useApprovePurchaseOrder() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: number) => purchaseOrderService.approvePurchaseOrder(id),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: purchaseOrderKeys.all });
      queryClient.invalidateQueries({ queryKey: purchaseOrderKeys.detail(data.id) });
      message.success('Purchase order approved successfully');
    },
    onError: (error: any) => {
      message.error(error?.response?.data?.message || 'Failed to approve purchase order');
    },
  });
}

export function useReceivePurchaseOrder() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: ReceivePurchaseOrderRequest) =>
      purchaseOrderService.receivePurchaseOrder(request),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: purchaseOrderKeys.all });
      queryClient.invalidateQueries({ queryKey: purchaseOrderKeys.detail(data.id) });
      message.success('Purchase order received successfully');
    },
    onError: (error: any) => {
      message.error(error?.response?.data?.message || 'Failed to receive purchase order');
    },
  });
}

export function useCancelPurchaseOrder() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, reason }: { id: number; reason?: string }) =>
      purchaseOrderService.cancelPurchaseOrder(id, reason),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: purchaseOrderKeys.all });
      queryClient.invalidateQueries({ queryKey: purchaseOrderKeys.detail(data.id) });
      message.success('Purchase order cancelled successfully');
    },
    onError: (error: any) => {
      message.error(error?.response?.data?.message || 'Failed to cancel purchase order');
    },
  });
}

