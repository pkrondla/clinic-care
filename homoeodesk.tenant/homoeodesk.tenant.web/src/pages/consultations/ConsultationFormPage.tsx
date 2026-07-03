import { useEffect, useState } from 'react'
import { useNavigate, useSearchParams, useParams } from 'react-router-dom'
import { Card, Form, Input, InputNumber, Button, message, Space, Spin, Typography, Row, Col, Table, Tag, Modal, Descriptions, Upload } from 'antd'
import { SaveOutlined, ArrowLeftOutlined, CopyOutlined, PlusOutlined, DeleteOutlined } from '@ant-design/icons'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { consultationService, type CreateConsultationRequest, type UpdateConsultationRequest, type ConsultationPhoto } from '@core/services/consultationService'
import { useConsultation, useUpdateConsultation, usePatientConsultations } from '@core/hooks/queries/useConsultations'
import { useAppointment } from '@core/hooks/queries/useAppointments'
import { prescriptionService, type Prescription } from '@core/services/prescriptionService'
import { useUser } from '@core/stores/authStore'
import { UserRole } from '@core/types'
import dayjs from 'dayjs'
import type { UploadFile, UploadProps } from 'antd/es/upload'

const { TextArea } = Input
const { Title, Text } = Typography

const DISPENSING_FORMS: Record<number, string> = {
  1: 'Globules',
  2: 'Tablets',
  3: 'Packet',
  4: 'Liquid',
  5: 'Tonic'
}

