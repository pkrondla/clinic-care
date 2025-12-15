import { useState, useEffect } from 'react'
import { useNavigate, useParams, useSearchParams } from 'react-router-dom'
import {
  Card,
  Form,
  Input,
  InputNumber,
  Button,
  message,
  Space,
  Table,
  Select,
  DatePicker,
  Row,
  Col,
  AutoComplete,
  Modal,
} from 'antd'
import { SaveOutlined, ArrowLeftOutlined, PlusOutlined, DeleteOutlined } from '@ant-design/icons'
import { useInvoice, useCreateInvoice, useUpdateInvoice } from '@core/hooks/queries/useInvoices'
import { useClinics } from '@core/hooks/queries/useClinics'
import { useSearchPatients } from '@core/hooks/queries/usePatients'
import { useDebouncedValue } from '@core/hooks/useDebouncedValue'
import { useQuery } from '@tanstack/react-query'
import type { InvoiceItemRequest, InvoiceItemUpdateRequest, InvoicePreparation } from '@core/services/invoiceService'
import invoiceService from '@core/services/invoiceService'
import type { PatientSearch } from '@core/services/patientService'
import { clinicMedicineService, type ClinicMedicineSearch } from '@core/services/clinicMedicineService'
import { useSelectedClinic } from '@core/stores/authStore'
import dayjs from 'dayjs'

const { Option } = Select
const { TextArea } = Input

const ITEM_TYPES = [
  { value: 'Consultation', label: 'Consultation' },
  { value: 'Medicine', label: 'Medicine' },
  { value: 'Courier', label: 'Courier' },
  { value: 'Other', label: 'Other' },
]

const INVOICE_STATUSES = [
  { value: 1, label: 'Draft' },
  { value: 2, label: 'Sent' },
  { value: 3, label: 'Paid' },
  { value: 4, label: 'Cancelled' },
]

