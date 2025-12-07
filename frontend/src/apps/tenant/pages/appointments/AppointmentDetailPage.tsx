import { Card, Row, Col, Typography, Tag, Button, Space, Descriptions, Spin, Divider } from 'antd'
import { ArrowLeftOutlined, EditOutlined, CalendarOutlined, UserOutlined, MedicineBoxOutlined, PlusOutlined } from '@ant-design/icons'
import { useParams, useNavigate } from 'react-router-dom'
import { useAppointment } from '@core/hooks/queries/useAppointments'
import { AppointmentStatus, AppointmentType } from '@core/types'
import dayjs from 'dayjs'

const { Title, Text } = Typography

export const AppointmentDetailPage = () => {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const { data: appointment, isLoading } = useAppointment(Number(id))

  if (isLoading) {
    return (
      <div style={{ textAlign: 'center', padding: '50px' }}>
        <Spin size="large" />
      </div>
    )
  }

  if (!appointment) {
    return (
      <Card>
        <div style={{ textAlign: 'center', padding: '50px' }}>
          <Title level={4}>Appointment not found</Title>
          <Button onClick={() => navigate('/appointments')}>
            Back to Appointments
          </Button>
        </div>
      </Card>
    )
  }

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

  const canEdit = appointment.status === AppointmentStatus.Scheduled
  const canStartConsultation = (appointment.status === AppointmentStatus.Scheduled || appointment.status === AppointmentStatus.InProgress) && !appointment.consultation

  return (
    <div>
      <div style={{ marginBottom: 24 }}>
        <Space>
          <Button
            icon={<ArrowLeftOutlined />}
            onClick={() => navigate('/appointments')}
          >
            Back
          </Button>
          {canEdit && (
            <Button
              icon={<EditOutlined />}
              onClick={() => navigate(`/appointments/${appointment.id}/edit`)}
            >
              Edit
            </Button>
          )}
          {canStartConsultation && (
            <Button
              type="primary"
              icon={<MedicineBoxOutlined />}
              onClick={() => navigate(`/consultations/new?appointmentId=${appointment.id}&patientId=${appointment.patient?.id}`)}
            >
              Start Consultation
            </Button>
          )}
        </Space>
      </div>

      <Row gutter={[24, 24]}>
        {/* Appointment Details */}
        <Col xs={24} lg={16}>
          <Card title="Appointment Details">
            <Descriptions column={1} bordered>
              <Descriptions.Item label="Token Number">
                <Tag color="blue" style={{ fontSize: '16px', padding: '4px 12px' }}>
                  #{appointment.tokenNumber}
                </Tag>
              </Descriptions.Item>
              <Descriptions.Item label="Date">
                <Space>
                  <CalendarOutlined />
                  {dayjs(appointment.appointmentDate).format('MMMM DD, YYYY')}
                </Space>
              </Descriptions.Item>
              <Descriptions.Item label="Type">
                <Tag color={appointment.type === AppointmentType.InPerson ? 'green' : 'blue'}>
                  {getTypeText(appointment.type)}
                </Tag>
              </Descriptions.Item>
              <Descriptions.Item label="Status">
                <Tag color={getStatusColor(appointment.status)}>
                  {getStatusText(appointment.status)}
                </Tag>
              </Descriptions.Item>
              <Descriptions.Item label="Notes">
                {appointment.notes || <Text type="secondary">No notes</Text>}
              </Descriptions.Item>
            </Descriptions>
          </Card>
        </Col>

        {/* Patient & Doctor Info */}
        <Col xs={24} lg={8}>
          <Card title="Patient Information">
            <Space direction="vertical" style={{ width: '100%' }} size="large">
              <div>
                <Text strong>Name:</Text>
                <br />
                <Text>{appointment.patient?.name || 'N/A'}</Text>
              </div>
              {appointment.patient?.patientCode && (
                <div>
                  <Text strong>Patient Code:</Text>
                  <br />
                  <Text>{appointment.patient.patientCode}</Text>
                </div>
              )}
              <Button
                type="link"
                icon={<UserOutlined />}
                onClick={() => navigate(`/patients/${appointment.patient?.id}`)}
                style={{ padding: 0 }}
              >
                View Patient Details
              </Button>
            </Space>
          </Card>

          <Card title="Doctor Information" style={{ marginTop: 16 }}>
            <Space direction="vertical" style={{ width: '100%' }} size="large">
              <div>
                <Text strong>Name:</Text>
                <br />
                <Text>{appointment.doctor?.name || 'N/A'}</Text>
              </div>
              {appointment.doctor?.specialization && (
                <div>
                  <Text strong>Specialization:</Text>
                  <br />
                  <Text>{appointment.doctor.specialization}</Text>
                </div>
              )}
            </Space>
          </Card>

          <Card title="Clinic Information" style={{ marginTop: 16 }}>
            <Space direction="vertical" style={{ width: '100%' }}>
              <div>
                <Text strong>Name:</Text>
                <br />
                <Text>{appointment.clinic?.name || 'N/A'}</Text>
              </div>
              {appointment.clinic?.code && (
                <div>
                  <Text strong>Code:</Text>
                  <br />
                  <Text>{appointment.clinic.code}</Text>
                </div>
              )}
            </Space>
          </Card>
        </Col>

        {/* Consultation Info */}
        {appointment.consultation && (
          <Col xs={24}>
            <Card 
              title={
                <Space>
                  <MedicineBoxOutlined />
                  Consultation Details
                </Space>
              }
            >
              <Descriptions column={1} bordered>
                <Descriptions.Item label="Consultation Date">
                  {dayjs(appointment.consultation.consultationDate).format('MMMM DD, YYYY HH:mm')}
                </Descriptions.Item>
                <Descriptions.Item label="Chief Complaint">
                  {appointment.consultation.chiefComplaint || <Text type="secondary">Not specified</Text>}
                </Descriptions.Item>
                <Descriptions.Item label="Diagnosis">
                  {appointment.consultation.diagnosis || <Text type="secondary">Not specified</Text>}
                </Descriptions.Item>
              </Descriptions>
              <div style={{ marginTop: 16 }}>
                <Button
                  type="link"
                  onClick={() => navigate(`/consultations?appointmentId=${appointment.id}`)}
                >
                  View Consultation Details
                </Button>
              </div>
            </Card>
          </Col>
        )}
      </Row>
    </div>
  )
}


