import { Card, Row, Col, Typography, Tag, Button, Space, Descriptions, Spin, Divider } from 'antd'
import { ArrowLeftOutlined, EditOutlined, MedicineBoxOutlined, UserOutlined, CalendarOutlined, DollarOutlined, FileTextOutlined } from '@ant-design/icons'
import { useParams, useNavigate } from 'react-router-dom'
import { useConsultation } from '@core/hooks/queries/useConsultations'
import { useQuery } from '@tanstack/react-query'
import { prescriptionService } from '@core/services/prescriptionService'
import { useCreateInvoiceFromPrescription } from '@core/hooks/queries/useInvoices'
import { message } from 'antd'
import dayjs from 'dayjs'

const { Title, Text } = Typography

export const ConsultationDetailPage = () => {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const createInvoiceMutation = useCreateInvoiceFromPrescription()
  const { data: consultation, isLoading } = useConsultation(Number(id))
  
  // Fetch prescription if it exists to check for invoice
  const { data: prescription, refetch: refetchPrescription } = useQuery({
    queryKey: ['prescription', consultation?.prescriptionId],
    queryFn: () => prescriptionService.getById(consultation!.prescriptionId!),
    enabled: !!consultation?.hasPrescription && !!consultation?.prescriptionId
  })

  const handleCreateInvoice = async () => {
    if (!prescription) return
    
    try {
      const invoice = await createInvoiceMutation.mutateAsync({
        prescriptionId: prescription.id,
      })
      message.success(`Invoice ${invoice.invoiceNumber} created successfully!`)
      // Refetch prescription to get updated invoice info
      await refetchPrescription()
      navigate(`/invoices/${invoice.id}`)
    } catch (error) {
      // Error is handled by the mutation
    }
  }

  if (isLoading) {
    return (
      <div style={{ textAlign: 'center', padding: '50px' }}>
        <Spin size="large" />
      </div>
    )
  }

  if (!consultation) {
    return (
      <Card>
        <div style={{ textAlign: 'center', padding: '50px' }}>
          <Title level={4}>Consultation not found</Title>
          <Button onClick={() => navigate('/consultations')}>
            Back to Consultations
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
            onClick={() => navigate('/consultations')}
          >
            Back
          </Button>
          <Button
            icon={<EditOutlined />}
            onClick={() => navigate(`/consultations/${consultation.id}/edit`)}
          >
            Edit
          </Button>
          {!consultation.hasPrescription && (
            <Button
              type="primary"
              icon={<MedicineBoxOutlined />}
              onClick={() => navigate(`/prescriptions/new?consultationId=${consultation.id}&patientId=${consultation.patientId}`)}
            >
              Create Prescription
            </Button>
          )}
          {consultation.hasPrescription && consultation.prescriptionId && (
            <>
              {prescription?.hasInvoice && prescription?.invoiceId ? (
                <Button
                  type="primary"
                  icon={<FileTextOutlined />}
                  onClick={() => navigate(`/invoices/${prescription.invoiceId}`)}
                >
                  View Invoice
                </Button>
              ) : (
                <Button
                  type="primary"
                  icon={<DollarOutlined />}
                  onClick={handleCreateInvoice}
                  loading={createInvoiceMutation.isPending}
                >
                  Create Invoice
                </Button>
              )}
              <Button
                icon={<MedicineBoxOutlined />}
                onClick={() => navigate(`/prescriptions/${consultation.prescriptionId}`)}
              >
                View Prescription
              </Button>
            </>
          )}
        </Space>
      </div>

      <Row gutter={[24, 24]}>
        {/* Consultation Details */}
        <Col xs={24} lg={16}>
          <Card title="Consultation Details">
            <Descriptions column={1} bordered>
              <Descriptions.Item label="Consultation Date">
                <Space>
                  <CalendarOutlined />
                  <Text>{dayjs(consultation.consultationDate).format('MMMM DD, YYYY [at] hh:mm A')}</Text>
                </Space>
              </Descriptions.Item>
              <Descriptions.Item label="Patient">
                <Space>
                  <UserOutlined />
                  <Text strong>{consultation.patientName}</Text>
                </Space>
              </Descriptions.Item>
              <Descriptions.Item label="Doctor">
                <Text strong>{consultation.doctorName}</Text>
              </Descriptions.Item>
              <Descriptions.Item label="Consultation Fee">
                <Text strong style={{ fontSize: '16px', color: '#1890ff' }}>
                  ₹{consultation.consultationFee.toFixed(2)}
                </Text>
              </Descriptions.Item>
            </Descriptions>

            <Divider>Clinical Information</Divider>

            <Descriptions column={1} bordered>
              <Descriptions.Item label="Chief Complaint">
                <Text>{consultation.chiefComplaint || '-'}</Text>
              </Descriptions.Item>
              {consultation.symptoms && (
                <Descriptions.Item label="Symptoms">
                  <Text style={{ whiteSpace: 'pre-wrap' }}>{consultation.symptoms}</Text>
                </Descriptions.Item>
              )}
              {consultation.examination && (
                <Descriptions.Item label="Physical Examination">
                  <Text style={{ whiteSpace: 'pre-wrap' }}>{consultation.examination}</Text>
                </Descriptions.Item>
              )}
              {consultation.diagnosis && (
                <Descriptions.Item label="Diagnosis">
                  <Text style={{ whiteSpace: 'pre-wrap' }}>{consultation.diagnosis}</Text>
                </Descriptions.Item>
              )}
              {consultation.treatmentPlan && (
                <Descriptions.Item label="Treatment Plan">
                  <Text style={{ whiteSpace: 'pre-wrap' }}>{consultation.treatmentPlan}</Text>
                </Descriptions.Item>
              )}
              {consultation.notes && (
                <Descriptions.Item label="Additional Notes">
                  <Text style={{ whiteSpace: 'pre-wrap' }}>{consultation.notes}</Text>
                </Descriptions.Item>
              )}
            </Descriptions>
          </Card>
        </Col>

        {/* Related Information */}
        <Col xs={24} lg={8}>
          <Card title="Related Information">
            <Space direction="vertical" style={{ width: '100%' }} size="large">
              <div>
                <Text type="secondary" style={{ fontSize: '12px' }}>Appointment ID</Text>
                <div>
                  <Button
                    type="link"
                    onClick={() => navigate(`/appointments/${consultation.appointmentId}`)}
                    style={{ padding: 0 }}
                  >
                    View Appointment #{consultation.appointmentId}
                  </Button>
                </div>
              </div>
              <Divider style={{ margin: '8px 0' }} />
              <div>
                <Text type="secondary" style={{ fontSize: '12px' }}>Patient ID</Text>
                <div>
                  <Button
                    type="link"
                    onClick={() => navigate(`/patients/${consultation.patientId}`)}
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
                  <Text>{dayjs(consultation.createdAt).format('MMMM DD, YYYY [at] hh:mm A')}</Text>
                </div>
              </div>
            </Space>
          </Card>
        </Col>
      </Row>
    </div>
  )
}

