import { Card, Row, Col, Typography, Tag, Button, Space, Descriptions, Spin, Image } from 'antd'
import { ArrowLeftOutlined, EditOutlined, MedicineBoxOutlined, DollarOutlined, FileTextOutlined } from '@ant-design/icons'
import { useParams, useNavigate } from 'react-router-dom'
import { useConsultation } from '@core/hooks/queries/useConsultations'
import { useQuery } from '@tanstack/react-query'
import { prescriptionService } from '@core/services/prescriptionService'
import { useCreateInvoiceFromPrescription } from '@core/hooks/queries/useInvoices'
import { useUser } from '@core/stores/authStore'
import { message } from 'antd'
import dayjs from 'dayjs'
import type { ConsultationPhoto } from '@core/services/consultationService'

const { Title, Text } = Typography

export const ConsultationDetailPage = () => {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const user = useUser()
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
            onClick={() => navigate(`/appointments/${consultation.appointmentId}`)}
          >
            View Appointment
          </Button>
          <Button
            icon={<EditOutlined />}
            onClick={() => navigate(`/consultations/${consultation.id}/edit`)}
          >
            Edit
          </Button>
          {!consultation.hasPrescription && (user?.role === 'Doctor' || user?.role === 'Admin') && (
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
        <Col xs={24}>
          <Card title="Consultation Details" size="small">
            <Descriptions column={2} bordered size="small">
              <Descriptions.Item label="Consultation Date">
                <Text>{dayjs(consultation.consultationDate).format('MMM DD, YYYY hh:mm A')}</Text>
              </Descriptions.Item>
              <Descriptions.Item label="Consultation Fee">
                <Text strong style={{ fontSize: '14px', color: '#1890ff' }}>
                  ₹{consultation.consultationFee.toFixed(2)}
                </Text>
              </Descriptions.Item>
              <Descriptions.Item label="Patient">
                <Space>
                  <Text strong>{consultation.patientName}</Text>
                  <Button
                    type="link"
                    size="small"
                    onClick={() => navigate(`/patients/${consultation.patientId}`)}
                    style={{ padding: 0, height: 'auto' }}
                  >
                    View Profile
                  </Button>
                </Space>
              </Descriptions.Item>
              <Descriptions.Item label="Doctor">
                <Text strong>{consultation.doctorName}</Text>
              </Descriptions.Item>
            </Descriptions>
          </Card>
        </Col>
      </Row>

      {/* Clinical Information Card */}
      <Row gutter={[24, 24]} style={{ marginTop: 24 }}>
        <Col xs={24}>
          <Card title="Clinical Information">
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
      </Row>

      {/* Consultation Photos */}
      {consultation.photos && consultation.photos.length > 0 && (
        <Row gutter={[24, 24]} style={{ marginTop: 24 }}>
          <Col xs={24}>
            <Card title="Consultation Photos">
              <Image.PreviewGroup>
                <Space wrap>
                  {consultation.photos.map((photo: ConsultationPhoto) => (
                    <Image
                      key={photo.id}
                      width={120}
                      height={120}
                      src={photo.photoUrl}
                      alt={photo.description || `Photo ${photo.id}`}
                      style={{ objectFit: 'cover', cursor: 'pointer', borderRadius: '4px' }}
                      preview={{
                        mask: 'View'
                      }}
                    />
                  ))}
                </Space>
              </Image.PreviewGroup>
            </Card>
          </Col>
        </Row>
      )}
    </div>
  )
}