export const InvoiceFormPage = () => {
  const { id } = useParams<{ id: string }>()
  const [searchParams] = useSearchParams()
  const navigate = useNavigate()
  const isEditMode = !!id
  const prescriptionIdParam = searchParams.get('prescriptionId')
  const prescriptionId = prescriptionIdParam ? parseInt(prescriptionIdParam) : null
  
  const [form] = Form.useForm()
  const [itemForm] = Form.useForm()
  const [items, setItems] = useState<InvoiceItemRequest[]>([])
  const [isItemModalOpen, setIsItemModalOpen] = useState(false)
  const [editingItemIndex, setEditingItemIndex] = useState<number | null>(null)

  const { data: invoice, isLoading: isLoadingInvoice } = useInvoice(Number(id!))
  const { data: clinics } = useClinics()
  const createMutation = useCreateInvoice()
  const updateMutation = useUpdateInvoice()

  // Load invoice preparation data from prescription
  const { data: invoicePreparation, isLoading: isLoadingPreparation } = useQuery<InvoicePreparation>({
    queryKey: ['invoice-preparation', prescriptionId],
    queryFn: () => invoiceService.prepareInvoiceFromPrescription(prescriptionId!),
    enabled: !isEditMode && !!prescriptionId && prescriptionId > 0,
  })

  // Patient search
  const [patientSearchTerm, setPatientSearchTerm] = useState('')
  const [selectedPatient, setSelectedPatient] = useState<PatientSearch | null>(null)
  const debouncedPatientSearch = useDebouncedValue(patientSearchTerm, 300)
  const { data: patientSearchResults = [], isLoading: isSearchingPatients } = useSearchPatients({
    searchTerm: debouncedPatientSearch,
    limit: 10,
  })

  // Medicine search for invoice items
  const [medicineSearchTerm, setMedicineSearchTerm] = useState('')
  const [selectedMedicine, setSelectedMedicine] = useState<ClinicMedicineSearch | null>(null)
  const debouncedMedicineSearch = useDebouncedValue(medicineSearchTerm, 300)
  const selectedClinic = useSelectedClinic()
  const { data: medicineSearchResults = [], isLoading: isSearchingMedicines } = useQuery<ClinicMedicineSearch[]>({
    queryKey: ['clinic-medicines-search', debouncedMedicineSearch, selectedClinic?.id],
    queryFn: () => clinicMedicineService.search(debouncedMedicineSearch),
    enabled: isItemModalOpen && debouncedMedicineSearch.length >= 2 && !!selectedClinic?.id,
  })

  // Load invoice data in edit mode
  useEffect(() => {
    if (isEditMode && invoice) {
      form.setFieldsValue({
        clinicId: invoice.clinicId,
        patientId: invoice.patientId,
        consultationId: invoice.consultationId,
        prescriptionId: invoice.prescriptionId,
        invoiceDate: invoice.invoiceDate ? dayjs(invoice.invoiceDate) : undefined,
        status: invoice.status,
      })
      setItems(
        invoice.items.map((item) => ({
          itemType: item.itemType,
          description: item.description,
          quantity: item.quantity,
          unitPrice: item.unitPrice,
        }))
      )
      // Set selected patient for display
      if (invoice.patientId) {
        setSelectedPatient({
          id: invoice.patientId,
          name: invoice.patientName,
          patientCode: invoice.patientCode,
        } as PatientSearch)
      }
    }
  }, [isEditMode, invoice, form])

  // Load invoice preparation data from prescription
  useEffect(() => {
    if (!isEditMode && invoicePreparation) {
      form.setFieldsValue({
        clinicId: invoicePreparation.clinicId,
        consultationId: invoicePreparation.consultationId,
        prescriptionId: invoicePreparation.prescriptionId,
        invoiceDate: dayjs(),
      })
      setItems(invoicePreparation.items)
      // Set selected patient for display
      setSelectedPatient({
        id: invoicePreparation.patientId,
        name: invoicePreparation.patientName,
        patientCode: invoicePreparation.patientCode,
      } as PatientSearch)
      message.info('Invoice details loaded from prescription. You can make changes before saving.')
    }
  }, [invoicePreparation, isEditMode, form])

  const handleAddItem = () => {
    itemForm
      .validateFields()
      .then((values) => {
        const newItem: InvoiceItemRequest = {
          itemType: values.itemType,
          description: values.description,
          quantity: values.quantity,
          unitPrice: values.unitPrice,
          medicineId: values.medicineId, // Include medicineId for stock reduction
        }

        if (editingItemIndex !== null) {
          // Update existing item
          const updatedItems = [...items]
          updatedItems[editingItemIndex] = newItem
          setItems(updatedItems)
          setEditingItemIndex(null)
        } else {
          // Add new item
          setItems([...items, newItem])
        }

        itemForm.resetFields()
        setSelectedMedicine(null)
        setMedicineSearchTerm('')
        setIsItemModalOpen(false)
      })
      .catch(() => {
        // Validation failed
      })
  }

  const handleEditItem = async (index: number) => {
    const item = items[index]
    itemForm.setFieldsValue(item)
    
    // If it's a medicine item with medicineId, load medicine details
    if (item.itemType === 'Medicine' && item.medicineId) {
      try {
        const medicine = await clinicMedicineService.getById(item.medicineId)
        setSelectedMedicine({
          id: medicine.id,
          name: medicine.name,
          genericName: medicine.genericName,
          manufacturer: medicine.manufacturer,
          type: medicine.type,
          potency: medicine.potency,
        })
        setMedicineSearchTerm(medicine.name)
      } catch (error) {
        console.error('Failed to load medicine details:', error)
      }
    } else {
      setSelectedMedicine(null)
      setMedicineSearchTerm('')
    }
    
    setEditingItemIndex(index)
    setIsItemModalOpen(true)
  }

  const handleDeleteItem = (index: number) => {
    const updatedItems = items.filter((_, i) => i !== index)
    setItems(updatedItems)
  }

  const handleSubmit = async (values: any) => {
    if (items.length === 0) {
      message.error('Please add at least one invoice item')
      return
    }

    if (!selectedPatient) {
      message.error('Please select a patient')
      return
    }

    try {
      if (isEditMode) {
        // Map items with their IDs from the original invoice
        const itemsWithIds: InvoiceItemUpdateRequest[] = items.map((item, index) => {
          // Try to find matching item by index first, then by description and type
          const originalItem = invoice?.items.find(
            (orig, idx) => idx === index || 
            (orig.description === item.description && orig.itemType === item.itemType)
          )
          return {
            id: originalItem?.id,
            ...item,
          }
        })

        const updateData = {
          id: Number(id!),
          clinicId: values.clinicId,
          patientId: selectedPatient.id,
          consultationId: values.consultationId,
          items: itemsWithIds,
          invoiceDate: values.invoiceDate ? values.invoiceDate.toISOString() : undefined,
          status: values.status,
        }

        await updateMutation.mutateAsync(updateData)
        message.success('Invoice updated successfully')
      } else {
        const createData = {
          clinicId: values.clinicId,
          patientId: selectedPatient.id,
          consultationId: values.consultationId,
          prescriptionId: values.prescriptionId,
          items: items,
          invoiceDate: values.invoiceDate ? values.invoiceDate.toISOString() : undefined,
        }

        const createdInvoice = await createMutation.mutateAsync(createData)
        message.success('Invoice created successfully')
        navigate(`/invoices/${createdInvoice.id}`)
      }
    } catch (error) {
      // Error is handled by mutation
    }
  }

  const itemColumns = [
    {
      title: 'Type',
      dataIndex: 'itemType',
      key: 'itemType',
      width: 120,
    },
    {
      title: 'Description',
      dataIndex: 'description',
      key: 'description',
    },
    {
      title: 'Quantity',
      dataIndex: 'quantity',
      key: 'quantity',
      width: 100,
      align: 'right' as const,
    },
    {
      title: 'Unit Price',
      dataIndex: 'unitPrice',
      key: 'unitPrice',
      width: 120,
      align: 'right' as const,
      render: (price: number) => `₹${price.toFixed(2)}`,
    },
    {
      title: 'Total',
      key: 'total',
      width: 120,
      align: 'right' as const,
      render: (_: any, record: InvoiceItemRequest) => (
        <span>₹{(record.quantity * record.unitPrice).toFixed(2)}</span>
      ),
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 100,
      render: (_: any, record: InvoiceItemRequest, index: number) => (
        <Space>
          <Button
            type="link"
            size="small"
            onClick={() => handleEditItem(index)}
          >
            Edit
          </Button>
          <Button
            type="link"
            danger
            size="small"
            onClick={() => handleDeleteItem(index)}
          >
            Delete
          </Button>
        </Space>
      ),
    },
  ]

  const totalAmount = items.reduce((sum, item) => sum + item.quantity * item.unitPrice, 0)

  if (isEditMode && isLoadingInvoice) {
    return <div>Loading...</div>
  }

  if (!isEditMode && prescriptionId && isLoadingPreparation) {
    return <div>Loading invoice details from prescription...</div>
  }

  return (
    <div>
      <div style={{ marginBottom: 24 }}>
        <Space>
          <Button icon={<ArrowLeftOutlined />} onClick={() => navigate('/invoices')}>
            Back
          </Button>
        </Space>
      </div>

      <Card title={isEditMode ? 'Edit Invoice' : 'Create New Invoice'}>
        <Form
          form={form}
          layout="vertical"
          onFinish={handleSubmit}
          initialValues={{
            invoiceDate: dayjs(),
            status: 1, // Draft
          }}
        >
          <Row gutter={16}>
            <Col xs={24} sm={8}>
              <Form.Item
                label="Clinic"
                name="clinicId"
                rules={[{ required: true, message: 'Please select a clinic' }]}
              >
                <Select placeholder="Select clinic" disabled={isEditMode || !!prescriptionId}>
                  {clinics?.map((clinic) => (
                    <Option key={clinic.id} value={clinic.id}>
                      {clinic.name}
                    </Option>
                  ))}
                </Select>
              </Form.Item>
            </Col>
            <Col xs={24} sm={8}>
              <Form.Item
                label="Patient"
                required
                rules={[{ required: true, message: 'Please select a patient' }]}
              >
                <AutoComplete
                  placeholder="Search patient..."
                  options={patientSearchResults.map((patient) => ({
                    value: patient.id,
                    label: (
                      <div>
                        <div style={{ fontWeight: 500 }}>{patient.name}</div>
                        {patient.patientCode && (
                          <div style={{ fontSize: '12px', color: '#666' }}>
                            Code: {patient.patientCode}
                          </div>
                        )}
                      </div>
                    ),
                    patient: patient,
                  }))}
                  onSearch={setPatientSearchTerm}
                  onSelect={(value, option) => {
                    const patient = (option as any).patient as PatientSearch
                    setSelectedPatient(patient)
                    setPatientSearchTerm('')
                  }}
                  value={selectedPatient ? `${selectedPatient.name} (${selectedPatient.patientCode || 'N/A'})` : patientSearchTerm}
                  filterOption={false}
                  loading={isSearchingPatients}
                  disabled={isEditMode || !!prescriptionId}
                />
              </Form.Item>
            </Col>
            <Col xs={24} sm={8}>
              <Form.Item label="Invoice Date" name="invoiceDate">
                <DatePicker style={{ width: '100%' }} format="DD/MM/YYYY" />
              </Form.Item>
            </Col>
          </Row>

          {isEditMode && (
            <Row gutter={16}>
              <Col xs={24} sm={12}>
                <Form.Item label="Status" name="status">
                  <Select>
                    {INVOICE_STATUSES.map((status) => (
                      <Option key={status.value} value={status.value}>
                        {status.label}
                      </Option>
                    ))}
                  </Select>
                </Form.Item>
              </Col>
            </Row>
          )}

          {/* Hidden form items to store consultation and prescription IDs when loading from prescription */}
          {!isEditMode && prescriptionId && (
            <>
              <Form.Item name="consultationId" hidden>
                <InputNumber />
              </Form.Item>
              <Form.Item name="prescriptionId" hidden>
                <InputNumber />
              </Form.Item>
            </>
          )}

          <div style={{ marginTop: 24, marginBottom: 16 }}>
            <Space style={{ width: '100%', justifyContent: 'space-between' }}>
              <h3>Invoice Items</h3>
              <Button
                type="primary"
                icon={<PlusOutlined />}
                onClick={() => {
                  setEditingItemIndex(null)
                  itemForm.resetFields()
                  setIsItemModalOpen(true)
                }}
              >
                Add Item
              </Button>
            </Space>
          </div>

          <Table
            columns={itemColumns}
            dataSource={items}
            rowKey={(_, index) => index.toString()}
            pagination={false}
            locale={{ emptyText: 'No items added. Click "Add Item" to add items.' }}
          />

          <div style={{ marginTop: 24, textAlign: 'right' }}>
            <div style={{ fontSize: '18px', fontWeight: 600 }}>
              Total Amount: ₹{totalAmount.toFixed(2)}
            </div>
          </div>

          <div style={{ marginTop: 24, textAlign: 'right' }}>
            <Space>
              <Button onClick={() => navigate('/invoices')}>Cancel</Button>
              <Button type="primary" htmlType="submit" icon={<SaveOutlined />} loading={createMutation.isPending || updateMutation.isPending}>
                {isEditMode ? 'Update Invoice' : 'Create Invoice'}
              </Button>
            </Space>
          </div>
        </Form>
      </Card>

      {/* Add/Edit Item Modal */}
      <Modal
        title={editingItemIndex !== null ? 'Edit Item' : 'Add Item'}
        open={isItemModalOpen}
        onOk={handleAddItem}
        onCancel={() => {
          setIsItemModalOpen(false)
          setEditingItemIndex(null)
          itemForm.resetFields()
          setSelectedMedicine(null)
          setMedicineSearchTerm('')
        }}
        okText={editingItemIndex !== null ? 'Update' : 'Add'}
      >
        <Form form={itemForm} layout="vertical">
          <Form.Item
            label="Item Type"
            name="itemType"
            rules={[{ required: true, message: 'Please select item type' }]}
          >
            <Select 
              placeholder="Select item type"
              onChange={(value) => {
                // Reset medicine selection when item type changes
                if (value !== 'Medicine') {
                  setSelectedMedicine(null)
                  setMedicineSearchTerm('')
                  itemForm.setFieldsValue({ medicineId: undefined })
                }
              }}
            >
              {ITEM_TYPES.map((type) => (
                <Option key={type.value} value={type.value}>
                  {type.label}
                </Option>
              ))}
            </Select>
          </Form.Item>
          
          {/* Medicine search - only show when item type is Medicine */}
          <Form.Item
            noStyle
            shouldUpdate={(prevValues, currentValues) => prevValues.itemType !== currentValues.itemType}
          >
            {({ getFieldValue }) => {
              const itemType = getFieldValue('itemType')
              if (itemType === 'Medicine') {
                return (
                  <Form.Item
                    label="Search Medicine"
                    name="medicineId"
                  >
                    <AutoComplete
                      placeholder="Search medicine by name..."
                      options={medicineSearchResults.map((medicine) => ({
                        value: medicine.id,
                        label: (
                          <div>
                            <div style={{ fontWeight: 500 }}>{medicine.name}</div>
                            {medicine.genericName && (
                              <div style={{ fontSize: '12px', color: '#666' }}>
                                {medicine.genericName}
                              </div>
                            )}
                          </div>
                        ),
                        medicine: medicine,
                      }))}
                      onSearch={setMedicineSearchTerm}
                      onSelect={(value, option) => {
                        const medicine = (option as any).medicine as ClinicMedicineSearch
                        setSelectedMedicine(medicine)
                        // Auto-fill description and fetch price
                        itemForm.setFieldsValue({
                          medicineId: medicine.id,
                          description: medicine.name,
                        })
                        // Fetch medicine details to get selling price
                        clinicMedicineService.getById(medicine.id).then((medicineDetails) => {
                          itemForm.setFieldsValue({
                            unitPrice: medicineDetails.sellingPrice,
                          })
                        })
                        setMedicineSearchTerm('')
                      }}
                      value={selectedMedicine ? selectedMedicine.name : medicineSearchTerm}
                      filterOption={false}
                      loading={isSearchingMedicines}
                    />
                  </Form.Item>
                )
              }
              return null
            }}
          </Form.Item>

          <Form.Item
            label="Description"
            name="description"
            rules={[{ required: true, message: 'Please enter description' }]}
          >
            <TextArea rows={2} placeholder="Enter item description" />
          </Form.Item>
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item
                label="Quantity"
                name="quantity"
                rules={[{ required: true, message: 'Please enter quantity' }]}
              >
                <InputNumber min={1} style={{ width: '100%' }} placeholder="Quantity" />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item
                label="Unit Price (₹)"
                name="unitPrice"
                rules={[{ required: true, message: 'Please enter unit price' }]}
              >
                <InputNumber min={0} step={0.01} style={{ width: '100%' }} placeholder="0.00" />
              </Form.Item>
            </Col>
          </Row>
        </Form>
      </Modal>
    </div>
  )
}

