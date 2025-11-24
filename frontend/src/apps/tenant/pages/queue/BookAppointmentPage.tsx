import { useEffect } from 'react'
import { Card, Form, Select, DatePicker, Button, Space, Typography, message, Input, Spin } from 'antd'
import { CalendarOutlined, CheckCircleOutlined } from '@ant-design/icons'
import { useBookAppointment } from '@core/hooks/queries/useQueues'
import { useClinics } from '@core/hooks/queries/useClinics'
import { useDoctors } from '@core/hooks/queries/useDoctors'
import { useSelectedClinic } from '@core/stores/authStore'
import { useNavigate } from 'react-router-dom'
import dayjs from 'dayjs'
import type { Clinic } from '@core/services/clinicService'

const { Title, Text } = Typography
const { Option } = Select

interface BookAppointmentFormValues {
  clinicId: number
  doctorId: number
  appointmentDate: dayjs.Dayjs
  type: number
  notes?: string
}

export const BookAppointmentPage = () => {
  const navigate = useNavigate()
  const selectedClinic = useSelectedClinic()
  const [form] = Form.useForm<BookAppointmentFormValues>()
  const bookAppointment = useBookAppointment()
  const { data: clinics } = useClinics()
  
  // Get selected clinic ID from form
  const selectedClinicId = Form.useWatch('clinicId', form) || selectedClinic?.id
  
  // Load doctors for selected clinic
  const { data: doctors, isLoading: doctorsLoading } = useDoctors({
    clinicId: selectedClinicId,
    isActive: true,
  })
  
  // Reset doctor selection when clinic changes
  useEffect(() => {
    if (selectedClinicId) {
      form.setFieldsValue({ doctorId: undefined })
    }
  }, [selectedClinicId, form])

  const handleSubmit = async (values: BookAppointmentFormValues) => {
    try {
      const result = await bookAppointment.mutateAsync({
        clinicId: values.clinicId,
        doctorId: values.doctorId,
        appointmentDate: values.appointmentDate.format('YYYY-MM-DD'),
        type: values.type || 1,
        notes: values.notes,
      })

      message.success(`Appointment booked! Your token number is ${result.tokenNumber}`)
      
      // Navigate to patient queue view or appointments
      navigate('/my-appointments')
    } catch (error) {
      // Error handled by mutation
    }
  }

  return (
    <div style={{ maxWidth: 600, margin: '0 auto', padding: '24px' }}>
      <Card>
        <div style={{ textAlign: 'center', marginBottom: 32 }}>
          <Title level={2}>
            <CalendarOutlined /> Book Appointment
          </Title>
          <Text type="secondary">
            Book an appointment with a doctor. You will receive a token number.
          </Text>
        </div>

        <Form
          form={form}
          layout="vertical"
          onFinish={handleSubmit}
          initialValues={{
            clinicId: selectedClinic?.id,
            appointmentDate: dayjs(),
            type: 1, // InPerson
          }}
        >
          <Form.Item
            name="clinicId"
            label="Clinic"
            rules={[{ required: true, message: 'Please select a clinic' }]}
          >
            <Select placeholder="Select clinic">
              {clinics?.map((clinic: Clinic) => (
                <Option key={clinic.id} value={clinic.id}>
                  {clinic.name}
                </Option>
              ))}
            </Select>
          </Form.Item>

          <Form.Item
            name="doctorId"
            label="Doctor"
            rules={[{ required: true, message: 'Please select a doctor' }]}
            dependencies={['clinicId']}
          >
            <Select 
              placeholder={selectedClinicId ? "Select doctor" : "Please select a clinic first"}
              disabled={!selectedClinicId || doctorsLoading}
              notFoundContent={doctorsLoading ? <Spin size="small" /> : "No doctors available"}
              loading={doctorsLoading}
            >
              {doctors?.map((doctor) => (
                <Option key={doctor.id} value={doctor.id}>
                  {doctor.doctorName} - {doctor.qualification}
                  {doctor.specialization && ` (${doctor.specialization})`}
                </Option>
              ))}
            </Select>
          </Form.Item>

          <Form.Item
            name="appointmentDate"
            label="Date"
            rules={[{ required: true, message: 'Please select a date' }]}
          >
            <DatePicker
              style={{ width: '100%' }}
              format="YYYY-MM-DD"
              disabledDate={(current) => current && current < dayjs().startOf('day')}
            />
          </Form.Item>

          <Form.Item
            name="type"
            label="Appointment Type"
            rules={[{ required: true }]}
          >
            <Select>
              <Option value={1}>In-Person</Option>
              <Option value={2}>Teleconsultation</Option>
            </Select>
          </Form.Item>

          <Form.Item
            name="notes"
            label="Notes (Optional)"
          >
            <Input.TextArea
              rows={3}
              placeholder="Any additional notes or symptoms..."
            />
          </Form.Item>

          <Form.Item>
            <Button
              type="primary"
              htmlType="submit"
              block
              size="large"
              icon={<CheckCircleOutlined />}
              loading={bookAppointment.isPending}
            >
              Book Appointment
            </Button>
          </Form.Item>
        </Form>
      </Card>
    </div>
  )
}

