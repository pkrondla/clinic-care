import apiClient, { api } from './apiClient';

export interface InitiateOnlinePaymentRequest {
  invoiceId: number;
  returnUrl: string;
  cancelUrl?: string;
}

export interface OnlinePaymentInitiation {
  invoiceId: number;
  invoiceNumber: string;
  transactionId: string;
  paymentUrl: string;
  amount: number;
  currency: string;
  additionalData: Record<string, string>;
}

const paymentService = {
  /**
   * Initiate online payment for an invoice
   */
  async initiateOnlinePayment(request: InitiateOnlinePaymentRequest): Promise<OnlinePaymentInitiation> {
    const response = await api.post<{ data: OnlinePaymentInitiation }>(
      `/invoices/${request.invoiceId}/pay/online`,
      {
        invoiceId: request.invoiceId,
        returnUrl: request.returnUrl,
        cancelUrl: request.cancelUrl,
      }
    );
    if (!response.data?.data) {
      throw new Error('Failed to initiate online payment');
    }
    return response.data.data;
  },
};

export default paymentService;

