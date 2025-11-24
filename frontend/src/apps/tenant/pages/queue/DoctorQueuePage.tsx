import { useState } from 'react'
import { Card, Row, Col, Typography, Tag, Badge, Button, Space, DatePicker, Empty, Spin } from 'antd'
import { 
  ClockCircleOutlined, 
  UserOutlined, 
  CheckCircleOutlined,
  ReloadOutlined,
  PlayCircleOutlined,
  PhoneOutlined
} from '@ant-design/icons'
import { useQueue, useStartAppointment, useCompleteAppointment } from '@core/hooks/queries/useQueues'
import { useUser, useSelectedClinic } from '@core/stores/authStore'
import { useDoctorQueueUpdates } from '@core/hooks/useSignalR'
import dayjs from 'dayjs'
import type { QueueTokenDto } from '@core/services/queueService'

const { Title } = Typography

export const DoctorQueuePage = () => {
  const user = useUser()
  const selectedClinic = useSelectedClinic()
  const [selectedDate, setSelectedDate] = useState(dayjs())
  const doctorId = user?.id || 0
  const clinicId = selectedClinic?.id

  // Get doctor's own queue with patient details
  const { data: queue, isLoading, refetch } = useQueue({
    doctorId,
    clinicId,
    date: selectedDate.format('YYYY-MM-DD'),
    includePatientDetails: true,
  })

  // Join doctor queue for real-time updates
  useDoctorQueueUpdates(doctorId, clinicId || 0)

  const startAppointment = useStartAppointment()
  const completeAppointment = useCompleteAppointment()

  const handleStart = async (appointmentId: number) => {
    await startAppointment.mutateAsync(appointmentId)
  }

  const handleComplete = async (appointmentId: number) => {
    await completeAppointment.mutateAsync(appointmentId)
  }

  if (!clinicId) {
    return (
      <Card>
        <Empty description="Please select a clinic to view your queue" />
      </Card>
    )
  }

  if (!doctorId) {
    return (
      <Card>
        <Empty description="Doctor profile not found" />
      </Card>
    )
  }

  const waitingTokens = queue?.tokens.filter(t => t.status === 1) || []
  const inProgressToken = queue?.tokens.find(t => t.status === 2)
  const completedTokens = queue?.tokens.filter(t => t.status === 3) || []

  return (
    <div>
      <div style={{ marginBottom: 24 }}>
        <Row justify="space-between" align="middle">
          <Col>
            <Title level={2} style={{ margin: 0 }}>
              My Queue
            </Title>
          </Col>
          <Col>
            <Space>
              <DatePicker
                value={selectedDate}
                onChange={(date) => date && setSelectedDate(date)}
                format="YYYY-MM-DD"
              />
              <Button
                icon={<ReloadOutlined />}
                onClick={() => refetch()}
                loading={isLoading}
              >
                Refresh
              </Button>
            </Space>
          </Col>
        </Row>
      </div>

      {isLoading ? (
        <Spin size="large" style={{ display: 'block', textAlign: 'center', padding: '50px' }} />
      ) : !queue ? (
        <Card>
          <Empty description="No queue found for this date" />
        </Card>
      ) : (
        <>
          {/* Statistics */}
          <Row gutter={[16, 16]} style={{ marginBottom: 24 }}>
            <Col xs={24} sm={8}>
              <Card>
                <div style={{ textAlign: 'center' }}>
                  <div style={{ fontSize: 32, fontWeight: 'bold', color: '#faad14' }}>
                    {waitingTokens.length}
                  </div>
                  <div style={{ fontSize: 14, color: '#999', marginTop: 8 }}>
                    <ClockCircleOutlined /> Waiting
                  </div>
                </div>
              </Card>
            </Col>
            <Col xs={24} sm={8}>
              <Card>
                <div style={{ textAlign: 'center' }}>
                  <div style={{ fontSize: 32, fontWeight: 'bold', color: '#1890ff' }}>
                    {inProgressToken ? 1 : 0}
                  </div>
                  <div style={{ fontSize: 14, color: '#999', marginTop: 8 }}>
                    <UserOutlined /> In Progress
                  </div>
                </div>
              </Card>
            </Col>
            <Col xs={24} sm={8}>
              <Card>
                <div style={{ textAlign: 'center' }}>
                  <div style={{ fontSize: 32, fontWeight: 'bold', color: '#52c41a' }}>
                    {completedTokens.length}
                  </div>
                  <div style={{ fontSize: 14, color: '#999', marginTop: 8 }}>
                    <CheckCircleOutlined /> Completed
                  </div>
                </div>
              </Card>
            </Col>
          </Row>

          <Row gutter={[16, 16]}>
            {/* Current Consultation */}
            <Col xs={24} lg={12}>
              <Card
                title={
                  <Space>
                    <UserOutlined />
                    <span>Current Consultation</span>
                  </Space>
                }
                extra={inProgressToken && <Tag color="processing">In Progress</Tag>}
              >
                {inProgressToken ? (
                  <div style={{ padding: '20px 0' }}>
                    <div style={{ textAlign: 'center', marginBottom: 24 }}>
                      <div style={{ fontSize: 48, fontWeight: 'bold', color: '#1890ff', marginBottom: 8 }}>
                        #{inProgressToken.tokenNumber}
                      </div>
                      <div style={{ fontSize: 24, marginBottom: 8 }}>
                        {inProgressToken.patientName || 'Unknown'}
                      </div>
                      {inProgressToken.patientMobile && (
                        <div style={{ fontSize: 14, color: '#999' }}>
                          <PhoneOutlined /> {inProgressToken.patientMobile}
                        </div>
                      )}
                    </div>
                    <Button
                      type="primary"
                      size="large"
                      block
                      icon={<CheckCircleOutlined />}
                      onClick={() => handleComplete(inProgressToken.appointmentId)}
                      loading={completeAppointment.isPending}
                    >
                      Complete Consultation
                    </Button>
                  </div>
                ) : (
                  <Empty description="No consultation in progress" />
                )}
              </Card>
            </Col>

            {/* Waiting Queue */}
            <Col xs={24} lg={12}>
              <Card
                title={
                  <Space>
                    <ClockCircleOutlined />
                    <span>Waiting Queue ({waitingTokens.length})</span>
                  </Space>
                }
              >
                {waitingTokens.length > 0 ? (
                  <div style={{ maxHeight: 400, overflowY: 'auto' }}>
                    {waitingTokens.map((token: QueueTokenDto, index: number) => (
                      <Card
                        key={token.appointmentId}
                        size="small"
                        style={{
                          marginBottom: 8,
                          borderLeft: index === 0 ? '3px solid #52c41a' : 'none',
                          background: index === 0 ? '#f6ffed' : 'transparent',
                        }}
                      >
                        <Row justify="space-between" align="middle">
                          <Col>
                            <Space>
                              <Badge
                                count={token.tokenNumber}
                                style={{ backgroundColor: '#1890ff' }}
                              />
                              <div>
                                <div style={{ fontWeight: 'bold' }}>
                                  {token.patientName || 'Unknown'}
                                </div>
                                {token.patientMobile && (
                                  <div style={{ fontSize: 12, color: '#999' }}>
                                    <PhoneOutlined /> {token.patientMobile}
                                  </div>
                                )}
                              </div>
                            </Space>
                          </Col>
                          <Col>
                            {index === 0 ? (
                              <Button
                                type="primary"
                                icon={<PlayCircleOutlined />}
                                onClick={() => handleStart(token.appointmentId)}
                                loading={startAppointment.isPending}
                              >
                                Start
                              </Button>
                            ) : (
                              <Tag color="default">#{index + 1} in queue</Tag>
                            )}
                          </Col>
                        </Row>
                      </Card>
                    ))}
                  </div>
                ) : (
                  <Empty description="No patients waiting" />
                )}
              </Card>
            </Col>
          </Row>
        </>
      )}
    </div>
  )
}

