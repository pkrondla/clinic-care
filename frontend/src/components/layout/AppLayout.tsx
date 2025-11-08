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
  BarChartOutlined
} from '@ant-design/icons'
import { useNavigate, useLocation } from 'react-router-dom'
import { useAuth, useSelectedClinic } from '../../stores/authStore'
import { useSidebar, useTheme } from '../../stores/uiStore'
import { useLogout, useSwitchClinic } from '../../hooks/queries/useAuth'
import { UserRole } from '../../types/auth'

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
  const switchClinicMutation = useSwitchClinic()

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
        navigate('/queue')
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
      [UserRole.SuperAdmin]: [
        ...commonItems,
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
          key: 'reports',
          icon: <BarChartOutlined />,
          label: 'System Reports'
        }
      ],
      [UserRole.Admin]: [
        ...commonItems,
        {
          key: 'clinics',
          icon: <HomeOutlined />,
          label: 'Clinics'
        },
        {
          key: 'users',
          icon: <TeamOutlined />,
          label: 'Users'
        },
        {
          key: 'reports',
          icon: <BarChartOutlined />,
          label: 'Reports'
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
      [UserRole.Staff]: [
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
          key: 'inventory',
          icon: <MedicineBoxOutlined />,
          label: 'Inventory'
        }
      ],
      [UserRole.Patient]: [
        ...commonItems,
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

    return roleBasedItems[user?.role || UserRole.Patient] || commonItems
  }

  const handleLogout = () => {
    logoutMutation.mutate()
  }

  const handleClinicSwitch = (clinicId: number) => {
    switchClinicMutation.mutate(clinicId)
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

  // Clinic switch dropdown (for users with multiple clinics)
  const clinicMenuItems = user?.availableClinics?.map((clinic: any) => ({
    key: clinic.id.toString(),
    label: clinic.name,
    onClick: () => handleClinicSwitch(clinic.id)
  })) || []

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
            
            {selectedClinic && (
              <Space>
                <Text type="secondary">Clinic:</Text>
                <Dropdown
                  menu={{ items: clinicMenuItems }}
                  placement="bottomLeft"
                  disabled={clinicMenuItems.length <= 1}
                >
                  <Button type="text" icon={<SwapOutlined />}>
                    {selectedClinic.name}
                  </Button>
                </Dropdown>
              </Space>
            )}
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
              <Space style={{ cursor: 'pointer' }}>
                <Avatar icon={<UserOutlined />} />
                <div style={{ display: collapsed ? 'none' : 'block' }}>
                  <Text strong>{user?.fullName}</Text>
                  <br />
                  <Text type="secondary" style={{ fontSize: 12 }}>
                    {user?.role === UserRole.SuperAdmin && 'Super Admin'}
                    {user?.role === UserRole.Admin && 'Admin'}
                    {user?.role === UserRole.Doctor && 'Doctor'}
                    {user?.role === UserRole.Staff && 'Staff'}
                    {user?.role === UserRole.Patient && 'Patient'}
                  </Text>
                </div>
              </Space>
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
