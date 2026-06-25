import { useState, useEffect } from 'react'
import { useParams, useNavigate, useSearchParams } from 'react-router-dom'
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
import type { InvoiceItem, PrescriptionItem } from '@core/services/invoiceService'
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
        
        initiateOnlinePaymentMutation.mutate(
          {
            invoiceId: invoiceId,
            returnUrl,
            cancelUrl,
          },
          {
            onSuccess: () => {
              // The mutation will redirect to payment URL
            },
            onError: (error: any) => {
              // Error message is shown by the mutation hook
              console.error('Online payment initiation failed:', error)
            },
          }
        )
        return
      }
      
      // Otherwise, process manual payment
      payInvoiceMutation.mutate(
        {
          invoiceId: invoiceId,
          amount: values.amount,
          paymentMethod: values.paymentMethod,
          paymentReference: values.paymentReference,
        },
        {
          onSuccess: async () => {
            // Success message is shown by the mutation hook
            // Close modal and reset form after successful payment
            setPaymentModalVisible(false)
            paymentForm.resetFields()
            // Refetch invoice data to update payment summary
            await refetch()
          },
          onError: (error: any) => {
            // Error message is shown by the mutation hook
            console.error('Payment failed:', error)
            // Don't close modal on error so user can retry
          },
        }
      )
    } catch (error: any) {
      // Validation error
      console.error('Form validation failed:', error)
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
      // Success message is shown by the mutation hook
      // Close modal and reset form
      setCourierModalVisible(false)
      courierForm.resetFields()
      // Refetch invoice data to show updated courier details
      await refetch()
    } catch (error) {
      // Error is handled by the mutation hook
      // Don't close modal on error so user can retry
    }
  }

  const DISPENSING_FORMS: Record<number, string> = {
    1: 'Globules',
    2: 'Tablets',
    3: 'Packet',
    4: 'Liquid',
    5: 'Tonic'
  }

  const formatQuantity = (quantity: number | undefined, dispensingForm: number): string => {
    if (quantity === undefined) return '-'
    if (dispensingForm === 4 || dispensingForm === 5) {
      return `${quantity} ml`
    }
    return quantity.toString()
  }

  // Create combined data source for medicines and other charges
  const getCombinedItems = () => {
    const items: Array<{
      id: string | number
      type: 'medicine' | 'other'
      serialNumber: number
      description: string
      details: string
      quantity: string | number
      unitPrice: number
      totalPrice: number
    }> = []

    let serialNumber = 0
    let medicineSerialNumber = 0

    // Add prescription medicines first
    if (invoice.prescriptionItems && invoice.prescriptionItems.length > 0) {
      invoice.prescriptionItems.forEach((prescriptionItem, index) => {
        medicineSerialNumber++
        serialNumber++
        
        // Build details string: Dosage, Frequency, Timing, Duration (comma-separated)
        const detailsParts: string[] = []
        if (prescriptionItem.dosage) detailsParts.push(prescriptionItem.dosage)
        if (prescriptionItem.frequency) detailsParts.push(prescriptionItem.frequency)
        if (prescriptionItem.timing) detailsParts.push(prescriptionItem.timing)
        if (prescriptionItem.duration) detailsParts.push(prescriptionItem.duration)
        
        const details = detailsParts.length > 0 ? detailsParts.join(', ') : '-'
        const instructions = prescriptionItem.instructions || null
        
        items.push({
          id: `medicine-${prescriptionItem.id}`,
          type: 'medicine',
          serialNumber: medicineSerialNumber,
          description: `Medicine #${medicineSerialNumber}${prescriptionItem.containerSize && prescriptionItem.dispensingForm === 1 ? ` (${DISPENSING_FORMS[prescriptionItem.dispensingForm]} ${prescriptionItem.containerSize} dram)` : prescriptionItem.dispensingForm ? ` (${DISPENSING_FORMS[prescriptionItem.dispensingForm]})` : ''}`,
          details,
          instructions,
          quantity: formatQuantity(prescriptionItem.quantity, prescriptionItem.dispensingForm),
          unitPrice: prescriptionItem.unitPrice,
          totalPrice: prescriptionItem.totalPrice
        })
      })
    }

    // Add other invoice items (Consultation, Courier)
    const otherItems = invoice.items.filter(item => item.itemType !== 'Medicine')
    otherItems.forEach((item) => {
      serialNumber++
      items.push({
        id: `other-${item.id}`,
        type: 'other',
        serialNumber,
        description: item.description,
        details: '-',
        instructions: null,
        quantity: item.quantity,
        unitPrice: item.unitPrice,
        totalPrice: item.totalPrice
      })
    })

    return items
  }

  // Type for combined items
  type CombinedItem = {
    id: string | number
    type: 'medicine' | 'other'
    serialNumber: number
    description: string
    details: string
    instructions?: string | null
    quantity: string | number
    unitPrice: number
    totalPrice: number
  }

  // Unified columns for all items
  const invoiceColumns = [
    {
      title: 'S.No',
      key: 'serialNumber',
      width: 60,
      align: 'center' as const,
      render: (_: any, record: CombinedItem) => record.serialNumber
    },
    {
      title: 'Description',
      key: 'description',
      render: (_: any, record: CombinedItem) => (
        <div style={{ whiteSpace: 'normal', wordWrap: 'break-word' }}>
          <div style={{ fontWeight: 500, marginBottom: '4px' }}>{record.description}</div>
          {record.details !== '-' && (
            <div style={{ color: '#666', marginBottom: '4px' }}>
              {record.details}
            </div>
          )}
          {record.instructions && (
            <div style={{ color: '#1890ff', marginTop: '4px' }}>
              Instructions: {record.instructions}
            </div>
          )}
        </div>
      )
    },
    {
      title: 'Qty',
      key: 'quantity',
      width: 80,
      align: 'right' as const,
      render: (_: any, record: CombinedItem) => record.quantity
    },
    {
      title: 'Unit Price',
      key: 'unitPrice',
      width: 100,
      align: 'right' as const,
      render: (_: any, record: CombinedItem) => `₹${record.unitPrice.toFixed(2)}`
    },
    {
      title: 'Total',
      key: 'totalPrice',
      width: 110,
      align: 'right' as const,
      render: (_: any, record: CombinedItem) => <strong>₹{record.totalPrice.toFixed(2)}</strong>
    }
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

          {/* Invoice Items - Combined Table */}
          <Card title="Invoice Items">
            <Table
              columns={invoiceColumns}
              dataSource={invoice ? getCombinedItems() : []}
              rowKey="id"
              pagination={false}
              size="small"
              scroll={{ x: 'max-content' }}
              style={{ width: '100%' }}
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
            <Space direction="vertical" style={{ width: '100%' }} size="small">
              <div>
                <Text type="secondary" style={{ fontSize: '12px' }}>Total Amount</Text>
                <div style={{ fontSize: '20px', fontWeight: 'bold', marginTop: '4px' }}>
                  ₹{invoice.totalAmount.toFixed(2)}
                </div>
              </div>
              <Divider style={{ margin: '8px 0' }} />
              <Row gutter={16}>
                <Col span={12}>
                  <div>
                    <Text type="secondary" style={{ fontSize: '12px' }}>Paid Amount</Text>
                    <div style={{ fontSize: '16px', color: '#3f8600', marginTop: '4px' }}>
                      ₹{invoice.paidAmount.toFixed(2)}
                    </div>
                  </div>
                </Col>
                <Col span={12}>
                  <div>
                    <Text type="secondary" style={{ fontSize: '12px' }}>Balance Amount</Text>
                    <div
                      style={{
                        fontSize: '16px',
                        color: invoice.balanceAmount > 0 ? '#cf1322' : '#3f8600',
                        marginTop: '4px',
                      }}
                    >
                      ₹{invoice.balanceAmount.toFixed(2)}
                    </div>
                  </div>
                </Col>
              </Row>
              {invoice.paymentDate && (
                <>
                  <Divider style={{ margin: '8px 0' }} />
                  <Row gutter={16}>
                    <Col span={12}>
                      <div>
                        <Text type="secondary" style={{ fontSize: '12px' }}>Payment Date</Text>
                        <div style={{ fontSize: '13px', marginTop: '4px' }}>{dayjs(invoice.paymentDate).format('DD/MM/YYYY HH:mm')}</div>
                      </div>
                    </Col>
                    <Col span={12}>
                      <div>
                        <Text type="secondary" style={{ fontSize: '12px' }}>Payment Method</Text>
                        <div style={{ fontSize: '13px', marginTop: '4px' }}>{invoice.paymentMethod}</div>
                      </div>
                    </Col>
                  </Row>
                  {invoice.paymentReference && (
                    <div>
                      <Text type="secondary" style={{ fontSize: '12px' }}>Payment Reference</Text>
                      <div style={{ fontSize: '13px', marginTop: '4px' }}>{invoice.paymentReference}</div>
                    </div>
                  )}
                </>
              )}
              {invoice.courierDocketNumber && (
                <>
                  <Divider style={{ margin: '8px 0' }} />
                  <div>
                    <Text type="secondary" style={{ fontSize: '12px' }}>Courier Docket</Text>
                    <div style={{ fontWeight: 'bold', fontSize: '13px', marginTop: '4px' }}>{invoice.courierDocketNumber}</div>
                  </div>
                  {invoice.courierCompany && (
                    <div>
                      <Text type="secondary" style={{ fontSize: '12px' }}>Courier Company</Text>
                      <div style={{ fontSize: '13px', marginTop: '4px' }}>{invoice.courierCompany}</div>
                    </div>
                  )}
                  {invoice.courierStatusText && (
                    <div>
                      <Text type="secondary" style={{ fontSize: '12px' }}>Courier Status</Text>
                      <div style={{ marginTop: '4px' }}>
                        <Tag color={
                          invoice.courierStatus === 4 ? 'green' :
                          invoice.courierStatus === 5 ? 'red' :
                          invoice.courierStatus === 3 ? 'orange' :
                          'blue'
                        } size="small">
                          {invoice.courierStatusText}
                        </Tag>
                      </div>
                    </div>
                  )}
                  {invoice.courierTrackingUrl && (
                    <div>
                      <Text type="secondary" style={{ fontSize: '12px' }}>Tracking</Text>
                      <div style={{ marginTop: '4px' }}>
                        <a href={invoice.courierTrackingUrl} target="_blank" rel="noopener noreferrer" style={{ fontSize: '13px' }}>
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
              {invoice.courierCharges > 0 && (
                <Button
                  type="default"
                  block
                  onClick={() => {
                    // Pre-fill form with existing courier details if available
                    if (invoice.courierDocketNumber) {
                      courierForm.setFieldsValue({
                        courierDocketNumber: invoice.courierDocketNumber,
                        courierCompany: invoice.courierCompany || '',
                        courierTrackingUrl: invoice.courierTrackingUrl || '',
                      })
                    }
                    setCourierModalVisible(true)
                  }}
                >
                  {invoice.courierDocketNumber ? 'Update Courier Docket' : 'Add Courier Docket'}
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
        title={invoice.courierDocketNumber ? "Update Courier Docket" : "Add Courier Docket"}
        open={courierModalVisible}
        onOk={handleUpdateCourier}
        onCancel={() => {
          setCourierModalVisible(false)
          courierForm.resetFields()
        }}
        confirmLoading={updateCourierMutation.isPending}
        okText={invoice.courierDocketNumber ? "Update" : "Add"}
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

