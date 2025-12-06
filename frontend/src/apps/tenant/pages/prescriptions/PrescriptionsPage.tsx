import { useState } from 'react'
import { Card, Table, Button, Space, Input, DatePicker, Row, Col, Tooltip, Tag } from 'antd'
import { 
  PlusOutlined, 
  SearchOutlined, 
  ReloadOutlined,
  EyeOutlined,
  DownloadOutlined
} from '@ant-design/icons'
import { useNavigate } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { prescriptionService, Prescription, GetPrescriptionsParams } from '@core/services/prescriptionService'
import { useSelectedClinic, useUser } from '@core/stores/authStore'
import dayjs, { Dayjs } from 'dayjs'

const { RangePicker } = DatePicker

export const PrescriptionsPage = () => {
  const navigate = useNavigate()
  const selectedClinic = useSelectedClinic()
  const user = useUser()
  
  const [searchText, setSearchText] = useState('')
  const [dateRange, setDateRange] = useState<[Dayjs | null, Dayjs | null]>([dayjs().subtract(7, 'days'), dayjs()])

  const params: GetPrescriptionsParams = {
    clinicId: selectedClinic?.id,
    doctorId: user?.role === 'Doctor' ? user.id : undefined,
    startDate: dateRange[0]?.format('YYYY-MM-DD'),
    endDate: dateRange[1]?.format('YYYY-MM-DD')
  }

  const { data: prescriptions = [], isLoading, refetch } = useQuery({
    queryKey: ['prescriptions', params],
    queryFn: () => prescriptionService.getAll(params),
    enabled: !!selectedClinic?.id
  })

  // Filter prescriptions based on search text
  const filteredPrescriptions = prescriptions.filter(prescription => {
    const searchLower = searchText.toLowerCase()
    return (
      prescription.prescriptionNumber?.toLowerCase().includes(searchLower) ||
      prescription.patientName?.toLowerCase().includes(searchLower) ||
      prescription.doctorName?.toLowerCase().includes(searchLower) ||
      prescription.medicines?.some(m => m.medicineName?.toLowerCase().includes(searchLower))
    )
  })

  const handleDownloadPdf = async (id: number, includeMedicineNames: boolean = true) => {
    try {
      const blob = await prescriptionService.downloadPdf(id, includeMedicineNames)
      const url = window.URL.createObjectURL(blob)
      const link = document.createElement('a')
      link.href = url
      link.download = `Prescription_${id}_${includeMedicineNames ? 'Internal' : 'Patient'}.pdf`
      document.body.appendChild(link)
      link.click()
      document.body.removeChild(link)
      window.URL.revokeObjectURL(url)
    } catch (error) {
      console.error('Failed to download PDF:', error)
    }
  }

  const columns = [
    {
      title: 'Prescription #',
      dataIndex: 'prescriptionNumber',
      key: 'prescriptionNumber',
      width: 150,
      render: (number: string) => <Tag color="blue">{number}</Tag>
    },
    {
      title: 'Date',
      dataIndex: 'prescriptionDate',
      key: 'prescriptionDate',
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
      title: 'Medicines',
      dataIndex: 'medicines',
      key: 'medicines',
      width: 200,
      render: (medicines: Prescription['medicines']) => 
        medicines?.length ? `${medicines.length} medicine(s)` : '-'
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 150,
      fixed: 'right' as const,
      render: (_: any, record: Prescription) => (
        <Space size="small">
          <Tooltip title="View Details">
            <Button
              type="text"
              icon={<EyeOutlined />}
              onClick={() => navigate(`/prescriptions/${record.id}`)}
            />
          </Tooltip>
          <Tooltip title="Download PDF">
            <Button
              type="text"
              icon={<DownloadOutlined />}
              onClick={() => handleDownloadPdf(record.id, true)}
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
          Please select a clinic to view prescriptions
        </div>
      </Card>
    )
  }

  return (
    <div>
      <div style={{ marginBottom: 24 }}>
        <Row justify="space-between" align="middle">
          <Col>
            <h2 style={{ margin: 0 }}>Prescriptions</h2>
          </Col>
          <Col>
            <Button
              type="primary"
              icon={<PlusOutlined />}
              onClick={() => navigate('/prescriptions/new')}
            >
              New Prescription
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
                placeholder="Search by prescription #, patient, doctor or medicine"
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
                Showing {filteredPrescriptions.length} prescriptions
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
            dataSource={filteredPrescriptions}
            columns={columns}
            rowKey="id"
            loading={isLoading}
            pagination={{
              pageSize: 20,
              showSizeChanger: true,
              showTotal: (total) => `Total ${total} prescriptions`
            }}
            scroll={{ x: 1200 }}
          />
        </Space>
      </Card>
    </div>
  )
}

