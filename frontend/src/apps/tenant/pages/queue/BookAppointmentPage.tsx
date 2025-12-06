import { useEffect, useState, useMemo } from 'react'
import { 
  Card, 
  Form, 
  Select, 
  DatePicker, 
  Button, 
  Space, 
  Typography, 
  message, 
  Input, 
  Spin,
  Divider,
  Tag,
  Empty,
  Avatar
} from 'antd'
import { 
  CalendarOutlined, 
  CheckCircleOutlined, 
  UserOutlined,
  SearchOutlined,
  PhoneOutlined,
  IdcardOutlined
} from '@ant-design/icons'
import { useBookAppointment } from '@core/hooks/queries/useQueues'
import { useCreateAppointment } from '@core/hooks/queries/useAppointments'
import { useClinics } from '@core/hooks/queries/useClinics'
import { useDoctors } from '@core/hooks/queries/useDoctors'
import { useSearchPatients } from '@core/hooks/queries/usePatients'
import { useSelectedClinic, useAuth } from '@core/stores/authStore'
import { useNavigate } from 'react-router-dom'
import dayjs from 'dayjs'
import type { Clinic } from '@core/services/clinicService'
import type { PatientSearch } from '@core/types/patient'
import { AppointmentType } from '@core/types/appointment'
import { UserRole } from '@core/types/auth'
import { useDebouncedValue } from '@core/hooks/useDebouncedValue'

const { Title, Text } = Typography
const { Option } = Select

interface BookAppointmentFormValues {
  clinicId: number
  doctorId: number
  appointmentDate: dayjs.Dayjs
  type: number
  notes?: string
}

