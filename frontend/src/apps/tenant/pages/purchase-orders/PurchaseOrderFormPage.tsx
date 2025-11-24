import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import {
  Card,
  Form,
  Input,
  InputNumber,
  Button,
  message,
  Space,
  Table,
  Modal,
  Select,
  DatePicker,
  Row,
  Col,
  Typography,
  Divider,
} from 'antd'
import {
  SaveOutlined,
  ArrowLeftOutlined,
  PlusOutlined,
  DeleteOutlined,
} from '@ant-design/icons'
import { useCreatePurchaseOrder } from '@core/hooks/queries/usePurchaseOrders'
import { useSuppliers } from '@core/hooks/queries/useSuppliers'
import { useClinics } from '@core/hooks/queries/useClinics'
import { useSelectedClinic } from '@core/stores/authStore'
import { inventoryService } from '@core/services/inventoryService'
import type { CreatePurchaseOrderItemRequest } from '@core/services/purchaseOrderService'
import dayjs from 'dayjs'

const { Title } = Typography
const { TextArea } = Input

interface PurchaseOrderItemForm {
  medicineId: number
  medicineName: string
  quantity: number
  unitPrice: number
  discountAmount?: number
  batchNumber?: string
  expiryDate?: string
  notes?: string
}

export const PurchaseOrderFormPage = () => {
  const [form] = Form.useForm()
  const [itemForm] = Form.useForm()
  const navigate = useNavigate()
  const selectedClinic = useSelectedClinic()

  const [items, setItems] = useState<PurchaseOrderItemForm[]>([])
  const [isItemModalOpen, setIsItemModalOpen] = useState(false)
  const [availableMedicines, setAvailableMedicines] = useState<any[]>([])

  const { data: suppliers = [] } = useSuppliers({ isActive: true })
  const { data: clinics = [] } = useClinics()
  const createMutation = useCreatePurchaseOrder()

  // Load medicines when clinic is selected
  const handleClinicChange = async (clinicId: number) => {
    try {
      const inventory = await inventoryService.getAll(clinicId)
      setAvailableMedicines(inventory.map(inv => ({
        id: inv.medicineId,
        name: inv.medicineName,
        purchasePrice: inv.purchasePrice,
      })))
    } catch (error) {
      console.error('Failed to load medicines:', error)
      message.error('Failed to load medicines')
    }
  }

  const handleAddItem = async () => {
    try {
      const values = await itemForm.validateFields()
      const medicine = availableMedicines.find(m => m.id === values.medicineId)
      
      if (!medicine) {
        message.error('Medicine not found')
        return
      }

      const item: PurchaseOrderItemForm = {
        medicineId: values.medicineId,
        medicineName: medicine.name,
        quantity: values.quantity,
        unitPrice: values.unitPrice,
        discountAmount: values.discountAmount,
        batchNumber: values.batchNumber,
        expiryDate: values.expiryDate ? dayjs(values.expiryDate).format('YYYY-MM-DD') : undefined,
        notes: values.notes,
      }

      setItems([...items, item])
      setIsItemModalOpen(false)
      itemForm.resetFields()
    } catch (error) {
      console.error('Validation failed:', error)
    }
  }

  const handleRemoveItem = (index: number) => {
    setItems(items.filter((_, i) => i !== index))
  }

  const handleSubmit = async () => {
    try {
      if (items.length === 0) {
        message.error('Please add at least one item')
        return
      }

      const values = await form.validateFields()
      
      const request = {
        clinicId: values.clinicId,
        supplierId: values.supplierId,
        orderDate: values.orderDate ? dayjs(values.orderDate).format('YYYY-MM-DD') : undefined,
        expectedDeliveryDate: values.expectedDeliveryDate
          ? dayjs(values.expectedDeliveryDate).format('YYYY-MM-DD')
          : undefined,
        discountAmount: values.discountAmount,
        taxAmount: values.taxAmount,
        notes: values.notes,
        items: items.map(item => ({
          medicineId: item.medicineId,
          quantity: item.quantity,
          unitPrice: item.unitPrice,
          discountAmount: item.discountAmount,
          batchNumber: item.batchNumber,
          expiryDate: item.expiryDate,
          notes: item.notes,
        })) as CreatePurchaseOrderItemRequest[],
      }

      const purchaseOrder = await createMutation.mutateAsync(request)
      message.success(`Purchase order ${purchaseOrder.orderNumber} created successfully!`)
      navigate(`/purchase-orders/${purchaseOrder.id}`)
    } catch (error) {
      console.error('Failed to create purchase order:', error)
    }
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
      title: 'Unit Price',
      dataIndex: 'unitPrice',
      key: 'unitPrice',
      width: 120,
      align: 'right' as const,
      render: (price: number) => `₹${price.toFixed(2)}`,
    },
    {
      title: 'Discount',
      dataIndex: 'discountAmount',
      key: 'discountAmount',
      width: 100,
      align: 'right' as const,
      render: (discount: number | undefined) => discount ? `₹${discount.toFixed(2)}` : '-',
    },
    {
      title: 'Total',
      key: 'total',
      width: 120,
      align: 'right' as const,
      render: (_: any, record: PurchaseOrderItemForm) => {
        const total = record.quantity * record.unitPrice - (record.discountAmount || 0)
        return <strong>₹{total.toFixed(2)}</strong>
      },
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 80,
      render: (_: any, record: PurchaseOrderItemForm, index: number) => (
        <Button
          type="link"
          danger
          icon={<DeleteOutlined />}
          onClick={() => handleRemoveItem(index)}
        />
      ),
    },
  ]

  const calculateTotals = () => {
    const subtotal = items.reduce((sum, item) => {
      return sum + (item.quantity * item.unitPrice - (item.discountAmount || 0))
    }, 0)
    const discount = form.getFieldValue('discountAmount') || 0
    const tax = form.getFieldValue('taxAmount') || 0
    const grandTotal = subtotal - discount + tax
    return { subtotal, discount, tax, grandTotal }
  }

  const totals = calculateTotals()

  return (
    <div>
      <Space style={{ marginBottom: 24 }}>
        <Button icon={<ArrowLeftOutlined />} onClick={() => navigate('/purchase-orders')}>
          Back
        </Button>
        <Title level={2} style={{ margin: 0 }}>
          Create Purchase Order
        </Title>
      </Space>

      <Row gutter={16}>
        <Col xs={24} lg={16}>
          <Card title="Purchase Order Information" style={{ marginBottom: 16 }}>
            <Form form={form} layout="vertical">
              <Row gutter={16}>
                <Col xs={24} sm={12}>
                  <Form.Item
                    label="Clinic"
                    name="clinicId"
                    rules={[{ required: true, message: 'Please select clinic' }]}
                    initialValue={selectedClinic?.id}
                  >
                    <Select
                      placeholder="Select clinic"
                      onChange={handleClinicChange}
                      options={clinics.map(c => ({ label: c.name, value: c.id }))}
                    />
                  </Form.Item>
                </Col>
                <Col xs={24} sm={12}>
                  <Form.Item
                    label="Supplier"
                    name="supplierId"
                    rules={[{ required: true, message: 'Please select supplier' }]}
                  >
                    <Select
                      placeholder="Select supplier"
                      options={suppliers.map(s => ({ label: s.name, value: s.id }))}
                    />
                  </Form.Item>
                </Col>
              </Row>

              <Row gutter={16}>
                <Col xs={24} sm={12}>
                  <Form.Item
                    label="Order Date"
                    name="orderDate"
                    initialValue={dayjs()}
                  >
                    <DatePicker style={{ width: '100%' }} />
                  </Form.Item>
                </Col>
                <Col xs={24} sm={12}>
                  <Form.Item
                    label="Expected Delivery Date"
                    name="expectedDeliveryDate"
                  >
                    <DatePicker style={{ width: '100%' }} />
                  </Form.Item>
                </Col>
              </Row>

              <Row gutter={16}>
                <Col xs={24} sm={12}>
                  <Form.Item
                    label="Discount Amount"
                    name="discountAmount"
                  >
                    <InputNumber
                      prefix="₹"
                      style={{ width: '100%' }}
                      min={0}
                      precision={2}
                    />
                  </Form.Item>
                </Col>
                <Col xs={24} sm={12}>
                  <Form.Item
                    label="Tax Amount"
                    name="taxAmount"
                  >
                    <InputNumber
                      prefix="₹"
                      style={{ width: '100%' }}
                      min={0}
                      precision={2}
                    />
                  </Form.Item>
                </Col>
              </Row>

              <Form.Item label="Notes" name="notes">
                <TextArea rows={3} placeholder="Enter any additional notes" />
              </Form.Item>
            </Form>
          </Card>

          <Card
            title="Order Items"
            extra={
              <Button
                type="primary"
                icon={<PlusOutlined />}
                onClick={() => setIsItemModalOpen(true)}
                disabled={!form.getFieldValue('clinicId')}
              >
                Add Item
              </Button>
            }
          >
            <Table
              columns={itemColumns}
              dataSource={items}
              rowKey={(record, index) => index?.toString() || record.medicineId.toString()}
              pagination={false}
              locale={{ emptyText: 'No items added. Click "Add Item" to add medicines.' }}
              summary={() => (
                <Table.Summary fixed>
                  <Table.Summary.Row>
                    <Table.Summary.Cell index={0} colSpan={4} align="right">
                      <strong>Subtotal:</strong>
                    </Table.Summary.Cell>
                    <Table.Summary.Cell index={1} align="right">
                      <strong>₹{totals.subtotal.toFixed(2)}</strong>
                    </Table.Summary.Cell>
                  </Table.Summary.Row>
                  {totals.discount > 0 && (
                    <Table.Summary.Row>
                      <Table.Summary.Cell index={0} colSpan={4} align="right">
                        <strong>Discount:</strong>
                      </Table.Summary.Cell>
                      <Table.Summary.Cell index={1} align="right">
                        <strong>-₹{totals.discount.toFixed(2)}</strong>
                      </Table.Summary.Cell>
                    </Table.Summary.Row>
                  )}
                  {totals.tax > 0 && (
                    <Table.Summary.Row>
                      <Table.Summary.Cell index={0} colSpan={4} align="right">
                        <strong>Tax:</strong>
                      </Table.Summary.Cell>
                      <Table.Summary.Cell index={1} align="right">
                        <strong>₹{totals.tax.toFixed(2)}</strong>
                      </Table.Summary.Cell>
                    </Table.Summary.Row>
                  )}
                  <Table.Summary.Row>
                    <Table.Summary.Cell index={0} colSpan={4} align="right">
                      <strong>Grand Total:</strong>
                    </Table.Summary.Cell>
                    <Table.Summary.Cell index={1} align="right">
                      <strong style={{ fontSize: '18px' }}>₹{totals.grandTotal.toFixed(2)}</strong>
                    </Table.Summary.Cell>
                  </Table.Summary.Row>
                </Table.Summary>
              )}
            />
          </Card>
        </Col>

        <Col xs={24} lg={8}>
          <Card title="Summary">
            <Space direction="vertical" style={{ width: '100%' }} size="large">
              <div>
                <Typography.Text type="secondary">Subtotal</Typography.Text>
                <div style={{ fontSize: '20px', fontWeight: 'bold' }}>
                  ₹{totals.subtotal.toFixed(2)}
                </div>
              </div>
              {totals.discount > 0 && (
                <div>
                  <Typography.Text type="secondary">Discount</Typography.Text>
                  <div style={{ fontSize: '16px', color: '#3f8600' }}>
                    -₹{totals.discount.toFixed(2)}
                  </div>
                </div>
              )}
              {totals.tax > 0 && (
                <div>
                  <Typography.Text type="secondary">Tax</Typography.Text>
                  <div style={{ fontSize: '16px' }}>
                    ₹{totals.tax.toFixed(2)}
                  </div>
                </div>
              )}
              <Divider />
              <div>
                <Typography.Text type="secondary">Grand Total</Typography.Text>
                <div style={{ fontSize: '24px', fontWeight: 'bold', color: '#1890ff' }}>
                  ₹{totals.grandTotal.toFixed(2)}
                </div>
              </div>
            </Space>
          </Card>

          <Card style={{ marginTop: 16 }}>
            <Space direction="vertical" style={{ width: '100%' }} size="middle">
              <Button
                type="primary"
                icon={<SaveOutlined />}
                block
                size="large"
                onClick={handleSubmit}
                loading={createMutation.isPending}
                disabled={items.length === 0}
              >
                Create Purchase Order
              </Button>
              <Button block onClick={() => navigate('/purchase-orders')}>
                Cancel
              </Button>
            </Space>
          </Card>
        </Col>
      </Row>

      {/* Add Item Modal */}
      <Modal
        title="Add Item"
        open={isItemModalOpen}
        onOk={handleAddItem}
        onCancel={() => {
          setIsItemModalOpen(false)
          itemForm.resetFields()
        }}
      >
        <Form form={itemForm} layout="vertical">
          <Form.Item
            label="Medicine"
            name="medicineId"
            rules={[{ required: true, message: 'Please select medicine' }]}
          >
            <Select
              placeholder="Select medicine"
              showSearch
              filterOption={(input, option) =>
                (option?.label ?? '').toLowerCase().includes(input.toLowerCase())
              }
              options={availableMedicines.map(m => ({
                label: m.name,
                value: m.id,
              }))}
              onChange={(value) => {
                const medicine = availableMedicines.find(m => m.id === value)
                if (medicine) {
                  itemForm.setFieldsValue({ unitPrice: medicine.purchasePrice })
                }
              }}
            />
          </Form.Item>

          <Row gutter={16}>
            <Col span={12}>
              <Form.Item
                label="Quantity"
                name="quantity"
                rules={[
                  { required: true, message: 'Please enter quantity' },
                  { type: 'number', min: 1, message: 'Quantity must be at least 1' },
                ]}
              >
                <InputNumber
                  style={{ width: '100%' }}
                  min={1}
                  placeholder="Enter quantity"
                />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item
                label="Unit Price"
                name="unitPrice"
                rules={[
                  { required: true, message: 'Please enter unit price' },
                  { type: 'number', min: 0, message: 'Price must be greater than 0' },
                ]}
              >
                <InputNumber
                  prefix="₹"
                  style={{ width: '100%' }}
                  min={0}
                  precision={2}
                  placeholder="Enter unit price"
                />
              </Form.Item>
            </Col>
          </Row>

          <Form.Item
            label="Discount Amount"
            name="discountAmount"
          >
            <InputNumber
              prefix="₹"
              style={{ width: '100%' }}
              min={0}
              precision={2}
              placeholder="Optional discount"
            />
          </Form.Item>

          <Row gutter={16}>
            <Col span={12}>
              <Form.Item
                label="Batch Number"
                name="batchNumber"
              >
                <Input placeholder="Optional batch number" />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item
                label="Expiry Date"
                name="expiryDate"
              >
                <DatePicker style={{ width: '100%' }} />
              </Form.Item>
            </Col>
          </Row>

          <Form.Item label="Notes" name="notes">
            <TextArea rows={2} placeholder="Optional notes" />
          </Form.Item>
        </Form>
      </Modal>
    </div>
  )
}

