import { useState } from 'react'
import { Card, Table, Button, Space, Input, DatePicker, Row, Col, Tooltip } from 'antd'
import { 
  PlusOutlined, 
  SearchOutlined, 
  ReloadOutlined,
  EyeOutlined
} from '@ant-design/icons'
import { useNavigate } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { consultationService, Consultation, GetConsultationsParams } from '@core/services/consultationService'
import { useSelectedClinic, useUser } from '@core/stores/authStore'
import dayjs, { Dayjs } from 'dayjs'

const { RangePicker } = DatePicker

export const ConsultationsPage = () => {
  const navigate = useNavigate()
  const selectedClinic = useSelectedClinic()
  const user = useUser()
  
  const [searchText, setSearchText] = useState('')
  const [dateRange, setDateRange] = useState<[Dayjs | null, Dayjs | null]>([dayjs().subtract(7, 'days'), dayjs()])

  const params: GetConsultationsParams = {
    clinicId: selectedClinic?.id,
    doctorId: user?.role === 'Doctor' ? user.id : undefined,
    startDate: dateRange[0]?.format('YYYY-MM-DD'),
    endDate: dateRange[1]?.format('YYYY-MM-DD')
  }

  const { data: consultations = [], isLoading, refetch } = useQuery({
    queryKey: ['consultations', params],
    queryFn: () => consultationService.getAll(params),
    enabled: !!selectedClinic?.id
  })

  // Filter consultations based on search text
  const filteredConsultations = consultations.filter(consultation => {
    const searchLower = searchText.toLowerCase()
    return (
      consultation.patientName?.toLowerCase().includes(searchLower) ||
      consultation.doctorName?.toLowerCase().includes(searchLower) ||
      consultation.chiefComplaint?.toLowerCase().includes(searchLower) ||
      consultation.diagnosis?.toLowerCase().includes(searchLower)
    )
  })

  const columns = [
    {
      title: 'Date',
      dataIndex: 'consultationDate',
      key: 'consultationDate',
      width: 120,
      render: (date: string) => dayjs(date).format('MMM DD, YYYY')
    },
    {
      title: 'Patient Name',
      dataIndex: 'patientName',
      key: 'patientName',
      width: 150
    },
    {
      title: 'Doctor',
      dataIndex: 'doctorName',
      key: 'doctorName',
      width: 150
    },
    {
      title: 'Chief Complaint',
      dataIndex: 'chiefComplaint',
      key: 'chiefComplaint',
      width: 200,
      ellipsis: true
    },
    {
      title: 'Diagnosis',
      dataIndex: 'diagnosis',
      key: 'diagnosis',
      width: 200,
      ellipsis: true,
      render: (diagnosis: string) => diagnosis || '-'
    },
    {
      title: 'Fee',
      dataIndex: 'consultationFee',
      key: 'consultationFee',
      width: 100,
      render: (fee: number) => `₹${fee.toFixed(2)}`
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 100,
      fixed: 'right' as const,
      render: (_: any, record: Consultation) => (
        <Space size="small">
          <Tooltip title="View Details">
            <Button
              type="text"
              icon={<EyeOutlined />}
              onClick={() => navigate(`/consultations/${record.id}`)}
            />
          </Tooltip>
        </Space>
      )
    }
  ]

  if (!selectedClinic?.id) {
    return (
      <Card>
        <div style={{ textAlign: 'center', padding: '50px' }}>
          Please select a clinic to view consultations
        </div>
      </Card>
    )
  }

  return (
    <div>
      <div style={{ marginBottom: 24 }}>
        <Row justify="space-between" align="middle">
          <Col>
            <h2 style={{ margin: 0 }}>Consultations</h2>
          </Col>
          <Col>
            <Button
              type="primary"
              icon={<PlusOutlined />}
              onClick={() => navigate('/consultations/new')}
            >
              New Consultation
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
                placeholder="Search by patient, doctor, complaint or diagnosis"
                prefix={<SearchOutlined />}
                value={searchText}
                onChange={(e) => setSearchText(e.target.value)}
                allowClear
              />
            </Col>
            <Col xs={24} sm={12} md={8}>
              <RangePicker
                style={{ width: '100%' }}
                value={dateRange}
                onChange={(dates) => setDateRange(dates as [Dayjs | null, Dayjs | null])}
                format="DD/MM/YYYY"
              />
            </Col>
          </Row>

          {/* Actions */}
          <Row justify="space-between" align="middle">
            <Col>
              <span>
                Showing {filteredConsultations.length} consultations
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
            dataSource={filteredConsultations}
            columns={columns}
            rowKey="id"
            loading={isLoading}
            pagination={{
              pageSize: 20,
              showSizeChanger: true,
              showTotal: (total) => `Total ${total} consultations`
            }}
            scroll={{ x: 1200 }}
          />
        </Space>
      </Card>
    </div>
  )
}

