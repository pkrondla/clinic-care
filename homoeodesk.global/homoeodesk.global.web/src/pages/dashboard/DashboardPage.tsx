import { Card, Row, Col, Statistic } from 'antd'
import { useSystemStats } from '@core/hooks/queries/useStats'

export function DashboardPage() {
  const { data: stats, isLoading } = useSystemStats()

  return (
    <div>
      <h1>System Dashboard</h1>
      <Row gutter={16}>
        <Col span={6}>
          <Card loading={isLoading}>
            <Statistic
              title="Total Organizations"
              value={stats?.totalOrganizations || 0}
            />
          </Card>
        </Col>
        <Col span={6}>
          <Card loading={isLoading}>
            <Statistic
              title="Active Users"
              value={stats?.activeUsers || 0}
            />
          </Card>
        </Col>
        <Col span={6}>
          <Card loading={isLoading}>
            <Statistic
              title="Total Medicines"
              value={stats?.totalMedicines || 0}
            />
          </Card>
        </Col>
        <Col span={6}>
          <Card loading={isLoading}>
            <Statistic
              title="Monthly Revenue"
              value={stats?.monthlyRevenue || 0}
              prefix="$"
            />
          </Card>
        </Col>
      </Row>
    </div>
  )
}