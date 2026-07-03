import { useState, useEffect } from 'react'
import { useNavigate, useSearchParams, useParams } from 'react-router-dom'
import { Card, Form, Input, InputNumber, Button, message, Space, Table, Modal, Select, AutoComplete, Typography, Tag, Descriptions, Spin } from 'antd'
import { SaveOutlined, ArrowLeftOutlined, PlusOutlined, DeleteOutlined } from '@ant-design/icons'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { prescriptionService, type CreatePrescriptionRequest, type UpdatePrescriptionRequest, type PrescriptionMedicine, type Prescription } from '@core/services/prescriptionService'
import { useAuth } from '@core/stores/authStore'
import { clinicMedicineService, type ClinicMedicine } from '@core/services/clinicMedicineService'
import { useDebouncedValue } from '@core/hooks/useDebouncedValue'
import { UserRole } from '@core/types'
import dayjs from 'dayjs'

const { Text } = Typography

const { TextArea } = Input
const { Option } = Select

const DISPENSING_FORMS = [
  { value: 1, label: 'Globules' },
  { value: 2, label: 'Tablets' },
  { value: 3, label: 'Packet' },
  { value: 4, label: 'Liquid' },
  { value: 5, label: 'Tonic' }
]

const LIQUID_QUANTITIES = [
  { value: 10, label: '10 ml' },
  { value: 15, label: '15 ml' }
]

const TONIC_QUANTITIES = [
  { value: 50, label: '50 ml' },
  { value: 100, label: '100 ml' }
]

const FREQUENCIES = [
  'Daily once',
  'Daily 2 times',
  'Daily 3 times',
  'Daily 4 times',
  'Weekly once',
  'Weekly twice',
  'Every 6 hours',
  'Every 8 hours',
  'Every 12 hours',
  'As needed'
]

const TIMINGS = [
  'Before food',
  'After food',
  'Before brushing',
  'After brushing',
  'On empty stomach',
  'At bedtime',
  'As directed'
]

