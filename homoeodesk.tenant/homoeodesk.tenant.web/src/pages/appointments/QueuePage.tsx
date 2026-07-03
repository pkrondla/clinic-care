import { useState, useEffect } from 'react'
import { Card, List, Badge, Tag, Button, Space, Select, Row, Col, Statistic, Empty } from 'antd'
import { 
  ClockCircleOutlined, 
  UserOutlined, 
  CheckCircleOutlined,
  ReloadOutlined,
  PhoneOutlined,
  HomeOutlined,
  MedicineBoxOutlined
} from '@ant-design/icons'
import { useAppointmentQueue } from '@core/hooks/queries/useAppointments'
import { useSelectedBranch, useUser } from '@core/stores/authStore'
import { Appointment, AppointmentStatus, AppointmentType, UserRole } from '@core/types'
import { useSignalR } from '@core/hooks/useSignalR'
import { useDoctors } from '@core/hooks/queries/useDoctors'
import { useNavigate } from 'react-router-dom'
import dayjs from 'dayjs'
import relativeTime from 'dayjs/plugin/relativeTime'

dayjs.extend(relativeTime)

const { Option } = Select

export const QueuePage = () => {
  const navigate = useNavigate()
  const user = useUser()
  const selectedClinic = useSelectedBranch()
  const [selectedDoctorId, setSelectedDoctorId] = useState<number | undefined>(
    user?.role === UserRole.Doctor ? user.id : undefined
  )

  // Load doctors for the selected clinic
  const { data: doctors } = useDoctors({
    BranchId: selectedClinic?.id,
    isActive: true,
  })

  // Fetch queue data
  const { data: queueData, isLoading, refetch } = useAppointmentQueue(
    selectedDoctorId,
    selectedClinic?.id,
    dayjs().format('YYYY-MM-DD')
  )

  // Setup SignalR for real-time updates
  const { isConnected } = useSignalR()

  // Auto-refresh every 30 seconds
  useEffect(() => {
    const interval = setInterval(() => {
      refetch()
    }, 30000)
    return () => clearInterval(interval)
  }, [refetch])

  const queue: Appointment[] = queueData || []
  const currentAppointment = queue.find((a) => a.status === AppointmentStatus.InProgress)
  const waitingQueue = queue.filter((a) => a.status === AppointmentStatus.Scheduled)
  const completedToday = queue.filter((a) => a.status === AppointmentStatus.Completed)

  const getAppointmentTypeIcon = (type: AppointmentType) => {
    return type === AppointmentType.InPerson ? <HomeOutlined /> : <PhoneOutlined />
  }

  const getAppointmentTypeColor = (type: AppointmentType) => {
    return type === AppointmentType.InPerson ? 'green' : 'blue'
  }

  return (
    <div>
      <div style={{ marginBottom: 24 }}>
        <Row justify="space-between" align="middle">
          <Col>
            <h2 style={{ margin: 0 }}>
              Patient Queue
              {isConnected && (
                <Badge 
                  status="success" 
                  text="Live" 
                  style={{ marginLeft: 16, fontSize: 14, fontWeight: 'normal' }}
                />
              )}
            </h2>
          </Col>
          <Col>
            <Space>
              {user?.role !== UserRole.Doctor && (
                <Select
                  style={{ width: 200 }}
                  placeholder="Select Doctor"
                  value={selectedDoctorId}
                  onChange={setSelectedDoctorId}
                  allowClear
                  disabled={!selectedClinic}
                >
                  {doctors?.map((doctor) => (
                    <Option key={doctor.id} value={doctor.id}>
                      {doctor.doctorName}
                    </Option>
                  ))}
                </Select>
              )}
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

      {/* Statistics */}
      <Row gutter={[16, 16]} style={{ marginBottom: 24 }}>
        <Col xs={24} sm={8}>
          <Card>
            <Statistic
              title="Waiting"
              value={waitingQueue.length}
              prefix={<ClockCircleOutlined />}
              valueStyle={{ color: '#faad14' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={8}>
          <Card>
            <Statistic
              title="In Progress"
              value={currentAppointment ? 1 : 0}
              prefix={<UserOutlined />}
              valueStyle={{ color: '#1890ff' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={8}>
          <Card>
            <Statistic
              title="Completed Today"
              value={completedToday.length}
              prefix={<CheckCircleOutlined />}
              valueStyle={{ color: '#52c41a' }}
            />
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
            extra={currentAppointment && (
              <Tag color="orange">In Progress</Tag>
            )}
          >
            {currentAppointment ? (
              <div style={{ padding: '20px 0' }}>
                <div style={{ marginBottom: 16 }}>
                  <div style={{ fontSize: 18, fontWeight: 'bold', marginBottom: 8 }}>
                    Token #{currentAppointment.tokenNumber}
                  </div>
                  <div style={{ fontSize: 24, marginBottom: 8 }}>
                    {currentAppointment.patient?.name}
                  </div>
                  <Space>
                    <Tag color={getAppointmentTypeColor(currentAppointment.type)} icon={getAppointmentTypeIcon(currentAppointment.type)}>
                      {currentAppointment.type === AppointmentType.InPerson ? 'In-Person' : 'Teleconsultation'}
                    </Tag>
                    <span style={{ color: '#666' }}>
                      Started {dayjs(currentAppointment.createdAt).fromNow()}
                    </span>
                  </Space>
                </div>
                {currentAppointment.notes && (
                  <div style={{ 
                    padding: 12, 
                    background: '#f5f5f5', 
                    borderRadius: 4,
                    marginTop: 16
                  }}>
                    <div style={{ fontSize: 12, color: '#666', marginBottom: 4 }}>Notes:</div>
                    <div>{currentAppointment.notes}</div>
                  </div>
                )}
              </div>
            ) : (
              <Empty 
                description="No consultation in progress"
                image={Empty.PRESENTED_IMAGE_SIMPLE}
              />
            )}
          </Card>
        </Col>

        {/* Waiting Queue */}
        <Col xs={24} lg={12}>
          <Card 
            title={
              <Space>
                <ClockCircleOutlined />
                <span>Waiting Queue ({waitingQueue.length})</span>
              </Space>
            }
          >
            {waitingQueue.length > 0 ? (
              <List
                dataSource={waitingQueue}
                renderItem={(appointment: Appointment, index) => (
                  <List.Item
                    style={{
                      background: index === 0 ? '#f0f5ff' : 'transparent',
                      padding: '12px 16px',
                      borderRadius: 4,
                      marginBottom: 8
                    }}
                  >
                    <List.Item.Meta
                      avatar={
                        <div style={{
                          width: 40,
                          height: 40,
                          borderRadius: '50%',
                          background: '#1890ff',
                          color: 'white',
                          display: 'flex',
                          alignItems: 'center',
                          justifyContent: 'center',
                          fontWeight: 'bold'
                        }}>
                          #{appointment.tokenNumber}
                        </div>
                      }
                      title={
                        <Space>
                          <span>{appointment.patient?.name}</span>
                          {index === 0 && <Tag color="blue">Next</Tag>}
                        </Space>
                      }
                      description={
                        <Space>
                          <Tag 
                            color={getAppointmentTypeColor(appointment.type)} 
                            icon={getAppointmentTypeIcon(appointment.type)}
                          >
                            {appointment.type === AppointmentType.InPerson ? 'In-Person' : 'Tele'}
                          </Tag>
                          <span style={{ fontSize: 12, color: '#999' }}>
                            Waiting: {dayjs(appointment.createdAt).fromNow()}
                          </span>
                        </Space>
                      }
                    />
                    {!appointment.consultation && (
                      <Button
                        type="primary"
                        icon={<MedicineBoxOutlined />}
                        size="small"
                        onClick={() => navigate(`/consultations/new?appointmentId=${appointment.id}&patientId=${appointment.patient?.id}`)}
                      >
                        Start Consultation
                      </Button>
                    )}
                  </List.Item>
                )}
              />
            ) : (
              <Empty 
                description="No patients waiting"
                image={Empty.PRESENTED_IMAGE_SIMPLE}
              />
            )}
          </Card>
        </Col>
      </Row>

      {/* Completed Today */}
      {completedToday.length > 0 && (
        <Card 
          title={
            <Space>
              <CheckCircleOutlined />
              <span>Completed Today ({completedToday.length})</span>
            </Space>
          }
          style={{ marginTop: 16 }}
        >
          <List
            dataSource={completedToday}
            grid={{ gutter: 16, xs: 1, sm: 2, md: 3, lg: 4 }}
            renderItem={(appointment: Appointment) => (
              <List.Item>
                <Card size="small">
                  <div style={{ textAlign: 'center' }}>
                    <div style={{ 
                      fontSize: 20, 
                      fontWeight: 'bold', 
                      color: '#52c41a',
                      marginBottom: 8
                    }}>
                      #{appointment.tokenNumber}
                    </div>
                    <div style={{ marginBottom: 4 }}>
                      {appointment.patient?.name}
                    </div>
                    <div style={{ fontSize: 12, color: '#999' }}>
                      {dayjs(appointment.createdAt).format('h:mm A')}
                    </div>
                  </div>
                </Card>
              </List.Item>
            )}
          />
        </Card>
      )}
    </div>
  )
}

