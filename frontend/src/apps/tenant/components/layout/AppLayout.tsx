import { Layout, Menu, Avatar, Dropdown, Button, Badge, Space, Typography } from 'antd'
import { 
  MenuFoldOutlined, 
  MenuUnfoldOutlined,
  UserOutlined, 
  LogoutOutlined, 
  SettingOutlined,
  BellOutlined,
  SwapOutlined,
  HomeOutlined,
  CalendarOutlined,
  TeamOutlined,
  MedicineBoxOutlined,
  FileTextOutlined,
  BarChartOutlined,
  BankOutlined,
  DollarOutlined,
  ShoppingOutlined,
  ShoppingCartOutlined,
  ClockCircleOutlined
} from '@ant-design/icons'
import { useNavigate, useLocation } from 'react-router-dom'
import { useAuth, useSelectedClinic } from '@core/stores/authStore'
import { useSidebar, useTheme } from '@core/stores/uiStore'
import { useLogout } from '@core/hooks/queries/useAuth'
import { UserRole } from '@core/types/auth'
import { ClinicSelector } from '../clinic/ClinicSelector'

const { Header, Sider, Content } = Layout
const { Text } = Typography

interface AppLayoutProps {
  children: React.ReactNode
}

export const AppLayout = ({ children }: AppLayoutProps) => {
  const navigate = useNavigate()
  const location = useLocation()
  const { user } = useAuth()
  const selectedClinic = useSelectedClinic()
  const { collapsed, toggle } = useSidebar()
  const { theme } = useTheme()
  const logoutMutation = useLogout()

  // Handle menu navigation
  const handleMenuClick = ({ key }: { key: string }) => {
    switch (key) {
      case 'dashboard':
        navigate('/')
        break
      case 'patients':
        navigate('/patients')
        break
      case 'appointments':
        navigate('/appointments')
        break
      case 'queue':
        // Route based on role
        if (user?.role === UserRole.Doctor) {
          navigate('/queue/doctor')
        } else if (user?.role === UserRole.Admin || user?.role === UserRole.Staff) {
          navigate('/queue/staff')
        } else {
          navigate('/queue')
        }
        break
      case 'consultations':
        navigate('/consultations')
        break
      case 'prescriptions':
        navigate('/prescriptions')
        break
      case 'inventory':
        navigate('/inventory')
        break
      case 'my-appointments':
        navigate('/my-appointments')
        break
      case 'medical-history':
        navigate('/medical-history')
        break
      case 'organizations':
        navigate('/organizations')
        break
      case 'global-medicines':
        navigate('/global-medicines')
        break
      case 'reports':
        navigate('/reports')
        break
      case 'clinics':
        navigate('/clinics')
        break
      case 'users':
        navigate('/users')
        break
      case 'invoices':
        navigate('/invoices')
        break
      case 'suppliers':
        navigate('/suppliers')
        break
      case 'purchase-orders':
        navigate('/purchase-orders')
        break
      case 'book-appointment':
        navigate('/book-appointment')
        break
      case 'doctor-schedule':
        navigate('/doctors/schedule')
        break
      default:
        break
    }
  }

  // Get current selected key based on location
  const getSelectedKey = () => {
    const path = location.pathname
    if (path === '/' || path === '/dashboard') return 'dashboard'
    if (path.startsWith('/patients')) return 'patients'
    if (path.startsWith('/appointments')) return 'appointments'
    if (path.startsWith('/queue')) return 'queue'
    if (path.startsWith('/consultations')) return 'consultations'
    if (path.startsWith('/prescriptions')) return 'prescriptions'
    if (path.startsWith('/inventory')) return 'inventory'
    if (path.startsWith('/my-appointments')) return 'my-appointments'
    if (path.startsWith('/medical-history')) return 'medical-history'
    if (path.startsWith('/organizations')) return 'organizations'
    if (path.startsWith('/global-medicines')) return 'global-medicines'
    if (path.startsWith('/reports')) return 'reports'
    if (path.startsWith('/clinics')) return 'clinics'
    if (path.startsWith('/users')) return 'users'
    if (path.startsWith('/invoices')) return 'invoices'
    if (path.startsWith('/suppliers')) return 'suppliers'
    if (path.startsWith('/purchase-orders')) return 'purchase-orders'
    if (path.startsWith('/book-appointment')) return 'book-appointment'
    if (path.startsWith('/doctors/schedule')) return 'doctor-schedule'
    if (path.startsWith('/queue/doctor')) return 'queue'
    if (path.startsWith('/queue/staff')) return 'queue'
    if (path.startsWith('/queue')) return 'queue'
    return 'dashboard'
  }

  // Get menu items based on user role
  const getMenuItems = () => {
    const commonItems = [
      {
        key: 'dashboard',
        icon: <HomeOutlined />,
        label: 'Dashboard'
      }
    ]

    const roleBasedItems = {
      [UserRole.OrganizationAdmin]: [
        ...commonItems,
        {
          key: 'clinics',
          icon: <BankOutlined />,
          label: 'Clinics'
        },
        {
          key: 'users',
          icon: <TeamOutlined />,
          label: 'Users'
        },
        {
          key: 'invoices',
          icon: <DollarOutlined />,
          label: 'Invoices'
        },
        {
          key: 'suppliers',
          icon: <ShoppingOutlined />,
          label: 'Suppliers'
        },
        {
          key: 'purchase-orders',
          icon: <ShoppingCartOutlined />,
          label: 'Purchase Orders'
        },
        {
          key: 'inventory',
          icon: <MedicineBoxOutlined />,
          label: 'Inventory'
        },
        {
          key: 'reports',
          icon: <BarChartOutlined />,
          label: 'Reports'
        },
        {
          key: 'doctor-schedule',
          icon: <ClockCircleOutlined />,
          label: 'Doctor Schedule'
        }
      ],
      [UserRole.Doctor]: [
        ...commonItems,
        {
          key: 'appointments',
          icon: <CalendarOutlined />,
          label: 'Appointments'
        },
        {
          key: 'doctor-schedule',
          icon: <ClockCircleOutlined />,
          label: 'My Schedule'
        },
        {
          key: 'queue',
          icon: <TeamOutlined />,
          label: 'Patient Queue'
        },
        {
          key: 'consultations',
          icon: <FileTextOutlined />,
          label: 'Consultations'
        },
        {
          key: 'prescriptions',
          icon: <MedicineBoxOutlined />,
          label: 'Prescriptions'
        }
      ],
      [UserRole.Reception]: [
        ...commonItems,
        {
          key: 'appointments',
          icon: <CalendarOutlined />,
          label: 'Appointments'
        },
        {
          key: 'patients',
          icon: <TeamOutlined />,
          label: 'Patients'
        },
        {
          key: 'queue',
          icon: <TeamOutlined />,
          label: 'Queue'
        },
        {
          key: 'invoices',
          icon: <DollarOutlined />,
          label: 'Invoices'
        },
        {
          key: 'suppliers',
          icon: <ShoppingOutlined />,
          label: 'Suppliers'
        },
        {
          key: 'purchase-orders',
          icon: <ShoppingCartOutlined />,
          label: 'Purchase Orders'
        },
        {
          key: 'inventory',
          icon: <MedicineBoxOutlined />,
          label: 'Inventory'
        }
      ],
      [UserRole.Patient]: [
        ...commonItems,
        {
          key: 'book-appointment',
          icon: <CalendarOutlined />,
          label: 'Book Appointment'
        },
        {
          key: 'my-appointments',
          icon: <CalendarOutlined />,
          label: 'My Appointments'
        },
        {
          key: 'prescriptions',
          icon: <MedicineBoxOutlined />,
          label: 'Prescriptions'
        },
        {
          key: 'medical-history',
          icon: <FileTextOutlined />,
          label: 'Medical History'
        }
      ]
    }

    const userRole = user?.role || UserRole.Patient
    return roleBasedItems[userRole as keyof typeof roleBasedItems] || commonItems
  }

  const handleLogout = () => {
    logoutMutation.mutate()
  }

  // User dropdown menu
  const userMenuItems = [
    {
      key: 'profile',
      icon: <UserOutlined />,
      label: 'Profile'
    },
    {
      key: 'settings',
      icon: <SettingOutlined />,
      label: 'Settings'
    },
    {
      type: 'divider' as const
    },
    {
      key: 'logout',
      icon: <LogoutOutlined />,
      label: 'Logout',
      onClick: handleLogout
    }
  ]

  return (
    <Layout style={{ minHeight: '100vh' }}>
      <Sider 
        trigger={null} 
        collapsible 
        collapsed={collapsed}
        theme={theme === 'dark' ? 'dark' : 'light'}
        style={{
          position: 'fixed',
          height: '100vh',
          left: 0,
          top: 0,
          zIndex: 100
        }}
      >
        <div style={{ 
          height: 64, 
          padding: '16px', 
          display: 'flex', 
          alignItems: 'center',
          borderBottom: '1px solid #f0f0f0'
        }}>
          {!collapsed && (
            <Text strong style={{ color: '#1890ff', fontSize: 18 }}>
              ClinicCare
            </Text>
          )}
        </div>
        
        <Menu
          theme={theme === 'dark' ? 'dark' : 'light'}
          mode="inline"
          selectedKeys={[getSelectedKey()]}
          items={getMenuItems()}
          onClick={handleMenuClick}
          style={{ borderRight: 0 }}
        />
      </Sider>

      <Layout style={{ marginLeft: collapsed ? 80 : 200, transition: 'margin-left 0.2s' }}>
        <Header style={{ 
          padding: '0 24px', 
          background: '#fff',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between',
          borderBottom: '1px solid #f0f0f0',
          position: 'sticky',
          top: 0,
          zIndex: 99
        }}>
          <Space>
            <Button
              type="text"
              icon={collapsed ? <MenuUnfoldOutlined /> : <MenuFoldOutlined />}
              onClick={toggle}
              style={{ fontSize: 16 }}
            />
            
            <ClinicSelector />
          </Space>

          <Space size="middle">
            <Badge count={0} showZero={false}>
              <Button type="text" icon={<BellOutlined />} size="large" />
            </Badge>
            
            <Dropdown
              menu={{ items: userMenuItems }}
              placement="bottomRight"
              trigger={['click']}
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
                    {user?.role === UserRole.OrganizationAdmin
                      ? 'Organization Admin'
                      : user?.role === UserRole.Doctor
                      ? 'Doctor'
                      : user?.role === UserRole.Reception
                      ? 'Reception'
                      : user?.role === UserRole.Pharmacy
                      ? 'Pharmacy'
                      : user?.role === UserRole.Patient
                      ? 'Patient'
                      : user?.email || 'User'}
                  </Text>
                </div>
              </div>
            </Dropdown>
          </Space>
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
