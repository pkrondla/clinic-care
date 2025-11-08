import { Card, Row, Col, Statistic, Typography, Table, Tag, Space, Button } from 'antd'
import { 
  TeamOutlined, 
  BankOutlined, 
  MedicineBoxOutlined,
  MoneyCollectOutlined,
  ReloadOutlined
} from '@ant-design/icons'
import { globalApi } from '@core/services/globalApi'
import { useUser } from '@core/stores/authStore'
import { UserRole } from '@core/types/auth'
import type { Organization } from '@core/types/organization'
import dayjs from 'dayjs'
import { useQuery } from '@tanstack/react-query'

const { Title } = Typography

export const GlobalDashboardPage = () => {
  const user = useUser()

  // Get system statistics
  const { data: stats, isLoading: statsLoading } = useQuery({
    queryKey: ['system-stats'],
    queryFn: async () => {
      const response = await globalApi.stats.getSystemStats()
      return response.data
    }
  })

  // Get recent organizations
  const { data: organizations, isLoading: orgsLoading, refetch } = useQuery({
    queryKey: ['recent-organizations'],
    queryFn: () => globalApi.organizations.getAll()
  })

  const organizationColumns = [
    {
      title: 'Organization',
      dataIndex: 'name',
      key: 'name',
      render: (name: string, record: Organization) => (
        <div>
          <div style={{ fontWeight: 500 }}>{name}</div>
          <div style={{ fontSize: 12, color: '#666' }}>{record.subdomain}.yourapp.com</div>
        </div>
      )
    },
    {
      title: 'Subscription',
      dataIndex: ['subscription', 'name'],
      key: 'subscription',
      render: (plan: string) => <Tag color="blue">{plan}</Tag>
    },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      render: (status: string) => (
        <Tag color={status === 'active' ? 'green' : 'red'}>
          {status.charAt(0).toUpperCase() + status.slice(1)}
        </Tag>
      )
    },
    {
      title: 'Created',
      dataIndex: 'createdAt',
      key: 'createdAt',
      render: (date: string) => dayjs(date).format('MMM DD, YYYY')
    }
  ]

  return (
    <div>
      <div style={{ marginBottom: 24 }}>
        <Title level={2}>System Overview</Title>
        <Typography.Text type="secondary" style={{ fontSize: 16 }}>
          Welcome back, {user?.firstName}! Here's what's happening in your system.
        </Typography.Text>
      </div>

      {/* Statistics Cards */}
      <Row gutter={[16, 16]} style={{ marginBottom: 24 }}>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Total Organizations"
              value={stats?.totalOrganizations || 0}
              prefix={<BankOutlined />}
              loading={statsLoading}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Active Users"
              value={stats?.activeUsers || 0}
              prefix={<TeamOutlined />}
              loading={statsLoading}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Global Medicines"
              value={stats?.totalMedicines || 0}
              prefix={<MedicineBoxOutlined />}
              loading={statsLoading}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Monthly Revenue"
              value={stats?.monthlyRevenue || 0}
              prefix={<MoneyCollectOutlined />}
              precision={2}
              loading={statsLoading}
            />
          </Card>
        </Col>
      </Row>

      {/* Recent Organizations */}
      <Card
        title={
          <Space>
            <span>Recent Organizations</span>
            <Button 
              type="text" 
              icon={<ReloadOutlined />} 
              onClick={() => refetch()}
              loading={orgsLoading}
            />
          </Space>
        }
      >
        <Table
          dataSource={organizations?.data}
          columns={organizationColumns}
          rowKey="id"
          loading={orgsLoading}
          pagination={false}
        />
      </Card>

      {/* Super Admin Quick Actions */}
      {user?.role === UserRole.SuperAdmin && (
        <Row gutter={[16, 16]} style={{ marginTop: 24 }}>
          <Col xs={24} lg={12}>
            <Card title="Quick Actions">
              <Space direction="vertical" style={{ width: '100%' }}>
                <Button type="primary" block>
                  Add New Organization
                </Button>
                <Button block>
                  Update Medicine Database
                </Button>
                <Button block>
                  System Settings
                </Button>
              </Space>
            </Card>
          </Col>
          <Col xs={24} lg={12}>
            <Card title="System Health">
              <div style={{ textAlign: 'center' }}>
                <Statistic
                  title="System Uptime"
                  value={stats?.uptime || 0}
                  suffix="days"
                />
                <div style={{ marginTop: 16 }}>
                  <Tag color="green">All Systems Operational</Tag>
                </div>
              </div>
            </Card>
          </Col>
        </Row>
      )}
    </div>
  )
}