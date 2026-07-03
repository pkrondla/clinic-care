import { useState } from 'react'
import { Button, Table, Tag, Space, Input, Modal, Form, message, InputNumber, DatePicker, Alert, Card, Statistic, Row, Col } from 'antd'
import { PlusOutlined, EditOutlined, SearchOutlined, WarningOutlined, ExclamationCircleOutlined, FileSearchOutlined } from '@ant-design/icons'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { inventoryService, type InventoryItem, type AdjustStockRequest } from '@core/services/inventoryService'
import { useNavigate } from 'react-router-dom'
import dayjs from 'dayjs'

export const InventoryPage = () => {
  const navigate = useNavigate()
  const [searchText, setSearchText] = useState('')
  const [isAdjustModalOpen, setIsAdjustModalOpen] = useState(false)
  const [selectedItem, setSelectedItem] = useState<InventoryItem | null>(null)
  const [form] = Form.useForm()
  const queryClient = useQueryClient()

  const { data: inventory, isLoading } = useQuery({
    queryKey: ['inventory'],
    queryFn: () => inventoryService.getAll()
  })

  const { data: lowStock } = useQuery({
    queryKey: ['inventory-low-stock'],
    queryFn: () => inventoryService.getLowStock()
  })

  const adjustStockMutation = useMutation({
    mutationFn: inventoryService.adjustStock,
    onSuccess: () => {
      message.success('Stock adjusted successfully')
      queryClient.invalidateQueries({ queryKey: ['inventory'] })
      queryClient.invalidateQueries({ queryKey: ['inventory-low-stock'] })
      setIsAdjustModalOpen(false)
      setSelectedItem(null)
      form.resetFields()
    },
    onError: () => {
      message.error('Failed to adjust stock')
    }
  })

  const handleAdjustStock = (item: InventoryItem) => {
    setSelectedItem(item)
    form.resetFields()
    setIsAdjustModalOpen(true)
  }

  const handleSubmit = async () => {
    try {
      const values = await form.validateFields()
      
      if (!selectedItem) return

      const adjustData: AdjustStockRequest = {
        inventoryId: selectedItem.id,
        quantity: values.transactionType === 'Sale' || values.transactionType === 'Expired' 
          ? -Math.abs(values.quantity) 
          : Math.abs(values.quantity),
        transactionType: values.transactionType,
        notes: values.notes
      }

      adjustStockMutation.mutate(adjustData)
    } catch (error) {
      console.error('Validation failed:', error)
    }
  }

  const filteredInventory = inventory?.filter(item =>
    item.medicineName.toLowerCase().includes(searchText.toLowerCase()) ||
    item.batchNumber.toLowerCase().includes(searchText.toLowerCase())
  )

  const lowStockCount = lowStock?.length || 0
  const totalItems = inventory?.length || 0
  const totalValue = inventory?.reduce((sum, item) => sum + (item.currentStock * item.sellingPrice), 0) || 0

  const columns = [
    {
      title: 'Medicine',
      dataIndex: 'medicineName',
      key: 'medicineName',
      sorter: (a: InventoryItem, b: InventoryItem) => a.medicineName.localeCompare(b.medicineName)
    },
    {
      title: 'Current Stock',
      dataIndex: 'currentStock',
      key: 'currentStock',
      sorter: (a: InventoryItem, b: InventoryItem) => a.currentStock - b.currentStock,
      render: (stock: number, record: InventoryItem) => {
        const color = record.isLowStock ? 'red' : 'green'
        return (
          <Tag color={color} icon={record.isLowStock ? <WarningOutlined /> : null}>
            {stock}
          </Tag>
        )
      }
    },
    {
      title: 'Min / Max',
      key: 'stock-levels',
      render: (_: any, record: InventoryItem) => (
        <span style={{ fontSize: '12px', color: '#666' }}>
          {record.minimumStock} / {record.maximumStock}
        </span>
      )
    },
    {
      title: 'Batch',
      dataIndex: 'batchNumber',
      key: 'batchNumber'
    },
    {
      title: 'Expiry Date',
      dataIndex: 'expiryDate',
      key: 'expiryDate',
      render: (date: string) => {
        const expiryDate = dayjs(date)
        const daysToExpiry = expiryDate.diff(dayjs(), 'days')
        const isExpiringSoon = daysToExpiry <= 30 && daysToExpiry > 0
        const isExpired = daysToExpiry <= 0
        
        return (
          <span style={{ 
            color: isExpired ? 'red' : isExpiringSoon ? 'orange' : 'inherit'
          }}>
            {expiryDate.format('MMM DD, YYYY')}
            {isExpired && ' (Expired)'}
            {isExpiringSoon && !isExpired && ' (Expiring Soon)'}
          </span>
        )
      }
    },
    {
      title: 'Purchase Price',
      dataIndex: 'purchasePrice',
      key: 'purchasePrice',
      render: (price: number) => `$${price.toFixed(2)}`
    },
    {
      title: 'Selling Price',
      dataIndex: 'sellingPrice',
      key: 'sellingPrice',
      render: (price: number) => `$${price.toFixed(2)}`
    },
    {
      title: 'Last Updated',
      dataIndex: 'lastUpdated',
      key: 'lastUpdated',
      render: (date: string) => dayjs(date).format('MMM DD, YYYY')
    },
    {
      title: 'Actions',
      key: 'actions',
      render: (_: any, record: InventoryItem) => (
        <Space>
          <Button
            type="link"
            icon={<EditOutlined />}
            onClick={() => handleAdjustStock(record)}
          >
            Adjust Stock
          </Button>
        </Space>
      )
    }
  ]

  return (
    <div style={{ padding: '24px' }}>
      {lowStockCount > 0 && (
        <Alert
          message={`${lowStockCount} item(s) are low on stock`}
          description="Some medicines are running low. Please reorder soon."
          type="warning"
          icon={<WarningOutlined />}
          showIcon
          closable
          style={{ marginBottom: '24px' }}
        />
      )}

      <Row gutter={16} style={{ marginBottom: '24px' }}>
        <Col span={8}>
          <Card>
            <Statistic
              title="Total Items"
              value={totalItems}
              prefix={<ExclamationCircleOutlined />}
            />
          </Card>
        </Col>
        <Col span={8}>
          <Card>
            <Statistic
              title="Low Stock Items"
              value={lowStockCount}
              valueStyle={{ color: lowStockCount > 0 ? '#cf1322' : '#3f8600' }}
              prefix={<WarningOutlined />}
            />
          </Card>
        </Col>
        <Col span={8}>
          <Card>
            <Statistic
              title="Total Inventory Value"
              value={totalValue}
              precision={2}
              prefix="$"
            />
          </Card>
        </Col>
      </Row>

      <div style={{ marginBottom: '24px', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <div>
          <h1 style={{ margin: 0, fontSize: '24px', fontWeight: 600 }}>Inventory Management</h1>
          <p style={{ margin: '8px 0 0 0', color: '#666' }}>
            Track and manage medicine stock levels
          </p>
        </div>
        <Button
          type="primary"
          icon={<FileSearchOutlined />}
          onClick={() => navigate('/inventory/audit')}
        >
          Stock Audit
        </Button>
      </div>

      <div style={{ marginBottom: '16px' }}>
        <Input
          placeholder="Search by medicine name or batch number..."
          prefix={<SearchOutlined />}
          value={searchText}
          onChange={(e) => setSearchText(e.target.value)}
          style={{ maxWidth: '400px' }}
          size="large"
        />
      </div>

      <Table
        columns={columns}
        dataSource={filteredInventory}
        loading={isLoading}
        rowKey="id"
        pagination={{
          pageSize: 10,
          showSizeChanger: true,
          showTotal: (total) => `Total ${total} items`
        }}
        rowClassName={(record) => record.isLowStock ? 'low-stock-row' : ''}
      />

      <Modal
        title={`Adjust Stock - ${selectedItem?.medicineName}`}
        open={isAdjustModalOpen}
        onOk={handleSubmit}
        onCancel={() => {
          setIsAdjustModalOpen(false)
          setSelectedItem(null)
          form.resetFields()
        }}
        confirmLoading={adjustStockMutation.isPending}
        width={500}
      >
        {selectedItem && (
          <div style={{ marginBottom: '16px', padding: '12px', background: '#f5f5f5', borderRadius: '4px' }}>
            <p style={{ margin: 0 }}><strong>Current Stock:</strong> {selectedItem.currentStock}</p>
            <p style={{ margin: '4px 0 0 0' }}><strong>Batch:</strong> {selectedItem.batchNumber}</p>
          </div>
        )}

        <Form
          form={form}
          layout="vertical"
          initialValues={{
            transactionType: 'Purchase'
          }}
        >
          <Form.Item
            label="Transaction Type"
            name="transactionType"
            rules={[{ required: true, message: 'Please select transaction type' }]}
          >
            <select style={{ width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #d9d9d9' }}>
              <option value="Purchase">Purchase (Add Stock)</option>
              <option value="Sale">Sale (Remove Stock)</option>
              <option value="Adjustment">Adjustment</option>
              <option value="Return">Return (Add Stock)</option>
              <option value="Expired">Expired (Remove Stock)</option>
            </select>
          </Form.Item>

          <Form.Item
            label="Quantity"
            name="quantity"
            rules={[
              { required: true, message: 'Please enter quantity' },
              { type: 'number', min: 1, message: 'Quantity must be greater than 0' }
            ]}
          >
            <InputNumber
              min={1}
              style={{ width: '100%' }}
              placeholder="Enter quantity"
            />
          </Form.Item>

          <Form.Item
            label="Notes"
            name="notes"
          >
            <Input.TextArea
              rows={3}
              placeholder="Transaction notes (e.g., supplier name, invoice number)"
            />
          </Form.Item>
        </Form>
      </Modal>

      <style>{`
        .low-stock-row {
          background-color: #fff1f0 !important;
        }
      `}</style>
    </div>
  )
}

