import { useEffect } from 'react'
import { Card, Form, Input, Button, Space, Typography, message, Spin } from 'antd'
import { ArrowLeftOutlined, SaveOutlined } from '@ant-design/icons'
import { useParams, useNavigate } from 'react-router-dom'
import { useAppointment, useUpdateAppointment } from '@core/hooks/queries/useAppointments'
import { AppointmentStatus } from '@core/types'

const { Title } = Typography
const { TextArea } = Input

interface AppointmentFormValues {
  notes: string
}

export const AppointmentFormPage = () => {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const [form] = Form.useForm<AppointmentFormValues>()
  
  const appointmentId = Number(id)
  const { data: appointment, isLoading } = useAppointment(appointmentId)
  const updateAppointment = useUpdateAppointment()

  useEffect(() => {
    if (appointment) {
      form.setFieldsValue({
        notes: appointment.notes || ''
      })
    }
  }, [appointment, form])

  const handleSubmit = async (values: AppointmentFormValues) => {
    try {
      await updateAppointment.mutateAsync({
        id: appointmentId,
        notes: values.notes || ''
      })
      navigate(`/appointments/${appointmentId}`)
    } catch (error) {
      // Error is handled by the mutation hook
    }
  }

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

  // Only scheduled appointments can be edited
  if (appointment.status !== AppointmentStatus.Scheduled) {
    return (
      <Card>
        <div style={{ textAlign: 'center', padding: '50px' }}>
          <Title level={4}>Cannot Edit Appointment</Title>
          <p>Only scheduled appointments can be edited.</p>
          <Space>
            <Button onClick={() => navigate(`/appointments/${appointmentId}`)}>
              View Details
            </Button>
            <Button onClick={() => navigate('/appointments')}>
              Back to Appointments
            </Button>
          </Space>
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
            onClick={() => navigate(`/appointments/${appointmentId}`)}
          >
            Back
          </Button>
        </Space>
      </div>

      <Card title="Edit Appointment">
        <Form
          form={form}
          layout="vertical"
          onFinish={handleSubmit}
          autoComplete="off"
        >
          <Form.Item
            label="Token Number"
          >
            <Input value={`#${appointment.tokenNumber}`} disabled />
          </Form.Item>

          <Form.Item
            label="Patient"
          >
            <Input value={appointment.patient?.name || 'N/A'} disabled />
          </Form.Item>

          <Form.Item
            label="Doctor"
          >
            <Input value={appointment.doctor?.name || 'N/A'} disabled />
          </Form.Item>

          <Form.Item
            label="Date"
          >
            <Input value={new Date(appointment.appointmentDate).toLocaleDateString()} disabled />
          </Form.Item>

          <Form.Item
            name="notes"
            label="Notes"
            rules={[
              { max: 1000, message: 'Notes cannot exceed 1000 characters' }
            ]}
          >
            <TextArea
              rows={6}
              placeholder="Enter appointment notes..."
              showCount
              maxLength={1000}
            />
          </Form.Item>

          <Form.Item>
            <Space>
              <Button
                type="primary"
                htmlType="submit"
                icon={<SaveOutlined />}
                loading={updateAppointment.isPending}
              >
                Save Changes
              </Button>
              <Button onClick={() => navigate(`/appointments/${appointmentId}`)}>
                Cancel
              </Button>
            </Space>
          </Form.Item>
        </Form>
      </Card>
    </div>
  )
}

