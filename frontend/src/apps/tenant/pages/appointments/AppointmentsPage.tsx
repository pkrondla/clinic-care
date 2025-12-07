import { useState } from 'react'
import { Card, Table, Button, Space, Tag, Input, Select, DatePicker, Row, Col, Tooltip } from 'antd'
import { 
  PlusOutlined, 
  SearchOutlined, 
  ReloadOutlined,
  EyeOutlined,
  EditOutlined,
  MedicineBoxOutlined
} from '@ant-design/icons'
import { useNavigate } from 'react-router-dom'
import { useAppointments } from '@core/hooks/queries/useAppointments'
import { useSelectedClinic } from '@core/stores/authStore'
import { Appointment, AppointmentStatus, AppointmentType } from '@core/types'
import dayjs, { Dayjs } from 'dayjs'

const { Option } = Select

export const AppointmentsPage = () => {
  const navigate = useNavigate()
  const selectedClinic = useSelectedClinic()
  
  const [searchText, setSearchText] = useState('')
  const [statusFilter, setStatusFilter] = useState<AppointmentStatus | undefined>()
  const [dateFilter, setDateFilter] = useState<Dayjs>(dayjs())

  const { data: appointments, isLoading, refetch } = useAppointments({
    clinicId: selectedClinic?.id,
    date: dateFilter.format('YYYY-MM-DD'),
    status: statusFilter
  })

  const getStatusColor = (status: AppointmentStatus) => {
    switch (status) {
      case AppointmentStatus.Scheduled: return 'blue'
      case AppointmentStatus.InProgress: return 'orange'
      case AppointmentStatus.Completed: return 'green'
      case AppointmentStatus.Cancelled: return 'red'
      default: return 'default'
    }
  }

  const getStatusText = (status: AppointmentStatus) => {
    switch (status) {
      case AppointmentStatus.Scheduled: return 'Scheduled'
      case AppointmentStatus.InProgress: return 'In Progress'
      case AppointmentStatus.Completed: return 'Completed'
      case AppointmentStatus.Cancelled: return 'Cancelled'
      default: return 'Unknown'
    }
  }

  const getTypeText = (type: AppointmentType) => {
    return type === AppointmentType.InPerson ? 'In-Person' : 'Teleconsultation'
  }

  // Filter appointments based on search text
  const filteredAppointments = appointments?.filter(appointment => {
    const searchLower = searchText.toLowerCase()
    return (
      appointment.patient?.name?.toLowerCase().includes(searchLower) ||
      appointment.doctor?.name?.toLowerCase().includes(searchLower) ||
      appointment.tokenNumber?.toString().includes(searchLower)
    )
  })

  const columns = [
    {
      title: 'Token',
      dataIndex: 'tokenNumber',
      key: 'tokenNumber',
      width: 80,
      render: (token: number) => <Tag color="blue">#{token}</Tag>
    },
    {
      title: 'Patient Name',
      dataIndex: ['patient', 'name'],
      key: 'patientName',
      width: 150
    },
    {
      title: 'Doctor',
      dataIndex: ['doctor', 'name'],
      key: 'doctorName',
      width: 150
    },
    {
      title: 'Type',
      dataIndex: 'type',
      key: 'type',
      width: 130,
      render: (type: AppointmentType) => (
        <Tag color={type === AppointmentType.InPerson ? 'green' : 'blue'}>
          {getTypeText(type)}
        </Tag>
      )
    },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      width: 120,
      render: (status: AppointmentStatus) => (
        <Tag color={getStatusColor(status)}>
          {getStatusText(status)}
        </Tag>
      )
    },
    {
      title: 'Date',
      dataIndex: 'appointmentDate',
      key: 'appointmentDate',
      width: 120,
      render: (date: string) => dayjs(date).format('MMM DD, YYYY')
    },
    {
      title: 'Notes',
      dataIndex: 'notes',
      key: 'notes',
      ellipsis: true,
      render: (notes: string) => notes || '-'
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 180,
      fixed: 'right' as const,
      render: (_: any, record: Appointment) => (
        <Space size="small">
          <Tooltip title="View Details">
            <Button
              type="text"
              icon={<EyeOutlined />}
              onClick={() => navigate(`/appointments/${record.id}`)}
            />
          </Tooltip>
          {record.status === AppointmentStatus.Scheduled && (
            <Tooltip title="Edit">
              <Button
                type="text"
                icon={<EditOutlined />}
                onClick={() => navigate(`/appointments/${record.id}/edit`)}
              />
            </Tooltip>
          )}
          {(record.status === AppointmentStatus.Scheduled || record.status === AppointmentStatus.InProgress) && !record.consultation && (
            <Tooltip title="Start Consultation">
              <Button
                type="text"
                icon={<MedicineBoxOutlined />}
                onClick={() => navigate(`/consultations/new?appointmentId=${record.id}&patientId=${record.patient?.id}`)}
              />
            </Tooltip>
          )}
        </Space>
      )
    }
  ]

  return (
    <div>
      <div style={{ marginBottom: 24 }}>
        <Row justify="space-between" align="middle">
          <Col>
            <h2 style={{ margin: 0 }}>Appointments</h2>
          </Col>
          <Col>
            <Button
              type="primary"
              icon={<PlusOutlined />}
              onClick={() => navigate('/appointments/new')}
            >
              New Appointment
            </Button>
          </Col>
        </Row>
      </div>

      <Card>
        <Space direction="vertical" size="middle" style={{ width: '100%' }}>
          {/* Filters */}
          <Row gutter={[16, 16]}>
            <Col xs={24} sm={12} md={8}>
              <Input
                placeholder="Search by patient, doctor or token"
                prefix={<SearchOutlined />}
                value={searchText}
                onChange={(e) => setSearchText(e.target.value)}
                allowClear
              />
            </Col>
            <Col xs={24} sm={12} md={8}>
              <DatePicker
                style={{ width: '100%' }}
                value={dateFilter}
                onChange={(date) => setDateFilter(date || dayjs())}
                format="DD/MM/YYYY"
              />
            </Col>
            <Col xs={24} sm={12} md={8}>
              <Select
                style={{ width: '100%' }}
                placeholder="Filter by status"
                value={statusFilter}
                onChange={setStatusFilter}
                allowClear
              >
                <Option value={AppointmentStatus.Scheduled}>Scheduled</Option>
                <Option value={AppointmentStatus.InProgress}>In Progress</Option>
                <Option value={AppointmentStatus.Completed}>Completed</Option>
                <Option value={AppointmentStatus.Cancelled}>Cancelled</Option>
              </Select>
            </Col>
          </Row>

          {/* Actions */}
          <Row justify="space-between" align="middle">
            <Col>
              <span>
                Showing {filteredAppointments?.length || 0} appointments
              </span>
            </Col>
            <Col>
              <Button
                icon={<ReloadOutlined />}
                onClick={() => refetch()}
                loading={isLoading}
              >
                Refresh
              </Button>
            </Col>
          </Row>

          {/* Table */}
          <Table
            dataSource={filteredAppointments}
            columns={columns}
            rowKey="id"
            loading={isLoading}
            pagination={{
              pageSize: 20,
              showSizeChanger: true,
              showTotal: (total) => `Total ${total} appointments`
            }}
            scroll={{ x: 1200 }}
          />
        </Space>
      </Card>
    </div>
  )
}

