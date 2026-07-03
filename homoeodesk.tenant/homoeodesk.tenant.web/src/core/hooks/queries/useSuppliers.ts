import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import supplierService, {
  GetSuppliersParams,
  CreateSupplierRequest,
  UpdateSupplierRequest,
} from '../../services/supplierService';
import { message } from 'antd';

export const supplierKeys = {
  all: ['suppliers'] as const,
  lists: () => [...supplierKeys.all, 'list'] as const,
  list: (params: GetSuppliersParams) => [...supplierKeys.lists(), params] as const,
  details: () => [...supplierKeys.all, 'detail'] as const,
  detail: (id: number) => [...supplierKeys.details(), id] as const,
};

export function useSuppliers(params: GetSuppliersParams = {}) {
  return useQuery({
    queryKey: supplierKeys.list(params),
    queryFn: () => supplierService.getSuppliers(params),
    staleTime: 30 * 1000, // 30 seconds
  });
}

export function useSupplier(id: number) {
  return useQuery({
    queryKey: supplierKeys.detail(id),
    queryFn: () => supplierService.getSupplier(id),
    enabled: !!id,
    staleTime: 30 * 1000,
  });
}

export function useCreateSupplier() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: CreateSupplierRequest) =>
      supplierService.createSupplier(request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: supplierKeys.all });
      message.success('Supplier created successfully');
    },
    onError: (error: any) => {
      message.error(error?.response?.data?.message || 'Failed to create supplier');
    },
  });
}

export function useUpdateSupplier() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: UpdateSupplierRequest) =>
      supplierService.updateSupplier(request),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: supplierKeys.all });
      queryClient.invalidateQueries({ queryKey: supplierKeys.detail(data.id) });
      message.success('Supplier updated successfully');
    },
    onError: (error: any) => {
      message.error(error?.response?.data?.message || 'Failed to update supplier');
    },
  });
}

export function useDeleteSupplier() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: number) => supplierService.deleteSupplier(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: supplierKeys.all });
      message.success('Supplier deleted successfully');
    },
    onError: (error: any) => {
      message.error(error?.response?.data?.message || 'Failed to delete supplier');
    },
  });
}

