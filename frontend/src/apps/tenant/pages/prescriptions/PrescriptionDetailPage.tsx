import { Card, Row, Col, Typography, Tag, Button, Space, Descriptions, Spin, Table, Divider } from 'antd'
import { ArrowLeftOutlined, DownloadOutlined, MedicineBoxOutlined, UserOutlined, CalendarOutlined } from '@ant-design/icons'
import { useParams, useNavigate } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { prescriptionService, Prescription } from '@core/services/prescriptionService'
import dayjs from 'dayjs'

const { Title, Text } = Typography

const DISPENSING_FORMS: Record<number, string> = {
  1: 'Globules',
  2: 'Tablets',
  3: 'Packet',
  4: 'Liquid',
  5: 'Tonic'
}

export const PrescriptionDetailPage = () => {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const { data: prescription, isLoading } = useQuery<Prescription>({
    queryKey: ['prescription', id],
    queryFn: () => prescriptionService.getById(Number(id!)),
    enabled: !!id && !isNaN(Number(id))
  })

  const handleDownloadPdf = async (includeMedicineNames: boolean = true) => {
    if (!prescription) return
    try {
      const blob = await prescriptionService.downloadPdf(prescription.id, includeMedicineNames)
      const url = window.URL.createObjectURL(blob)
      const link = document.createElement('a')
      link.href = url
      link.download = `Prescription_${prescription.prescriptionNumber}_${includeMedicineNames ? 'Internal' : 'Patient'}.pdf`
      document.body.appendChild(link)
      link.click()
      document.body.removeChild(link)
      window.URL.revokeObjectURL(url)
    } catch (error) {
      console.error('Failed to download PDF:', error)
    }
  }

  const formatDispensedQuantity = (dispensedQuantity: number | undefined, dispensingForm: number): string => {
    if (dispensedQuantity === undefined) return '-'
    // Globules: show in drops
    if (dispensingForm === 1) {
      return `${Math.round(dispensedQuantity)} drops`
    }
    // Packets: show in drops (1 Packet = 5 drops)
    if (dispensingForm === 3) {
      return `${Math.round(dispensedQuantity)} drops`
    }
    // Liquid: show in ml
    if (dispensingForm === 4) {
      return `${dispensedQuantity} ml`
    }
    // Tonic, Tablets: show as count
    return Math.round(dispensedQuantity).toString()
  }

  const formatQuantity = (quantity: number | undefined, dispensingForm: number): string => {
    if (quantity === undefined) return '-'
    if (dispensingForm === 4 || dispensingForm === 5) {
      return `${quantity} ml`
    }
    return quantity.toString()
  }

  const medicineColumns = [
    {
      title: 'Medicine Name',
      dataIndex: 'medicineName',
      key: 'medicineName',
      render: (name: string, record: Prescription['medicines'][0]) => (
        <div>
          <div style={{ fontWeight: 500 }}>{name}</div>
          <div style={{ fontSize: '12px', color: '#666' }}>
            {DISPENSING_FORMS[record.dispensingForm] || 'Unknown'} 
            {record.containerSize && record.dispensingForm === 1 && ` (${record.containerSize} dram)`}
          </div>
        </div>
      )
    },
    {
      title: 'Dosage',
      dataIndex: 'dosage',
      key: 'dosage',
      width: 120
    },
    {
      title: 'Frequency',
      dataIndex: 'frequency',
      key: 'frequency',
      width: 150
    },
    {
      title: 'Duration',
      dataIndex: 'duration',
      key: 'duration',
      width: 120
    },
    {
      title: 'Timing',
      dataIndex: 'timing',
      key: 'timing',
      width: 120
    },
    {
      title: 'Quantity',
      dataIndex: 'quantity',
      key: 'quantity',
      width: 100,
      render: (quantity: number | undefined, record: Prescription['medicines'][0]) => 
        formatQuantity(quantity, record.dispensingForm)
    },
    {
      title: 'Dispensed Qty',
      dataIndex: 'dispensedQuantity',
      key: 'dispensedQuantity',
      width: 120,
      render: (dispensedQuantity: number | undefined, record: Prescription['medicines'][0]) =>
        formatDispensedQuantity(dispensedQuantity, record.dispensingForm)
    },
    {
      title: 'Instructions',
      dataIndex: 'instructions',
      key: 'instructions',
      ellipsis: true,
      render: (instructions: string) => instructions || '-'
    }
  ]

  if (isLoading) {
    return (
      <div style={{ textAlign: 'center', padding: '50px' }}>
        <Spin size="large" />
      </div>
    )
  }

  if (!prescription) {
    return (
      <Card>
        <div style={{ textAlign: 'center', padding: '50px' }}>
          <Title level={4}>Prescription not found</Title>
          <Button onClick={() => navigate('/prescriptions')}>
            Back to Prescriptions
          </Button>
        </div>
      </Card>
    )
  }

  return (
    <div>
      <div style={{ marginBottom: 24 }}>
        <Space>
          <Button
            icon={<ArrowLeftOutlined />}
            onClick={() => navigate('/prescriptions')}
          >
            Back
          </Button>
          <Button
            icon={<DownloadOutlined />}
            onClick={() => handleDownloadPdf(true)}
          >
            Download PDF (Internal)
          </Button>
          <Button
            icon={<DownloadOutlined />}
            onClick={() => handleDownloadPdf(false)}
          >
            Download PDF (Patient)
          </Button>
        </Space>
      </div>

      <Row gutter={[24, 24]}>
        {/* Prescription Details */}
        <Col xs={24} lg={16}>
          <Card title="Prescription Details">
            <Descriptions column={1} bordered>
              <Descriptions.Item label="Prescription Number">
                <Tag color="blue" style={{ fontSize: '14px', padding: '4px 12px' }}>
                  {prescription.prescriptionNumber}
                </Tag>
              </Descriptions.Item>
              <Descriptions.Item label="Date">
                <Space>
                  <CalendarOutlined />
                  <Text>{dayjs(prescription.prescriptionDate).format('MMMM DD, YYYY [at] hh:mm A')}</Text>
                </Space>
              </Descriptions.Item>
              <Descriptions.Item label="Patient">
                <Space>
                  <UserOutlined />
                  <Text strong>{prescription.patientName}</Text>
                </Space>
              </Descriptions.Item>
              <Descriptions.Item label="Doctor">
                <Text strong>{prescription.doctorName}</Text>
              </Descriptions.Item>
              {prescription.notes && (
                <Descriptions.Item label="Notes">
                  <Text style={{ whiteSpace: 'pre-wrap' }}>{prescription.notes}</Text>
                </Descriptions.Item>
              )}
            </Descriptions>

            <Divider>Medicines</Divider>

            <Table
              dataSource={prescription.medicines || []}
              columns={medicineColumns}
              rowKey={(record, index) => `${record.medicineId}-${index}`}
              pagination={false}
              size="small"
            />
          </Card>
        </Col>

        {/* Related Information */}
        <Col xs={24} lg={8}>
          <Card title="Related Information">
            <Space direction="vertical" style={{ width: '100%' }} size="large">
              <div>
                <Text type="secondary" style={{ fontSize: '12px' }}>Consultation ID</Text>
                <div>
                  <Button
                    type="link"
                    icon={<MedicineBoxOutlined />}
                    onClick={() => navigate(`/consultations/${prescription.consultationId}`)}
                    style={{ padding: 0 }}
                  >
                    View Consultation #{prescription.consultationId}
                  </Button>
                </div>
              </div>
              <Divider style={{ margin: '8px 0' }} />
              <div>
                <Text type="secondary" style={{ fontSize: '12px' }}>Patient ID</Text>
                <div>
                  <Button
                    type="link"
                    icon={<UserOutlined />}
                    onClick={() => navigate(`/patients/${prescription.patientId}`)}
                    style={{ padding: 0 }}
                  >
                    View Patient Profile
                  </Button>
                </div>
              </div>
              <Divider style={{ margin: '8px 0' }} />
              <div>
                <Text type="secondary" style={{ fontSize: '12px' }}>Created At</Text>
                <div>
                  <Text>{dayjs(prescription.createdAt).format('MMMM DD, YYYY [at] hh:mm A')}</Text>
                </div>
              </div>
            </Space>
          </Card>
        </Col>
      </Row>
    </div>
  )
}

