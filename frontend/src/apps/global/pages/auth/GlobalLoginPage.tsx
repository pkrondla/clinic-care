import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { Form, Input, Button, Card, Typography, message } from 'antd'
import { UserOutlined, LockOutlined } from '@ant-design/icons'
import { useGlobalAuthStore } from '@core/stores/authStore'
import { api } from '@core/services/apiClient'
import { UserRole } from '@core/types/auth'
import type { User } from '@core/types/auth'
import type { LoginResponse } from '@core/types/auth'

const { Title } = Typography

interface LoginFormValues {
  email: string
  password: string
}

export const GlobalLoginPage = () => {
  console.log('GlobalLoginPage: Rendering login page')
  const [loading, setLoading] = useState(false)
  const navigate = useNavigate()
  const { login } = useGlobalAuthStore()
  
  const onFinish = async (values: LoginFormValues) => {
    setLoading(true)
    try {
      const response = await api.post<LoginResponse>('/auth/login', {
        email: values.email,
        password: values.password
      })
      
      // The api.post returns the response data directly (already unwrapped)
      // Backend returns camelCase due to JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase
      const loginData = response as any
      
      // Handle both camelCase (from API) and PascalCase (if serialization didn't work)
      const accessToken = loginData.accessToken || loginData.AccessToken
      const userData = loginData.user || loginData.User
      const availableClinics = loginData.availableClinics || loginData.AvailableClinics || []
      
      if (accessToken && userData) {
        // Map role from backend (numeric or string) to frontend enum
        const rawRole = userData.role || userData.Role
        let mappedRole: UserRole
        
        // Backend UserRole enum: SuperAdmin=1, Admin=2, Doctor=3, Staff=4, Patient=5
        // Frontend expects string enum values
        if (typeof rawRole === 'number') {
          // Map numeric role to string enum
          mappedRole = rawRole === 1 ? UserRole.SuperAdmin : 
                      rawRole === 2 ? UserRole.Admin : // Backend Admin maps to frontend Admin/OrganizationAdmin
                      rawRole === 3 ? UserRole.Doctor :
                      rawRole === 4 ? UserRole.Reception : // Backend Staff maps to frontend Reception
                      rawRole === 5 ? UserRole.Patient :
                      UserRole.SuperAdmin // default
        } else if (typeof rawRole === 'string') {
          // Already a string, try to match it
          mappedRole = rawRole === 'SuperAdmin' || rawRole === '1' ? UserRole.SuperAdmin :
                      rawRole === 'Admin' || rawRole === 'OrganizationAdmin' || rawRole === '2' ? UserRole.Admin :
                      rawRole === 'Doctor' || rawRole === '3' ? UserRole.Doctor :
                      rawRole === 'Staff' || rawRole === 'Reception' || rawRole === '4' ? UserRole.Reception :
                      rawRole === 'Patient' || rawRole === '5' ? UserRole.Patient :
                      UserRole.SuperAdmin // default
        } else {
          mappedRole = UserRole.SuperAdmin // default
        }
        
        console.log('GlobalLoginPage: Role mapping', { rawRole, mappedRole })
        
        // Map user data to User interface
        const user: User = {
          id: userData.id || userData.Id,
          email: userData.email || userData.Email,
          firstName: userData.firstName || userData.FirstName,
          lastName: userData.lastName || userData.LastName,
          fullName: userData.fullName || userData.FullName,
          role: mappedRole,
          organizationId: userData.organizationId || userData.OrganizationId || 0,
          organizationName: userData.organizationName || userData.OrganizationName || 'System',
          selectedClinicId: userData.selectedClinicId || userData.SelectedClinicId,
          selectedClinicName: userData.selectedClinicName || userData.SelectedClinicName,
          availableClinics: availableClinics.map((c: any) => ({
            id: c.id || c.Id,
            name: c.name || c.Name,
            code: c.code || c.Code
          }))
        }
        
        login(user, accessToken)
        message.success('Login successful!')
        navigate('/dashboard')
      } else {
        message.error('Invalid credentials. Please try again.')
      }
    } catch (error: any) {
      console.error('Login error:', error)
      const errorMessage = error?.response?.data?.message || 
                          error?.response?.data?.errors?.[0] ||
                          error?.message || 
                          'Login failed. Please check your credentials.'
      message.error(errorMessage)
    } finally {
      setLoading(false)
    }
  }

  return (
    <div style={{ 
      minHeight: '100vh', 
      display: 'flex', 
      alignItems: 'center', 
      justifyContent: 'center',
      background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)'
    }}>
      <Card 
        style={{ 
          width: 400,
          boxShadow: '0 4px 12px rgba(0, 0, 0, 0.15)'
        }}
      >
        <div style={{ textAlign: 'center', marginBottom: 32 }}>
          <Title level={2} style={{ marginBottom: 8 }}>
            ClinicCare Admin
          </Title>
          <p style={{ color: '#666', margin: 0 }}>
            Global System Administration
          </p>
        </div>

        <Form
          name="login"
          onFinish={onFinish}
          autoComplete="off"
          layout="vertical"
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
              loading={loading}
              style={{ height: 40 }}
            >
              Sign In
            </Button>
          </Form.Item>
        </Form>

        <div style={{ 
          marginTop: 16, 
          textAlign: 'center', 
          fontSize: 12, 
          color: '#999' 
        }}>
          <p>Demo Credentials:</p>
          <p>Email: superadmin@cliniccare.com</p>
          <p>Password: Admin@123</p>
        </div>
      </Card>
    </div>
  )
}

