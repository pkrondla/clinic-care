import { useState } from 'react'
import { useNavigate, useSearchParams } from 'react-router-dom'
import { Card, Form, Input, InputNumber, Button, message, Space, Spin } from 'antd'
import { SaveOutlined, ArrowLeftOutlined } from '@ant-design/icons'
import { useMutation, useQuery } from '@tanstack/react-query'
import { consultationService, type CreateConsultationRequest } from '@core/services/consultationService'
import { useAuth } from '@core/stores/authStore'

const { TextArea } = Input

export const ConsultationFormPage = () => {
  const [form] = Form.useForm()
  const navigate = useNavigate()
  const [searchParams] = useSearchParams()
  const { user } = useAuth()
  
  const appointmentId = searchParams.get('appointmentId')
  const patientId = searchParams.get('patientId')

  const createMutation = useMutation({
    mutationFn: consultationService.create,
    onSuccess: (data) => {
      message.success('Consultation saved successfully')
      // Navigate to prescription creation with consultation ID
      navigate(`/prescriptions/new?consultationId=${data.id}&patientId=${patientId}`)
    },
    onError: () => {
      message.error('Failed to save consultation')
    }
  })

  const handleSubmit = async () => {
    try {
      const values = await form.validateFields()
      
      const consultationData: CreateConsultationRequest = {
        appointmentId: parseInt(appointmentId || '0'),
        patientId: parseInt(patientId || '0'),
        doctorId: user?.id || 0,
        ...values
      }

      createMutation.mutate(consultationData)
    } catch (error) {
      console.error('Validation failed:', error)
    }
  }

  if (!appointmentId || !patientId) {
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

  return (
    <div style={{ padding: '24px', maxWidth: '900px', margin: '0 auto' }}>
      <div style={{ marginBottom: '24px' }}>
        <Button
          icon={<ArrowLeftOutlined />}
          onClick={() => navigate(-1)}
          style={{ marginBottom: '16px' }}
        >
          Back
        </Button>
        <h1 style={{ margin: 0, fontSize: '24px', fontWeight: 600 }}>
          New Consultation
        </h1>
        <p style={{ margin: '8px 0 0 0', color: '#666' }}>
          Record patient consultation details
        </p>
      </div>

      <Card>
        <Form
          form={form}
          layout="vertical"
          initialValues={{
            consultationFee: 50.00
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
                loading={createMutation.isPending}
                size="large"
              >
                Save & Create Prescription
              </Button>
              <Button
                onClick={() => navigate(-1)}
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

