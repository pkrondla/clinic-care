import { useEffect, useState } from 'react'
import { 
  Card, 
  Form, 
  Input, 
  Select, 
  DatePicker, 
  Button, 
  Row, 
  Col, 
  Typography, 
  Space,
  Alert
} from 'antd'
import { ArrowLeftOutlined, SaveOutlined } from '@ant-design/icons'
import { useNavigate, useParams } from 'react-router-dom'
import { useCreatePatient, useUpdatePatient, usePatient } from '@core/hooks/queries/usePatients'
import type { CreatePatientRequest, UpdatePatientRequest } from '@core/types/patient'
import { GENDER_OPTIONS, BLOOD_GROUP_OPTIONS } from '@core/types/patient'
import dayjs from 'dayjs'

const { Title } = Typography
const { TextArea } = Input

// Helper function to extract error message from API error
const getErrorMessage = (error: any): string[] => {
  if (!error) return []
  
  const responseData = error.response?.data
  if (responseData?.errors && Array.isArray(responseData.errors) && responseData.errors.length > 0) {
    return responseData.errors
  }
  if (responseData?.message) {
    return [responseData.message]
  }
  if (error.message) {
    return [error.message]
  }
  return ['An unexpected error occurred']
}

export const PatientFormPage = () => {
  const navigate = useNavigate()
  const { id } = useParams<{ id: string }>()
  const [form] = Form.useForm()
  const isEdit = !!id
  const [errorMessages, setErrorMessages] = useState<string[]>([])

  const { data: patient, isLoading: patientLoading } = usePatient(Number(id))
  const createPatientMutation = useCreatePatient()
  const updatePatientMutation = useUpdatePatient()

  useEffect(() => {
    if (isEdit && patient) {
      form.setFieldsValue({
        ...patient,
        dateOfBirth: dayjs(patient.dateOfBirth)
      })
    }
  }, [isEdit, patient, form])

  // Clear errors when user starts typing
  const handleFieldChange = () => {
    if (errorMessages.length > 0) {
      setErrorMessages([])
    }
  }

  const onFinish = async (values: any) => {
    setErrorMessages([]) // Clear previous errors
    
    try {
      const formData = {
        ...values,
        dateOfBirth: values.dateOfBirth.format('YYYY-MM-DD')
      }

      if (isEdit) {
        await updatePatientMutation.mutateAsync({
          id: Number(id),
          patient: formData as UpdatePatientRequest
        })
      } else {
        await createPatientMutation.mutateAsync(formData as CreatePatientRequest)
      }
      
      navigate('/patients')
    } catch (error: any) {
      // Extract and display error messages
      const messages = getErrorMessage(error)
      setErrorMessages(messages)
    }
  }

  const isLoading = createPatientMutation.isPending || updatePatientMutation.isPending || patientLoading

  return (
    <div>
      <div style={{ marginBottom: 24, display: 'flex', alignItems: 'center', gap: 16 }}>
        <Button
          icon={<ArrowLeftOutlined />}
          onClick={() => navigate('/patients')}
        >
          Back to Patients
        </Button>
        <Title level={2} style={{ margin: 0 }}>
          {isEdit ? 'Edit Patient' : 'Add New Patient'}
        </Title>
      </div>

      <Card>
        {/* Display error messages as a persistent Alert */}
        {errorMessages.length > 0 && (
          <Alert
            message="Error"
            description={
              <ul style={{ margin: 0, paddingLeft: 20 }}>
                {errorMessages.map((msg, index) => (
                  <li key={index}>{msg}</li>
                ))}
              </ul>
            }
            type="error"
            showIcon
            closable
            onClose={() => setErrorMessages([])}
            style={{ marginBottom: 24 }}
          />
        )}

        <Form
          form={form}
          layout="vertical"
          onFinish={onFinish}
          onValuesChange={handleFieldChange}
          disabled={isLoading}
          autoComplete="off"
          initialValues={{
            gender: 'Male',
            bloodGroup: 'O+'
          }}
        >
          <Row gutter={[24, 0]}>
            <Col xs={24} sm={12}>
              <Form.Item
                name="firstName"
                label="First Name"
                rules={[
                  { required: true, message: 'Please enter first name' },
                  { min: 2, message: 'First name must be at least 2 characters' }
                ]}
              >
                <Input placeholder="Enter first name" />
              </Form.Item>
            </Col>
            <Col xs={24} sm={12}>
              <Form.Item
                name="lastName"
                label="Last Name"
                rules={[
                  { required: true, message: 'Please enter last name' },
                  { min: 2, message: 'Last name must be at least 2 characters' }
                ]}
              >
                <Input placeholder="Enter last name" />
              </Form.Item>
            </Col>
          </Row>

          <Row gutter={[24, 0]}>
            <Col xs={24} sm={12}>
              <Form.Item
                name="email"
                label="Email"
                rules={[
                  { required: true, message: 'Please enter email' },
                  { type: 'email', message: 'Please enter a valid email' }
                ]}
              >
                <Input placeholder="Enter email address" autoComplete="new-email" />
              </Form.Item>
            </Col>
            <Col xs={24} sm={12}>
              <Form.Item
                name="phone"
                label="Phone"
                rules={[
                  { required: true, message: 'Please enter phone number' },
                  { pattern: /^[0-9+\-\s()]+$/, message: 'Please enter a valid phone number' }
                ]}
              >
                <Input placeholder="Enter phone number" />
              </Form.Item>
            </Col>
          </Row>

          <Row gutter={[24, 0]}>
            <Col xs={24} sm={8}>
              <Form.Item
                name="dateOfBirth"
                label="Date of Birth"
                rules={[{ required: true, message: 'Please select date of birth' }]}
              >
                <DatePicker 
                  style={{ width: '100%' }} 
                  placeholder="Select date of birth"
                  maxDate={dayjs().subtract(1, 'year')}
                />
              </Form.Item>
            </Col>
            <Col xs={24} sm={8}>
              <Form.Item
                name="gender"
                label="Gender"
                rules={[{ required: true, message: 'Please select gender' }]}
              >
                <Select placeholder="Select gender" options={[...GENDER_OPTIONS]} />
              </Form.Item>
            </Col>
            <Col xs={24} sm={8}>
              <Form.Item
                name="bloodGroup"
                label="Blood Group"
              >
                <Select placeholder="Select blood group" options={[...BLOOD_GROUP_OPTIONS]} />
              </Form.Item>
            </Col>
          </Row>

          <Form.Item
            name="address"
            label="Address"
          >
            <TextArea 
              rows={3} 
              placeholder="Enter full address"
            />
          </Form.Item>

          <Form.Item
            name="emergencyContact"
            label="Emergency Contact"
            rules={[
              { pattern: /^[0-9+\-\s()]+$/, message: 'Please enter a valid phone number' }
            ]}
          >
            <Input placeholder="Enter emergency contact number" />
          </Form.Item>

          <Form.Item
            name="medicalHistory"
            label="Medical History"
          >
            <TextArea 
              rows={4} 
              placeholder="Enter any relevant medical history, allergies, or conditions"
            />
          </Form.Item>

          {!isEdit && (
            <Form.Item
              name="password"
              label="Password"
              rules={[
                { required: true, message: 'Please enter password' },
                { min: 6, message: 'Password must be at least 6 characters' }
              ]}
            >
              <Input.Password placeholder="Enter password for patient login" autoComplete="new-password" />
            </Form.Item>
          )}

          <Form.Item>
            <Space>
              <Button
                type="primary"
                htmlType="submit"
                icon={<SaveOutlined />}
                loading={isLoading}
              >
                {isEdit ? 'Update Patient' : 'Create Patient'}
              </Button>
              <Button onClick={() => navigate('/patients')}>
                Cancel
              </Button>
            </Space>
          </Form.Item>
        </Form>
      </Card>
    </div>
  )
}
