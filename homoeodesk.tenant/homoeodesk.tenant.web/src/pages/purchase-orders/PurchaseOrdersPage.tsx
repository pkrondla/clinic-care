import { useState } from 'react'
import {
  Card,
  Table,
  Button,
  Input,
  Select,
  Space,
  Tag,
  Typography,
  Row,
  Col,
  DatePicker,
  Statistic,
  Popconfirm,
} from 'antd'
import {
  PlusOutlined,
  EyeOutlined,
  ReloadOutlined,
  CheckOutlined,
  CloseOutlined,
  ShoppingCartOutlined,
} from '@ant-design/icons'
import {
  usePurchaseOrders,
  useApprovePurchaseOrder,
  useCancelPurchaseOrder,
} from '@core/hooks/queries/usePurchaseOrders'
import { useNavigate } from 'react-router-dom'
import type { PurchaseOrder } from '@core/services/purchaseOrderService'
import { useSelectedBranch } from '@core/stores/authStore'
import dayjs from 'dayjs'

const { Title } = Typography
const { Search } = Input
const { RangePicker } = DatePicker

const PURCHASE_ORDER_STATUS_OPTIONS = [
  { label: 'All', value: undefined },
  { label: 'Draft', value: 1 },
  { label: 'Pending', value: 2 },
  { label: 'Approved', value: 3 },
  { label: 'Ordered', value: 4 },
  { label: 'Partially Received', value: 5 },
  { label: 'Received', value: 6 },
  { label: 'Cancelled', value: 7 },
]

const getStatusColor = (status: number) => {
  switch (status) {
    case 1: return 'default' // Draft
    case 2: return 'orange' // Pending
    case 3: return 'blue' // Approved
    case 4: return 'cyan' // Ordered
    case 5: return 'purple' // Partially Received
    case 6: return 'green' // Received
    case 7: return 'red' // Cancelled
    default: return 'default'
  }
}

