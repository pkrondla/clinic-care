import { useState } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import {
  Card,
  Button,
  Space,
  Tag,
  Typography,
  Row,
  Col,
  Table,
  Descriptions,
  Modal,
  Form,
  InputNumber,
  Input,
  Select,
  message,
  Divider,
} from 'antd'
import {
  ArrowLeftOutlined,
  DollarOutlined,
  PrinterOutlined,
  DownloadOutlined,
} from '@ant-design/icons'
import { useInvoice, usePayInvoice, useUpdateCourierDocket } from '@core/hooks/queries/useInvoices'
import { useInitiateOnlinePayment } from '@core/hooks/queries/usePayments'
import invoiceService from '@core/services/invoiceService'
import type { InvoiceItem } from '@core/services/invoiceService'
import dayjs from 'dayjs'

const { Title, Text } = Typography

const PAYMENT_METHODS = [
  { label: 'Cash', value: 'Cash' },
  { label: 'Card', value: 'Card' },
  { label: 'UPI', value: 'UPI' },
  { label: 'Bank Transfer', value: 'Bank Transfer' },
  { label: 'Cheque', value: 'Cheque' },
  { label: 'Online Payment', value: 'Online' },
]

export const InvoiceDetailPage = () => {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const [searchParams] = useSearchParams()
  const [paymentModalVisible, setPaymentModalVisible] = useState(false)
  const [courierModalVisible, setCourierModalVisible] = useState(false)
  const [paymentForm] = Form.useForm()
  const [courierForm] = Form.useForm()

  const invoiceId = id ? parseInt(id) : 0
  const { data: invoice, isLoading, refetch } = useInvoice(invoiceId)
  const payInvoiceMutation = usePayInvoice()
  const updateCourierMutation = useUpdateCourierDocket()
  const initiateOnlinePaymentMutation = useInitiateOnlinePayment()

  // Handle payment return from gateway
  useEffect(() => {
    const paymentStatus = searchParams.get('payment')
    if (paymentStatus === 'success') {
      message.success('Payment completed successfully!')
      refetch()
      // Remove query parameter
      navigate(`/invoices/${invoiceId}`, { replace: true })
    } else if (paymentStatus === 'cancelled') {
      message.warning('Payment was cancelled')
      navigate(`/invoices/${invoiceId}`, { replace: true })
    }
  }, [searchParams, refetch, navigate, invoiceId])

  const handlePayInvoice = async () => {
    try {
      const values = await paymentForm.validateFields()
      
      // If payment method is "Online", initiate online payment
      if (values.paymentMethod === 'Online') {
        const returnUrl = `${window.location.origin}/invoices/${invoiceId}?payment=success`
        const cancelUrl = `${window.location.origin}/invoices/${invoiceId}?payment=cancelled`
        
        await initiateOnlinePaymentMutation.mutateAsync({
          invoiceId: invoiceId,
          returnUrl,
          cancelUrl,
        })
        // The mutation will redirect to payment URL
        return
      }
      
      // Otherwise, process manual payment
      await payInvoiceMutation.mutateAsync({
        invoiceId: invoiceId,
        amount: values.amount,
        paymentMethod: values.paymentMethod,
        paymentReference: values.paymentReference,
      })
      setPaymentModalVisible(false)
      paymentForm.resetFields()
      refetch()
    } catch (error) {
      console.error('Payment failed:', error)
    }
  }

  const handleDownloadPdf = async () => {
    try {
      const blob = await invoiceService.downloadInvoicePdf(invoiceId)
      const url = window.URL.createObjectURL(blob)
      const link = document.createElement('a')
      link.href = url
      link.download = `Invoice_${invoice?.invoiceNumber || invoiceId}.pdf`
      document.body.appendChild(link)
      link.click()
      document.body.removeChild(link)
      window.URL.revokeObjectURL(url)
      message.success('Invoice PDF downloaded successfully')
    } catch (error) {
      message.error('Failed to download invoice PDF')
    }
  }

  const handleUpdateCourier = async () => {
    try {
      const values = await courierForm.validateFields()
      await updateCourierMutation.mutateAsync({
        invoiceId: invoiceId,
        courierDocketNumber: values.courierDocketNumber,
        courierCompany: values.courierCompany,
        courierTrackingUrl: values.courierTrackingUrl,
      })
      setCourierModalVisible(false)
      courierForm.resetFields()
      refetch()
    } catch (error) {
      // Error is handled by the mutation hook
    }
  }

  const itemColumns = [
    {
      title: 'Item Type',
      dataIndex: 'itemType',
      key: 'itemType',
      width: 120,
    },
    {
      title: 'Description',
      dataIndex: 'description',
      key: 'description',
    },
    {
      title: 'Quantity',
      dataIndex: 'quantity',
      key: 'quantity',
      width: 100,
      align: 'right' as const,
    },
    {
      title: 'Unit Price',
      dataIndex: 'unitPrice',
      key: 'unitPrice',
      width: 120,
      align: 'right' as const,
      render: (price: number) => `₹${price.toFixed(2)}`,
    },
    {
      title: 'Total',
      dataIndex: 'totalPrice',
      key: 'totalPrice',
      width: 120,
      align: 'right' as const,
      render: (price: number) => <strong>₹{price.toFixed(2)}</strong>,
    },
  ]

  if (isLoading) {
    return <div>Loading...</div>
  }

  if (!invoice) {
    return <div>Invoice not found</div>
  }

  const canPay = invoice.balanceAmount > 0 && invoice.status !== 4

  return (
    <div>
      <Space style={{ marginBottom: 24 }}>
        <Button icon={<ArrowLeftOutlined />} onClick={() => navigate('/invoices')}>
          Back
        </Button>
        <Title level={2} style={{ margin: 0 }}>
          Invoice {invoice.invoiceNumber}
        </Title>
      </Space>

      <Row gutter={16}>
        <Col xs={24} lg={16}>
          {/* Invoice Details */}
          <Card title="Invoice Details" style={{ marginBottom: 16 }}>
            <Descriptions column={2} bordered>
              <Descriptions.Item label="Invoice Number">
                <Tag color="blue">{invoice.invoiceNumber}</Tag>
              </Descriptions.Item>
              <Descriptions.Item label="Prescription Number">
                {invoice.prescriptionNumber || '-'}
              </Descriptions.Item>
              <Descriptions.Item label="Patient">
                {invoice.patientName} ({invoice.patientCode})
              </Descriptions.Item>
              <Descriptions.Item label="Clinic">{invoice.clinicName}</Descriptions.Item>
              <Descriptions.Item label="Invoice Date">
                {dayjs(invoice.invoiceDate).format('DD/MM/YYYY HH:mm')}
              </Descriptions.Item>
              <Descriptions.Item label="Status">
                <Tag
                  color={
                    invoice.status === 3
                      ? 'green'
                      : invoice.status === 4
                      ? 'red'
                      : invoice.status === 2
                      ? 'blue'
                      : 'default'
                  }
                >
                  {invoice.statusText}
                </Tag>
              </Descriptions.Item>
            </Descriptions>
          </Card>

          {/* Invoice Items */}
          <Card title="Invoice Items">
            <Table
              columns={itemColumns}
              dataSource={invoice.items}
              rowKey="id"
              pagination={false}
              summary={() => (
                <Table.Summary fixed>
                  <Table.Summary.Row>
                    <Table.Summary.Cell index={0} colSpan={4} align="right">
                      <strong>Subtotal:</strong>
                    </Table.Summary.Cell>
                    <Table.Summary.Cell index={1} align="right">
                      <strong>₹{invoice.totalAmount.toFixed(2)}</strong>
                    </Table.Summary.Cell>
                  </Table.Summary.Row>
                </Table.Summary>
              )}
            />
          </Card>
        </Col>

        <Col xs={24} lg={8}>
          {/* Payment Summary */}
          <Card title="Payment Summary" style={{ marginBottom: 16 }}>
            <Space direction="vertical" style={{ width: '100%' }} size="large">
              <div>
                <Text type="secondary">Total Amount</Text>
                <div style={{ fontSize: '24px', fontWeight: 'bold' }}>
                  ₹{invoice.totalAmount.toFixed(2)}
                </div>
              </div>
              <Divider />
              <div>
                <Text type="secondary">Paid Amount</Text>
                <div style={{ fontSize: '20px', color: '#3f8600' }}>
                  ₹{invoice.paidAmount.toFixed(2)}
                </div>
              </div>
              <div>
                <Text type="secondary">Balance Amount</Text>
                <div
                  style={{
                    fontSize: '20px',
                    color: invoice.balanceAmount > 0 ? '#cf1322' : '#3f8600',
                  }}
                >
                  ₹{invoice.balanceAmount.toFixed(2)}
                </div>
              </div>
              {invoice.paymentDate && (
                <>
                  <Divider />
                  <div>
                    <Text type="secondary">Payment Date</Text>
                    <div>{dayjs(invoice.paymentDate).format('DD/MM/YYYY HH:mm')}</div>
                  </div>
                  <div>
                    <Text type="secondary">Payment Method</Text>
                    <div>{invoice.paymentMethod}</div>
                  </div>
                  {invoice.paymentReference && (
                    <div>
                      <Text type="secondary">Payment Reference</Text>
                      <div>{invoice.paymentReference}</div>
                    </div>
                  )}
                </>
              )}
              {invoice.courierDocketNumber && (
                <>
                  <Divider />
                  <div>
                    <Text type="secondary">Courier Docket</Text>
                    <div style={{ fontWeight: 'bold' }}>{invoice.courierDocketNumber}</div>
                  </div>
                  {invoice.courierCompany && (
                    <div>
                      <Text type="secondary">Courier Company</Text>
                      <div>{invoice.courierCompany}</div>
                    </div>
                  )}
                  {invoice.courierStatusText && (
                    <div>
                      <Text type="secondary">Courier Status</Text>
                      <div>
                        <Tag color={
                          invoice.courierStatus === 4 ? 'green' :
                          invoice.courierStatus === 5 ? 'red' :
                          invoice.courierStatus === 3 ? 'orange' :
                          'blue'
                        }>
                          {invoice.courierStatusText}
                        </Tag>
                      </div>
                    </div>
                  )}
                  {invoice.courierTrackingUrl && (
                    <div>
                      <Text type="secondary">Tracking</Text>
                      <div>
                        <a href={invoice.courierTrackingUrl} target="_blank" rel="noopener noreferrer">
                          Track Shipment
                        </a>
                      </div>
                    </div>
                  )}
                </>
              )}
            </Space>
          </Card>

          {/* Actions */}
          <Card>
            <Space direction="vertical" style={{ width: '100%' }} size="middle">
              {canPay && (
                <Button
                  type="primary"
                  icon={<DollarOutlined />}
                  block
                  onClick={() => setPaymentModalVisible(true)}
                >
                  Process Payment
                </Button>
              )}
              {invoice.courierCharges > 0 && !invoice.courierDocketNumber && (
                <Button
                  type="default"
                  block
                  onClick={() => setCourierModalVisible(true)}
                >
                  Update Courier Docket
                </Button>
              )}
              <Button
                icon={<DownloadOutlined />}
                block
                onClick={handleDownloadPdf}
              >
                Download PDF
              </Button>
              <Button icon={<PrinterOutlined />} block onClick={handleDownloadPdf}>
                Print Invoice
              </Button>
            </Space>
          </Card>
        </Col>
      </Row>

      {/* Payment Modal */}
      <Modal
        title="Process Payment"
        open={paymentModalVisible}
        onOk={handlePayInvoice}
        onCancel={() => {
          setPaymentModalVisible(false)
          paymentForm.resetFields()
        }}
        confirmLoading={payInvoiceMutation.isPending || initiateOnlinePaymentMutation.isPending}
      >
        <Form form={paymentForm} layout="vertical">
          <Form.Item
            label="Amount"
            name="amount"
            rules={[
              { required: true, message: 'Please enter payment amount' },
              {
                type: 'number',
                min: 0.01,
                max: invoice.balanceAmount,
                message: `Amount must be between ₹0.01 and ₹${invoice.balanceAmount.toFixed(2)}`,
              },
            ]}
            initialValue={invoice.balanceAmount}
          >
            <InputNumber
              prefix="₹"
              style={{ width: '100%' }}
              precision={2}
              max={invoice.balanceAmount}
            />
          </Form.Item>
          <Form.Item
            label="Payment Method"
            name="paymentMethod"
            rules={[{ required: true, message: 'Please select payment method' }]}
          >
            <Select 
              placeholder="Select payment method"
              onChange={(value) => {
                // If Online is selected, hide amount and reference fields
                if (value === 'Online') {
                  paymentForm.setFieldsValue({ amount: invoice.balanceAmount })
                }
              }}
            >
              {PAYMENT_METHODS.map((method) => (
                <Select.Option key={method.value} value={method.value}>
                  {method.label}
                </Select.Option>
              ))}
            </Select>
          </Form.Item>
          <Form.Item
            noStyle
            shouldUpdate={(prevValues, currentValues) => prevValues.paymentMethod !== currentValues.paymentMethod}
          >
            {({ getFieldValue }) => {
              const paymentMethod = getFieldValue('paymentMethod')
              if (paymentMethod === 'Online') {
                return (
                  <div style={{ padding: '12px', background: '#f0f0f0', borderRadius: '4px', marginBottom: '16px' }}>
                    <Text type="secondary">
                      You will be redirected to the payment gateway to complete the payment.
                    </Text>
                  </div>
                )
              }
              return (
                <>
                  <Form.Item label="Payment Reference" name="paymentReference">
                    <Input placeholder="Optional reference number" />
                  </Form.Item>
                </>
              )
            }}
          </Form.Item>
        </Form>
      </Modal>

      {/* Courier Docket Modal */}
      <Modal
        title="Update Courier Docket"
        open={courierModalVisible}
        onOk={handleUpdateCourier}
        onCancel={() => {
          setCourierModalVisible(false)
          courierForm.resetFields()
        }}
        confirmLoading={updateCourierMutation.isPending}
      >
        <Form form={courierForm} layout="vertical">
          <Form.Item
            label="Courier Docket Number"
            name="courierDocketNumber"
            rules={[{ required: true, message: 'Please enter courier docket number' }]}
          >
            <Input placeholder="Enter docket number" />
          </Form.Item>
          <Form.Item
            label="Courier Company"
            name="courierCompany"
            rules={[{ required: true, message: 'Please enter courier company name' }]}
          >
            <Input placeholder="Enter courier company" />
          </Form.Item>
          <Form.Item
            label="Tracking URL (Optional)"
            name="courierTrackingUrl"
          >
            <Input placeholder="Enter tracking URL" />
          </Form.Item>
        </Form>
      </Modal>
    </div>
  )
}

