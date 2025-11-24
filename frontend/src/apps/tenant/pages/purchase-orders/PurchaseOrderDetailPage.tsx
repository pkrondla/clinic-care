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
  DatePicker,
  message,
  Divider,
  Popconfirm,
} from 'antd'
import {
  ArrowLeftOutlined,
  CheckOutlined,
  CloseOutlined,
  ShoppingCartOutlined,
} from '@ant-design/icons'
import {
  usePurchaseOrder,
  useApprovePurchaseOrder,
  useReceivePurchaseOrder,
  useCancelPurchaseOrder,
} from '@core/hooks/queries/usePurchaseOrders'
import type { PurchaseOrderItem } from '@core/services/purchaseOrderService'
import dayjs from 'dayjs'

const { Title, Text } = Typography
const { TextArea } = Input

export const PurchaseOrderDetailPage = () => {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const [receiveModalVisible, setReceiveModalVisible] = useState(false)
  const [receiveForm] = Form.useForm()

  const purchaseOrderId = id ? parseInt(id) : 0
  const { data: purchaseOrder, isLoading, refetch } = usePurchaseOrder(purchaseOrderId)
  const approveMutation = useApprovePurchaseOrder()
  const receiveMutation = useReceivePurchaseOrder()
  const cancelMutation = useCancelPurchaseOrder()

  const handleApprove = async () => {
    await approveMutation.mutateAsync(purchaseOrderId)
    refetch()
  }

  const handleReceive = async () => {
    try {
      const values = await receiveForm.validateFields()
      const receivedItems = purchaseOrder!.items.map((item, index) => ({
        purchaseOrderItemId: item.id,
        receivedQuantity: values[`received_${item.id}`] || 0,
        batchNumber: values[`batch_${item.id}`],
        expiryDate: values[`expiry_${item.id}`]
          ? dayjs(values[`expiry_${item.id}`]).format('YYYY-MM-DD')
          : undefined,
      })).filter(item => item.receivedQuantity > 0)

      if (receivedItems.length === 0) {
        message.error('Please enter received quantities for at least one item')
        return
      }

      await receiveMutation.mutateAsync({
        id: purchaseOrderId,
        receivedItems,
      })
      setReceiveModalVisible(false)
      receiveForm.resetFields()
      refetch()
    } catch (error) {
      console.error('Receive failed:', error)
    }
  }

  const handleCancel = async () => {
    await cancelMutation.mutateAsync({ id: purchaseOrderId })
    refetch()
  }

  const itemColumns = [
    {
      title: 'Medicine',
      dataIndex: 'medicineName',
      key: 'medicineName',
    },
    {
      title: 'Quantity',
      dataIndex: 'quantity',
      key: 'quantity',
      width: 100,
      align: 'right' as const,
    },
    {
      title: 'Received',
      dataIndex: 'receivedQuantity',
      key: 'receivedQuantity',
      width: 100,
      align: 'right' as const,
      render: (received: number | undefined, record: PurchaseOrderItem) => (
        <span style={{ color: received === record.quantity ? '#3f8600' : '#faad14' }}>
          {received || 0} / {record.quantity}
        </span>
      ),
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

  if (!purchaseOrder) {
    return <div>Purchase order not found</div>
  }

  const canApprove = purchaseOrder.status === 1 || purchaseOrder.status === 2
  const canReceive = purchaseOrder.status === 3 || purchaseOrder.status === 4 || purchaseOrder.status === 5
  const canCancel = purchaseOrder.status !== 6 && purchaseOrder.status !== 7

  return (
    <div>
      <Space style={{ marginBottom: 24 }}>
        <Button icon={<ArrowLeftOutlined />} onClick={() => navigate('/purchase-orders')}>
          Back
        </Button>
        <Title level={2} style={{ margin: 0 }}>
          Purchase Order {purchaseOrder.orderNumber}
        </Title>
      </Space>

      <Row gutter={16}>
        <Col xs={24} lg={16}>
          {/* Purchase Order Details */}
          <Card title="Purchase Order Details" style={{ marginBottom: 16 }}>
            <Descriptions column={2} bordered>
              <Descriptions.Item label="Order Number">
                <Tag color="blue">{purchaseOrder.orderNumber}</Tag>
              </Descriptions.Item>
              <Descriptions.Item label="Status">
                <Tag color={
                  purchaseOrder.status === 6 ? 'green' :
                  purchaseOrder.status === 7 ? 'red' :
                  purchaseOrder.status === 5 ? 'purple' :
                  purchaseOrder.status === 3 ? 'blue' :
                  'orange'
                }>
                  {purchaseOrder.statusText}
                </Tag>
              </Descriptions.Item>
              <Descriptions.Item label="Supplier">
                {purchaseOrder.supplierName}
              </Descriptions.Item>
              <Descriptions.Item label="Clinic">
                {purchaseOrder.clinicName}
              </Descriptions.Item>
              <Descriptions.Item label="Order Date">
                {dayjs(purchaseOrder.orderDate).format('DD/MM/YYYY')}
              </Descriptions.Item>
              {purchaseOrder.expectedDeliveryDate && (
                <Descriptions.Item label="Expected Delivery">
                  {dayjs(purchaseOrder.expectedDeliveryDate).format('DD/MM/YYYY')}
                </Descriptions.Item>
              )}
              {purchaseOrder.approvedDate && (
                <>
                  <Descriptions.Item label="Approved Date">
                    {dayjs(purchaseOrder.approvedDate).format('DD/MM/YYYY HH:mm')}
                  </Descriptions.Item>
                  <Descriptions.Item label="Approved By">
                    {purchaseOrder.approvedByUserName || '-'}
                  </Descriptions.Item>
                </>
              )}
              {purchaseOrder.receivedDate && (
                <>
                  <Descriptions.Item label="Received Date">
                    {dayjs(purchaseOrder.receivedDate).format('DD/MM/YYYY HH:mm')}
                  </Descriptions.Item>
                  <Descriptions.Item label="Received By">
                    {purchaseOrder.receivedByUserName || '-'}
                  </Descriptions.Item>
                </>
              )}
            </Descriptions>
          </Card>

          {/* Purchase Order Items */}
          <Card title="Order Items">
            <Table
              columns={itemColumns}
              dataSource={purchaseOrder.items}
              rowKey="id"
              pagination={false}
              summary={() => (
                <Table.Summary fixed>
                  <Table.Summary.Row>
                    <Table.Summary.Cell index={0} colSpan={3} align="right">
                      <strong>Subtotal:</strong>
                    </Table.Summary.Cell>
                    <Table.Summary.Cell index={1} align="right">
                      <strong>₹{purchaseOrder.totalAmount.toFixed(2)}</strong>
                    </Table.Summary.Cell>
                  </Table.Summary.Row>
                  {purchaseOrder.discountAmount && purchaseOrder.discountAmount > 0 && (
                    <Table.Summary.Row>
                      <Table.Summary.Cell index={0} colSpan={3} align="right">
                        <strong>Discount:</strong>
                      </Table.Summary.Cell>
                      <Table.Summary.Cell index={1} align="right">
                        <strong>-₹{purchaseOrder.discountAmount.toFixed(2)}</strong>
                      </Table.Summary.Cell>
                    </Table.Summary.Row>
                  )}
                  {purchaseOrder.taxAmount && purchaseOrder.taxAmount > 0 && (
                    <Table.Summary.Row>
                      <Table.Summary.Cell index={0} colSpan={3} align="right">
                        <strong>Tax:</strong>
                      </Table.Summary.Cell>
                      <Table.Summary.Cell index={1} align="right">
                        <strong>₹{purchaseOrder.taxAmount.toFixed(2)}</strong>
                      </Table.Summary.Cell>
                    </Table.Summary.Row>
                  )}
                  <Table.Summary.Row>
                    <Table.Summary.Cell index={0} colSpan={3} align="right">
                      <strong>Grand Total:</strong>
                    </Table.Summary.Cell>
                    <Table.Summary.Cell index={1} align="right">
                      <strong style={{ fontSize: '18px' }}>₹{purchaseOrder.grandTotal.toFixed(2)}</strong>
                    </Table.Summary.Cell>
                  </Table.Summary.Row>
                </Table.Summary>
              )}
            />
          </Card>
        </Col>

        <Col xs={24} lg={8}>
          {/* Actions */}
          <Card>
            <Space direction="vertical" style={{ width: '100%' }} size="middle">
              {canApprove && (
                <Button
                  type="primary"
                  icon={<CheckOutlined />}
                  block
                  onClick={handleApprove}
                  loading={approveMutation.isPending}
                >
                  Approve Order
                </Button>
              )}
              {canReceive && (
                <Button
                  type="primary"
                  icon={<ShoppingCartOutlined />}
                  block
                  onClick={() => setReceiveModalVisible(true)}
                >
                  Receive Items
                </Button>
              )}
              {canCancel && (
                <Popconfirm
                  title="Are you sure you want to cancel this purchase order?"
                  onConfirm={handleCancel}
                  okText="Yes"
                  cancelText="No"
                >
                  <Button
                    danger
                    icon={<CloseOutlined />}
                    block
                    loading={cancelMutation.isPending}
                  >
                    Cancel Order
                  </Button>
                </Popconfirm>
              )}
            </Space>
          </Card>

          {/* Summary */}
          <Card title="Summary" style={{ marginTop: 16 }}>
            <Space direction="vertical" style={{ width: '100%' }} size="large">
              <div>
                <Text type="secondary">Total Amount</Text>
                <div style={{ fontSize: '20px', fontWeight: 'bold' }}>
                  ₹{purchaseOrder.totalAmount.toFixed(2)}
                </div>
              </div>
              {purchaseOrder.discountAmount && purchaseOrder.discountAmount > 0 && (
                <div>
                  <Text type="secondary">Discount</Text>
                  <div style={{ fontSize: '16px', color: '#3f8600' }}>
                    -₹{purchaseOrder.discountAmount.toFixed(2)}
                  </div>
                </div>
              )}
              {purchaseOrder.taxAmount && purchaseOrder.taxAmount > 0 && (
                <div>
                  <Text type="secondary">Tax</Text>
                  <div style={{ fontSize: '16px' }}>
                    ₹{purchaseOrder.taxAmount.toFixed(2)}
                  </div>
                </div>
              )}
              <Divider />
              <div>
                <Text type="secondary">Grand Total</Text>
                <div style={{ fontSize: '24px', fontWeight: 'bold', color: '#1890ff' }}>
                  ₹{purchaseOrder.grandTotal.toFixed(2)}
                </div>
              </div>
            </Space>
          </Card>
        </Col>
      </Row>

      {/* Receive Items Modal */}
      <Modal
        title="Receive Items"
        open={receiveModalVisible}
        onOk={handleReceive}
        onCancel={() => {
          setReceiveModalVisible(false)
          receiveForm.resetFields()
        }}
        width={800}
        confirmLoading={receiveMutation.isPending}
      >
        <Form form={receiveForm} layout="vertical">
          {purchaseOrder.items.map((item) => {
            const remaining = item.quantity - (item.receivedQuantity || 0)
            return (
              <div key={item.id} style={{ marginBottom: 16, padding: 16, border: '1px solid #f0f0f0', borderRadius: 4 }}>
                <Text strong>{item.medicineName}</Text>
                <div style={{ marginTop: 8 }}>
                  <Row gutter={16}>
                    <Col span={8}>
                      <Form.Item
                        label="Received Quantity"
                        name={`received_${item.id}`}
                        rules={[
                          { required: true, message: 'Please enter received quantity' },
                          {
                            type: 'number',
                            min: 1,
                            max: remaining,
                            message: `Must be between 1 and ${remaining}`,
                          },
                        ]}
                        initialValue={remaining}
                      >
                        <InputNumber
                          style={{ width: '100%' }}
                          min={1}
                          max={remaining}
                          placeholder={`Max: ${remaining}`}
                        />
                      </Form.Item>
                    </Col>
                    <Col span={8}>
                      <Form.Item
                        label="Batch Number"
                        name={`batch_${item.id}`}
                      >
                        <Input placeholder="Enter batch number" />
                      </Form.Item>
                    </Col>
                    <Col span={8}>
                      <Form.Item
                        label="Expiry Date"
                        name={`expiry_${item.id}`}
                      >
                        <DatePicker style={{ width: '100%' }} />
                      </Form.Item>
                    </Col>
                  </Row>
                </div>
              </div>
            )
          })}
        </Form>
      </Modal>
    </div>
  )
}

