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
} from 'antd'
import {
  EyeOutlined,
  ReloadOutlined,
  FilterOutlined,
  DollarOutlined,
  PlusOutlined,
  EditOutlined,
} from '@ant-design/icons'
import { useInvoices } from '@core/hooks/queries/useInvoices'
import { useNavigate } from 'react-router-dom'
import type { Invoice } from '@core/services/invoiceService'
import { useSelectedBranch } from '@core/stores/authStore'
import dayjs from 'dayjs'

const { Title } = Typography
const { Search } = Input
const { RangePicker } = DatePicker

const INVOICE_STATUS_OPTIONS = [
  { label: 'All', value: undefined },
  { label: 'Draft', value: 1 },
  { label: 'Sent', value: 2 },
  { label: 'Paid', value: 3 },
  { label: 'Cancelled', value: 4 },
]

export const InvoicesPage = () => {
  const navigate = useNavigate()
  const selectedClinic = useSelectedBranch()
  const [filters, setFilters] = useState({
    BranchId: selectedClinic?.id,
    status: undefined as number | undefined,
    startDate: undefined as string | undefined,
    endDate: undefined as string | undefined,
  })

  const { data: invoices, isLoading, refetch } = useInvoices(filters)

  const handleView = (id: number) => {
    navigate(`/invoices/${id}`)
  }

  const handleEdit = (id: number) => {
    navigate(`/invoices/${id}/edit`)
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
  const totalInvoices = invoices?.length || 0
  const totalAmount = invoices?.reduce((sum, inv) => sum + inv.totalAmount, 0) || 0
  const paidAmount = invoices?.reduce((sum, inv) => sum + inv.paidAmount, 0) || 0
  const pendingAmount = invoices?.reduce((sum, inv) => sum + inv.balanceAmount, 0) || 0

  const columns = [
    {
      title: 'Invoice Number',
      dataIndex: 'invoiceNumber',
      key: 'invoiceNumber',
      width: 120,
      render: (text: string) => <Tag color="blue">{text}</Tag>,
    },
    {
      title: 'Patient',
      dataIndex: 'patientName',
      key: 'patientName',
      width: 150,
      ellipsis: true,
      render: (text: string, record: Invoice) => (
        <div>
          <div style={{ overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>{text}</div>
          <div style={{ fontSize: '12px', color: '#999' }}>{record.patientCode}</div>
        </div>
      ),
    },
    {
      title: 'Prescription',
      dataIndex: 'prescriptionNumber',
      key: 'prescriptionNumber',
      width: 100,
      render: (text: string) => text || '-',
    },
    {
      title: 'Total Amount',
      dataIndex: 'totalAmount',
      key: 'totalAmount',
      width: 100,
      align: 'right' as const,
      render: (amount: number) => `₹${amount.toFixed(2)}`,
    },
    {
      title: 'Paid',
      dataIndex: 'paidAmount',
      key: 'paidAmount',
      width: 100,
      align: 'right' as const,
      render: (amount: number) => `₹${amount.toFixed(2)}`,
    },
    {
      title: 'Balance',
      dataIndex: 'balanceAmount',
      key: 'balanceAmount',
      width: 100,
      align: 'right' as const,
      render: (amount: number, record: Invoice) => (
        <Tag color={amount > 0 ? 'orange' : 'green'}>₹{amount.toFixed(2)}</Tag>
      ),
    },
    {
      title: 'Status',
      dataIndex: 'statusText',
      key: 'status',
      width: 90,
      render: (text: string, record: Invoice) => {
        const color =
          record.status === 3
            ? 'green'
            : record.status === 4
            ? 'red'
            : record.status === 2
            ? 'blue'
            : 'default'
        return <Tag color={color}>{text}</Tag>
      },
    },
    {
      title: 'Date',
      dataIndex: 'invoiceDate',
      key: 'invoiceDate',
      width: 100,
      render: (date: string) => dayjs(date).format('DD/MM/YYYY'),
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 130,
      fixed: 'right' as const,
      render: (_: any, record: Invoice) => (
        <Space size="small">
          <Button
            type="link"
            icon={<EyeOutlined />}
            onClick={() => handleView(record.id)}
            size="small"
          >
            View
          </Button>
          <Button
            type="link"
            icon={<EditOutlined />}
            onClick={() => handleEdit(record.id)}
            size="small"
          >
            Edit
          </Button>
        </Space>
      ),
    },
  ]

  return (
    <div>
      <Row justify="space-between" align="middle" style={{ marginBottom: 24 }}>
        <Col>
          <Title level={2} style={{ margin: 0 }}>
            Invoices
          </Title>
        </Col>
        <Col>
          <Space>
            <Button
              type="primary"
              icon={<PlusOutlined />}
              onClick={() => navigate('/invoices/new')}
            >
              New Invoice
            </Button>
            <Button icon={<ReloadOutlined />} onClick={() => refetch()} loading={isLoading}>
              Refresh
            </Button>
          </Space>
        </Col>
      </Row>

      {/* Statistics */}
      <Row gutter={16} style={{ marginBottom: 24 }}>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="Total Invoices"
              value={totalInvoices}
              prefix={<DollarOutlined />}
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
              title="Paid Amount"
              value={paidAmount}
              prefix="₹"
              precision={2}
              valueStyle={{ color: '#3f8600' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="Pending Amount"
              value={pendingAmount}
              prefix="₹"
              precision={2}
              valueStyle={{ color: '#cf1322' }}
            />
          </Card>
        </Col>
      </Row>

      {/* Filters */}
      <Card style={{ marginBottom: 24 }}>
        <Row gutter={16}>
          <Col xs={24} sm={12} md={6}>
            <Select
              placeholder="Status"
              style={{ width: '100%' }}
              value={filters.status}
              onChange={(value) => handleFilterChange('status', value)}
              allowClear
            >
              {INVOICE_STATUS_OPTIONS.map((opt) => (
                <Select.Option key={opt.value || 'all'} value={opt.value}>
                  {opt.label}
                </Select.Option>
              ))}
            </Select>
          </Col>
          <Col xs={24} sm={12} md={8}>
            <RangePicker
              style={{ width: '100%' }}
              onChange={handleDateRangeChange}
              format="DD/MM/YYYY"
            />
          </Col>
        </Row>
      </Card>

      {/* Table */}
      <Card>
        <Table
          columns={columns}
          dataSource={invoices}
          loading={isLoading}
          rowKey="id"
          scroll={{ x: 1000 }}
          pagination={{
            pageSize: 10,
            showSizeChanger: true,
            showTotal: (total) => `Total ${total} invoices`,
          }}
        />
      </Card>
    </div>
  )
}

