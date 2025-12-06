import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { Form, Input, Button, Card, Typography, message, Space } from 'antd'
import { UserOutlined, LockOutlined, HomeOutlined } from '@ant-design/icons'
import { useTenantAuthStore } from '@core/stores/authStore'
import { api } from '@core/services/apiClient'
import type { User } from '@core/types/auth'
import type { LoginResponse } from '@core/types/auth'
import { UserRole } from '@core/types/auth'

const { Title, Text } = Typography

interface LoginFormValues {
  email: string
  password: string
}

export const LoginPage = () => {
  const [loading, setLoading] = useState(false)
  const navigate = useNavigate()
  const { login } = useTenantAuthStore()
  
  // Get tenant subdomain
  const hostname = window.location.hostname
  const subdomain = hostname.split('.')[0]
  const isTenantDomain = subdomain !== 'www' && subdomain !== 'localhost'

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
        // Frontend UserRole enum: SuperAdmin, OrganizationAdmin (Admin), Doctor, Reception (Staff), Patient
        if (typeof rawRole === 'number') {
          // Map numeric role to string enum
          mappedRole = rawRole === 1 ? UserRole.SuperAdmin :
                      rawRole === 2 ? UserRole.OrganizationAdmin : // Backend Admin (2) = Frontend OrganizationAdmin
                      rawRole === 3 ? UserRole.Doctor :
                      rawRole === 4 ? UserRole.Reception : // Backend Staff (4) = Frontend Reception
                      rawRole === 5 ? UserRole.Patient :
                      UserRole.OrganizationAdmin // default to Admin
        } else if (typeof rawRole === 'string') {
          // Already a string, try to match it
          mappedRole = rawRole === 'SuperAdmin' || rawRole === '1' ? UserRole.SuperAdmin :
                      rawRole === 'Admin' || rawRole === 'OrganizationAdmin' || rawRole === '2' ? UserRole.OrganizationAdmin :
                      rawRole === 'Doctor' || rawRole === '3' ? UserRole.Doctor :
                      rawRole === 'Staff' || rawRole === 'Reception' || rawRole === '4' ? UserRole.Reception :
                      rawRole === 'Patient' || rawRole === '5' ? UserRole.Patient :
                      UserRole.OrganizationAdmin // default to Admin
        } else {
          mappedRole = UserRole.OrganizationAdmin // default to Admin
        }
        
        console.log('TenantLoginPage: Role mapping', { rawRole, mappedRole })
        
        // Map user data to User interface
        const user: User = {
          id: userData.id || userData.Id,
          email: userData.email || userData.Email,
          firstName: userData.firstName || userData.FirstName,
          lastName: userData.lastName || userData.LastName,
          fullName: userData.fullName || userData.FullName || `${userData.firstName || userData.FirstName || ''} ${userData.lastName || userData.LastName || ''}`.trim(),
          role: mappedRole,
          organizationId: userData.organizationId || userData.OrganizationId || 0,
          organizationName: userData.organizationName || userData.OrganizationName || 'Clinic',
          selectedClinicId: userData.selectedClinicId || userData.SelectedClinicId,
          selectedClinicName: userData.selectedClinicName || userData.SelectedClinicName,
          availableClinics: availableClinics.map((c: any) => ({
            id: c.id || c.Id,
            name: c.name || c.Name,
            code: c.code || c.Code
          }))
        }
        
        // Set auth state first
        login(user, accessToken, user.availableClinics)
        
        // Verify state is set before navigating
        const authState = useTenantAuthStore.getState()
        console.log('LoginPage: Auth state after login', { 
          isAuthenticated: authState.isAuthenticated, 
          user: authState.user?.email,
          hasToken: !!authState.token 
        })
        
        if (authState.isAuthenticated) {
          message.success('Login successful!')
          // Small delay to ensure React re-renders with new auth state
          // This ensures ProtectedTenantRoute sees the updated state
          setTimeout(() => {
            navigate('/dashboard', { replace: true })
          }, 100)
        } else {
          console.error('LoginPage: Auth state not set correctly')
          message.error('Login failed. Please try again.')
        }
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
          width: '100%', 
          maxWidth: 400, 
          margin: 20,
          boxShadow: '0 4px 20px rgba(0,0,0,0.1)'
        }}
      >
        <div style={{ textAlign: 'center', marginBottom: 32 }}>
          <HomeOutlined style={{ fontSize: 48, color: '#667eea', marginBottom: 16 }} />
          <Title level={2} style={{ marginBottom: 8 }}>
            {isTenantDomain ? `${subdomain}` : 'ClinicCare'}
          </Title>
          <Text type="secondary">
            Sign in to your clinic account
          </Text>
        </div>

        <Form
          name="tenant-login"
          onFinish={onFinish}
          layout="vertical"
          requiredMark={false}
        >
          <Form.Item
            name="email"
            rules={[
              { required: true, message: 'Please enter your email' },
              { type: 'email', message: 'Please enter a valid email' }
            ]}
          >
            <Input 
              prefix={<UserOutlined />}
              placeholder="Email"
              size="large"
            />
          </Form.Item>

          <Form.Item
            name="password"
            rules={[
              { required: true, message: 'Please enter your password' }
            ]}
          >
            <Input.Password
              prefix={<LockOutlined />}
              placeholder="Password"
              size="large"
            />
          </Form.Item>

          <Form.Item>
            <Button 
              type="primary" 
              htmlType="submit" 
              size="large" 
              block
              loading={loading}
            >
              Sign In
            </Button>
          </Form.Item>

          <div style={{ textAlign: 'center' }}>
            <Space direction="vertical" size="small">
              <a href="/forgot-password">Forgot password?</a>
              <Text type="secondary">
                Need help? Contact your clinic administrator
              </Text>
            </Space>
          </div>
        </Form>
      </Card>
    </div>
  )
}

