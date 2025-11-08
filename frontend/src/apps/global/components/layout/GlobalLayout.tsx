import { Layout, Menu, Avatar, Dropdown, Space, Typography } from 'antd'
import { 
  UserOutlined, 
  LogoutOutlined,

  TeamOutlined,
  MedicineBoxOutlined,
  BarChartOutlined
} from '@ant-design/icons'
import { useNavigate, useLocation } from 'react-router-dom'
import { useAuth } from '@core/stores/authStore'
import { useLogout } from '@core/hooks/queries/useAuth'
import { useTheme } from '@core/stores/uiStore'
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
  const { theme } = useTheme()
  const { mutateAsync: logout } = useLogout()

  // Menu items for global admin
  const menuItems = [
    {
      key: 'organizations',
      icon: <TeamOutlined />,
      label: 'Organizations'
    },
    {
      key: 'global-medicines',
      icon: <MedicineBoxOutlined />,
      label: 'Global Medicines'
    },
    {
      key: 'system-reports',
      icon: <BarChartOutlined />,
      label: 'System Reports'
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
      case 'organizations':
        navigate('/organizations')
        break
      case 'global-medicines':
        navigate('/global-medicines')
        break
      case 'system-reports':
        navigate('/system-reports')
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
        theme={theme === 'dark' ? 'dark' : 'light'}
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
          theme={theme === 'dark' ? 'dark' : 'light'}
          mode="inline"
          selectedKeys={[location.pathname.split('/')[1] || 'organizations']}
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
          justifyContent: 'flex-end'
        }}>
          <Dropdown
            menu={{ 
              items: userMenuItems,
              onClick: handleUserMenuClick 
            }}
            trigger={['click']}
          >
            <Space style={{ cursor: 'pointer' }}>
              <Avatar icon={<UserOutlined />} />
              <div>
                <Text strong>{user?.fullName}</Text>
                <br />
                <Text type="secondary" style={{ fontSize: 12 }}>
                  {user?.role === UserRole.SuperAdmin ? 'Super Admin' : 'System Admin'}
                </Text>
              </div>
            </Space>
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