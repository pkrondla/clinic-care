import { Layout, Menu, Avatar, Dropdown, Typography } from 'antd'
import { 
  UserOutlined, 
  LogoutOutlined,
  DashboardOutlined,
  TeamOutlined,
  MedicineBoxOutlined,
  BarChartOutlined
} from '@ant-design/icons'
import { useNavigate, useLocation } from 'react-router-dom'
import { useAuth } from '@core/stores/authStore'
import { useLogout } from '@core/hooks/queries/useAuth'
import { UserRole } from '@core/types/auth'

const { Header, Sider, Content } = Layout
const { Text } = Typography

interface GlobalLayoutProps {
  children: React.ReactNode
}

export const GlobalLayout = ({ children }: GlobalLayoutProps) => {
  const navigate = useNavigate()
  const location = useLocation()
  const { user } = useAuth()
  const { mutateAsync: logout } = useLogout()
  const theme: 'light' | 'dark' = 'light' // Default theme, can be made dynamic later

  // Menu items for global admin
  const menuItems = [
    {
      key: 'dashboard',
      icon: <DashboardOutlined />,
      label: 'Dashboard'
    },
    {
      key: 'organizations',
      icon: <TeamOutlined />,
      label: 'Organizations'
    },
    {
      key: 'medicines',
      icon: <MedicineBoxOutlined />,
      label: 'Global Medicines'
    }
  ]

  // User menu items
  const userMenuItems = [
    {
      key: 'profile',
      label: 'Profile Settings',
      icon: <UserOutlined />
    },
    {
      key: 'logout',
      label: 'Logout',
      icon: <LogoutOutlined />,
      danger: true
    }
  ]

  const handleMenuClick = ({ key }: { key: string }) => {
    switch (key) {
      case 'dashboard':
        navigate('/dashboard')
        break
      case 'organizations':
        navigate('/organizations')
        break
      case 'medicines':
        navigate('/medicines')
        break
    }
  }

  const handleUserMenuClick = async ({ key }: { key: string }) => {
    if (key === 'logout') {
      await logout()
      navigate('/login')
    }
  }

  return (
    <Layout style={{ minHeight: '100vh' }}>
      <Sider
        theme={theme}
        width={200}
      >
        <div style={{ 
          height: 64, 
          padding: '16px', 
          display: 'flex', 
          alignItems: 'center',
          borderBottom: '1px solid #f0f0f0'
        }}>
          <Text strong style={{ color: '#1890ff', fontSize: 18 }}>
            ClinicCare Admin
          </Text>
        </div>
        
        <Menu
          theme={theme}
          mode="inline"
          selectedKeys={[location.pathname === '/' || location.pathname === '/dashboard' ? 'dashboard' : location.pathname.split('/')[1] || 'dashboard']}
          items={menuItems}
          onClick={handleMenuClick}
          style={{ borderRight: 0 }}
        />
      </Sider>

      <Layout>
        <Header style={{ 
          padding: '0 24px', 
          background: '#fff',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'flex-end',
          boxShadow: '0 2px 8px rgba(0,0,0,0.1)',
          zIndex: 1
        }}>
          <Dropdown
            menu={{ 
              items: userMenuItems,
              onClick: handleUserMenuClick 
            }}
            trigger={['click']}
            placement="bottomRight"
          >
            <div
              style={{ 
                cursor: 'pointer',
                padding: '8px 12px',
                borderRadius: '4px',
                transition: 'background-color 0.2s',
                display: 'flex',
                alignItems: 'center',
                gap: '12px'
              }}
              onMouseEnter={(e) => {
                e.currentTarget.style.backgroundColor = '#f5f5f5'
              }}
              onMouseLeave={(e) => {
                e.currentTarget.style.backgroundColor = 'transparent'
              }}
            >
              <Avatar 
                size="default"
                icon={<UserOutlined />}
                style={{ backgroundColor: '#1890ff', flexShrink: 0 }}
              />
              <div style={{ 
                display: 'flex', 
                flexDirection: 'column',
                lineHeight: 1.5,
                minWidth: 120
              }}>
                <Text strong style={{ fontSize: 14, display: 'block', whiteSpace: 'nowrap' }}>
                  {user?.fullName || `${user?.firstName || ''} ${user?.lastName || ''}`.trim() || user?.email || 'User'}
                </Text>
                <Text type="secondary" style={{ fontSize: 12, display: 'block', whiteSpace: 'nowrap' }}>
                  {user?.role === UserRole.SuperAdmin
                    ? 'Super Admin' 
                    : user?.role === UserRole.Admin || user?.role === UserRole.SystemAdmin
                    ? 'System Admin'
                    : user?.email || 'User'}
                </Text>
              </div>
            </div>
          </Dropdown>
        </Header>

        <Content style={{ 
          padding: 24, 
          minHeight: 'calc(100vh - 64px)',
          background: '#f5f5f5'
        }}>
          {children}
        </Content>
      </Layout>
    </Layout>
  )
}