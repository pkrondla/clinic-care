import { useState } from 'react'
import { useNavigate, useSearchParams } from 'react-router-dom'
import { Card, Form, Input, InputNumber, Button, message, Space, Table, Modal, Select } from 'antd'
import { SaveOutlined, ArrowLeftOutlined, PlusOutlined, DeleteOutlined, DollarOutlined, DownloadOutlined } from '@ant-design/icons'
import { useMutation } from '@tanstack/react-query'
import { prescriptionService, type CreatePrescriptionRequest, type PrescriptionMedicine } from '@core/services/prescriptionService'
import { useCreateInvoiceFromPrescription } from '@core/hooks/queries/useInvoices'
import { useAuth } from '@core/stores/authStore'

const { TextArea } = Input
const { Option } = Select

const FREQUENCIES = [
  'Once daily',
  'Twice daily',
  'Three times daily',
  'Four times daily',
  'Every 6 hours',
  'Every 8 hours',
  'Every 12 hours',
  'As needed',
  'Before meals',
  'After meals',
  'At bedtime'
]

export const PrescriptionFormPage = () => {
  const [form] = Form.useForm()
  const [medicineForm] = Form.useForm()
  const navigate = useNavigate()
  const [searchParams] = useSearchParams()
  const { user } = useAuth()
  
  const consultationId = searchParams.get('consultationId')
  const patientId = searchParams.get('patientId')

  const [medicines, setMedicines] = useState<PrescriptionMedicine[]>([])
  const [isMedicineModalOpen, setIsMedicineModalOpen] = useState(false)

  const createMutation = useMutation({
    mutationFn: prescriptionService.create,
    onSuccess: (data) => {
      message.success(`Prescription created successfully! Number: ${data.prescriptionNumber}`)
      // Don't navigate away - allow user to generate invoice
    },
    onError: () => {
      message.error('Failed to create prescription')
    }
  })

  const createInvoiceMutation = useCreateInvoiceFromPrescription()
  const [prescriptionId, setPrescriptionId] = useState<number | null>(null)

  const handleGenerateInvoice = async () => {
    if (!prescriptionId) {
      message.error('Please create prescription first')
      return
    }

    try {
      const invoice = await createInvoiceMutation.mutateAsync({
        prescriptionId: prescriptionId,
      })
      message.success(`Invoice ${invoice.invoiceNumber} created successfully!`)
      navigate(`/invoices/${invoice.id}`)
    } catch (error) {
      // Error is handled by the mutation
    }
  }

  const handleAddMedicine = async () => {
    try {
      const values = await medicineForm.validateFields()
      
      const medicine: PrescriptionMedicine = {
        medicineId: 0, // Would be selected from clinic medicines
        medicineName: values.medicineName,
        dosage: values.dosage,
        frequency: values.frequency,
        duration: values.duration,
        quantity: values.quantity,
        instructions: values.instructions
      }

      setMedicines([...medicines, medicine])
      setIsMedicineModalOpen(false)
      medicineForm.resetFields()
    } catch (error) {
      console.error('Validation failed:', error)
    }
  }

  const handleRemoveMedicine = (index: number) => {
    setMedicines(medicines.filter((_, i) => i !== index))
  }

  const handleSubmit = async () => {
    try {
      if (medicines.length === 0) {
        message.error('Please add at least one medicine')
        return
      }

      const values = await form.validateFields()
      
      const prescriptionData: CreatePrescriptionRequest = {
        consultationId: parseInt(consultationId || '0'),
        patientId: parseInt(patientId || '0'),
        doctorId: user?.id || 0,
        medicines: medicines,
        notes: values.notes
      }

      createMutation.mutate(prescriptionData, {
        onSuccess: (data) => {
          setPrescriptionId(data.id)
        }
      })
    } catch (error) {
      console.error('Validation failed:', error)
    }
  }

  if (!consultationId || !patientId) {
    return (
      <div style={{ padding: '24px', textAlign: 'center' }}>
        <Card>
          <h3>Create New Prescription</h3>
          <p>To create a prescription, please start from:</p>
          <ul style={{ textAlign: 'left', maxWidth: '400px', margin: '20px auto' }}>
            <li><strong>Consultations Page:</strong> Complete a consultation first, which will automatically redirect to prescription creation</li>
            <li><strong>After Consultation:</strong> The system will automatically navigate here with the correct consultation context</li>
          </ul>
          <Space>
            <Button type="primary" onClick={() => navigate('/consultations')}>
              Go to Consultations
            </Button>
            <Button onClick={() => navigate('/appointments')}>
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

  const medicineColumns = [
    {
      title: 'Medicine',
      dataIndex: 'medicineName',
      key: 'medicineName'
    },
    {
      title: 'Dosage',
      dataIndex: 'dosage',
      key: 'dosage'
    },
    {
      title: 'Frequency',
      dataIndex: 'frequency',
      key: 'frequency'
    },
    {
      title: 'Duration',
      dataIndex: 'duration',
      key: 'duration',
      render: (duration: number) => `${duration} days`
    },
    {
      title: 'Quantity',
      dataIndex: 'quantity',
      key: 'quantity'
    },
    {
      title: 'Instructions',
      dataIndex: 'instructions',
      key: 'instructions',
      ellipsis: true
    },
    {
      title: 'Actions',
      key: 'actions',
      render: (_: any, __: any, index: number) => (
        <Button
          type="link"
          danger
          icon={<DeleteOutlined />}
          onClick={() => handleRemoveMedicine(index)}
        >
          Remove
        </Button>
      )
    }
  ]

  return (
    <div style={{ padding: '24px', maxWidth: '1200px', margin: '0 auto' }}>
      <div style={{ marginBottom: '24px' }}>
        <Button
          icon={<ArrowLeftOutlined />}
          onClick={() => navigate(-1)}
          style={{ marginBottom: '16px' }}
        >
          Back
        </Button>
        <h1 style={{ margin: 0, fontSize: '24px', fontWeight: 600 }}>
          Create Prescription
        </h1>
        <p style={{ margin: '8px 0 0 0', color: '#666' }}>
          Add medicines and instructions for the patient
        </p>
      </div>

      <Card style={{ marginBottom: '24px' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '16px' }}>
          <h3 style={{ margin: 0 }}>Medicines</h3>
          <Button
            type="primary"
            icon={<PlusOutlined />}
            onClick={() => setIsMedicineModalOpen(true)}
          >
            Add Medicine
          </Button>
        </div>

        {medicines.length === 0 ? (
          <div style={{ textAlign: 'center', padding: '40px', color: '#999' }}>
            <p>No medicines added yet.</p>
            <p>Click "Add Medicine" to start adding medicines to the prescription.</p>
          </div>
        ) : (
          <Table
            columns={medicineColumns}
            dataSource={medicines}
            rowKey={(_, index) => index?.toString() || '0'}
            pagination={false}
          />
        )}
      </Card>

      <Card>
        <Form
          form={form}
          layout="vertical"
        >
          <Form.Item
            label="Patient Instructions / Notes"
            name="notes"
          >
            <TextArea
              rows={4}
              placeholder="Additional instructions for the patient (e.g., Complete the full course. Avoid alcohol. Follow up if symptoms persist beyond 3 days.)"
            />
          </Form.Item>

          <Form.Item style={{ marginBottom: 0 }}>
            <Space>
              {!prescriptionId ? (
                <>
                  <Button
                    type="primary"
                    icon={<SaveOutlined />}
                    onClick={handleSubmit}
                    loading={createMutation.isPending}
                    size="large"
                    disabled={medicines.length === 0}
                  >
                    Create Prescription
                  </Button>
                  <Button
                    onClick={() => navigate(-1)}
                    size="large"
                  >
                    Cancel
                  </Button>
                </>
              ) : (
                <>
                  <Button
                    type="primary"
                    icon={<DollarOutlined />}
                    onClick={handleGenerateInvoice}
                    loading={createInvoiceMutation.isPending}
                    size="large"
                  >
                    Generate Invoice
                  </Button>
                  <Button
                    onClick={async () => {
                      try {
                        const blob = await prescriptionService.downloadPdf(prescriptionId, true)
                        const url = window.URL.createObjectURL(blob)
                        const link = document.createElement('a')
                        link.href = url
                        link.download = `Prescription_${prescriptionId}_Internal.pdf`
                        document.body.appendChild(link)
                        link.click()
                        document.body.removeChild(link)
                        window.URL.revokeObjectURL(url)
                        message.success('Prescription PDF downloaded')
                      } catch (error) {
                        message.error('Failed to download prescription PDF')
                      }
                    }}
                    size="large"
                  >
                    Download PDF (Internal)
                  </Button>
                  <Button
                    onClick={() => navigate(`/patients/${patientId}`)}
                    size="large"
                  >
                    Back to Patient
                  </Button>
                </>
              )}
            </Space>
          </Form.Item>
        </Form>
      </Card>

      {/* Add Medicine Modal */}
      <Modal
        title="Add Medicine"
        open={isMedicineModalOpen}
        onOk={handleAddMedicine}
        onCancel={() => {
          setIsMedicineModalOpen(false)
          medicineForm.resetFields()
        }}
        width={600}
      >
        <Form
          form={medicineForm}
          layout="vertical"
          style={{ marginTop: '24px' }}
        >
          <Form.Item
            label="Medicine Name"
            name="medicineName"
            rules={[{ required: true, message: 'Please enter medicine name' }]}
          >
            <Input placeholder="e.g., Paracetamol 500mg" />
          </Form.Item>

          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '16px' }}>
            <Form.Item
              label="Dosage"
              name="dosage"
              rules={[{ required: true, message: 'Please enter dosage' }]}
            >
              <Input placeholder="e.g., 500mg" />
            </Form.Item>

            <Form.Item
              label="Frequency"
              name="frequency"
              rules={[{ required: true, message: 'Please select frequency' }]}
            >
              <Select placeholder="Select frequency">
                {FREQUENCIES.map(freq => (
                  <Option key={freq} value={freq}>{freq}</Option>
                ))}
              </Select>
            </Form.Item>
          </div>

          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '16px' }}>
            <Form.Item
              label="Duration (days)"
              name="duration"
              rules={[{ required: true, message: 'Please enter duration' }]}
            >
              <InputNumber
                min={1}
                style={{ width: '100%' }}
                placeholder="e.g., 5"
              />
            </Form.Item>

            <Form.Item
              label="Quantity"
              name="quantity"
              rules={[{ required: true, message: 'Please enter quantity' }]}
            >
              <InputNumber
                min={1}
                style={{ width: '100%' }}
                placeholder="e.g., 20"
              />
            </Form.Item>
          </div>

          <Form.Item
            label="Special Instructions"
            name="instructions"
          >
            <TextArea
              rows={2}
              placeholder="e.g., Take after meals with water"
            />
          </Form.Item>
        </Form>
      </Modal>
    </div>
  )
}

