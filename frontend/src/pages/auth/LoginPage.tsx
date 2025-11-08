import { Card, Form, Input, Button, Typography, Space, Divider } from 'antd'
import { UserOutlined, LockOutlined, HomeOutlined } from '@ant-design/icons'
import { useLogin } from '../../hooks/queries/useAuth'
import { useEffect, useState } from 'react'
import type { LoginRequest, Clinic } from '../../types/auth'

const { Title, Text } = Typography

export const LoginPage = () => {
  const [form] = Form.useForm()
  const [showClinicSelect, setShowClinicSelect] = useState(false)
  const [availableClinics, setAvailableClinics] = useState<Clinic[]>([])
  const loginMutation = useLogin()

  const onFinish = async (values: LoginRequest) => {
    try {
      const result = await loginMutation.mutateAsync(values)
      
      // If user has multiple clinics and none was selected, show clinic selector
      if (result.availableClinics.length > 1 && !values.clinicId) {
        setAvailableClinics(result.availableClinics)
        setShowClinicSelect(true)
        form.setFieldsValue({ ...values, clinics: result.availableClinics })
      }
    } catch (error) {
      // Error is handled by the mutation
    }
  }

  const handleClinicSelect = (clinicId: number) => {
    const formValues = form.getFieldsValue()
    form.setFieldsValue({ ...formValues, clinicId })
    onFinish({ ...formValues, clinicId })
  }

  useEffect(() => {
    // Focus on email field when component mounts
    const emailInput = document.getElementById('email')
    if (emailInput) {
      emailInput.focus()
    }
  }, [])

  return (
    <div style={{
      minHeight: '100vh',
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'center',
      background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
      padding: '20px'
    }}>
      <Card
        style={{
          width: '100%',
          maxWidth: 400,
          boxShadow: '0 10px 25px rgba(0, 0, 0, 0.2)'
        }}
        variant="borderless"
      >
        <div style={{ textAlign: 'center', marginBottom: 32 }}>
          <div style={{
            width: 64,
            height: 64,
            background: '#1890ff',
            borderRadius: '50%',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            margin: '0 auto 16px',
            color: 'white',
            fontSize: 24
          }}>
            <HomeOutlined />
          </div>
          <Title level={2} style={{ margin: 0, color: '#262626' }}>
            ClinicCare
          </Title>
          <Text type="secondary">
            Homoeopathy Clinic Management
          </Text>
        </div>

        {!showClinicSelect ? (
          <Form
            form={form}
            name="login"
            onFinish={onFinish}
            autoComplete="off"
            size="large"
          >
            <Form.Item
              name="email"
              rules={[
                { required: true, message: 'Please input your email!' },
                { type: 'email', message: 'Please enter a valid email!' }
              ]}
            >
              <Input
                prefix={<UserOutlined />}
                placeholder="Email"
                id="email"
              />
            </Form.Item>

            <Form.Item
              name="password"
              rules={[{ required: true, message: 'Please input your password!' }]}
            >
              <Input.Password
                prefix={<LockOutlined />}
                placeholder="Password"
              />
            </Form.Item>

            <Form.Item>
              <Button
                type="primary"
                htmlType="submit"
                block
                loading={loginMutation.isPending}
              >
                Sign In
              </Button>
            </Form.Item>
          </Form>
        ) : (
          <div>
            <Title level={4} style={{ textAlign: 'center', marginBottom: 24 }}>
              Select Clinic
            </Title>
            <Text type="secondary" style={{ display: 'block', textAlign: 'center', marginBottom: 20 }}>
              You have access to multiple clinics. Please select one to continue.
            </Text>
            
            <Space direction="vertical" style={{ width: '100%' }}>
              {availableClinics.map(clinic => (
                <Button
                  key={clinic.id}
                  block
                  size="large"
                  onClick={() => handleClinicSelect(clinic.id)}
                  loading={loginMutation.isPending}
                  style={{
                    textAlign: 'left',
                    height: 'auto',
                    padding: '12px 16px'
                  }}
                >
                  <div>
                    <div style={{ fontWeight: 500 }}>{clinic.name}</div>
                    <div style={{ fontSize: 12, opacity: 0.7 }}>
                      Code: {clinic.code}
                    </div>
                  </div>
                </Button>
              ))}
            </Space>

            <Divider />
            
            <Button
              block
              onClick={() => {
                setShowClinicSelect(false)
                setAvailableClinics([])
              }}
            >
              Back to Login
            </Button>
          </div>
        )}

        <div style={{ marginTop: 24, textAlign: 'center' }}>
          <Text type="secondary" style={{ fontSize: 12 }}>
            Demo Credentials:<br />
            <strong>Admin:</strong> admin@healthcareplus.com<br />
            <strong>Doctor:</strong> dr.smith@healthcareplus.com<br />
            <strong>Staff:</strong> reception1@healthcareplus.com<br />
            <strong>Patient:</strong> patient1@email.com
          </Text>
        </div>
      </Card>
    </div>
  )
}
