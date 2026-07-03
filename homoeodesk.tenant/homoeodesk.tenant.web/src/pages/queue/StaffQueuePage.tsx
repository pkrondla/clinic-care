import { useState } from 'react'
import { Card, Row, Col, Typography, Tag, Badge, Button, Space, Select, DatePicker, Empty, Spin, Modal, Alert } from 'antd'
import { 
  ClockCircleOutlined, 
  UserOutlined, 
  CheckCircleOutlined,
  ReloadOutlined,
  PhoneOutlined,
  HomeOutlined,
  ExclamationCircleOutlined
} from '@ant-design/icons'
import { useQueues, useStartAppointment, useCompleteAppointment } from '@core/hooks/queries/useQueues'
import { useUser, useSelectedBranch } from '@core/stores/authStore'
import { UserRole } from '@core/types'
import { useClinicUpdates } from '@core/hooks/useSignalR'
import { useBranches } from '@core/hooks/queries/useBranches'
import dayjs from 'dayjs'
import type { DoctorQueueDto, QueueTokenDto } from '@core/services/queueService'

const { Title } = Typography
const { Option } = Select

export const StaffQueuePage = () => {
  const user = useUser()
  const selectedClinic = useSelectedBranch()
  const [selectedDate, setSelectedDate] = useState(dayjs())
  const [confirmStartAppointmentId, setConfirmStartAppointmentId] = useState<number | null>(null)
  const [confirmStartToken, setConfirmStartToken] = useState<QueueTokenDto | null>(null)
  const [confirmStartDoctorName, setConfirmStartDoctorName] = useState<string | null>(null)
  const { data: Branches } = useBranches()
  const BranchId = selectedClinic?.id

  // Get all queues with patient details
  const { data: queues, isLoading, refetch } = useQueues({
    BranchId,
    date: selectedDate.format('YYYY-MM-DD'),
    includePatientDetails: true, // Show patient names and mobile
  })

  // Join clinic updates for real-time
  useClinicUpdates(BranchId || 0)

  const startAppointment = useStartAppointment()
  const completeAppointment = useCompleteAppointment()

  const handleStart = (token: QueueTokenDto, doctorName: string) => {
    setConfirmStartToken(token)
    setConfirmStartAppointmentId(token.appointmentId)
    setConfirmStartDoctorName(doctorName)
  }

  const handleConfirmStart = async () => {
    if (!confirmStartAppointmentId) return

    try {
      await startAppointment.mutateAsync(confirmStartAppointmentId)
      setConfirmStartAppointmentId(null)
      setConfirmStartToken(null)
      setConfirmStartDoctorName(null)
    } catch (error) {
      // Error is handled by the mutation's onError
      setConfirmStartAppointmentId(null)
      setConfirmStartToken(null)
      setConfirmStartDoctorName(null)
    }
  }

  const handleComplete = async (appointmentId: number) => {
    await completeAppointment.mutateAsync(appointmentId)
  }

  const getStatusColor = (status: number) => {
    switch (status) {
      case 1: return 'default' // Scheduled/Waiting
      case 2: return 'processing' // In Progress
      case 3: return 'success' // Completed
      case 4: return 'error' // Cancelled
      default: return 'default'
    }
  }

  const getStatusText = (status: number) => {
    switch (status) {
      case 1: return 'Waiting'
      case 2: return 'In Progress'
      case 3: return 'Completed'
      case 4: return 'Cancelled'
      default: return 'Unknown'
    }
  }

  if (!BranchId) {
    return (
      <Card>
        <Empty description="Please select a clinic to view queues" />
      </Card>
    )
  }

  return (
    <div>
      <div style={{ marginBottom: 24 }}>
        <Row justify="space-between" align="middle">
          <Col>
            <Title level={2} style={{ margin: 0 }}>
              Queue Management
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
      ) : !queues || queues.length === 0 ? (
        <Card>
          <Empty description="No queues found for this date" />
        </Card>
      ) : (
        <Row gutter={[16, 16]}>
          {queues.map((queue: DoctorQueueDto) => (
            <Col xs={24} lg={12} key={queue.doctorId}>
              <Card
                title={
                  <Space>
                    <UserOutlined />
                    <span>{queue.doctorName}</span>
                    <Tag>{queue.qualification}</Tag>
                  </Space>
                }
                extra={
                  <Space>
                    <Badge status="success" text={`${queue.waitingTokens} waiting`} />
                    {queue.currentToken > 0 && (
                      <Tag color="orange">Serving: #{queue.currentToken}</Tag>
                    )}
                  </Space>
                }
              >
                <div style={{ marginBottom: 16 }}>
                  <Row gutter={8}>
                    <Col span={8}>
                      <div style={{ textAlign: 'center' }}>
                        <div style={{ fontSize: 24, fontWeight: 'bold', color: '#faad14' }}>
                          {queue.waitingTokens}
                        </div>
                        <div style={{ fontSize: 12, color: '#999' }}>Waiting</div>
                      </div>
                    </Col>
                    <Col span={8}>
                      <div style={{ textAlign: 'center' }}>
                        <div style={{ fontSize: 24, fontWeight: 'bold', color: '#1890ff' }}>
                          {queue.currentToken > 0 ? 1 : 0}
                        </div>
                        <div style={{ fontSize: 12, color: '#999' }}>In Progress</div>
                      </div>
                    </Col>
                    <Col span={8}>
                      <div style={{ textAlign: 'center' }}>
                        <div style={{ fontSize: 24, fontWeight: 'bold', color: '#52c41a' }}>
                          {queue.tokens.filter(t => t.status === 3).length}
                        </div>
                        <div style={{ fontSize: 12, color: '#999' }}>Completed</div>
                      </div>
                    </Col>
                  </Row>
                </div>

                <div style={{ maxHeight: 400, overflowY: 'auto' }}>
                  {queue.tokens.map((token: QueueTokenDto) => (
                    <Card
                      key={token.appointmentId}
                      size="small"
                      style={{
                        marginBottom: 8,
                        borderLeft: token.status === 2 ? '3px solid #1890ff' : 'none',
                        background: token.status === 2 ? '#f0f5ff' : 'transparent',
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
                              {token.patientCode && (
                                <div style={{ fontSize: 12, color: '#999' }}>
                                  Code: {token.patientCode}
                                </div>
                              )}
                            </div>
                          </Space>
                        </Col>
                        <Col>
                          <Space>
                            <Tag color={getStatusColor(token.status)}>
                              {token.statusText}
                            </Tag>
                            {token.status === 1 && (
                              <Button
                                size="small"
                                type="primary"
                                onClick={() => handleStart(token, queue.doctorName)}
                                loading={startAppointment.isPending}
                              >
                                Start
                              </Button>
                            )}
                            {token.status === 2 && (
                              <Button
                                size="small"
                                type="default"
                                onClick={() => handleComplete(token.appointmentId)}
                                loading={completeAppointment.isPending}
                              >
                                Complete
                              </Button>
                            )}
                          </Space>
                        </Col>
                      </Row>
                    </Card>
                  ))}
                </div>
              </Card>
            </Col>
          ))}
        </Row>
      )}

      {/* Start Appointment Confirmation Modal */}
      <Modal
        title={
          <Space>
            <ExclamationCircleOutlined style={{ color: '#faad14' }} />
            <span>Confirm Start Appointment</span>
          </Space>
        }
        open={!!confirmStartToken}
        onOk={handleConfirmStart}
        onCancel={() => {
          setConfirmStartAppointmentId(null)
          setConfirmStartToken(null)
          setConfirmStartDoctorName(null)
        }}
        okText="Yes, Start Appointment"
        cancelText="Cancel"
        okButtonProps={{ 
          type: 'primary',
          loading: startAppointment.isPending
        }}
        width={500}
      >
        {confirmStartToken && (
          <Space direction="vertical" style={{ width: '100%' }} size="middle">
            {user?.role !== UserRole.Doctor && (
              <Alert
                message="Important Reminder"
                description={
                  <div>
                    <Typography.Text strong>
                      Please confirm with the doctor before starting this appointment.
                    </Typography.Text>
                    <div style={{ marginTop: 8 }}>
                      <Typography.Text type="secondary" style={{ fontSize: '12px' }}>
                        Doctors are the actual persons who start appointments and consultations. 
                        Make sure the doctor is ready before proceeding.
                      </Typography.Text>
                    </div>
                  </div>
                }
                type="warning"
                icon={<ExclamationCircleOutlined />}
                showIcon
                style={{ marginBottom: 16 }}
              />
            )}
            <div>
              <Typography.Text strong>Are you sure you want to start this appointment?</Typography.Text>
            </div>
            <Card size="small" style={{ backgroundColor: '#f5f5f5' }}>
              <Space direction="vertical" style={{ width: '100%' }} size="small">
                <div>
                  <Typography.Text type="secondary">Token Number: </Typography.Text>
                  <Tag color="blue" style={{ fontSize: '14px', padding: '2px 8px' }}>
                    #{confirmStartToken.tokenNumber}
                  </Tag>
                </div>
                <div>
                  <Typography.Text type="secondary">Patient Name: </Typography.Text>
                  <Typography.Text strong>{confirmStartToken.patientName || 'Unknown'}</Typography.Text>
                </div>
                {confirmStartToken.patientCode && (
                  <div>
                    <Typography.Text type="secondary">Patient Code: </Typography.Text>
                    <Typography.Text>{confirmStartToken.patientCode}</Typography.Text>
                  </div>
                )}
                {confirmStartToken.patientMobile && (
                  <div>
                    <Typography.Text type="secondary">Mobile: </Typography.Text>
                    <Typography.Text>
                      <PhoneOutlined /> {confirmStartToken.patientMobile}
                    </Typography.Text>
                  </div>
                )}
                {confirmStartDoctorName && (
                  <div>
                    <Typography.Text type="secondary">Doctor: </Typography.Text>
                    <Typography.Text>{confirmStartDoctorName}</Typography.Text>
                  </div>
                )}
              </Space>
            </Card>
            <Typography.Text type="secondary" style={{ fontSize: '12px' }}>
              This will change the appointment status to "In Progress".
            </Typography.Text>
          </Space>
        )}
      </Modal>
    </div>
  )
}

