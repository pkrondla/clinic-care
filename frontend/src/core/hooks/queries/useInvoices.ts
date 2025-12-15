import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import invoiceService, {
  GetInvoicesParams,
  CreateInvoiceFromPrescriptionRequest,
  CreateInvoiceRequest,
  UpdateInvoiceRequest,
  PayInvoiceRequest,
  UpdateCourierDocketRequest,
} from '../../services/invoiceService';
import { message } from 'antd';

export const invoiceKeys = {
  all: ['invoices'] as const,
  lists: () => [...invoiceKeys.all, 'list'] as const,
  list: (params: GetInvoicesParams) => [...invoiceKeys.lists(), params] as const,
  details: () => [...invoiceKeys.all, 'detail'] as const,
  detail: (id: number) => [...invoiceKeys.details(), id] as const,
};

export function useInvoices(params: GetInvoicesParams = {}) {
  return useQuery({
    queryKey: invoiceKeys.list(params),
    queryFn: () => invoiceService.getInvoices(params),
    staleTime: 30 * 1000, // 30 seconds
  });
}

export function useInvoice(id: number) {
  return useQuery({
    queryKey: invoiceKeys.detail(id),
    queryFn: () => invoiceService.getInvoice(id),
    enabled: !!id,
    staleTime: 30 * 1000,
  });
}

export function useCreateInvoiceFromPrescription() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: CreateInvoiceFromPrescriptionRequest) =>
      invoiceService.createInvoiceFromPrescription(request),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: invoiceKeys.all });
      message.success('Invoice created successfully');
    },
    onError: (error: any) => {
      message.error(error?.response?.data?.message || 'Failed to create invoice');
    },
  });
}

export function usePayInvoice() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: PayInvoiceRequest) => invoiceService.payInvoice(request),
    onSuccess: (data, variables) => {
      queryClient.invalidateQueries({ queryKey: invoiceKeys.all });
      queryClient.invalidateQueries({ queryKey: invoiceKeys.detail(variables.invoiceId) });
      message.success('Payment processed successfully');
    },
    onError: (error: any) => {
      message.error(error?.response?.data?.message || 'Failed to process payment');
    },
  });
}

export function useUpdateCourierDocket() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: UpdateCourierDocketRequest) =>
      invoiceService.updateCourierDocket(request),
    onSuccess: (data, variables) => {
      queryClient.invalidateQueries({ queryKey: invoiceKeys.all });
      queryClient.invalidateQueries({ queryKey: invoiceKeys.detail(variables.invoiceId) });
      message.success('Courier docket updated successfully');
    },
    onError: (error: any) => {
      message.error(error?.response?.data?.message || 'Failed to update courier docket');
    },
  });
}

export function useCreateInvoice() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: CreateInvoiceRequest) => invoiceService.createInvoice(request),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: invoiceKeys.all });
      message.success('Invoice created successfully');
    },
    onError: (error: any) => {
      message.error(error?.response?.data?.message || 'Failed to create invoice');
    },
  });
}

export function useUpdateInvoice() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: UpdateInvoiceRequest) => invoiceService.updateInvoice(request),
    onSuccess: (data, variables) => {
      queryClient.invalidateQueries({ queryKey: invoiceKeys.all });
      queryClient.invalidateQueries({ queryKey: invoiceKeys.detail(variables.id) });
      message.success('Invoice updated successfully');
    },
    onError: (error: any) => {
      message.error(error?.response?.data?.message || 'Failed to update invoice');
    },
  });
}

