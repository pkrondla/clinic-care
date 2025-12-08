import { useEffect } from 'react'
import { useNavigate, useSearchParams, useParams } from 'react-router-dom'
import { Card, Form, Input, InputNumber, Button, message, Space, Spin, Typography } from 'antd'
import { SaveOutlined, ArrowLeftOutlined } from '@ant-design/icons'
import { useMutation } from '@tanstack/react-query'
import { consultationService, type CreateConsultationRequest, type UpdateConsultationRequest } from '@core/services/consultationService'
import { useConsultation, useUpdateConsultation } from '@core/hooks/queries/useConsultations'
import { useAppointment } from '@core/hooks/queries/useAppointments'
import dayjs from 'dayjs'

const { TextArea } = Input
const { Title, Text } = Typography

export const ConsultationFormPage = () => {
  const [form] = Form.useForm()
  const navigate = useNavigate()
  const { id } = useParams<{ id: string }>()
  const [searchParams] = useSearchParams()
  
  const isEditMode = !!id
  const consultationId = id ? parseInt(id) : undefined
  
  const appointmentId = searchParams.get('appointmentId')
  const patientId = searchParams.get('patientId')
  
  // Fetch consultation if in edit mode
  const { data: consultation, isLoading: isLoadingConsultation } = useConsultation(consultationId || 0)
  
  // Fetch appointment to get the doctor ID (for create mode)
  const { data: appointment, isLoading: isLoadingAppointment } = useAppointment(
    appointmentId ? parseInt(appointmentId) : (consultation?.appointmentId || 0)
  )
  
  const updateConsultation = useUpdateConsultation()

  const createMutation = useMutation({
    mutationFn: consultationService.create,
    onSuccess: (data) => {
      message.success('Consultation saved successfully')
      // Navigate to prescription creation with consultation ID
      navigate(`/prescriptions/new?consultationId=${data.id}&patientId=${patientId}`)
    },
    onError: (error: any) => {
      const errorMessage = error.response?.data?.errors?.[0] || error.response?.data?.message || error.message || 'Failed to save consultation'
      message.error(errorMessage)
      console.error('Consultation creation error:', error.response?.data || error)
    }
  })

  // Load consultation data when in edit mode
  useEffect(() => {
    if (isEditMode && consultation) {
      form.setFieldsValue({
        chiefComplaint: consultation.chiefComplaint,
        symptoms: consultation.symptoms,
        examination: consultation.examination,
        diagnosis: consultation.diagnosis,
        treatmentPlan: consultation.treatmentPlan,
        notes: consultation.notes,
        consultationFee: consultation.consultationFee
      })
    }
  }, [isEditMode, consultation, form])

  const handleSubmit = async () => {
    try {
      const values = await form.validateFields()
      
      if (isEditMode) {
        // Update existing consultation
        const updateData: UpdateConsultationRequest = {
          chiefComplaint: values.chiefComplaint,
          symptoms: values.symptoms,
          examination: values.examination,
          diagnosis: values.diagnosis,
          treatmentPlan: values.treatmentPlan,
          notes: values.notes,
          consultationFee: values.consultationFee
        }
        
        await updateConsultation.mutateAsync({
          id: consultationId!,
          data: updateData
        })
        
        navigate(`/consultations/${consultationId}`)
      } else {
        // Create new consultation
        if (!appointment) {
          message.error('Appointment not found. Please try again.')
          return
        }
        
        const consultationData: CreateConsultationRequest = {
          appointmentId: parseInt(appointmentId || '0'),
          patientId: parseInt(patientId || '0'),
          doctorId: appointment.doctor?.id || 0, // Use doctor ID from appointment, not logged-in user
          ...values
        }

        createMutation.mutate(consultationData)
      }
    } catch (error) {
      console.error('Validation failed:', error)
    }
  }

  // Loading states
  if (isEditMode && isLoadingConsultation) {
    return (
      <div style={{ textAlign: 'center', padding: '50px' }}>
        <Spin size="large" />
      </div>
    )
  }

  if (isEditMode && !consultation) {
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

  // For create mode, check if appointmentId and patientId are provided
  if (!isEditMode && (!appointmentId || !patientId)) {
    return (
      <div style={{ padding: '24px', textAlign: 'center' }}>
        <Card>
          <h3>Create New Consultation</h3>
          <p>To create a consultation, please start from:</p>
          <ul style={{ textAlign: 'left', maxWidth: '400px', margin: '20px auto' }}>
            <li><strong>Appointments Page:</strong> Select an appointment and click "Start Consultation"</li>
            <li><strong>Queue Page:</strong> Select a patient from the queue and start consultation</li>
            <li><strong>Patient Detail Page:</strong> Click "New Consultation" for a specific patient</li>
          </ul>
          <Space>
            <Button type="primary" onClick={() => navigate('/appointments')}>
              Go to Appointments
            </Button>
            <Button onClick={() => navigate('/queue')}>
              Go to Queue
            </Button>
            <Button onClick={() => navigate(-1)}>
              Go Back
            </Button>
          </Space>
        </Card>
      </div>
    )
  }

  if (!isEditMode && isLoadingAppointment) {
    return (
      <div style={{ textAlign: 'center', padding: '50px' }}>
        <Spin size="large" />
      </div>
    )
  }

  if (!isEditMode && !appointment) {
    return (
      <div style={{ padding: '24px', textAlign: 'center' }}>
        <Card>
          <h3>Appointment Not Found</h3>
          <p>The appointment you're trying to create a consultation for could not be found.</p>
          <Space>
            <Button type="primary" onClick={() => navigate('/appointments')}>
              Go to Appointments
            </Button>
            <Button onClick={() => navigate(-1)}>
              Go Back
            </Button>
          </Space>
        </Card>
      </div>
    )
  }

  const currentPatientId = isEditMode ? consultation?.patientId : parseInt(patientId || '0')
  const currentAppointmentId = isEditMode ? consultation?.appointmentId : parseInt(appointmentId || '0')
  const displayPatientName = isEditMode ? consultation?.patientName : appointment?.patient?.name
  const displayDoctorName = isEditMode ? consultation?.doctorName : appointment?.doctor?.name
  const displayDate = isEditMode && consultation 
    ? dayjs(consultation.consultationDate).format('MMMM DD, YYYY')
    : appointment 
    ? dayjs(appointment.appointmentDate).format('MMMM DD, YYYY')
    : ''

  return (
    <div style={{ padding: '24px', maxWidth: '900px', margin: '0 auto' }}>
      <div style={{ marginBottom: '24px' }}>
        <Button
          icon={<ArrowLeftOutlined />}
          onClick={() => isEditMode ? navigate(`/consultations/${consultationId}`) : navigate(-1)}
          style={{ marginBottom: '16px' }}
        >
          Back
        </Button>
        <Title level={2} style={{ margin: 0 }}>
          {isEditMode ? 'Edit Consultation' : 'New Consultation'}
        </Title>
        <p style={{ margin: '8px 0 0 0', color: '#666' }}>
          {isEditMode ? 'Update consultation details' : 'Record patient consultation details'}
        </p>
        {(displayPatientName || displayDoctorName || displayDate) && (
          <Card size="small" style={{ marginTop: 16 }}>
            <Space direction="vertical" style={{ width: '100%' }}>
              {displayPatientName && (
                <Space>
                  <Text strong>Patient:</Text>
                  <Text>{displayPatientName}</Text>
                </Space>
              )}
              {displayDoctorName && (
                <Space>
                  <Text strong>Doctor:</Text>
                  <Text>{displayDoctorName}</Text>
                </Space>
              )}
              {displayDate && (
                <Space>
                  <Text strong>Date:</Text>
                  <Text>{displayDate}</Text>
                </Space>
              )}
            </Space>
          </Card>
        )}
      </div>

      <Card>
        <Form
          form={form}
          layout="vertical"
          initialValues={{
            consultationFee: isEditMode ? consultation?.consultationFee : 50.00
          }}
        >
          <Form.Item
            label="Chief Complaint"
            name="chiefComplaint"
            rules={[{ required: true, message: 'Please enter chief complaint' }]}
          >
            <TextArea
              rows={2}
              placeholder="Main reason for visit (e.g., Fever and cough for 3 days)"
            />
          </Form.Item>

          <Form.Item
            label="Symptoms"
            name="symptoms"
          >
            <TextArea
              rows={3}
              placeholder="Detailed symptoms (e.g., High fever (102°F), dry cough, body aches, headache)"
            />
          </Form.Item>

          <Form.Item
            label="Physical Examination"
            name="examination"
          >
            <TextArea
              rows={3}
              placeholder="Examination findings (e.g., Temperature 102°F, BP 120/80, clear chest sounds, throat slightly red)"
            />
          </Form.Item>

          <Form.Item
            label="Diagnosis"
            name="diagnosis"
          >
            <TextArea
              rows={2}
              placeholder="Clinical diagnosis (e.g., Upper Respiratory Tract Infection)"
            />
          </Form.Item>

          <Form.Item
            label="Treatment Plan"
            name="treatmentPlan"
          >
            <TextArea
              rows={3}
              placeholder="Recommended treatment (e.g., Rest, fluids, prescribed medications, follow-up in 3 days if symptoms persist)"
            />
          </Form.Item>

          <Form.Item
            label="Additional Notes"
            name="notes"
          >
            <TextArea
              rows={2}
              placeholder="Any additional notes or observations"
            />
          </Form.Item>

          <Form.Item
            label="Consultation Fee ($)"
            name="consultationFee"
            rules={[{ required: true, message: 'Please enter consultation fee' }]}
          >
            <InputNumber
              min={0}
              step={0.01}
              style={{ width: '200px' }}
              placeholder="0.00"
            />
          </Form.Item>

          <Form.Item style={{ marginBottom: 0 }}>
            <Space>
              <Button
                type="primary"
                icon={<SaveOutlined />}
                onClick={handleSubmit}
                loading={isEditMode ? updateConsultation.isPending : createMutation.isPending}
                size="large"
              >
                {isEditMode ? 'Save Changes' : 'Save & Create Prescription'}
              </Button>
              <Button
                onClick={() => isEditMode ? navigate(`/consultations/${consultationId}`) : navigate(-1)}
                size="large"
              >
                Cancel
              </Button>
            </Space>
          </Form.Item>
        </Form>
      </Card>
    </div>
  )
}

