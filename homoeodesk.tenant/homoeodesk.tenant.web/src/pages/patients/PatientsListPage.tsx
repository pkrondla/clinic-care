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
  Tooltip,
  Popconfirm
} from 'antd'
import { 
  PlusOutlined, 
  EditOutlined, 
  DeleteOutlined, 
  EyeOutlined,
  ReloadOutlined,
  FilterOutlined
} from '@ant-design/icons'
import { usePatients, useDeletePatient } from '@core/hooks/queries/usePatients'
import { useNavigate } from 'react-router-dom'
import type { Patient, PatientFilters } from '@core/types/patient'
import { GENDER_OPTIONS, BLOOD_GROUP_OPTIONS } from '@core/types/patient'
import dayjs from 'dayjs'

const { Title } = Typography
const { Search } = Input

export const PatientsListPage = () => {
  const navigate = useNavigate()
  const [filters, setFilters] = useState<PatientFilters>({
    page: 1,
    pageSize: 10,
    sortBy: 'createdAt',
    sortOrder: 'desc'
  })
  const [searchVisible, setSearchVisible] = useState(false)

  const { data: patientsData, isLoading, refetch } = usePatients(filters)
  const deletePatientMutation = useDeletePatient()

  const handleSearch = (value: string) => {
    setFilters(prev => ({ ...prev, search: value, page: 1 }))
  }

  const handleFilterChange = (key: keyof PatientFilters, value: any) => {
    setFilters(prev => ({ ...prev, [key]: value, page: 1 }))
  }

  const handleTableChange = (pagination: any, _tableFilters: any, sorter: any) => {
    setFilters(prev => ({
      ...prev,
      page: pagination.current,
      pageSize: pagination.pageSize,
      sortBy: sorter.field || 'createdAt',
      sortOrder: sorter.order === 'ascend' ? 'asc' : 'desc'
    }))
  }

  const handleDelete = async (id: number) => {
    try {
      await deletePatientMutation.mutateAsync(id)
    } catch (error) {
      // Error is handled by the mutation
    }
  }

  const columns = [
    {
      title: 'Patient Code',
      dataIndex: 'patientCode',
      key: 'patientCode',
      width: 120,
      render: (code: string) => <Tag color="blue">{code}</Tag>
    },
    {
      title: 'Name',
      dataIndex: 'fullName',
      key: 'fullName',
      sorter: true,
      render: (name: string, record: Patient) => (
        <div>
          <div style={{ fontWeight: 500 }}>{name}</div>
          <div style={{ fontSize: 12, color: '#666' }}>{record.email}</div>
        </div>
      )
    },
    {
      title: 'Phone',
      dataIndex: 'phone',
      key: 'phone',
      width: 120
    },
    {
      title: 'Age/Gender',
      key: 'ageGender',
      width: 100,
      render: (record: Patient) => (
        <div>
          <div>{record.age} years</div>
          <div style={{ fontSize: 12, color: '#666' }}>{record.gender}</div>
        </div>
      )
    },
    {
      title: 'Blood Group',
      dataIndex: 'bloodGroup',
      key: 'bloodGroup',
      width: 100,
      render: (group: string) => group ? <Tag color="red">{group}</Tag> : '-'
    },
    {
      title: 'Last Visit',
      dataIndex: 'lastVisitDate',
      key: 'lastVisitDate',
      width: 120,
      render: (date: string) => date ? dayjs(date).format('MMM DD, YYYY') : 'Never'
    },
    {
      title: 'Appointments',
      key: 'appointments',
      width: 100,
      render: (record: Patient) => (
        <div style={{ textAlign: 'center' }}>
          <div style={{ fontWeight: 500 }}>{record.totalAppointments}</div>
          <div style={{ fontSize: 12, color: '#666' }}>total</div>
        </div>
      )
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 120,
      fixed: 'right' as const,
      render: (record: Patient) => (
        <Space size="small">
          <Tooltip title="View Details">
            <Button
              type="text"
              icon={<EyeOutlined />}
              onClick={() => navigate(`/patients/${record.id}`)}
            />
          </Tooltip>
          <Tooltip title="Edit">
            <Button
              type="text"
              icon={<EditOutlined />}
              onClick={() => navigate(`/patients/${record.id}/edit`)}
            />
          </Tooltip>
          <Tooltip title="Delete">
            <Popconfirm
              title="Delete Patient"
              description="Are you sure you want to delete this patient? This action cannot be undone."
              onConfirm={() => handleDelete(record.id)}
              okText="Yes"
              cancelText="No"
            >
              <Button
                type="text"
                danger
                icon={<DeleteOutlined />}
                loading={deletePatientMutation.isPending}
              />
            </Popconfirm>
          </Tooltip>
        </Space>
      )
    }
  ]

  return (
    <div>
      <div style={{ marginBottom: 24, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <Title level={2} style={{ margin: 0 }}>Patients</Title>
        <Button
          type="primary"
          icon={<PlusOutlined />}
          onClick={() => navigate('/patients/new')}
        >
          Add Patient
        </Button>
      </div>

      <Card>
        <Row gutter={[16, 16]} style={{ marginBottom: 16 }}>
          <Col xs={24} sm={12} md={8} lg={6}>
            <Search
              placeholder="Search patients..."
              allowClear
              onSearch={handleSearch}
              style={{ width: '100%' }}
            />
          </Col>
          <Col xs={24} sm={12} md={8} lg={6}>
            <Select
              placeholder="Gender"
              allowClear
              style={{ width: '100%' }}
              options={[...GENDER_OPTIONS]}
              onChange={(value) => handleFilterChange('gender', value)}
            />
          </Col>
          <Col xs={24} sm={12} md={8} lg={6}>
            <Select
              placeholder="Blood Group"
              allowClear
              style={{ width: '100%' }}
              options={[...BLOOD_GROUP_OPTIONS]}
              onChange={(value) => handleFilterChange('bloodGroup', value)}
            />
          </Col>
          <Col xs={24} sm={12} md={8} lg={6}>
            <Space>
              <Button
                icon={<ReloadOutlined />}
                onClick={() => refetch()}
                loading={isLoading}
              >
                Refresh
              </Button>
              <Button
                icon={<FilterOutlined />}
                onClick={() => setSearchVisible(!searchVisible)}
              >
                More Filters
              </Button>
            </Space>
          </Col>
        </Row>

        {searchVisible && (
          <Row gutter={[16, 16]} style={{ marginBottom: 16, padding: 16, background: '#f5f5f5', borderRadius: 6 }}>
            <Col xs={24} sm={12} md={8}>
              <Input
                placeholder="Min Age"
                type="number"
                onChange={(e) => handleFilterChange('minAge', e.target.value ? parseInt(e.target.value) : undefined)}
              />
            </Col>
            <Col xs={24} sm={12} md={8}>
              <Input
                placeholder="Max Age"
                type="number"
                onChange={(e) => handleFilterChange('maxAge', e.target.value ? parseInt(e.target.value) : undefined)}
              />
            </Col>
            <Col xs={24} sm={12} md={8}>
              <Button onClick={() => setFilters({ page: 1, pageSize: 10, sortBy: 'createdAt', sortOrder: 'desc' })}>
                Clear Filters
              </Button>
            </Col>
          </Row>
        )}

        <Table
          columns={columns}
          dataSource={patientsData?.data}
          rowKey="id"
          loading={isLoading}
          pagination={{
            current: filters.page,
            pageSize: filters.pageSize,
            total: patientsData?.total,
            showSizeChanger: true,
            showQuickJumper: true,
            showTotal: (total, range) => `${range[0]}-${range[1]} of ${total} patients`
          }}
          onChange={handleTableChange}
        />
      </Card>
    </div>
  )
}
