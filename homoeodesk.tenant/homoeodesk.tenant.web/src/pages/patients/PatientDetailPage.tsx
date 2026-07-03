import { useState } from 'react'
import { Card, Row, Col, Typography, Tag, Table, Button, Space, Descriptions, Modal, Spin, Image, Badge } from 'antd'
import { ArrowLeftOutlined, EditOutlined, PlusOutlined, EyeOutlined, UserOutlined, CameraOutlined } from '@ant-design/icons'
import { useParams, useNavigate } from 'react-router-dom'
import { usePatient } from '@core/hooks/queries/usePatients'
import { useConsultation } from '@core/hooks/queries/useConsultations'
import { useQuery } from '@tanstack/react-query'
import { prescriptionService, type Prescription } from '@core/services/prescriptionService'
import dayjs from 'dayjs'

const { Title, Text } = Typography

export const PatientDetailPage = () => {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const { data: patient, isLoading } = usePatient(Number(id))
  const [selectedConsultationId, setSelectedConsultationId] = useState<number | null>(null)
  const [photoPreviewConsultation, setPhotoPreviewConsultation] = useState<any | null>(null)
  
  // Fetch selected consultation details
  const { data: selectedConsultation, isLoading: isLoadingConsultation } = useConsultation(selectedConsultationId || 0)
  
  // Fetch prescription for selected consultation
  const { data: selectedPrescription, isLoading: isLoadingPrescription } = useQuery<Prescription>({
    queryKey: ['prescription', selectedConsultation?.prescriptionId],
    queryFn: () => prescriptionService.getById(selectedConsultation!.prescriptionId!),
    enabled: !!selectedConsultation?.hasPrescription && !!selectedConsultation?.prescriptionId
  })

  if (isLoading) {
    return <div>Loading...</div>
  }

  if (!patient) {
    return <div>Patient not found</div>
  }

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
    },
    {
      title: 'Photos',
      key: 'photos',
      width: 80,
      align: 'center' as const,
      render: (_: any, record: any) => {
        const photoCount = record.photos?.length || 0
        return photoCount > 0 ? (
          <Button
            type="text"
            icon={
              <Badge count={photoCount} showZero={false}>
                <CameraOutlined style={{ fontSize: 18, color: '#1890ff' }} />
              </Badge>
            }
            onClick={() => setPhotoPreviewConsultation(record)}
            size="small"
          />
        ) : (
          <CameraOutlined style={{ fontSize: 18, color: '#d9d9d9' }} />
        )
      }
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 80,
      render: (_: any, record: any) => (
        <Button
          type="text"
          icon={<EyeOutlined />}
          onClick={() => setSelectedConsultationId(record.id)}
          size="small"
        />
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
        <Button
          type="default"
          icon={<PlusOutlined />}
          onClick={() => navigate(`/consultations/new?patientId=${patient.id}`)}
        >
          New Consultation
        </Button>
      </div>

      <Row gutter={[24, 24]} style={{ display: 'flex', alignItems: 'stretch' }}>
        {/* Patient Information */}
        <Col xs={24} lg={16} style={{ display: 'flex' }}>
          <Card title="Patient Information" size="small" style={{ width: '100%', display: 'flex', flexDirection: 'column' }}>
            <div style={{ flex: 1 }}>
              <Descriptions column={2} bordered size="small">
                <Descriptions.Item label="Patient Code">
                  <Tag color="blue">{patient.patientCode}</Tag>
                </Descriptions.Item>
                <Descriptions.Item label="Email">
                  <Text>{patient.email}</Text>
                </Descriptions.Item>
                <Descriptions.Item label="Phone">
                  <Text>{patient.phone}</Text>
                </Descriptions.Item>
                <Descriptions.Item label="Date of Birth">
                  <Text>{dayjs(patient.dateOfBirth).format('MMM DD, YYYY')}</Text>
                </Descriptions.Item>
                <Descriptions.Item label="Age">
                  <Text>{patient.age} years</Text>
                </Descriptions.Item>
                <Descriptions.Item label="Gender">
                  <Text>{patient.gender}</Text>
                </Descriptions.Item>
                <Descriptions.Item label="Blood Group">
                  {patient.bloodGroup ? <Tag color="red">{patient.bloodGroup}</Tag> : <Text>-</Text>}
                </Descriptions.Item>
                <Descriptions.Item label="Emergency Contact">
                  <Text>{patient.emergencyContact || '-'}</Text>
                </Descriptions.Item>
                <Descriptions.Item label="Address" span={2}>
                  <Text>{patient.address || '-'}</Text>
                </Descriptions.Item>
                <Descriptions.Item label="Medical History" span={2}>
                  <Text>{patient.medicalHistory || 'No medical history recorded'}</Text>
                </Descriptions.Item>
              </Descriptions>
            </div>
          </Card>
        </Col>

        {/* Patient Photo */}
        <Col xs={24} lg={8} style={{ display: 'flex' }}>
          <Card title="Patient Photo" size="small" style={{ width: '100%', display: 'flex', flexDirection: 'column' }}>
            <div style={{ flex: 1, display: 'flex', alignItems: 'center', justifyContent: 'center', padding: '20px 0' }}>
              {patient.photoUrl ? (
                <img
                  src={patient.photoUrl}
                  alt={patient.fullName}
                  style={{
                    width: '100%',
                    maxWidth: '300px',
                    height: 'auto',
                    borderRadius: '8px',
                    objectFit: 'cover'
                  }}
                />
              ) : (
                <div style={{ 
                  width: '100%', 
                  maxWidth: '300px', 
                  margin: '0 auto',
                  aspectRatio: '1',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  backgroundColor: '#f0f0f0',
                  borderRadius: '8px'
                }}>
                  <UserOutlined style={{ fontSize: '80px', color: '#bfbfbf' }} />
                </div>
              )}
            </div>
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

      {/* Consultation Details Modal */}
      <Modal
        title="Consultation Details"
        open={!!selectedConsultationId}
        onCancel={() => setSelectedConsultationId(null)}
        footer={null}
        width={900}
      >
        {isLoadingConsultation ? (
          <div style={{ textAlign: 'center', padding: '40px' }}>
            <Spin size="large" />
          </div>
        ) : selectedConsultation ? (
          <div>
            {/* Consultation Details */}
            <Card size="small" style={{ marginBottom: 16 }}>
              <Descriptions column={2} bordered size="small">
                <Descriptions.Item label="Date">
                  <Text>{dayjs(selectedConsultation.consultationDate).format('MMM DD, YYYY hh:mm A')}</Text>
                </Descriptions.Item>
                <Descriptions.Item label="Doctor">
                  <Text strong>{selectedConsultation.doctorName}</Text>
                </Descriptions.Item>
                <Descriptions.Item label="Consultation Fee">
                  <Text strong style={{ color: '#1890ff' }}>
                    ₹{selectedConsultation.consultationFee.toFixed(2)}
                  </Text>
                </Descriptions.Item>
                <Descriptions.Item label="Chief Complaint">
                  <Text>{selectedConsultation.chiefComplaint || '-'}</Text>
                </Descriptions.Item>
                {selectedConsultation.symptoms && (
                  <Descriptions.Item label="Symptoms" span={2}>
                    <Text style={{ whiteSpace: 'pre-wrap', fontSize: '13px' }}>{selectedConsultation.symptoms}</Text>
                  </Descriptions.Item>
                )}
                {selectedConsultation.examination && (
                  <Descriptions.Item label="Physical Examination" span={2}>
                    <Text style={{ whiteSpace: 'pre-wrap', fontSize: '13px' }}>{selectedConsultation.examination}</Text>
                  </Descriptions.Item>
                )}
                {selectedConsultation.diagnosis && (
                  <Descriptions.Item label="Diagnosis" span={2}>
                    <Text style={{ whiteSpace: 'pre-wrap', fontSize: '13px' }}>{selectedConsultation.diagnosis}</Text>
                  </Descriptions.Item>
                )}
                {selectedConsultation.treatmentPlan && (
                  <Descriptions.Item label="Treatment Plan" span={2}>
                    <Text style={{ whiteSpace: 'pre-wrap', fontSize: '13px' }}>{selectedConsultation.treatmentPlan}</Text>
                  </Descriptions.Item>
                )}
                {selectedConsultation.notes && (
                  <Descriptions.Item label="Additional Notes" span={2}>
                    <Text style={{ whiteSpace: 'pre-wrap', fontSize: '13px' }}>{selectedConsultation.notes}</Text>
                  </Descriptions.Item>
                )}
              </Descriptions>
            </Card>

            {/* Prescription Details */}
            {isLoadingPrescription ? (
              <Card size="small">
                <div style={{ textAlign: 'center', padding: '20px' }}>
                  <Spin />
                </div>
              </Card>
            ) : selectedPrescription ? (
              <Card size="small" title="Prescription">
                <Descriptions column={2} bordered size="small" style={{ marginBottom: 16 }}>
                  <Descriptions.Item label="Prescription Number">
                    <Tag color="blue">{selectedPrescription.prescriptionNumber}</Tag>
                  </Descriptions.Item>
                  <Descriptions.Item label="Date">
                    {dayjs(selectedPrescription.prescriptionDate).format('MMM DD, YYYY')}
                  </Descriptions.Item>
                  {selectedPrescription.notes && (
                    <Descriptions.Item label="Notes" span={2}>
                      <Text style={{ whiteSpace: 'pre-wrap', fontSize: '13px' }}>{selectedPrescription.notes}</Text>
                    </Descriptions.Item>
                  )}
                </Descriptions>
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
                        render: (_: any, record: any) => (
                          <div>
                            <Text strong style={{ fontSize: '13px' }}>{record.medicineName}</Text>
                            <div style={{ fontSize: '11px', color: '#999' }}>
                              {record.dispensingForm === 1 ? 'Globules' : 
                               record.dispensingForm === 2 ? 'Tablets' :
                               record.dispensingForm === 3 ? 'Packet' :
                               record.dispensingForm === 4 ? 'Liquid' : 'Tonic'}
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
                        width: 100
                      },
                      {
                        title: 'Duration',
                        dataIndex: 'duration',
                        key: 'duration',
                        width: 80
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
              </Card>
            ) : selectedConsultation.hasPrescription ? (
              <Card size="small">No prescription found for this consultation.</Card>
            ) : null}

            {/* Consultation Photos */}
            {selectedConsultation.photos && selectedConsultation.photos.length > 0 && (
              <Card size="small" title="Consultation Photos" style={{ marginTop: 16 }}>
                <Image.PreviewGroup>
                  <Space wrap size={[8, 8]}>
                    {selectedConsultation.photos.map((photo: any) => (
                      <div key={photo.id} style={{ position: 'relative' }}>
                        <Image
                          src={photo.photoUrl}
                          alt={photo.description || 'Consultation photo'}
                          style={{
                            width: 100,
                            height: 100,
                            objectFit: 'cover',
                            borderRadius: 4,
                            cursor: 'pointer'
                          }}
                        />
                        {photo.description && (
                          <div style={{ 
                            fontSize: '11px', 
                            marginTop: 4, 
                            maxWidth: 100, 
                            overflow: 'hidden', 
                            textOverflow: 'ellipsis',
                            whiteSpace: 'nowrap'
                          }}>
                            {photo.description}
                          </div>
                        )}
                      </div>
                    ))}
                  </Space>
                </Image.PreviewGroup>
              </Card>
            )}
          </div>
        ) : (
          <div>Consultation not found</div>
        )}
      </Modal>

      {/* Photo Preview Modal */}
      <Modal
        title="Consultation Photos"
        open={!!photoPreviewConsultation}
        onCancel={() => setPhotoPreviewConsultation(null)}
        footer={null}
        width={800}
      >
        {photoPreviewConsultation?.photos && photoPreviewConsultation.photos.length > 0 ? (
          <Image.PreviewGroup>
            <Space wrap size={[16, 16]}>
              {photoPreviewConsultation.photos.map((photo: any) => (
                <div key={photo.id} style={{ position: 'relative' }}>
                  <Image
                    src={photo.photoUrl}
                    alt={photo.description || 'Consultation photo'}
                    style={{
                      width: 150,
                      height: 150,
                      objectFit: 'cover',
                      borderRadius: 8,
                      cursor: 'pointer'
                    }}
                  />
                  {photo.description && (
                    <div style={{ 
                      fontSize: '12px', 
                      marginTop: 8, 
                      maxWidth: 150, 
                      overflow: 'hidden', 
                      textOverflow: 'ellipsis',
                      whiteSpace: 'nowrap',
                      textAlign: 'center'
                    }}>
                      {photo.description}
                    </div>
                  )}
                </div>
              ))}
            </Space>
          </Image.PreviewGroup>
        ) : (
          <div style={{ textAlign: 'center', padding: '40px', color: '#999' }}>
            No photos available
          </div>
        )}
      </Modal>
    </div>
  )
}
