import { useState } from 'react'
import { Card, Row, Col, Typography, Tag, Badge, Select, DatePicker, Empty, Spin, Space } from 'antd'
import { 
  ClockCircleOutlined, 
  UserOutlined, 
  CheckCircleOutlined,
  ReloadOutlined
} from '@ant-design/icons'
import { usePublicQueues } from '@core/hooks/queries/useQueues'
import { useBranches } from '@core/hooks/queries/useBranches'
import dayjs from 'dayjs'
import type { DoctorQueueDto, QueueTokenDto } from '@core/services/queueService'

const { Title } = Typography
const { Option } = Select

export const PublicQueuePage = () => {
  const [SelectedBranchId, setSelectedBranchId] = useState<number | undefined>()
  const [selectedDate, setSelectedDate] = useState(dayjs())
  const { data: Branches } = useBranches()

  // Get all queues (public - token numbers only, no patient names)
  const { data: queues, isLoading, refetch } = usePublicQueues({
    BranchId: SelectedBranchId,
    date: selectedDate.format('YYYY-MM-DD'),
  })

  const getStatusColor = (status: number) => {
    switch (status) {
      case 1: return 'default' // Waiting
      case 2: return 'processing' // In Progress
      case 3: return 'success' // Completed
      case 4: return 'error' // Cancelled
      default: return 'default'
    }
  }

  return (
    <div style={{ padding: '24px', maxWidth: 1200, margin: '0 auto' }}>
      <div style={{ marginBottom: 24, textAlign: 'center' }}>
        <Title level={2} style={{ margin: 0 }}>
          Doctor Queues
        </Title>
        <div style={{ marginTop: 8, color: '#999' }}>
          View all doctor queues - Token numbers only (privacy protected)
        </div>
      </div>

      <div style={{ marginBottom: 24 }}>
        <Row gutter={16} justify="center">
          <Col>
            <Select
              style={{ width: 200 }}
              placeholder="Select Clinic"
              value={SelectedBranchId}
              onChange={setSelectedBranchId}
              allowClear
            >
              {Branches?.map((clinic) => (
                <Option key={clinic.id} value={clinic.id}>
                  {clinic.name}
                </Option>
              ))}
            </Select>
          </Col>
          <Col>
            <DatePicker
              value={selectedDate}
              onChange={(date) => date && setSelectedDate(date)}
              format="YYYY-MM-DD"
            />
          </Col>
          <Col>
            <Space>
              <Tag icon={<ReloadOutlined />} onClick={() => refetch()} style={{ cursor: 'pointer' }}>
                Refresh
              </Tag>
            </Space>
          </Col>
        </Row>
      </div>

      {isLoading ? (
        <Spin size="large" style={{ display: 'block', textAlign: 'center', padding: '50px' }} />
      ) : !queues || queues.length === 0 ? (
        <Card>
          <Empty description="No queues found for this date" />
        </Card>
      ) : (
        <Row gutter={[16, 16]}>
          {queues.map((queue: DoctorQueueDto) => {
            const waitingTokens = queue.tokens.filter(t => t.status === 1)
            const inProgressToken = queue.tokens.find(t => t.status === 2)
            const completedCount = queue.tokens.filter(t => t.status === 3).length

            return (
              <Col xs={24} sm={12} lg={8} key={queue.doctorId}>
                <Card
                  title={
                    <Space>
                      <UserOutlined />
                      <span>{queue.doctorName}</span>
                    </Space>
                  }
                  extra={
                    <Space>
                      <Badge status="success" text={`${waitingTokens.length} waiting`} />
                      {inProgressToken && (
                        <Tag color="orange">Serving: #{inProgressToken.tokenNumber}</Tag>
                      )}
                    </Space>
                  }
                >
                  {/* Current Token */}
                  {inProgressToken && (
                    <div style={{ 
                      textAlign: 'center', 
                      padding: '16px',
                      background: '#f0f5ff',
                      borderRadius: 8,
                      marginBottom: 16
                    }}>
                      <div style={{ fontSize: 12, color: '#999', marginBottom: 4 }}>
                        Currently Serving
                      </div>
                      <div style={{ fontSize: 32, fontWeight: 'bold', color: '#1890ff' }}>
                        #{inProgressToken.tokenNumber}
                      </div>
                    </div>
                  )}

                  {/* Waiting Tokens */}
                  {waitingTokens.length > 0 && (
                    <div>
                      <div style={{ fontSize: 12, color: '#999', marginBottom: 8 }}>
                        <ClockCircleOutlined /> Waiting ({waitingTokens.length})
                      </div>
                      <div style={{ display: 'flex', flexWrap: 'wrap', gap: 8 }}>
                        {waitingTokens.slice(0, 10).map((token: QueueTokenDto) => (
                          <Badge
                            key={token.appointmentId}
                            count={token.tokenNumber}
                            style={{ backgroundColor: '#faad14' }}
                          />
                        ))}
                        {waitingTokens.length > 10 && (
                          <Tag>+{waitingTokens.length - 10} more</Tag>
                        )}
                      </div>
                    </div>
                  )}

                  {/* Completed Count */}
                  {completedCount > 0 && (
                    <div style={{ marginTop: 16, fontSize: 12, color: '#999', textAlign: 'center' }}>
                      <CheckCircleOutlined /> {completedCount} completed today
                    </div>
                  )}
                </Card>
              </Col>
            )
          })}
        </Row>
      )}
    </div>
  )
}