export const BookAppointmentPage = () => {
  const navigate = useNavigate()
  const selectedClinic = useSelectedClinic()
  const { user } = useAuth()
  const [form] = Form.useForm<BookAppointmentFormValues>()
  
  // Patient search state
  const [searchTerm, setSearchTerm] = useState('')
  const [selectedPatient, setSelectedPatient] = useState<PatientSearch | null>(null)
  const debouncedSearchTerm = useDebouncedValue(searchTerm, 300)
  
  // Determine if user is staff/admin (can book for patients) or patient (books for self)
  const isStaffOrAdmin = user?.role === UserRole.Admin || user?.role === UserRole.Staff || user?.role === UserRole.Doctor
  
  // Mutations
  const bookAppointmentSelf = useBookAppointment() // For patient self-booking
  const createAppointment = useCreateAppointment() // For staff booking on behalf of patient
  
  const { data: clinics } = useClinics()
  const { data: searchResults, isLoading: isSearching } = useSearchPatients({
    searchTerm: debouncedSearchTerm,
    limit: 10
  })
  
  // Get selected clinic ID from form
  const selectedClinicId = Form.useWatch('clinicId', form) || selectedClinic?.id
  
  // Load doctors for selected clinic
  const { data: doctors, isLoading: doctorsLoading } = useDoctors({
    clinicId: selectedClinicId,
    isActive: true,
  })
  
  // Reset doctor selection when clinic changes
  useEffect(() => {
    if (selectedClinicId) {
      form.setFieldsValue({ doctorId: undefined })
    }
  }, [selectedClinicId, form])

  const handlePatientSearch = (value: string) => {
    setSearchTerm(value)
    if (!value) {
      setSelectedPatient(null)
    }
  }

  const handlePatientSelect = (patient: PatientSearch) => {
    setSelectedPatient(patient)
    setSearchTerm('') // Clear search term after selection
  }

  const handleClearPatient = () => {
    setSelectedPatient(null)
    setSearchTerm('')
  }

  const handleSubmit = async (values: BookAppointmentFormValues) => {
    try {
      if (isStaffOrAdmin) {
        // Staff/Admin booking on behalf of patient
        if (!selectedPatient) {
          message.error('Please select a patient')
          return
        }
        
        const result = await createAppointment.mutateAsync({
          clinicId: values.clinicId,
          doctorId: values.doctorId,
          patientId: selectedPatient.id,
          appointmentDate: values.appointmentDate.format('YYYY-MM-DD'),
          type: values.type || AppointmentType.InPerson,
          notes: values.notes,
        })

        message.success(`Appointment booked for ${selectedPatient.fullName}! Token number: ${result.tokenNumber}`)
        navigate('/appointments')
      } else {
        // Patient self-booking
        const result = await bookAppointmentSelf.mutateAsync({
          clinicId: values.clinicId,
          doctorId: values.doctorId,
          appointmentDate: values.appointmentDate.format('YYYY-MM-DD'),
          type: values.type || 1,
          notes: values.notes,
        })

        message.success(`Appointment booked! Your token number is ${result.tokenNumber}`)
        navigate('/my-appointments')
      }
    } catch (error) {
      // Error handled by mutation
    }
  }

  const isLoading = bookAppointmentSelf.isPending || createAppointment.isPending

  return (
    <div style={{ maxWidth: 700, margin: '0 auto', padding: '24px' }}>
      <Card>
        <div style={{ textAlign: 'center', marginBottom: 32 }}>
          <Title level={2}>
            <CalendarOutlined /> Book Appointment
          </Title>
          <Text type="secondary">
            {isStaffOrAdmin 
              ? 'Search for a patient and book an appointment' 
              : 'Book an appointment with a doctor. You will receive a token number.'}
          </Text>
        </div>

        {/* Patient Search Section - Only for Staff/Admin */}
        {isStaffOrAdmin && (
          <>
            <div style={{ marginBottom: 24 }}>
              <Text strong style={{ display: 'block', marginBottom: 8 }}>
                <UserOutlined /> Patient <span style={{ color: '#ff4d4f' }}>*</span>
              </Text>
              
              {selectedPatient ? (
                // Selected patient display
                <Card 
                  size="small" 
                  style={{ 
                    background: '#f6ffed', 
                    borderColor: '#b7eb8f' 
                  }}
                >
                  <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                    <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
                      <Avatar size={40} icon={<UserOutlined />} style={{ backgroundColor: '#52c41a' }} />
                      <div>
                        <div style={{ fontWeight: 600 }}>{selectedPatient.fullName}</div>
                        <Space size="small">
                          <Tag icon={<IdcardOutlined />} color="blue">{selectedPatient.patientCode}</Tag>
                          <Tag icon={<PhoneOutlined />}>{selectedPatient.phone}</Tag>
                          <Tag>{selectedPatient.age} yrs, {selectedPatient.gender}</Tag>
                        </Space>
                      </div>
                    </div>
                    <Button type="link" danger onClick={handleClearPatient}>
                      Change
                    </Button>
                  </div>
                </Card>
              ) : (
                // Patient search input
                <div>
                  <Input
                    placeholder="Search patient by name, phone, or patient code..."
                    prefix={<SearchOutlined />}
                    value={searchTerm}
                    onChange={(e) => handlePatientSearch(e.target.value)}
                    allowClear
                    size="large"
                    autoComplete="off"
                  />
                  
                  {/* Search Results */}
                  {searchTerm.length >= 2 && (
                    <Card 
                      size="small" 
                      style={{ marginTop: 8, maxHeight: 300, overflow: 'auto' }}
                      bodyStyle={{ padding: 0 }}
                    >
                      {isSearching ? (
                        <div style={{ padding: 24, textAlign: 'center' }}>
                          <Spin size="small" />
                          <Text type="secondary" style={{ marginLeft: 8 }}>Searching...</Text>
                        </div>
                      ) : searchResults && searchResults.length > 0 ? (
                        <div>
                          {searchResults.map((patient) => (
                            <div
                              key={patient.id}
                              onClick={() => handlePatientSelect(patient)}
                              style={{
                                padding: '12px 16px',
                                cursor: 'pointer',
                                borderBottom: '1px solid #f0f0f0',
                                transition: 'background-color 0.2s'
                              }}
                              onMouseEnter={(e) => e.currentTarget.style.backgroundColor = '#f5f5f5'}
                              onMouseLeave={(e) => e.currentTarget.style.backgroundColor = 'transparent'}
                            >
                              <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
                                <Avatar size={36} icon={<UserOutlined />} />
                                <div style={{ flex: 1 }}>
                                  <div style={{ fontWeight: 500 }}>{patient.fullName}</div>
                                  <Space size="small">
                                    <Text type="secondary" style={{ fontSize: 12 }}>
                                      {patient.patientCode}
                                    </Text>
                                    <Text type="secondary" style={{ fontSize: 12 }}>
                                      • {patient.phone}
                                    </Text>
                                    <Text type="secondary" style={{ fontSize: 12 }}>
                                      • {patient.age} yrs, {patient.gender}
                                    </Text>
                                  </Space>
                                </div>
                              </div>
                            </div>
                          ))}
                        </div>
                      ) : (
                        <Empty 
                          image={Empty.PRESENTED_IMAGE_SIMPLE} 
                          description="No patients found"
                          style={{ padding: 24 }}
                        >
                          <Button type="link" onClick={() => navigate('/patients/new')}>
                            Register New Patient
                          </Button>
                        </Empty>
                      )}
                    </Card>
                  )}
                </div>
              )}
            </div>
            <Divider />
          </>
        )}

        <Form
          form={form}
          layout="vertical"
          onFinish={handleSubmit}
          initialValues={{
            clinicId: selectedClinic?.id,
            appointmentDate: dayjs(),
            type: AppointmentType.InPerson,
          }}
        >
          <Form.Item
            name="clinicId"
            label="Clinic"
            rules={[{ required: true, message: 'Please select a clinic' }]}
          >
            <Select placeholder="Select clinic" size="large">
              {clinics?.map((clinic: Clinic) => (
                <Option key={clinic.id} value={clinic.id}>
                  {clinic.name}
                </Option>
              ))}
            </Select>
          </Form.Item>

          <Form.Item
            name="doctorId"
            label="Doctor"
            rules={[{ required: true, message: 'Please select a doctor' }]}
            dependencies={['clinicId']}
          >
            <Select 
              placeholder={selectedClinicId ? "Select doctor" : "Please select a clinic first"}
              disabled={!selectedClinicId || doctorsLoading}
              notFoundContent={doctorsLoading ? <Spin size="small" /> : "No doctors available"}
              loading={doctorsLoading}
              size="large"
            >
              {doctors?.map((doctor) => (
                <Option key={doctor.id} value={doctor.id}>
                  {doctor.doctorName} - {doctor.qualification}
                  {doctor.specialization && ` (${doctor.specialization})`}
                </Option>
              ))}
            </Select>
          </Form.Item>

          <Form.Item
            name="appointmentDate"
            label="Date"
            rules={[{ required: true, message: 'Please select a date' }]}
          >
            <DatePicker
              style={{ width: '100%' }}
              format="YYYY-MM-DD"
              disabledDate={(current) => current && current < dayjs().startOf('day')}
              size="large"
            />
          </Form.Item>

          <Form.Item
            name="type"
            label="Appointment Type"
            rules={[{ required: true }]}
          >
            <Select size="large">
              <Option value={AppointmentType.InPerson}>In-Person</Option>
              <Option value={AppointmentType.Teleconsultation}>Teleconsultation</Option>
            </Select>
          </Form.Item>

          <Form.Item
            name="notes"
            label="Notes (Optional)"
          >
            <Input.TextArea
              rows={3}
              placeholder="Any additional notes or symptoms..."
            />
          </Form.Item>

          <Form.Item>
            <Button
              type="primary"
              htmlType="submit"
              block
              size="large"
              icon={<CheckCircleOutlined />}
              loading={isLoading}
              disabled={isStaffOrAdmin && !selectedPatient}
            >
              Book Appointment
            </Button>
          </Form.Item>
        </Form>
      </Card>
    </div>
  )
}
