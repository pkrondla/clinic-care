import { Card, Row, Col } from 'antd'
import { useSubscriptions } from '@core/hooks/queries/useSubscriptions'
import { LoadingCard } from '@components/shared/LoadingCard'
import { DataTable } from '@components/shared/DataTable'
import { SubscriptionTable } from './SubscriptionTable'

export const SubscriptionsPage = () => {
  const { data: subscriptions, isLoading } = useSubscriptions()

  return (
    <Row gutter={[24, 24]}>
      <Col span={24}>
        <Card title="Subscriptions">
          <LoadingCard loading={isLoading}>
            <DataTable
              dataSource={subscriptions}
              columns={SubscriptionTable}
              rowKey="id"
              scroll={{ x: true }}
            />
          </LoadingCard>
        </Card>
      </Col>
    </Row>
  )
}