export const PrescriptionFormPage = () => {
  const [form] = Form.useForm()
  const [medicineForm] = Form.useForm()
  const navigate = useNavigate()
  const [searchParams] = useSearchParams()
  const { id } = useParams<{ id: string }>()
  const { user } = useAuth()
  const queryClient = useQueryClient()
  
  const isEditMode = !!id
  const prescriptionId = isEditMode ? parseInt(id!) : null
  const consultationId = searchParams.get('consultationId')
  const patientId = searchParams.get('patientId')

  const [medicines, setMedicines] = useState<PrescriptionMedicine[]>([])
  const [isMedicineModalOpen, setIsMedicineModalOpen] = useState(false)
  const [medicineSearchTerm, setMedicineSearchTerm] = useState('')
  const [selectedPrescriptionId, setSelectedPrescriptionId] = useState<number | null>(null)
  const debouncedSearchTerm = useDebouncedValue(medicineSearchTerm, 300)

  // Load prescription data for edit mode
  const { data: existingPrescription, isLoading: isLoadingPrescription } = useQuery({
    queryKey: ['prescription', prescriptionId],
    queryFn: () => prescriptionService.getById(prescriptionId!),
    enabled: isEditMode && !!prescriptionId
  })
  
  // Fetch patient prescriptions for doctors in create mode
  const currentPatientId = isEditMode ? existingPrescription?.patientId : parseInt(patientId || '0')
  const { data: patientPrescriptions = [], isLoading: isLoadingPatientPrescriptions } = useQuery<Prescription[]>({
    queryKey: ['patient-prescriptions', currentPatientId],
    queryFn: () => prescriptionService.getByPatient(currentPatientId || 0),
    enabled: !isEditMode && !!currentPatientId && currentPatientId > 0 && (user?.role === UserRole.Doctor || user?.role === UserRole.Admin)
  })
  
  // Get last 5 prescriptions (already sorted descending by backend)
  const last5Prescriptions = patientPrescriptions.slice(0, 5)
  
  // Fetch selected prescription details
  const { data: selectedPrescription, isLoading: isLoadingSelectedPrescription } = useQuery<Prescription>({
    queryKey: ['prescription', selectedPrescriptionId],
    queryFn: () => prescriptionService.getById(selectedPrescriptionId!),
    enabled: !!selectedPrescriptionId && selectedPrescriptionId > 0
  })
  
  // Show previous prescriptions only for doctors/admins in create mode
  const showPreviousPrescriptions = !isEditMode && (user?.role === UserRole.Doctor || user?.role === UserRole.Admin) && currentPatientId > 0 && last5Prescriptions.length > 0

  // Search clinic medicines
  const { data: clinicMedicines = [], isLoading: isSearchingMedicines } = useQuery({
    queryKey: ['clinic-medicines', debouncedSearchTerm],
    queryFn: () => clinicMedicineService.search(debouncedSearchTerm || undefined),
    enabled: isMedicineModalOpen && debouncedSearchTerm.length >= 2 // Only search when modal is open and user has typed at least 2 characters
  })

  const createMutation = useMutation({
    mutationFn: prescriptionService.create,
    onSuccess: (data) => {
      message.success(`Prescription saved successfully! Number: ${data.prescriptionNumber}`)
      // Refresh previous prescriptions list
      if (currentPatientId && currentPatientId > 0) {
        queryClient.invalidateQueries({ queryKey: ['patient-prescriptions', currentPatientId] })
      }
      // Navigate to invoice form with prescription ID
      navigate(`/invoices/new?prescriptionId=${data.id}`)
    },
    onError: () => {
      message.error('Failed to create prescription')
    }
  })


  // Populate form when prescription data is loaded
  useEffect(() => {
    if (isEditMode && existingPrescription) {
      form.setFieldsValue({
        notes: existingPrescription.notes
      })
      setMedicines(existingPrescription.medicines || [])
    }
  }, [isEditMode, existingPrescription, form])

  const updateMutation = useMutation({
    mutationFn: prescriptionService.update,
    onSuccess: (data) => {
      message.success(`Prescription updated successfully! Number: ${data.prescriptionNumber}`)
      navigate(`/prescriptions/${data.id}`)
    },
    onError: () => {
      message.error('Failed to update prescription')
    }
  })


  // Calculate times per day from frequency string
  const getTimesPerDay = (frequency: string): number => {
    const freqLower = frequency.toLowerCase()
    if (freqLower.includes('daily once')) return 1
    if (freqLower.includes('daily 2 times')) return 2
    if (freqLower.includes('daily 3 times')) return 3
    if (freqLower.includes('daily 4 times')) return 4
    if (freqLower.includes('weekly once')) return 1 / 7
    if (freqLower.includes('weekly twice')) return 2 / 7
    if (freqLower.includes('every 6 hours')) return 4
    if (freqLower.includes('every 8 hours')) return 3
    if (freqLower.includes('every 12 hours')) return 2
    if (freqLower.includes('as needed')) return 1 // Default for "as needed"
    return 1 // Default fallback
  }

  // Parse duration string to days (e.g., "4 weeks" = 28 days, "2 days" = 2 days)
  const parseDurationToDays = (duration: string): number => {
    if (!duration) return 0
    const durationLower = duration.toLowerCase().trim()
    const weekMatch = durationLower.match(/(\d+)\s*weeks?/)
    if (weekMatch) {
      return parseInt(weekMatch[1]) * 7
    }
    const dayMatch = durationLower.match(/(\d+)\s*days?/)
    if (dayMatch) {
      return parseInt(dayMatch[1])
    }
    return 0
  }

  // Format duration from number and unit to string (e.g., 4, "weeks" -> "4 weeks")
  const formatDuration = (value: number, unit: string): string => {
    if (!value || value <= 0) return ''
    return `${value} ${unit}`
  }

  // Auto-calculate quantity based on duration and frequency
  const calculateQuantity = (duration: string, frequency: string): number => {
    const days = parseDurationToDays(duration)
    const timesPerDay = getTimesPerDay(frequency)
    const calculated = Math.ceil(days * timesPerDay)
    return calculated > 0 ? calculated : 1 // Minimum 1
  }

  // Auto-set dosage based on dispensing form
  const handleDispensingFormChange = (formValue: number) => {
    let autoDosage = ''
    if (formValue === 1) { // Globules
      autoDosage = '4 pills per dose'
      // Set default quantity to 1 for globules
      medicineForm.setFieldsValue({ dosage: autoDosage, quantity: 1 })
    } else if (formValue === 2) { // Tablets
      autoDosage = '1 tablet per dose'
      // Auto-calculate quantity if duration and frequency are set
      const durationValue = medicineForm.getFieldValue('durationValue')
      const durationUnit = medicineForm.getFieldValue('durationUnit')
      const frequency = medicineForm.getFieldValue('frequency')
      if (durationValue && durationUnit && frequency) {
        const duration = formatDuration(durationValue, durationUnit)
        const calculatedQty = calculateQuantity(duration, frequency)
        medicineForm.setFieldsValue({ dosage: autoDosage, quantity: calculatedQty })
      } else {
        medicineForm.setFieldsValue({ dosage: autoDosage })
      }
    } else if (formValue === 3) { // Packet
      autoDosage = '1 packet per dose'
      // Auto-calculate quantity if duration and frequency are set
      const durationValue = medicineForm.getFieldValue('durationValue')
      const durationUnit = medicineForm.getFieldValue('durationUnit')
      const frequency = medicineForm.getFieldValue('frequency')
      if (durationValue && durationUnit && frequency) {
        const duration = formatDuration(durationValue, durationUnit)
        const calculatedQty = calculateQuantity(duration, frequency)
        medicineForm.setFieldsValue({ dosage: autoDosage, quantity: calculatedQty })
      } else {
        medicineForm.setFieldsValue({ dosage: autoDosage })
      }
    } else if (formValue === 4) { // Liquid
      autoDosage = '4 Drops'
      // Clear quantity - will be selected from dropdown
      medicineForm.setFieldsValue({ dosage: autoDosage, quantity: undefined })
    } else if (formValue === 5) { // Tonic
      autoDosage = '5 ml'
      // Clear quantity - will be selected from dropdown
      medicineForm.setFieldsValue({ dosage: autoDosage, quantity: undefined })
    }
  }

  // Handle duration or frequency change to auto-calculate quantity
  const handleDurationOrFrequencyChange = () => {
    const dispensingForm = medicineForm.getFieldValue('dispensingForm')
    const durationValue = medicineForm.getFieldValue('durationValue')
    const durationUnit = medicineForm.getFieldValue('durationUnit')
    const frequency = medicineForm.getFieldValue('frequency')
    
    // Format duration string for calculation
    const duration = formatDuration(durationValue, durationUnit)
    
    // Only auto-calculate for Tablets and Packets (not Globules)
    if ((dispensingForm === 2 || dispensingForm === 3) && durationValue && durationUnit && frequency) {
      const calculatedQty = calculateQuantity(duration, frequency)
      medicineForm.setFieldsValue({ quantity: calculatedQty })
    }
  }

  // Calculate dispensed quantity based on dispensing form
  const calculateDispensedQuantity = (dispensingForm: number, quantity: number, containerSize?: number): number => {
    switch (dispensingForm) {
      case 1: // Globules
        // quantity (containers) × containerSize (drams) × 4 drops/dram = drops
        return quantity * (containerSize || 1) * 4
      case 4: // Liquid
        // Dispensed Qty in ml (same as prescribed quantity)
        return quantity
      case 5: // Tonic
        // Dispensed Qty same as prescribed quantity
        return quantity
      case 2: // Tablets
        // Dispensed Qty same as prescribed quantity
        return quantity
      case 3: // Packets
        // 1 Packet = 5 drops
        return quantity * 5
      default:
        return quantity
    }
  }

  const handleAddMedicine = async () => {
    try {
      const values = await medicineForm.validateFields()
      
      // Format duration from value and unit
      const duration = formatDuration(values.durationValue, values.durationUnit)
      
      const quantity = values.quantity || 1
      const containerSize = values.dispensingForm === 1 ? values.containerSize : undefined
      
      const medicine: PrescriptionMedicine = {
        medicineId: values.medicineId || 0, // Set from selected medicine, or 0 for custom
        medicineName: values.medicineName,
        dispensingForm: values.dispensingForm,
        dosage: values.dosage,
        frequency: values.frequency,
        duration: duration, // Formatted as "4 weeks" or "7 days"
        timing: values.timing || '',
        containerSize: containerSize, // Only for Globules
        quantity: quantity, // Prescribed quantity for patient
        dispensedQuantity: calculateDispensedQuantity(values.dispensingForm, quantity, containerSize), // Internal: for inventory
        instructions: values.instructions
      }

      setMedicines([...medicines, medicine])
      setIsMedicineModalOpen(false)
      medicineForm.resetFields()
      // Reset quantity to default 1 for next medicine entry
      medicineForm.setFieldsValue({ quantity: 1 })
    } catch (error) {
      console.error('Validation failed:', error)
    }
  }

  const handleRemoveMedicine = (index: number) => {
    setMedicines(medicines.filter((_, i) => i !== index))
  }
  
  const handleViewPrescription = (prescriptionId: number) => {
    setSelectedPrescriptionId(prescriptionId)
  }
  
  const handleClonePrescription = () => {
    if (!selectedPrescription) return
    
    // Clone medicines
    const clonedMedicines: PrescriptionMedicine[] = selectedPrescription.medicines.map(med => ({
      medicineId: med.medicineId || 0,
      medicineName: med.medicineName,
      dispensingForm: med.dispensingForm,
      dosage: med.dosage,
      frequency: med.frequency,
      duration: med.duration,
      timing: med.timing,
      containerSize: med.containerSize,
      quantity: med.quantity,
      dispensedQuantity: med.dispensedQuantity,
      instructions: med.instructions
    }))
    
    setMedicines(clonedMedicines)
    form.setFieldsValue({
      notes: selectedPrescription.notes || ''
    })
    
    message.success('Prescription cloned successfully')
    setSelectedPrescriptionId(null) // Close modal after cloning
  }

  const handleSubmit = async () => {
    try {
      if (medicines.length === 0) {
        message.error('Please add at least one medicine')
        return
      }

      const values = await form.validateFields()
      
      if (isEditMode && prescriptionId) {
        // Update existing prescription
        const updateData: UpdatePrescriptionRequest = {
          id: prescriptionId,
          medicines: medicines,
          notes: values.notes
        }
        updateMutation.mutate(updateData)
      } else {
        // Create new prescription
        if (!consultationId || !patientId) {
          message.error('Consultation ID and Patient ID are required to create a prescription')
          return
        }
        
        const prescriptionData: CreatePrescriptionRequest = {
          consultationId: parseInt(consultationId),
          patientId: parseInt(patientId),
          doctorId: user?.id || 0,
          medicines: medicines,
          notes: values.notes
        }

        createMutation.mutate(prescriptionData)
      }
    } catch (error) {
      console.error('Validation failed:', error)
    }
  }

  if (!isEditMode && (!consultationId || !patientId)) {
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

  const getDispensingFormLabel = (form: number) => {
    const formObj = DISPENSING_FORMS.find(f => f.value === form)
    return formObj?.label || 'Unknown'
  }

  const formatDosageForDisplay = (dosage: string) => {
    // Convert "4 pills per dose" to "4 pills", "1 tablet per dose" to "1 tablet", etc.
    return dosage.replace(' per dose', '')
  }

  const formatQuantityForDisplay = (quantity: number | undefined, dispensingForm: number): string => {
    if (quantity === undefined) return ''
    if (dispensingForm === 4) { // Liquid
      return `${quantity} ml`
    }
    if (dispensingForm === 5) { // Tonic
      return `${quantity} ml`
    }
    return quantity.toString()
  }

  const medicineColumns = [
    {
      title: 'Medicine',
      key: 'medicine',
      width: 300,
      render: (_: any, record: PrescriptionMedicine) => {
        const formLabel = getDispensingFormLabel(record.dispensingForm)
        const containerInfo = record.dispensingForm === 1 && record.containerSize 
          ? ` (${record.containerSize} dram)`
          : ''
        const quantityInfo = record.dispensingForm === 4 || record.dispensingForm === 5
          ? ` (${formatQuantityForDisplay(record.quantity, record.dispensingForm)})`
          : ''
        const displayDosage = formatDosageForDisplay(record.dosage)
        return (
          <div>
            <div style={{ fontWeight: 500, marginBottom: '4px' }}>
              {record.medicineName} – {formLabel}{containerInfo}{quantityInfo}
            </div>
            <div style={{ fontSize: '13px', color: '#666', marginTop: '4px', lineHeight: '1.5' }}>
              Take {displayDosage}, {record.frequency}, {record.timing}
            </div>
            <div style={{ fontSize: '13px', color: '#666', marginTop: '2px' }}>
              Duration: {record.duration}
            </div>
          </div>
        )
      }
    },
    {
      title: 'Quantity',
      dataIndex: 'quantity',
      key: 'quantity',
      width: 100,
      render: (quantity: number | undefined, record: PrescriptionMedicine) => {
        if (quantity === undefined) return '-'
        if (record.dispensingForm === 4 || record.dispensingForm === 5) {
          return `${quantity} ml`
        }
        return quantity.toString()
      }
    },
    {
      title: 'Dispensed Qty',
      dataIndex: 'dispensedQuantity',
      key: 'dispensedQuantity',
      width: 120,
      render: (dispensedQuantity: number | undefined, record: PrescriptionMedicine) => {
        if (dispensedQuantity === undefined) return '-'
        // Globules: show in drops
        if (record.dispensingForm === 1) {
          return `${Math.round(dispensedQuantity)} drops`
        }
        // Packets: show in drops (1 Packet = 5 drops)
        if (record.dispensingForm === 3) {
          return `${Math.round(dispensedQuantity)} drops`
        }
        // Liquid: show in ml
        if (record.dispensingForm === 4) {
          return `${dispensedQuantity} ml`
        }
        // Tonic, Tablets: show as count (same as prescribed quantity)
        return Math.round(dispensedQuantity).toString()
      }
    },
    {
      title: 'Instructions',
      dataIndex: 'instructions',
      key: 'instructions',
      ellipsis: true,
      render: (instructions: string) => instructions || '-'
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 100,
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
      {isLoadingPrescription ? (
        <Card>
          <div style={{ textAlign: 'center', padding: '50px' }}>
            Loading prescription...
          </div>
        </Card>
      ) : (
        <>
      <div style={{ marginBottom: '24px' }}>
        <Button
          icon={<ArrowLeftOutlined />}
          onClick={() => navigate(-1)}
          style={{ marginBottom: '16px' }}
        >
          Back
        </Button>
        <h1 style={{ margin: 0, fontSize: '24px', fontWeight: 600 }}>
          {isEditMode ? 'Edit Prescription' : 'Create Prescription'}
        </h1>
        <p style={{ margin: '8px 0 0 0', color: '#666' }}>
          {isEditMode ? 'Update medicines and instructions for the patient' : 'Add medicines and instructions for the patient'}
        </p>
      </div>

      {/* Previous Prescriptions - Only for doctors/admins in create mode */}
      {showPreviousPrescriptions && (
        <Card 
          title="Previous Prescriptions" 
          style={{ marginBottom: 24 }}
          extra={<Text type="secondary">Last 5 prescriptions</Text>}
        >
          <Table
            dataSource={last5Prescriptions}
            rowKey="id"
            pagination={false}
            size="small"
            loading={isLoadingPatientPrescriptions}
            columns={[
              {
                title: 'Date',
                dataIndex: 'prescriptionDate',
                key: 'prescriptionDate',
                width: 120,
                render: (date: string) => dayjs(date).format('MMM DD, YYYY'),
              },
              {
                title: 'Prescription Number',
                dataIndex: 'prescriptionNumber',
                key: 'prescriptionNumber',
                width: 150,
                render: (text: string) => <Tag color="blue">{text}</Tag>,
              },
              {
                title: 'Medicines',
                key: 'medicines',
                render: (_: any, record: Prescription) => `${record.medicines?.length || 0} medicine(s)`,
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
                render: (_: any, record: Prescription) => (
                  <Button
                    type="link"
                    size="small"
                    onClick={() => handleViewPrescription(record.id)}
                  >
                    View
                  </Button>
                ),
              },
            ]}
          />
        </Card>
      )}

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
              {isEditMode ? (
                <>
                  <Button
                    type="primary"
                    icon={<SaveOutlined />}
                    onClick={handleSubmit}
                    loading={updateMutation.isPending}
                    size="large"
                    disabled={medicines.length === 0 || isLoadingPrescription}
                  >
                    Update Prescription
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
                    icon={<SaveOutlined />}
                    onClick={handleSubmit}
                    loading={createMutation.isPending}
                    size="large"
                    disabled={medicines.length === 0}
                  >
                    Save & Create Invoice
                  </Button>
                  <Button
                    onClick={() => navigate(-1)}
                    size="large"
                  >
                    Cancel
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
          setMedicineSearchTerm('')
        }}
        width={700}
        okText="Add Medicine"
      >
        <Form
          form={medicineForm}
          layout="vertical"
          style={{ marginTop: '24px' }}
        >
          <Form.Item
            label="Medicine Name"
            name="medicineName"
            rules={[{ required: true, message: 'Please enter or select medicine name' }]}
          >
            <AutoComplete
              placeholder="Search or type medicine name (e.g., Nux Vomica)"
              options={clinicMedicines.map(med => ({
                value: med.name,
                label: (
                  <div>
                    <div style={{ fontWeight: 500 }}>{med.name}</div>
                    {med.genericName && (
                      <div style={{ fontSize: '12px', color: '#666' }}>{med.genericName}</div>
                    )}
                    {med.manufacturer && (
                      <div style={{ fontSize: '12px', color: '#999' }}>{med.manufacturer}</div>
                    )}
                  </div>
                ),
                medicine: med // Store the full medicine object
              }))}
              onSearch={setMedicineSearchTerm}
              onSelect={(value, option) => {
                const selectedMedicine = (option as any).medicine as ClinicMedicine
                if (selectedMedicine) {
                  medicineForm.setFieldsValue({
                    medicineName: selectedMedicine.name,
                    medicineId: selectedMedicine.id
                  })
                }
              }}
              filterOption={false} // We're doing server-side filtering
              loading={isSearchingMedicines}
              notFoundContent={debouncedSearchTerm.length < 2 ? 'Type at least 2 characters to search' : isSearchingMedicines ? 'Searching...' : 'No medicines found'}
              allowClear
            />
          </Form.Item>
          
          {/* Hidden field to store medicineId */}
          <Form.Item name="medicineId" hidden>
            <Input type="hidden" />
          </Form.Item>

          <Form.Item
            noStyle
            shouldUpdate={(prevValues, currentValues) => prevValues.dispensingForm !== currentValues.dispensingForm}
          >
            {({ getFieldValue }) => {
              const dispensingForm = getFieldValue('dispensingForm')
              const isGlobules = dispensingForm === 1
              
              return (
                <div style={{ display: 'grid', gridTemplateColumns: isGlobules ? '1fr 1fr' : '1fr', gap: '16px' }}>
                  <Form.Item
                    label="Dispensing Form"
                    name="dispensingForm"
                    rules={[{ required: true, message: 'Please select dispensing form' }]}
                  >
                    <Select 
                      placeholder="Select dispensing form"
                      onChange={handleDispensingFormChange}
                    >
                      {DISPENSING_FORMS.map(form => (
                        <Option key={form.value} value={form.value}>{form.label}</Option>
                      ))}
                    </Select>
                  </Form.Item>

                  {isGlobules && (
                    <Form.Item
                      label="Container Size"
                      name="containerSize"
                      rules={[{ required: true, message: 'Please select container size' }]}
                    >
                      <Select placeholder="Select dram size">
                        <Option value={1}>1 dram</Option>
                        <Option value={2}>2 dram</Option>
                        <Option value={3}>3 dram</Option>
                      </Select>
                    </Form.Item>
                  )}
                </div>
              )
            }}
          </Form.Item>

          <Form.Item
            label="Dosage (auto-set)"
            name="dosage"
            rules={[{ required: true, message: 'Dosage is required' }]}
          >
            <Input 
              placeholder="Will be auto-set based on dispensing form"
              readOnly
              style={{ backgroundColor: '#f5f5f5' }}
            />
          </Form.Item>

          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '16px' }}>
            <Form.Item
              label="Frequency"
              name="frequency"
              rules={[{ required: true, message: 'Please select frequency' }]}
            >
              <Select 
                placeholder="Select frequency"
                onChange={() => handleDurationOrFrequencyChange()}
              >
                {FREQUENCIES.map(freq => (
                  <Option key={freq} value={freq}>{freq}</Option>
                ))}
              </Select>
            </Form.Item>

            <Form.Item
              label="Timing"
              name="timing"
              rules={[{ required: true, message: 'Please select timing' }]}
            >
              <Select placeholder="Select timing">
                {TIMINGS.map(timing => (
                  <Option key={timing} value={timing}>{timing}</Option>
                ))}
              </Select>
            </Form.Item>
          </div>

          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '16px' }}>
            <Form.Item
              label="Duration"
              required
              rules={[{ required: true, message: 'Please enter duration' }]}
            >
              <Input.Group compact>
                <Form.Item
                  name="durationValue"
                  noStyle
                  rules={[{ required: true, message: 'Required' }]}
                >
                  <InputNumber
                    min={1}
                    style={{ width: '60%' }}
                    placeholder="Number"
                    onChange={() => handleDurationOrFrequencyChange()}
                  />
                </Form.Item>
                <Form.Item
                  name="durationUnit"
                  noStyle
                  rules={[{ required: true, message: 'Required' }]}
                >
                  <Select
                    style={{ width: '40%' }}
                    placeholder="Unit"
                    onChange={() => handleDurationOrFrequencyChange()}
                  >
                    <Option value="days">days</Option>
                    <Option value="weeks">weeks</Option>
                  </Select>
                </Form.Item>
              </Input.Group>
            </Form.Item>

            <Form.Item
              noStyle
              shouldUpdate={(prevValues, currentValues) => prevValues.dispensingForm !== currentValues.dispensingForm}
            >
              {({ getFieldValue }) => {
                const dispensingForm = getFieldValue('dispensingForm')
                // Show quantity dropdown for Liquid
                if (dispensingForm === 4) {
                  return (
                    <Form.Item
                      label="Quantity"
                      name="quantity"
                      rules={[{ required: true, message: 'Please select quantity' }]}
                    >
                      <Select placeholder="Select quantity">
                        {LIQUID_QUANTITIES.map(qty => (
                          <Option key={qty.value} value={qty.value}>{qty.label}</Option>
                        ))}
                      </Select>
                    </Form.Item>
                  )
                }
                // Show quantity dropdown for Tonic
                if (dispensingForm === 5) {
                  return (
                    <Form.Item
                      label="Quantity"
                      name="quantity"
                      rules={[{ required: true, message: 'Please select quantity' }]}
                    >
                      <Select placeholder="Select quantity">
                        {TONIC_QUANTITIES.map(qty => (
                          <Option key={qty.value} value={qty.value}>{qty.label}</Option>
                        ))}
                      </Select>
                    </Form.Item>
                  )
                }
                // Show editable quantity for Globules (default 1)
                if (dispensingForm === 1) {
                  return (
                    <Form.Item
                      label="Quantity"
                      name="quantity"
                      rules={[{ required: true, message: 'Please enter quantity' }]}
                      initialValue={1}
                    >
                      <InputNumber
                        min={1}
                        style={{ width: '100%' }}
                        placeholder="Enter quantity (default: 1)"
                      />
                    </Form.Item>
                  )
                }
                // Show auto-calculated quantity for Tablets and Packets
                if (dispensingForm === 2 || dispensingForm === 3) {
                  return (
                    <Form.Item
                      label="Quantity (auto-calculated, editable)"
                      name="quantity"
                      rules={[{ required: true, message: 'Please enter quantity' }]}
                      tooltip="Quantity is auto-calculated based on duration and frequency. You can edit it if needed."
                    >
                      <InputNumber
                        min={1}
                        style={{ width: '100%' }}
                        placeholder="Auto-calculated based on duration and frequency"
                      />
                    </Form.Item>
                  )
                }
                return null
              }}
            </Form.Item>
          </div>

          <Form.Item
            label="Special Instructions"
            name="instructions"
          >
            <TextArea
              rows={2}
              placeholder="Additional instructions (optional)"
            />
          </Form.Item>
        </Form>
        </Modal>

      {/* Prescription Details Modal */}
      <Modal
        title={
          <Space>
            <Text strong>Previous Prescription Details</Text>
            {selectedPrescription && (
              <Text type="secondary" style={{ fontSize: '12px' }}>
                {dayjs(selectedPrescription.prescriptionDate).format('MMMM DD, YYYY')}
              </Text>
            )}
          </Space>
        }
        open={!!selectedPrescriptionId}
        onCancel={() => setSelectedPrescriptionId(null)}
        footer={[
          <Button key="close" onClick={() => setSelectedPrescriptionId(null)}>
            Close
          </Button>,
          <Button 
            key="clone" 
            type="primary" 
            onClick={handleClonePrescription}
            disabled={!selectedPrescription}
          >
            Clone
          </Button>
        ]}
        width={900}
      >
        {isLoadingSelectedPrescription ? (
          <div style={{ textAlign: 'center', padding: '40px' }}>
            <Spin size="large" />
          </div>
        ) : selectedPrescription ? (
          <div>
            {/* Prescription Details */}
            <Card size="small" style={{ marginBottom: 16 }}>
              <Descriptions column={2} size="small" bordered>
                <Descriptions.Item label="Prescription Number">
                  <Tag color="blue">{selectedPrescription.prescriptionNumber}</Tag>
                </Descriptions.Item>
                <Descriptions.Item label="Date">
                  {dayjs(selectedPrescription.prescriptionDate).format('MMMM DD, YYYY [at] hh:mm A')}
                </Descriptions.Item>
                <Descriptions.Item label="Doctor">
                  {selectedPrescription.doctorName}
                </Descriptions.Item>
                <Descriptions.Item label="Patient">
                  {selectedPrescription.patientName}
                </Descriptions.Item>
                {selectedPrescription.notes && (
                  <Descriptions.Item label="Notes" span={2}>
                    <Text style={{ whiteSpace: 'pre-wrap' }}>{selectedPrescription.notes}</Text>
                  </Descriptions.Item>
                )}
              </Descriptions>
            </Card>

            {/* Medicines Table */}
            {selectedPrescription.medicines && selectedPrescription.medicines.length > 0 && (
              <Card size="small" title="Medicines">
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
                      render: (_: any, record: PrescriptionMedicine) => (
                        <div>
                          <Text strong>{record.medicineName}</Text>
                          <div style={{ fontSize: '11px', color: '#999' }}>
                            {getDispensingFormLabel(record.dispensingForm)}
                            {record.containerSize && record.dispensingForm === 1 && ` (${record.containerSize} dram)`}
                          </div>
                        </div>
                      )
                    },
                    {
                      title: 'Dosage',
                      dataIndex: 'dosage',
                      key: 'dosage',
                      width: 120
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
                      title: 'Quantity',
                      dataIndex: 'quantity',
                      key: 'quantity',
                      width: 100,
                      render: (quantity: number | undefined, record: PrescriptionMedicine) => {
                        if (quantity === undefined) return '-'
                        if (record.dispensingForm === 4 || record.dispensingForm === 5) {
                          return `${quantity} ml`
                        }
                        return quantity.toString()
                      }
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
              </Card>
            )}
          </div>
        ) : (
          <div>Prescription not found</div>
        )}
      </Modal>
        </>
      )}
    </div>
  )
}

