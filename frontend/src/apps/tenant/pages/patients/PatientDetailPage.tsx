import { Card, Row, Col, Typography, Tag, Table, Button, Space, Statistic, Divider } from 'antd'
import { ArrowLeftOutlined, EditOutlined, CalendarOutlined, MedicineBoxOutlined } from '@ant-design/icons'
import { useParams, useNavigate } from 'react-router-dom'
import { usePatient } from '@core/hooks/queries/usePatients'
import dayjs from 'dayjs'

const { Title, Text } = Typography

export const PatientDetailPage = () => {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const { data: patient, isLoading } = usePatient(Number(id))

  if (isLoading) {
    return <div>Loading...</div>
  }

  if (!patient) {
    return <div>Patient not found</div>
  }

  const appointmentColumns = [
    {
      title: 'Token',
      dataIndex: 'tokenNumber',
      key: 'tokenNumber',
      width: 80,
      render: (token: number) => <Tag color="blue">#{token}</Tag>
    },
    {
      title: 'Date',
      dataIndex: 'appointmentDate',
      key: 'appointmentDate',
      width: 120,
      render: (date: string) => dayjs(date).format('MMM DD, YYYY')
    },
    {
      title: 'Type',
      dataIndex: 'type',
      key: 'type',
      width: 100,
      render: (type: string) => (
        <Tag color={type === 'InPerson' ? 'green' : 'blue'}>
          {type}
        </Tag>
      )
    },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      width: 100,
      render: (status: string) => {
        const color = status === 'Completed' ? 'green' : 
                     status === 'Cancelled' ? 'red' : 
                     status === 'InProgress' ? 'orange' : 'blue'
        return <Tag color={color}>{status}</Tag>
      }
    },
    {
      title: 'Doctor',
      dataIndex: 'doctorName',
      key: 'doctorName'
    },
    {
      title: 'Clinic',
      dataIndex: 'clinicName',
      key: 'clinicName'
    }
  ]

  const consultationColumns = [
    {
      title: 'Date',
      dataIndex: 'consultationDate',
      key: 'consultationDate',
      width: 120,
      render: (date: string) => dayjs(date).format('MMM DD, YYYY')
    },
    {
      title: 'Chief Complaint',
      dataIndex: 'chiefComplaint',
      key: 'chiefComplaint',
      ellipsis: true
    },
    {
      title: 'Diagnosis',
      dataIndex: 'diagnosis',
      key: 'diagnosis',
      ellipsis: true
    },
    {
      title: 'Doctor',
      dataIndex: 'doctorName',
      key: 'doctorName'
    },
    {
      title: 'Prescription',
      dataIndex: 'hasPrescription',
      key: 'hasPrescription',
      width: 100,
      render: (hasPrescription: boolean) => (
        <Tag color={hasPrescription ? 'green' : 'default'}>
          {hasPrescription ? 'Yes' : 'No'}
        </Tag>
      )
    }
  ]

  return (
    <div>
      <div style={{ marginBottom: 24, display: 'flex', alignItems: 'center', gap: 16 }}>
        <Button
          icon={<ArrowLeftOutlined />}
          onClick={() => navigate('/patients')}
        >
          Back to Patients
        </Button>
        <Title level={2} style={{ margin: 0 }}>
          {patient.fullName}
        </Title>
        <Button
          type="primary"
          icon={<EditOutlined />}
          onClick={() => navigate(`/patients/${patient.id}/edit`)}
        >
          Edit Patient
        </Button>
      </div>

      <Row gutter={[24, 24]}>
        {/* Patient Information */}
        <Col xs={24} lg={16}>
          <Card title="Patient Information">
            <Row gutter={[16, 16]}>
              <Col xs={24} sm={12}>
                <div>
                  <Text strong>Patient Code:</Text>
                  <br />
                  <Tag color="blue">{patient.patientCode}</Tag>
                </div>
              </Col>
              <Col xs={24} sm={12}>
                <div>
                  <Text strong>Email:</Text>
                  <br />
                  <Text>{patient.email}</Text>
                </div>
              </Col>
              <Col xs={24} sm={12}>
                <div>
                  <Text strong>Phone:</Text>
                  <br />
                  <Text>{patient.phone}</Text>
                </div>
              </Col>
              <Col xs={24} sm={12}>
                <div>
                  <Text strong>Date of Birth:</Text>
                  <br />
                  <Text>{dayjs(patient.dateOfBirth).format('MMMM DD, YYYY')}</Text>
                </div>
              </Col>
              <Col xs={24} sm={12}>
                <div>
                  <Text strong>Age:</Text>
                  <br />
                  <Text>{patient.age} years</Text>
                </div>
              </Col>
              <Col xs={24} sm={12}>
                <div>
                  <Text strong>Gender:</Text>
                  <br />
                  <Text>{patient.gender}</Text>
                </div>
              </Col>
              <Col xs={24} sm={12}>
                <div>
                  <Text strong>Blood Group:</Text>
                  <br />
                  {patient.bloodGroup ? <Tag color="red">{patient.bloodGroup}</Tag> : <Text>-</Text>}
                </div>
              </Col>
              <Col xs={24} sm={12}>
                <div>
                  <Text strong>Emergency Contact:</Text>
                  <br />
                  <Text>{patient.emergencyContact || '-'}</Text>
                </div>
              </Col>
              <Col xs={24}>
                <div>
                  <Text strong>Address:</Text>
                  <br />
                  <Text>{patient.address || '-'}</Text>
                </div>
              </Col>
              <Col xs={24}>
                <div>
                  <Text strong>Medical History:</Text>
                  <br />
                  <Text>{patient.medicalHistory || 'No medical history recorded'}</Text>
                </div>
              </Col>
            </Row>
          </Card>
        </Col>

        {/* Statistics */}
        <Col xs={24} lg={8}>
          <Card title="Statistics">
            <Space direction="vertical" style={{ width: '100%' }} size="large">
              <Statistic
                title="Total Appointments"
                value={patient.totalAppointments}
                prefix={<CalendarOutlined />}
              />
              <Statistic
                title="Completed Appointments"
                value={patient.completedAppointments}
                valueStyle={{ color: '#3f8600' }}
              />
              <Statistic
                title="Total Consultations"
                value={patient.totalConsultations}
                prefix={<MedicineBoxOutlined />}
              />
              <Divider />
              <div>
                <Text strong>First Visit:</Text>
                <br />
                <Text>{patient.firstVisitDate ? dayjs(patient.firstVisitDate).format('MMM DD, YYYY') : 'Never'}</Text>
              </div>
              <div>
                <Text strong>Last Visit:</Text>
                <br />
                <Text>{patient.lastVisitDate ? dayjs(patient.lastVisitDate).format('MMM DD, YYYY') : 'Never'}</Text>
              </div>
            </Space>
          </Card>
        </Col>

        {/* Recent Appointments */}
        <Col xs={24}>
          <Card 
            title="Recent Appointments"
            extra={
              <Button 
                type="link" 
                onClick={() => navigate(`/appointments?patientId=${patient.id}`)}
              >
                View All
              </Button>
            }
          >
            <Table
              columns={appointmentColumns}
              dataSource={patient.recentAppointments}
              rowKey="id"
              pagination={false}
              size="small"
            />
          </Card>
        </Col>

        {/* Recent Consultations */}
        <Col xs={24}>
          <Card 
            title="Recent Consultations"
            extra={
              <Button 
                type="link" 
                onClick={() => navigate(`/consultations?patientId=${patient.id}`)}
              >
                View All
              </Button>
            }
          >
            <Table
              columns={consultationColumns}
              dataSource={patient.recentConsultations}
              rowKey="id"
              pagination={false}
              size="small"
            />
          </Card>
        </Col>
      </Row>
    </div>
  )
}