export const PurchaseOrdersPage = () => {
  const navigate = useNavigate()
  const selectedClinic = useSelectedBranch()
  const [filters, setFilters] = useState({
    BranchId: selectedClinic?.id,
    supplierId: undefined as number | undefined,
    status: undefined as number | undefined,
    startDate: undefined as string | undefined,
    endDate: undefined as string | undefined,
  })

  const { data: purchaseOrders = [], isLoading, refetch } = usePurchaseOrders(filters)
  const approveMutation = useApprovePurchaseOrder()
  const cancelMutation = useCancelPurchaseOrder()

  const handleView = (id: number) => {
    navigate(`/purchase-orders/${id}`)
  }

  const handleCreate = () => {
    navigate('/purchase-orders/new')
  }

  const handleApprove = async (id: number) => {
    await approveMutation.mutateAsync(id)
  }

  const handleCancel = async (id: number) => {
    await cancelMutation.mutateAsync({ id })
  }

  const handleFilterChange = (key: string, value: any) => {
    setFilters((prev) => ({ ...prev, [key]: value }))
  }

  const handleDateRangeChange = (dates: any) => {
    if (dates && dates.length === 2) {
      setFilters((prev) => ({
        ...prev,
        startDate: dates[0].format('YYYY-MM-DD'),
        endDate: dates[1].format('YYYY-MM-DD'),
      }))
    } else {
      setFilters((prev) => ({
        ...prev,
        startDate: undefined,
        endDate: undefined,
      }))
    }
  }

  // Calculate statistics
  const totalOrders = purchaseOrders?.length || 0
  const totalAmount = purchaseOrders?.reduce((sum, po) => sum + po.grandTotal, 0) || 0
  const pendingOrders = purchaseOrders?.filter(po => po.status === 2 || po.status === 3).length || 0
  const receivedOrders = purchaseOrders?.filter(po => po.status === 6).length || 0

  const columns = [
    {
      title: 'Order Number',
      dataIndex: 'orderNumber',
      key: 'orderNumber',
      width: 150,
      render: (text: string) => <Tag color="blue">{text}</Tag>,
    },
    {
      title: 'Supplier',
      dataIndex: 'supplierName',
      key: 'supplierName',
    },
    {
      title: 'Clinic',
      dataIndex: 'clinicName',
      key: 'clinicName',
    },
    {
      title: 'Order Date',
      dataIndex: 'orderDate',
      key: 'orderDate',
      width: 120,
      render: (date: string) => dayjs(date).format('DD/MM/YYYY'),
    },
    {
      title: 'Items',
      key: 'items',
      width: 80,
      render: (_: any, record: PurchaseOrder) => record.items.length,
    },
    {
      title: 'Grand Total',
      dataIndex: 'grandTotal',
      key: 'grandTotal',
      width: 120,
      align: 'right' as const,
      render: (amount: number) => `₹${amount.toFixed(2)}`,
    },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      width: 150,
      render: (status: number, record: PurchaseOrder) => (
        <Tag color={getStatusColor(status)}>{record.statusText}</Tag>
      ),
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 200,
      render: (_: any, record: PurchaseOrder) => (
        <Space>
          <Button
            type="link"
            icon={<EyeOutlined />}
            onClick={() => handleView(record.id)}
          >
            View
          </Button>
          {record.status === 1 && (
            <Button
              type="link"
              icon={<CheckOutlined />}
              onClick={() => handleApprove(record.id)}
            >
              Approve
            </Button>
          )}
          {record.status !== 6 && record.status !== 7 && (
            <Popconfirm
              title="Are you sure you want to cancel this purchase order?"
              onConfirm={() => handleCancel(record.id)}
              okText="Yes"
              cancelText="No"
            >
              <Button type="link" danger icon={<CloseOutlined />}>
                Cancel
              </Button>
            </Popconfirm>
          )}
        </Space>
      ),
    },
  ]

  return (
    <div>
      <Card>
        <Row justify="space-between" align="middle" style={{ marginBottom: 16 }}>
          <Col>
            <Title level={2} style={{ margin: 0 }}>
              Purchase Orders
            </Title>
          </Col>
          <Col>
            <Space>
              <Button icon={<ReloadOutlined />} onClick={() => refetch()}>
                Refresh
              </Button>
              <Button
                type="primary"
                icon={<PlusOutlined />}
                onClick={handleCreate}
              >
                Create Purchase Order
              </Button>
            </Space>
          </Col>
        </Row>

        {/* Statistics */}
        <Row gutter={16} style={{ marginBottom: 24 }}>
          <Col xs={24} sm={12} md={6}>
            <Card>
              <Statistic
                title="Total Orders"
                value={totalOrders}
                prefix={<ShoppingCartOutlined />}
              />
            </Card>
          </Col>
          <Col xs={24} sm={12} md={6}>
            <Card>
              <Statistic
                title="Total Amount"
                value={totalAmount}
                prefix="₹"
                precision={2}
              />
            </Card>
          </Col>
          <Col xs={24} sm={12} md={6}>
            <Card>
              <Statistic
                title="Pending Orders"
                value={pendingOrders}
                valueStyle={{ color: '#faad14' }}
              />
            </Card>
          </Col>
          <Col xs={24} sm={12} md={6}>
            <Card>
              <Statistic
                title="Received Orders"
                value={receivedOrders}
                valueStyle={{ color: '#3f8600' }}
              />
            </Card>
          </Col>
        </Row>

        {/* Filters */}
        <Row gutter={16} style={{ marginBottom: 16 }}>
          <Col xs={24} sm={12} md={6}>
            <Select
              placeholder="Status"
              allowClear
              style={{ width: '100%' }}
              value={filters.status}
              onChange={(value) => handleFilterChange('status', value)}
              options={PURCHASE_ORDER_STATUS_OPTIONS}
            />
          </Col>
          <Col xs={24} sm={12} md={8}>
            <RangePicker
              style={{ width: '100%' }}
              onChange={handleDateRangeChange}
            />
          </Col>
        </Row>

        <Table
          columns={columns}
          dataSource={purchaseOrders}
          rowKey="id"
          loading={isLoading}
          pagination={{
            pageSize: 10,
            showSizeChanger: true,
            showTotal: (total) => `Total ${total} purchase orders`,
          }}
        />
      </Card>
    </div>
  )
}

