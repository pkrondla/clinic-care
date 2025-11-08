import { Card, Row, Col, Statistic, Typography, Table, Tag, Space, Button } from 'antd'
import { 
  CalendarOutlined, 
  TeamOutlined, 
  ClockCircleOutlined, 
  CheckCircleOutlined,
  ReloadOutlined
} from '@ant-design/icons'
import { useAppointments, useAppointmentStats } from '../../hooks/queries/useAppointments'
import { useUser, useSelectedClinic } from '../../stores/authStore'
import { UserRole, AppointmentStatus, AppointmentType } from '../../types/index'
import dayjs from 'dayjs'

const { Title } = Typography

export const DashboardPage = () => {
  const user = useUser()
  const selectedClinic = useSelectedClinic()
  
  // Get today's appointments
  const { data: todayAppointments, isLoading: appointmentsLoading, refetch: refetchAppointments } = useAppointments({
    date: dayjs().format('YYYY-MM-DD'),
    clinicId: selectedClinic?.id
  })

  // Get appointment statistics  
  useAppointmentStats(
    selectedClinic?.id,
    user?.role === UserRole.Doctor ? user.id : undefined
  )

  const getStatusColor = (status: AppointmentStatus) => {
    switch (status) {
      case AppointmentStatus.Scheduled: return 'blue'
      case AppointmentStatus.InProgress: return 'orange'
      case AppointmentStatus.Completed: return 'green'
      case AppointmentStatus.Cancelled: return 'red'
      default: return 'default'
    }
  }

  const getStatusText = (status: AppointmentStatus) => {
    switch (status) {
      case AppointmentStatus.Scheduled: return 'Scheduled'
      case AppointmentStatus.InProgress: return 'In Progress'
      case AppointmentStatus.Completed: return 'Completed'
      case AppointmentStatus.Cancelled: return 'Cancelled'
      default: return 'Unknown'
    }
  }

  const getTypeText = (type: AppointmentType) => {
    return type === AppointmentType.InPerson ? 'In-Person' : 'Teleconsultation'
  }

  // Table columns for appointments
  const appointmentColumns = [
    {
      title: 'Token',
      dataIndex: 'tokenNumber',
      key: 'tokenNumber',
      render: (token: number) => <Tag color="blue">#{token}</Tag>
    },
    {
      title: 'Patient',
      dataIndex: ['patient', 'name'],
      key: 'patientName'
    },
    {
      title: 'Doctor',
      dataIndex: ['doctor', 'name'],
      key: 'doctorName'
    },
    {
      title: 'Type',
      dataIndex: 'type',
      key: 'type',
      render: (type: AppointmentType) => (
        <Tag color={type === AppointmentType.InPerson ? 'green' : 'blue'}>
          {getTypeText(type)}
        </Tag>
      )
    },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      render: (status: AppointmentStatus) => (
        <Tag color={getStatusColor(status)}>
          {getStatusText(status)}
        </Tag>
      )
    },
    {
      title: 'Time',
      dataIndex: 'appointmentDate',
      key: 'appointmentDate',
      render: (date: string) => dayjs(date).format('MMM DD, YYYY')
    }
  ]

  const getDashboardTitle = () => {
    switch (user?.role) {
      case UserRole.SuperAdmin:
        return 'System Overview'
      case UserRole.Admin:
        return 'Organization Dashboard'
      case UserRole.Doctor:
        return 'Doctor Dashboard'
      case UserRole.Staff:
        return 'Staff Dashboard'
      case UserRole.Patient:
        return 'Patient Portal'
      default:
        return 'Dashboard'
    }
  }

  const getWelcomeMessage = () => {
    const greeting = dayjs().hour() < 12 ? 'Good morning' : dayjs().hour() < 18 ? 'Good afternoon' : 'Good evening'
    return `${greeting}, ${user?.firstName}!`
  }

  return (
    <div>
      <div style={{ marginBottom: 24 }}>
        <Title level={2}>{getDashboardTitle()}</Title>
        <Typography.Text type="secondary" style={{ fontSize: 16 }}>
          {getWelcomeMessage()}
          {selectedClinic && ` You're currently at ${selectedClinic.name}.`}
        </Typography.Text>
      </div>

      {/* Statistics Cards */}
      <Row gutter={[16, 16]} style={{ marginBottom: 24 }}>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="Today's Appointments"
              value={todayAppointments?.length || 0}
              prefix={<CalendarOutlined />}
              loading={appointmentsLoading}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="Patients Seen"
              value={todayAppointments?.filter(a => a.status === AppointmentStatus.Completed).length || 0}
              prefix={<CheckCircleOutlined />}
              loading={appointmentsLoading}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="In Progress"
              value={todayAppointments?.filter(a => a.status === AppointmentStatus.InProgress).length || 0}
              prefix={<ClockCircleOutlined />}
              loading={appointmentsLoading}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="Waiting"
              value={todayAppointments?.filter(a => a.status === AppointmentStatus.Scheduled).length || 0}
              prefix={<TeamOutlined />}
              loading={appointmentsLoading}
            />
          </Card>
        </Col>
      </Row>

      {/* Recent Appointments */}
      <Card
        title={
          <Space>
            <span>Today's Appointments</span>
            <Button 
              type="text" 
              icon={<ReloadOutlined />} 
              onClick={() => refetchAppointments()}
              loading={appointmentsLoading}
            />
          </Space>
        }
      >
        <Table
          dataSource={todayAppointments}
          columns={appointmentColumns}
          rowKey="id"
          loading={appointmentsLoading}
          pagination={{
            pageSize: 10,
            showSizeChanger: false
          }}
          scroll={{ x: 800 }}
        />
      </Card>

      {/* Role-specific widgets */}
      {user?.role === UserRole.Doctor && (
        <Row gutter={[16, 16]} style={{ marginTop: 24 }}>
          <Col xs={24} lg={12}>
            <Card title="Quick Actions">
              <Space direction="vertical" style={{ width: '100%' }}>
                <Button type="primary" block>
                  View Patient Queue
                </Button>
                <Button block>
                  New Consultation
                </Button>
                <Button block>
                  Prescription History
                </Button>
              </Space>
            </Card>
          </Col>
          <Col xs={24} lg={12}>
            <Card title="Today's Summary">
              <div style={{ textAlign: 'center' }}>
                <Statistic
                  title="Consultation Fees Earned"
                  value={1250}
                  prefix="₹"
                  precision={2}
                />
              </div>
            </Card>
          </Col>
        </Row>
      )}

      {user?.role === UserRole.Patient && (
        <Row gutter={[16, 16]} style={{ marginTop: 24 }}>
          <Col xs={24} lg={12}>
            <Card title="My Next Appointment">
              <div style={{ textAlign: 'center', padding: 20 }}>
                <Typography.Text type="secondary">
                  No upcoming appointments
                </Typography.Text>
              </div>
            </Card>
          </Col>
          <Col xs={24} lg={12}>
            <Card title="Recent Prescriptions">
              <div style={{ textAlign: 'center', padding:20 }}>
                <Typography.Text type="secondary">
                  No recent prescriptions
                </Typography.Text>
              </div>
            </Card>
          </Col>
        </Row>
      )}
    </div>
  )
}
