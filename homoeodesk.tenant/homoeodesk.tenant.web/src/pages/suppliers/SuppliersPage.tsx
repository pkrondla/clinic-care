import { useState } from 'react'
import {
  Card,
  Table,
  Button,
  Input,
  Space,
  Tag,
  Typography,
  Row,
  Col,
  Popconfirm,
  Modal,
  Form,
  Switch,
  message,
  Divider,
} from 'antd'
import {
  PlusOutlined,
  EditOutlined,
  DeleteOutlined,
  ReloadOutlined,
  PhoneOutlined,
  MailOutlined,
  EnvironmentOutlined,
} from '@ant-design/icons'
import {
  useSuppliers,
  useCreateSupplier,
  useUpdateSupplier,
  useDeleteSupplier,
} from '@core/hooks/queries/useSuppliers'
import type {
  Supplier,
  CreateSupplierRequest,
  UpdateSupplierRequest,
} from '@core/services/supplierService'

const { Title } = Typography
const { Search } = Input
const { TextArea } = Input

interface SupplierFormData {
  name: string
  contactPerson: string
  email: string
  phone: string
  alternatePhone?: string
  address: string
  city?: string
  state?: string
  pinCode?: string
  gstNumber?: string
  panNumber?: string
  bankName?: string
  bankAccountNumber?: string
  ifscCode?: string
  notes?: string
  isActive: boolean
}

