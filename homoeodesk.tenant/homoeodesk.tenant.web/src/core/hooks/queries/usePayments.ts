import { useMutation } from '@tanstack/react-query';
import paymentService, { InitiateOnlinePaymentRequest } from '../../services/paymentService';
import { message } from 'antd';

export function useInitiateOnlinePayment() {
  return useMutation({
    mutationFn: (request: InitiateOnlinePaymentRequest) =>
      paymentService.initiateOnlinePayment(request),
    onSuccess: (data) => {
      // Redirect to payment URL
      if (data.paymentUrl) {
        window.location.href = data.paymentUrl;
      } else {
        message.warning('Payment URL not available');
      }
    },
    onError: (error: any) => {
      message.error(error?.response?.data?.message || 'Failed to initiate online payment');
    },
  });
}