export const ConsultationFormPage = () => {
  const [form] = Form.useForm()
  const navigate = useNavigate()
  const { id } = useParams<{ id: string }>()
  const [searchParams] = useSearchParams()
  const user = useUser()
  const queryClient = useQueryClient()
  const [selectedConsultationId, setSelectedConsultationId] = useState<number | null>(null)
  const [photoFileList, setPhotoFileList] = useState<UploadFile[]>([])
  const [uploadingPhotos, setUploadingPhotos] = useState(false)
  
  const isEditMode = !!id
  const consultationId = id ? parseInt(id) : undefined
  
  const appointmentId = searchParams.get('appointmentId')
  const patientId = searchParams.get('patientId')
  
  // Fetch consultation if in edit mode
  const { data: consultation, isLoading: isLoadingConsultation } = useConsultation(consultationId || 0)
  
  // Fetch appointment to get the doctor ID (for create mode)
  const { data: appointment, isLoading: isLoadingAppointment } = useAppointment(
    appointmentId ? parseInt(appointmentId) : (consultation?.appointmentId || 0)
  )
  
  // Fetch patient consultations for doctors in create mode
  const currentPatientId = isEditMode ? consultation?.patientId : parseInt(patientId || '0')
  const { data: patientConsultations = [], isLoading: isLoadingPatientConsultations } = usePatientConsultations(
    currentPatientId || 0
  )
  
  // Get last 5 consultations (already sorted descending by backend)
  const last5Consultations = patientConsultations.slice(0, 5)
  
  // Fetch selected consultation details
  const { data: selectedConsultation, isLoading: isLoadingSelectedConsultation } = useConsultation(selectedConsultationId || 0)
  
  // Fetch prescription for selected consultation
  const { data: selectedPrescription, isLoading: isLoadingSelectedPrescription } = useQuery<Prescription>({
    queryKey: ['prescription', selectedConsultation?.prescriptionId],
    queryFn: () => prescriptionService.getById(selectedConsultation!.prescriptionId!),
    enabled: !!selectedConsultation?.hasPrescription && !!selectedConsultation?.prescriptionId
  })
  
  const updateConsultation = useUpdateConsultation()
  
  const handleViewConsultation = (consultationId: number) => {
    setSelectedConsultationId(consultationId)
  }
  
  const handleCopyToField = (fieldName: string, value: string) => {
    if (value) {
      form.setFieldsValue({ [fieldName]: value })
      message.success(`Copied to ${fieldName}`)
    }
  }
  
  const handleCloneConsultation = () => {
    if (!selectedConsultation) return
    
    form.setFieldsValue({
      chiefComplaint: selectedConsultation.chiefComplaint || '',
      symptoms: selectedConsultation.symptoms || '',
      examination: selectedConsultation.examination || '',
      diagnosis: selectedConsultation.diagnosis || '',
      treatmentPlan: selectedConsultation.treatmentPlan || '',
      notes: selectedConsultation.notes || '',
      consultationFee: selectedConsultation.consultationFee || 50.00
    })
    
    message.success('All consultation details copied to form')
    setSelectedConsultationId(null) // Close modal after cloning
  }

  const createMutation = useMutation({
    mutationFn: consultationService.create,
    onError: (error: any) => {
      const errorMessage = error.response?.data?.errors?.[0] || error.response?.data?.message || error.message || 'Failed to save consultation'
      message.error(errorMessage)
      console.error('Consultation creation error:', error.response?.data || error)
    }
  })

  // Load consultation data when in edit mode
  useEffect(() => {
    if (isEditMode && consultation) {
      form.setFieldsValue({
        chiefComplaint: consultation.chiefComplaint,
        symptoms: consultation.symptoms,
        examination: consultation.examination,
        diagnosis: consultation.diagnosis,
        treatmentPlan: consultation.treatmentPlan,
        notes: consultation.notes,
        consultationFee: consultation.consultationFee
      })
      
      // Load existing photos
      if (consultation.photos && consultation.photos.length > 0) {
        const files: UploadFile[] = consultation.photos.map((photo, index) => ({
          uid: `existing-${photo.id}`,
          name: `Photo ${index + 1}`,
          status: 'done',
          url: photo.photoUrl,
          thumbUrl: photo.photoUrl,
        }))
        setPhotoFileList(files)
      }
    }
  }, [isEditMode, consultation, form])
  
  // Convert image to base64
  const getBase64 = (file: File): Promise<string> => {
    return new Promise((resolve, reject) => {
      const reader = new FileReader()
      reader.readAsDataURL(file)
      reader.onload = () => resolve(reader.result as string)
      reader.onerror = error => reject(error)
    })
  }
  
  // Handle photo upload
  const handlePhotoChange: UploadProps['onChange'] = ({ fileList: newFileList }) => {
    setPhotoFileList(newFileList)
  }
  
  // Before upload validation
  const beforeUpload = (file: File) => {
    const isImage = file.type.startsWith('image/')
    if (!isImage) {
      message.error('You can only upload image files!')
      return false
    }
    const isLt5M = file.size / 1024 / 1024 < 5
    if (!isLt5M) {
      message.error('Image must be smaller than 5MB!')
      return false
    }
    return false // Prevent auto upload
  }
  
  // Upload photos after consultation is saved
  const uploadPhotos = async (consultationId: number) => {
    const newPhotos = photoFileList.filter(file => file.status === 'uploading' || (!file.url && file.originFileObj))
    
    if (newPhotos.length === 0) return
    
    setUploadingPhotos(true)
    try {
      for (const photo of newPhotos) {
        if (photo.originFileObj) {
          const base64 = await getBase64(photo.originFileObj)
          await consultationService.addPhoto(consultationId, base64, photo.name)
        }
      }
      message.success('Photos uploaded successfully')
      // Refresh consultation data
      queryClient.invalidateQueries({ queryKey: ['consultation', consultationId] })
    } catch (error) {
      message.error('Failed to upload photos')
      console.error('Photo upload error:', error)
    } finally {
      setUploadingPhotos(false)
    }
  }
  
  // Delete photo
  const deletePhotoMutation = useMutation({
    mutationFn: consultationService.deletePhoto,
    onSuccess: () => {
      message.success('Photo deleted successfully')
      if (consultationId) {
        queryClient.invalidateQueries({ queryKey: ['consultation', consultationId] })
      }
    },
    onError: () => {
      message.error('Failed to delete photo')
    }
  })
  
  const handlePhotoRemove = async (file: UploadFile) => {
    // If it's an existing photo (has uid starting with 'existing-'), delete it from server
    if (file.uid?.startsWith('existing-')) {
      const photoId = parseInt(file.uid.replace('existing-', ''))
      if (photoId && consultationId) {
        await deletePhotoMutation.mutateAsync(photoId)
      }
    }
    return true
  }

  const handleSubmit = async () => {
    try {
      const values = await form.validateFields()
      
      if (isEditMode) {
        // Update existing consultation
        const updateData: UpdateConsultationRequest = {
          chiefComplaint: values.chiefComplaint,
          symptoms: values.symptoms,
          examination: values.examination,
          diagnosis: values.diagnosis,
          treatmentPlan: values.treatmentPlan,
          notes: values.notes,
          consultationFee: values.consultationFee
        }
        
        await updateConsultation.mutateAsync({
          id: consultationId!,
          data: updateData
        })
        
        // Upload new photos if any
        await uploadPhotos(consultationId!)
        
        navigate(`/consultations/${consultationId}`)
      } else {
        // Create new consultation
        if (!appointment) {
          message.error('Appointment not found. Please try again.')
          return
        }
        
        const consultationData: CreateConsultationRequest = {
          appointmentId: parseInt(appointmentId || '0'),
          patientId: parseInt(patientId || '0'),
          doctorId: appointment.doctor?.id || 0, // Use doctor ID from appointment, not logged-in user
          ...values
        }

        const result = await createMutation.mutateAsync(consultationData)
        
        // Upload photos after consultation is created
        if (photoFileList.length > 0) {
          await uploadPhotos(result.id)
        }
        
        // Navigate based on user role
        if (user?.role === 'Doctor' || user?.role === 'Admin') {
          navigate(`/prescriptions/new?consultationId=${result.id}&patientId=${patientId}`)
        } else {
          navigate(`/consultations/${result.id}`)
        }
      }
    } catch (error) {
      console.error('Validation failed:', error)
    }
  }

  // Loading states
  if (isEditMode && isLoadingConsultation) {
    return (
      <div style={{ textAlign: 'center', padding: '50px' }}>
        <Spin size="large" />
      </div>
    )
  }

  if (isEditMode && !consultation) {
    return (
      <Card>
        <div style={{ textAlign: 'center', padding: '50px' }}>
          <Title level={4}>Consultation not found</Title>
          <Button onClick={() => navigate('/consultations')}>
            Back to Consultations
          </Button>
        </div>
      </Card>
    )
  }

  // For create mode, check if appointmentId and patientId are provided
  if (!isEditMode && (!appointmentId || !patientId)) {
    return (
      <div style={{ padding: '24px', textAlign: 'center' }}>
        <Card>
          <h3>Create New Consultation</h3>
          <p>To create a consultation, please start from:</p>
          <ul style={{ textAlign: 'left', maxWidth: '400px', margin: '20px auto' }}>
            <li><strong>Appointments Page:</strong> Select an appointment and click "Start Consultation"</li>
            <li><strong>Queue Page:</strong> Select a patient from the queue and start consultation</li>
            <li><strong>Patient Detail Page:</strong> Click "New Consultation" for a specific patient</li>
          </ul>
          <Space>
            <Button type="primary" onClick={() => navigate('/appointments')}>
              Go to Appointments
            </Button>
            <Button onClick={() => navigate('/queue')}>
              Go to Queue
            </Button>
            <Button onClick={() => navigate(-1)}>
              Go Back
            </Button>
          </Space>
        </Card>
      </div>
    )
  }

  if (!isEditMode && isLoadingAppointment) {
    return (
      <div style={{ textAlign: 'center', padding: '50px' }}>
        <Spin size="large" />
      </div>
    )
  }

  if (!isEditMode && !appointment) {
    return (
      <div style={{ padding: '24px', textAlign: 'center' }}>
        <Card>
          <h3>Appointment Not Found</h3>
          <p>The appointment you're trying to create a consultation for could not be found.</p>
          <Space>
            <Button type="primary" onClick={() => navigate('/appointments')}>
              Go to Appointments
            </Button>
            <Button onClick={() => navigate(-1)}>
              Go Back
            </Button>
          </Space>
        </Card>
      </div>
    )
  }

  const currentAppointmentId = isEditMode ? consultation?.appointmentId : parseInt(appointmentId || '0')
  const displayPatientName = isEditMode ? consultation?.patientName : appointment?.patient?.name
  const displayDoctorName = isEditMode ? consultation?.doctorName : appointment?.doctor?.name
  const displayDate = isEditMode && consultation 
    ? dayjs(consultation.consultationDate).format('MMMM DD, YYYY')
    : appointment 
    ? dayjs(appointment.appointmentDate).format('MMMM DD, YYYY')
    : ''
  
  // Show previous consultations only for doctors in create mode
  const showPreviousConsultations = !isEditMode && user?.role === UserRole.Doctor && currentPatientId > 0 && last5Consultations.length > 0

  return (
    <div style={{ padding: '24px', maxWidth: '900px', margin: '0 auto' }}>
      <div style={{ marginBottom: '24px' }}>
        <Button
          icon={<ArrowLeftOutlined />}
          onClick={() => isEditMode ? navigate(`/consultations/${consultationId}`) : navigate(-1)}
          style={{ marginBottom: '16px' }}
        >
          Back
        </Button>
        <Title level={2} style={{ margin: 0 }}>
          {isEditMode ? 'Edit Consultation' : 'New Consultation'}
        </Title>
        <p style={{ margin: '8px 0 0 0', color: '#666' }}>
          {isEditMode ? 'Update consultation details' : 'Record patient consultation details'}
        </p>
        {(displayPatientName || displayDoctorName || displayDate) && (
          <Card size="small" style={{ marginTop: 16 }}>
            <Row gutter={16}>
              {displayPatientName && (
                <Col span={8}>
                  <Space>
                    <Text strong>Patient:</Text>
                    <Text>{displayPatientName}</Text>
                  </Space>
                </Col>
              )}
              {displayDoctorName && (
                <Col span={8}>
                  <Space>
                    <Text strong>Doctor:</Text>
                    <Text>{displayDoctorName}</Text>
                  </Space>
                </Col>
              )}
              {displayDate && (
                <Col span={8}>
                  <Space>
                    <Text strong>Date:</Text>
                    <Text>{displayDate}</Text>
                  </Space>
                </Col>
              )}
            </Row>
          </Card>
        )}
      </div>

      {/* Previous Consultations - Only for doctors in create mode */}
      {showPreviousConsultations && (
        <Card 
          title="Previous Consultations" 
          style={{ marginBottom: 24 }}
          extra={<Text type="secondary">Last 5 consultations</Text>}
        >
          <Table
            dataSource={last5Consultations}
            rowKey="id"
            pagination={false}
            size="small"
            loading={isLoadingPatientConsultations}
            columns={[
              {
                title: 'Date',
                dataIndex: 'consultationDate',
                key: 'consultationDate',
                width: 120,
                render: (date: string) => dayjs(date).format('MMM DD, YYYY'),
              },
              {
                title: 'Chief Complaint',
                dataIndex: 'chiefComplaint',
                key: 'chiefComplaint',
                ellipsis: true,
              },
              {
                title: 'Diagnosis',
                dataIndex: 'diagnosis',
                key: 'diagnosis',
                ellipsis: true,
                render: (text: string) => text || '-',
              },
              {
                title: 'Doctor',
                dataIndex: 'doctorName',
                key: 'doctorName',
                width: 150,
              },
              {
                title: 'Actions',
                key: 'actions',
                width: 100,
                render: (_: any, record: any) => (
                  <Button
                    type="link"
                    size="small"
                    onClick={() => handleViewConsultation(record.id)}
                  >
                    View
                  </Button>
                ),
              },
            ]}
          />
        </Card>
      )}

      {/* Consultation Details Modal */}
      <Modal
        title={
          <Space>
            <Text strong>Previous Consultation Details</Text>
            {selectedConsultation && (
              <Text type="secondary" style={{ fontSize: '12px' }}>
                {dayjs(selectedConsultation.consultationDate).format('MMMM DD, YYYY')}
              </Text>
            )}
          </Space>
        }
        open={!!selectedConsultationId}
        onCancel={() => setSelectedConsultationId(null)}
        footer={[
          <Button key="close" onClick={() => setSelectedConsultationId(null)}>
            Close
          </Button>,
          <Button 
            key="clone" 
            type="primary" 
            onClick={handleCloneConsultation}
            disabled={!selectedConsultation}
          >
            Clone
          </Button>
        ]}
        width={900}
      >
        {isLoadingSelectedConsultation ? (
          <div style={{ textAlign: 'center', padding: '40px' }}>
            <Spin size="large" />
          </div>
        ) : selectedConsultation ? (
          <div>
            {/* Consultation Details */}
            <Card size="small" style={{ marginBottom: 16 }}>
              <Descriptions column={2} size="small" bordered>
                <Descriptions.Item label="Date">
                  {dayjs(selectedConsultation.consultationDate).format('MMMM DD, YYYY [at] hh:mm A')}
                </Descriptions.Item>
                <Descriptions.Item label="Doctor">
                  {selectedConsultation.doctorName}
                </Descriptions.Item>
                <Descriptions.Item label="Chief Complaint" span={2}>
                  <Space>
                    <Text>{selectedConsultation.chiefComplaint || '-'}</Text>
                    {selectedConsultation.chiefComplaint && (
                      <Button
                        type="text"
                        size="small"
                        icon={<CopyOutlined />}
                        onClick={() => handleCopyToField('chiefComplaint', selectedConsultation.chiefComplaint)}
                        title="Copy to Chief Complaint"
                      />
                    )}
                  </Space>
                </Descriptions.Item>
                {selectedConsultation.symptoms && (
                  <Descriptions.Item label="Symptoms" span={2}>
                    <Space>
                      <Text style={{ whiteSpace: 'pre-wrap' }}>{selectedConsultation.symptoms}</Text>
                      <Button
                        type="text"
                        size="small"
                        icon={<CopyOutlined />}
                        onClick={() => handleCopyToField('symptoms', selectedConsultation.symptoms || '')}
                        title="Copy to Symptoms"
                      />
                    </Space>
                  </Descriptions.Item>
                )}
                {selectedConsultation.examination && (
                  <Descriptions.Item label="Physical Examination" span={2}>
                    <Space>
                      <Text style={{ whiteSpace: 'pre-wrap' }}>{selectedConsultation.examination}</Text>
                      <Button
                        type="text"
                        size="small"
                        icon={<CopyOutlined />}
                        onClick={() => handleCopyToField('examination', selectedConsultation.examination || '')}
                        title="Copy to Examination"
                      />
                    </Space>
                  </Descriptions.Item>
                )}
                {selectedConsultation.diagnosis && (
                  <Descriptions.Item label="Diagnosis" span={2}>
                    <Space>
                      <Text style={{ whiteSpace: 'pre-wrap' }}>{selectedConsultation.diagnosis}</Text>
                      <Button
                        type="text"
                        size="small"
                        icon={<CopyOutlined />}
                        onClick={() => handleCopyToField('diagnosis', selectedConsultation.diagnosis || '')}
                        title="Copy to Diagnosis"
                      />
                    </Space>
                  </Descriptions.Item>
                )}
                {selectedConsultation.treatmentPlan && (
                  <Descriptions.Item label="Treatment Plan" span={2}>
                    <Space>
                      <Text style={{ whiteSpace: 'pre-wrap' }}>{selectedConsultation.treatmentPlan}</Text>
                      <Button
                        type="text"
                        size="small"
                        icon={<CopyOutlined />}
                        onClick={() => handleCopyToField('treatmentPlan', selectedConsultation.treatmentPlan || '')}
                        title="Copy to Treatment Plan"
                      />
                    </Space>
                  </Descriptions.Item>
                )}
                {selectedConsultation.notes && (
                  <Descriptions.Item label="Notes" span={2}>
                    <Space>
                      <Text style={{ whiteSpace: 'pre-wrap' }}>{selectedConsultation.notes}</Text>
                      <Button
                        type="text"
                        size="small"
                        icon={<CopyOutlined />}
                        onClick={() => handleCopyToField('notes', selectedConsultation.notes || '')}
                        title="Copy to Notes"
                      />
                    </Space>
                  </Descriptions.Item>
                )}
              </Descriptions>
            </Card>

            {/* Prescription Details */}
            {isLoadingSelectedPrescription ? (
              <Card size="small">
                <div style={{ textAlign: 'center', padding: '20px' }}>
                  <Spin />
                </div>
              </Card>
            ) : selectedPrescription ? (
              <Card size="small" title="Prescription">
                <Space direction="vertical" style={{ width: '100%' }} size="small">
                  <div>
                    <Text strong>Prescription Number: </Text>
                    <Tag color="blue">{selectedPrescription.prescriptionNumber}</Tag>
                  </div>
                  {selectedPrescription.medicines && selectedPrescription.medicines.length > 0 && (
                    <Table
                      dataSource={selectedPrescription.medicines}
                      rowKey={(record, index) => `${record.medicineId}-${index}`}
                      pagination={false}
                      size="small"
                      columns={[
                        {
                          title: 'S.No',
                          key: 'serial',
                          width: 50,
                          align: 'center' as const,
                          render: (_: any, __: any, index: number) => index + 1
                        },
                        {
                          title: 'Medicine',
                          key: 'medicine',
                          render: (_: any, record: Prescription['medicines'][0]) => (
                            <div>
                              <Text strong>{record.medicineName}</Text>
                              <div style={{ fontSize: '11px', color: '#999' }}>
                                {DISPENSING_FORMS[record.dispensingForm] || 'Unknown'}
                                {record.containerSize && record.dispensingForm === 1 && ` (${record.containerSize} dram)`}
                              </div>
                            </div>
                          )
                        },
                        {
                          title: 'Dosage',
                          dataIndex: 'dosage',
                          key: 'dosage',
                          width: 100
                        },
                        {
                          title: 'Frequency',
                          dataIndex: 'frequency',
                          key: 'frequency',
                          width: 120
                        },
                        {
                          title: 'Duration',
                          dataIndex: 'duration',
                          key: 'duration',
                          width: 100
                        },
                        {
                          title: 'Timing',
                          dataIndex: 'timing',
                          key: 'timing',
                          width: 100
                        },
                        {
                          title: 'Instructions',
                          dataIndex: 'instructions',
                          key: 'instructions',
                          ellipsis: true,
                          render: (text: string) => text || '-'
                        }
                      ]}
                    />
                  )}
                  {selectedPrescription.notes && (
                    <div>
                      <Text strong>Prescription Notes: </Text>
                      <Text>{selectedPrescription.notes}</Text>
                    </div>
                  )}
                </Space>
              </Card>
            ) : selectedConsultation.hasPrescription ? (
              <Card size="small">
                <Text type="secondary">Prescription not found</Text>
              </Card>
            ) : (
              <Card size="small">
                <Text type="secondary">No prescription for this consultation</Text>
              </Card>
            )}
          </div>
        ) : (
          <div>Consultation not found</div>
        )}
      </Modal>

      <Card>
        <Form
          form={form}
          layout="vertical"
          initialValues={{
            consultationFee: isEditMode ? consultation?.consultationFee : 50.00
          }}
        >
          <Form.Item
            label="Chief Complaint"
            name="chiefComplaint"
            rules={[{ required: true, message: 'Please enter chief complaint' }]}
          >
            <TextArea
              rows={2}
              placeholder="Main reason for visit (e.g., Fever and cough for 3 days)"
            />
          </Form.Item>

          <Form.Item
            label="Symptoms"
            name="symptoms"
          >
            <TextArea
              rows={3}
              placeholder="Detailed symptoms (e.g., High fever (102°F), dry cough, body aches, headache)"
            />
          </Form.Item>

          <Form.Item
            label="Physical Examination"
            name="examination"
          >
            <TextArea
              rows={3}
              placeholder="Examination findings (e.g., Temperature 102°F, BP 120/80, clear chest sounds, throat slightly red)"
            />
          </Form.Item>

          <Form.Item
            label="Diagnosis"
            name="diagnosis"
          >
            <TextArea
              rows={2}
              placeholder="Clinical diagnosis (e.g., Upper Respiratory Tract Infection)"
            />
          </Form.Item>

          <Form.Item
            label="Treatment Plan"
            name="treatmentPlan"
          >
            <TextArea
              rows={3}
              placeholder="Recommended treatment (e.g., Rest, fluids, prescribed medications, follow-up in 3 days if symptoms persist)"
            />
          </Form.Item>

          <Form.Item
            label="Additional Notes"
            name="notes"
          >
            <TextArea
              rows={2}
              placeholder="Any additional notes or observations"
            />
          </Form.Item>

          <Form.Item
            label="Consultation Fee ($)"
            name="consultationFee"
            rules={[{ required: true, message: 'Please enter consultation fee' }]}
          >
            <InputNumber
              min={0}
              step={0.01}
              style={{ width: '200px' }}
              placeholder="0.00"
            />
          </Form.Item>

          {/* Photo Upload - Only for Doctors */}
          {(user?.role === UserRole.Doctor || user?.role === UserRole.Admin) && (
            <Form.Item
              label="Consultation Photos"
              tooltip="Upload photos related to this consultation (e.g., examination findings, test results)"
            >
              <Upload
                listType="picture-card"
                fileList={photoFileList}
                onChange={handlePhotoChange}
                beforeUpload={beforeUpload}
                onRemove={handlePhotoRemove}
                accept="image/*"
                multiple
              >
                {photoFileList.length < 10 && (
                  <div>
                    <PlusOutlined />
                    <div style={{ marginTop: 8 }}>Upload</div>
                  </div>
                )}
              </Upload>
              <div style={{ marginTop: 8, color: '#999', fontSize: '12px' }}>
                Maximum 10 photos, 5MB per photo
              </div>
            </Form.Item>
          )}

          <Form.Item style={{ marginBottom: 0 }}>
            <Space>
              <Button
                type="primary"
                icon={<SaveOutlined />}
                onClick={handleSubmit}
                loading={isEditMode ? (updateConsultation.isPending || uploadingPhotos) : (createMutation.isPending || uploadingPhotos)}
                size="large"
              >
                {isEditMode 
                  ? 'Save Changes' 
                  : (user?.role === 'Doctor' || user?.role === 'Admin') 
                    ? 'Save & Create Prescription' 
                    : 'Save Consultation'}
              </Button>
              <Button
                onClick={() => isEditMode ? navigate(`/consultations/${consultationId}`) : navigate(-1)}
                size="large"
              >
                Cancel
              </Button>
            </Space>
          </Form.Item>
        </Form>
      </Card>
    </div>
  )
}