export const SuppliersPage = () => {
  const [filters, setFilters] = useState({
    searchTerm: '',
    isActive: undefined as boolean | undefined,
  })
  const [modalVisible, setModalVisible] = useState(false)
  const [editingSupplier, setEditingSupplier] = useState<Supplier | null>(null)
  const [form] = Form.useForm<SupplierFormData>()

  const { data: suppliers = [], isLoading, refetch } = useSuppliers(filters)
  const createSupplierMutation = useCreateSupplier()
  const updateSupplierMutation = useUpdateSupplier()
  const deleteSupplierMutation = useDeleteSupplier()

  const handleSearch = (value: string) => {
    setFilters((prev) => ({ ...prev, searchTerm: value }))
  }

  const handleFilterChange = (key: string, value: any) => {
    setFilters((prev) => ({ ...prev, [key]: value }))
  }

  const handleCreate = () => {
    setEditingSupplier(null)
    form.resetFields()
    form.setFieldsValue({
      isActive: true,
    })
    setModalVisible(true)
  }

  const handleEdit = (supplier: Supplier) => {
    setEditingSupplier(supplier)
    form.setFieldsValue({
      name: supplier.name,
      contactPerson: supplier.contactPerson,
      email: supplier.email,
      phone: supplier.phone,
      alternatePhone: supplier.alternatePhone,
      address: supplier.address,
      city: supplier.city,
      state: supplier.state,
      pinCode: supplier.pinCode,
      gstNumber: supplier.gstNumber,
      panNumber: supplier.panNumber,
      bankName: supplier.bankName,
      bankAccountNumber: supplier.bankAccountNumber,
      ifscCode: supplier.ifscCode,
      notes: supplier.notes,
      isActive: supplier.isActive,
    })
    setModalVisible(true)
  }

  const handleDelete = async (id: number) => {
    await deleteSupplierMutation.mutateAsync(id)
  }

  const handleSubmit = async () => {
    try {
      const values = await form.validateFields()
      if (editingSupplier) {
        const updateRequest: UpdateSupplierRequest = {
          id: editingSupplier.id,
          ...values,
        }
        await updateSupplierMutation.mutateAsync(updateRequest)
      } else {
        const createRequest: CreateSupplierRequest = values
        await createSupplierMutation.mutateAsync(createRequest)
      }
      setModalVisible(false)
      form.resetFields()
    } catch (error) {
      console.error('Form validation failed:', error)
    }
  }

  const columns = [
    {
      title: 'Name',
      dataIndex: 'name',
      key: 'name',
      sorter: (a: Supplier, b: Supplier) => a.name.localeCompare(b.name),
    },
    {
      title: 'Contact Person',
      dataIndex: 'contactPerson',
      key: 'contactPerson',
    },
    {
      title: 'Email',
      dataIndex: 'email',
      key: 'email',
      render: (email: string) => (
        <Space>
          <MailOutlined />
          {email || '-'}
        </Space>
      ),
    },
    {
      title: 'Phone',
      dataIndex: 'phone',
      key: 'phone',
      render: (phone: string) => (
        <Space>
          <PhoneOutlined />
          {phone || '-'}
        </Space>
      ),
    },
    {
      title: 'City',
      dataIndex: 'city',
      key: 'city',
    },
    {
      title: 'GST Number',
      dataIndex: 'gstNumber',
      key: 'gstNumber',
      render: (gst: string) => gst || '-',
    },
    {
      title: 'Status',
      dataIndex: 'isActive',
      key: 'isActive',
      render: (isActive: boolean) => (
        <Tag color={isActive ? 'green' : 'red'}>
          {isActive ? 'Active' : 'Inactive'}
        </Tag>
      ),
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 150,
      render: (_: any, record: Supplier) => (
        <Space>
          <Button
            type="link"
            icon={<EditOutlined />}
            onClick={() => handleEdit(record)}
          >
            Edit
          </Button>
          <Popconfirm
            title="Are you sure you want to delete this supplier?"
            onConfirm={() => handleDelete(record.id)}
            okText="Yes"
            cancelText="No"
          >
            <Button type="link" danger icon={<DeleteOutlined />}>
              Delete
            </Button>
          </Popconfirm>
        </Space>
      ),
    },
  ]

  return (
    <div>
      <Card>
        <Row justify="space-between" align="middle" style={{ marginBottom: 16 }}>
          <Col>
            <Title level={2} style={{ margin: 0 }}>
              Suppliers
            </Title>
          </Col>
          <Col>
            <Space>
              <Button icon={<ReloadOutlined />} onClick={() => refetch()}>
                Refresh
              </Button>
              <Button type="primary" icon={<PlusOutlined />} onClick={handleCreate}>
                Add Supplier
              </Button>
            </Space>
          </Col>
        </Row>

        <Row gutter={16} style={{ marginBottom: 16 }}>
          <Col xs={24} sm={12} md={8}>
            <Search
              placeholder="Search suppliers..."
              allowClear
              onSearch={handleSearch}
              style={{ width: '100%' }}
            />
          </Col>
          <Col xs={24} sm={12} md={8}>
            <Space>
              <span>Status:</span>
              <Switch
                checkedChildren="Active"
                unCheckedChildren="All"
                checked={filters.isActive === true}
                onChange={(checked) =>
                  handleFilterChange('isActive', checked ? true : undefined)
                }
              />
            </Space>
          </Col>
        </Row>

        <Table
          columns={columns}
          dataSource={suppliers}
          rowKey="id"
          loading={isLoading}
          pagination={{
            pageSize: 10,
            showSizeChanger: true,
            showTotal: (total) => `Total ${total} suppliers`,
          }}
        />
      </Card>

      {/* Create/Edit Modal */}
      <Modal
        title={editingSupplier ? 'Edit Supplier' : 'Add Supplier'}
        open={modalVisible}
        onOk={handleSubmit}
        onCancel={() => {
          setModalVisible(false)
          form.resetFields()
        }}
        width={800}
        confirmLoading={
          createSupplierMutation.isPending || updateSupplierMutation.isPending
        }
      >
        <Form form={form} layout="vertical">
          <Row gutter={16}>
            <Col xs={24} sm={12}>
              <Form.Item
                label="Supplier Name"
                name="name"
                rules={[{ required: true, message: 'Please enter supplier name' }]}
              >
                <Input placeholder="Enter supplier name" />
              </Form.Item>
            </Col>
            <Col xs={24} sm={12}>
              <Form.Item
                label="Contact Person"
                name="contactPerson"
                rules={[
                  { required: true, message: 'Please enter contact person name' },
                ]}
              >
                <Input placeholder="Enter contact person name" />
              </Form.Item>
            </Col>
          </Row>

          <Row gutter={16}>
            <Col xs={24} sm={12}>
              <Form.Item
                label="Email"
                name="email"
                rules={[
                  { type: 'email', message: 'Please enter a valid email' },
                  { required: true, message: 'Please enter email' },
                ]}
              >
                <Input placeholder="Enter email" />
              </Form.Item>
            </Col>
            <Col xs={24} sm={12}>
              <Form.Item
                label="Phone"
                name="phone"
                rules={[{ required: true, message: 'Please enter phone number' }]}
              >
                <Input placeholder="Enter phone number" />
              </Form.Item>
            </Col>
          </Row>

          <Row gutter={16}>
            <Col xs={24} sm={12}>
              <Form.Item label="Alternate Phone" name="alternatePhone">
                <Input placeholder="Enter alternate phone (optional)" />
              </Form.Item>
            </Col>
            <Col xs={24} sm={12}>
              <Form.Item label="Status" name="isActive" valuePropName="checked">
                <Switch checkedChildren="Active" unCheckedChildren="Inactive" />
              </Form.Item>
            </Col>
          </Row>

          <Form.Item
            label="Address"
            name="address"
            rules={[{ required: true, message: 'Please enter address' }]}
          >
            <Input placeholder="Enter address" />
          </Form.Item>

          <Row gutter={16}>
            <Col xs={24} sm={8}>
              <Form.Item label="City" name="city">
                <Input placeholder="Enter city" />
              </Form.Item>
            </Col>
            <Col xs={24} sm={8}>
              <Form.Item label="State" name="state">
                <Input placeholder="Enter state" />
              </Form.Item>
            </Col>
            <Col xs={24} sm={8}>
              <Form.Item label="Pin Code" name="pinCode">
                <Input placeholder="Enter pin code" />
              </Form.Item>
            </Col>
          </Row>

          <Divider>Tax & Banking Information</Divider>

          <Row gutter={16}>
            <Col xs={24} sm={12}>
              <Form.Item label="GST Number" name="gstNumber">
                <Input placeholder="Enter GST number (optional)" />
              </Form.Item>
            </Col>
            <Col xs={24} sm={12}>
              <Form.Item label="PAN Number" name="panNumber">
                <Input placeholder="Enter PAN number (optional)" />
              </Form.Item>
            </Col>
          </Row>

          <Row gutter={16}>
            <Col xs={24} sm={12}>
              <Form.Item label="Bank Name" name="bankName">
                <Input placeholder="Enter bank name (optional)" />
              </Form.Item>
            </Col>
            <Col xs={24} sm={12}>
              <Form.Item label="Account Number" name="bankAccountNumber">
                <Input placeholder="Enter account number (optional)" />
              </Form.Item>
            </Col>
          </Row>

          <Form.Item label="IFSC Code" name="ifscCode">
            <Input placeholder="Enter IFSC code (optional)" />
          </Form.Item>

          <Form.Item label="Notes" name="notes">
            <TextArea rows={3} placeholder="Enter any additional notes (optional)" />
          </Form.Item>
        </Form>
      </Modal>
    </div>
  )
}

