import { useState } from 'react'
import { 
  Card, 
  Table, 
  Button, 
  Input, 
  Select, 
  Space, 
  Tag, 
  Typography, 
  Row, 
  Col, 
  Tooltip,
  Popconfirm,
  Modal,
  Form,
  Switch,
  message,
  Divider
} from 'antd'
import { 
  PlusOutlined, 
  EditOutlined, 
  DeleteOutlined, 
  ReloadOutlined,
  FilterOutlined,
  UserOutlined
} from '@ant-design/icons'
import { useUsers, useCreateUser, useUpdateUser, useDeleteUser } from '@core/hooks/queries/useUsers'
import { useClinics } from '@core/hooks/queries/useClinics'
import type { Clinic } from '@core/services/clinicService'
import type { User as UserType, CreateUserRequest, UpdateUserRequest } from '@core/services/userService'
import { UserRole } from '@core/types/auth'
import dayjs from 'dayjs'

const { Title } = Typography
const { Search } = Input
const { Option } = Select

interface UserFormData {
  email: string
  password?: string
  firstName: string
  lastName: string
  phone: string
  role: string
  isActive: boolean
  clinicIds: number[]
  // Doctor fields
  registrationNumber?: string
  qualification?: string
  specialization?: string
  experienceYears?: number
  consultationFeeInPerson?: number
  consultationFeeTele?: number
  followupFeeInPerson?: number
  followupFeeTele?: number
}

export const UsersPage = () => {
  const [filters, setFilters] = useState({
    searchTerm: '',
    role: undefined as string | undefined,
    clinicId: undefined as number | undefined,
    isActive: undefined as boolean | undefined
  })
  const [modalVisible, setModalVisible] = useState(false)
  const [editingUser, setEditingUser] = useState<UserType | null>(null)
  const [form] = Form.useForm<UserFormData>()

  const { data: users = [], isLoading, refetch } = useUsers(filters)
  const { data: clinics = [] } = useClinics()
  
  // Ensure clinics is an array
  const clinicsList: Clinic[] = Array.isArray(clinics) ? clinics : []
  const createUserMutation = useCreateUser()
  const updateUserMutation = useUpdateUser()
  const deleteUserMutation = useDeleteUser()

  const handleSearch = (value: string) => {
    setFilters(prev => ({ ...prev, searchTerm: value }))
  }

  const handleFilterChange = (key: string, value: any) => {
    setFilters(prev => ({ ...prev, [key]: value }))
  }

  const handleCreate = () => {
    setEditingUser(null)
    form.resetFields()
    form.setFieldsValue({
      isActive: true,
      clinicIds: [],
      role: UserRole.Doctor
    })
    setModalVisible(true)
  }

  const handleEdit = (user: UserType) => {
    setEditingUser(user)
    form.setFieldsValue({
      email: user.email,
      firstName: user.firstName,
      lastName: user.lastName,
      phone: user.phone || '',
      role: user.role,
      isActive: user.isActive,
      clinicIds: user.clinicAccess?.map(ca => ca.clinicId) || [],
      registrationNumber: user.doctorProfile?.registrationNumber || '',
      qualification: user.doctorProfile?.qualification || '',
      specialization: user.doctorProfile?.specialization || '',
      experienceYears: user.doctorProfile?.experienceYears,
      consultationFeeInPerson: user.doctorProfile?.consultationFeeInPerson,
      consultationFeeTele: user.doctorProfile?.consultationFeeTele,
      followupFeeInPerson: user.doctorProfile?.followupFeeInPerson,
      followupFeeTele: user.doctorProfile?.followupFeeTele
    })
    setModalVisible(true)
  }

  const handleDelete = async (id: number) => {
    try {
      await deleteUserMutation.mutateAsync(id)
    } catch (error) {
      // Error handled by mutation
    }
  }

  const handleSubmit = async () => {
    try {
      const values = await form.validateFields()
      
      if (editingUser) {
        const updateData: UpdateUserRequest = {
          email: values.email,
          firstName: values.firstName,
          lastName: values.lastName,
          phone: values.phone,
          role: values.role,
          isActive: values.isActive,
          clinicIds: values.clinicIds,
          registrationNumber: values.registrationNumber,
          qualification: values.qualification,
          specialization: values.specialization,
          experienceYears: values.experienceYears,
          consultationFeeInPerson: values.consultationFeeInPerson,
          consultationFeeTele: values.consultationFeeTele,
          followupFeeInPerson: values.followupFeeInPerson,
          followupFeeTele: values.followupFeeTele
        }
        
        if (values.password) {
          updateData.password = values.password
        }

        await updateUserMutation.mutateAsync({ id: editingUser.id, user: updateData })
      } else {
        if (!values.password) {
          message.error('Password is required for new users')
          return
        }

        const createData: CreateUserRequest = {
          email: values.email,
          password: values.password,
          firstName: values.firstName,
          lastName: values.lastName,
          phone: values.phone,
          role: values.role,
          clinicIds: values.clinicIds,
          registrationNumber: values.registrationNumber,
          qualification: values.qualification,
          specialization: values.specialization,
          experienceYears: values.experienceYears,
          consultationFeeInPerson: values.consultationFeeInPerson,
          consultationFeeTele: values.consultationFeeTele,
          followupFeeInPerson: values.followupFeeInPerson,
          followupFeeTele: values.followupFeeTele
        }

        await createUserMutation.mutateAsync(createData)
      }

      setModalVisible(false)
      form.resetFields()
    } catch (error) {
      // Validation errors are handled by form
    }
  }

  const getRoleColor = (role: string) => {
    switch (role) {
      case UserRole.Admin:
      case UserRole.OrganizationAdmin:
        return 'red'
      case UserRole.Doctor:
        return 'blue'
      case UserRole.Reception:
      case UserRole.Staff:
        return 'green'
      case UserRole.Pharmacy:
        return 'orange'
      default:
        return 'default'
    }
  }

  const getRoleLabel = (role: string) => {
    switch (role) {
      case UserRole.Admin:
      case UserRole.OrganizationAdmin:
        return 'Admin'
      case UserRole.Doctor:
        return 'Doctor'
      case UserRole.Reception:
      case UserRole.Staff:
        return 'Staff'
      case UserRole.Pharmacy:
        return 'Pharmacy'
      default:
        return role
    }
  }

  const isDoctor = Form.useWatch('role', form) === UserRole.Doctor

  const columns = [
    {
      title: 'Name',
      dataIndex: 'fullName',
      key: 'fullName',
      render: (name: string, record: UserType) => (
        <div>
          <div style={{ fontWeight: 500 }}>{name}</div>
          <div style={{ fontSize: 12, color: '#666' }}>{record.email}</div>
        </div>
      )
    },
    {
      title: 'Phone',
      dataIndex: 'phone',
      key: 'phone',
      width: 120,
      render: (phone: string) => phone || '-'
    },
    {
      title: 'Role',
      dataIndex: 'role',
      key: 'role',
      width: 120,
      render: (role: string) => (
        <Tag color={getRoleColor(role)}>{getRoleLabel(role)}</Tag>
      )
    },
    {
      title: 'Clinics',
      key: 'clinics',
      width: 200,
      render: (record: UserType) => (
        <div>
          {record.clinicAccess && record.clinicAccess.length > 0 ? (
            <Space wrap size={[4, 4]}>
              {record.clinicAccess.slice(0, 2).map(ca => (
                <Tag key={ca.clinicId}>{ca.clinicName}</Tag>
              ))}
              {record.clinicAccess.length > 2 && (
                <Tag>+{record.clinicAccess.length - 2}</Tag>
              )}
            </Space>
          ) : (
            <span style={{ color: '#999' }}>No clinics</span>
          )}
        </div>
      )
    },
    {
      title: 'Status',
      dataIndex: 'isActive',
      key: 'isActive',
      width: 100,
      render: (isActive: boolean) => (
        <Tag color={isActive ? 'green' : 'red'}>
          {isActive ? 'Active' : 'Inactive'}
        </Tag>
      )
    },
    {
      title: 'Created',
      dataIndex: 'createdAt',
      key: 'createdAt',
      width: 120,
      render: (date: string) => dayjs(date).format('MMM DD, YYYY')
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 120,
      fixed: 'right' as const,
      render: (record: UserType) => (
        <Space size="small">
          <Tooltip title="Edit">
            <Button
              type="text"
              icon={<EditOutlined />}
              onClick={() => handleEdit(record)}
            />
          </Tooltip>
          <Popconfirm
            title="Delete user"
            description="Are you sure you want to delete this user? This action cannot be undone."
            onConfirm={() => handleDelete(record.id)}
            okText="Yes"
            cancelText="No"
          >
            <Tooltip title="Delete">
              <Button
                type="text"
                danger
                icon={<DeleteOutlined />}
              />
            </Tooltip>
          </Popconfirm>
        </Space>
      )
    }
  ]

  return (
    <div>
      <Row justify="space-between" align="middle" style={{ marginBottom: 16 }}>
        <Col>
          <Title level={2} style={{ margin: 0 }}>
            <UserOutlined /> Users Management
          </Title>
        </Col>
        <Col>
          <Space>
            <Button
              icon={<ReloadOutlined />}
              onClick={() => refetch()}
              loading={isLoading}
            >
              Refresh
            </Button>
            <Button
              type="primary"
              icon={<PlusOutlined />}
              onClick={handleCreate}
            >
              Add User
            </Button>
          </Space>
        </Col>
      </Row>

      <Card>
        <Row gutter={[16, 16]} style={{ marginBottom: 16 }}>
          <Col xs={24} sm={12} md={8}>
            <Search
              placeholder="Search by name or email"
              allowClear
              onSearch={handleSearch}
              style={{ width: '100%' }}
            />
          </Col>
          <Col xs={24} sm={12} md={4}>
            <Select
              placeholder="Filter by role"
              allowClear
              style={{ width: '100%' }}
              onChange={(value) => handleFilterChange('role', value)}
            >
              <Option value={UserRole.Admin}>Admin</Option>
              <Option value={UserRole.Doctor}>Doctor</Option>
              <Option value={UserRole.Reception}>Staff</Option>
              <Option value={UserRole.Pharmacy}>Pharmacy</Option>
            </Select>
          </Col>
          <Col xs={24} sm={12} md={4}>
            <Select
              placeholder="Filter by clinic"
              allowClear
              style={{ width: '100%' }}
              onChange={(value) => handleFilterChange('clinicId', value)}
            >
              {clinicsList.map(clinic => (
                <Option key={clinic.id} value={clinic.id}>{clinic.name}</Option>
              ))}
            </Select>
          </Col>
          <Col xs={24} sm={12} md={4}>
            <Select
              placeholder="Filter by status"
              allowClear
              style={{ width: '100%' }}
              onChange={(value) => handleFilterChange('isActive', value)}
            >
              <Option value={true}>Active</Option>
              <Option value={false}>Inactive</Option>
            </Select>
          </Col>
        </Row>

        <Table
          dataSource={users}
          columns={columns}
          rowKey="id"
          loading={isLoading}
          pagination={{
            pageSize: 10,
            showSizeChanger: true,
            showTotal: (total) => `Total ${total} users`
          }}
          scroll={{ x: 1000 }}
        />
      </Card>

      <Modal
        title={editingUser ? 'Edit User' : 'Create User'}
        open={modalVisible}
        onOk={handleSubmit}
        onCancel={() => {
          setModalVisible(false)
          form.resetFields()
        }}
        width={800}
        okText={editingUser ? 'Update' : 'Create'}
        confirmLoading={createUserMutation.isPending || updateUserMutation.isPending}
      >
        <Form
          form={form}
          layout="vertical"
          initialValues={{
            isActive: true,
            clinicIds: [],
            role: UserRole.Doctor
          }}
        >
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item
                name="firstName"
                label="First Name"
                rules={[{ required: true, message: 'Please enter first name' }]}
              >
                <Input placeholder="First Name" />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item
                name="lastName"
                label="Last Name"
                rules={[{ required: true, message: 'Please enter last name' }]}
              >
                <Input placeholder="Last Name" />
              </Form.Item>
            </Col>
          </Row>

          <Row gutter={16}>
            <Col span={12}>
              <Form.Item
                name="email"
                label="Email"
                rules={[
                  { required: true, message: 'Please enter email' },
                  { type: 'email', message: 'Please enter a valid email' }
                ]}
              >
                <Input placeholder="Email" />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item
                name="phone"
                label="Phone"
              >
                <Input placeholder="Phone" />
              </Form.Item>
            </Col>
          </Row>

          <Row gutter={16}>
            <Col span={12}>
              <Form.Item
                name="password"
                label={editingUser ? 'Password (leave blank to keep current)' : 'Password'}
                rules={editingUser ? [] : [{ required: true, message: 'Please enter password' }]}
              >
                <Input.Password placeholder="Password" />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item
                name="role"
                label="Role"
                rules={[{ required: true, message: 'Please select role' }]}
              >
                <Select placeholder="Select Role">
                  <Option value={UserRole.Admin}>Admin</Option>
                  <Option value={UserRole.Doctor}>Doctor</Option>
                  <Option value={UserRole.Reception}>Staff</Option>
                  <Option value={UserRole.Pharmacy}>Pharmacy</Option>
                </Select>
              </Form.Item>
            </Col>
          </Row>

          <Form.Item
            name="clinicIds"
            label="Clinic Access"
          >
            <Select
              mode="multiple"
              placeholder="Select clinics"
              style={{ width: '100%' }}
            >
              {clinicsList.map(clinic => (
                <Option key={clinic.id} value={clinic.id}>{clinic.name}</Option>
              ))}
            </Select>
          </Form.Item>

          {editingUser && (
            <Form.Item
              name="isActive"
              label="Status"
              valuePropName="checked"
            >
              <Switch checkedChildren="Active" unCheckedChildren="Inactive" />
            </Form.Item>
          )}

          {isDoctor && (
            <>
              <Divider orientation="left">Doctor Information</Divider>
              <Row gutter={16}>
                <Col span={12}>
                  <Form.Item
                    name="registrationNumber"
                    label="Registration Number"
                  >
                    <Input placeholder="Registration Number" />
                  </Form.Item>
                </Col>
                <Col span={12}>
                  <Form.Item
                    name="qualification"
                    label="Qualification"
                  >
                    <Input placeholder="Qualification" />
                  </Form.Item>
                </Col>
              </Row>

              <Row gutter={16}>
                <Col span={12}>
                  <Form.Item
                    name="specialization"
                    label="Specialization"
                  >
                    <Input placeholder="Specialization" />
                  </Form.Item>
                </Col>
                <Col span={12}>
                  <Form.Item
                    name="experienceYears"
                    label="Experience (Years)"
                  >
                    <Input type="number" placeholder="Years" />
                  </Form.Item>
                </Col>
              </Row>

              <Divider orientation="left">Consultation Fees</Divider>
              <Row gutter={16}>
                <Col span={12}>
                  <Form.Item
                    name="consultationFeeInPerson"
                    label="In-Person Consultation Fee"
                  >
                    <Input type="number" placeholder="0.00" addonBefore="₹" />
                  </Form.Item>
                </Col>
                <Col span={12}>
                  <Form.Item
                    name="consultationFeeTele"
                    label="Teleconsultation Fee"
                  >
                    <Input type="number" placeholder="0.00" addonBefore="₹" />
                  </Form.Item>
                </Col>
              </Row>

              <Row gutter={16}>
                <Col span={12}>
                  <Form.Item
                    name="followupFeeInPerson"
                    label="Follow-up In-Person Fee"
                  >
                    <Input type="number" placeholder="0.00" addonBefore="₹" />
                  </Form.Item>
                </Col>
                <Col span={12}>
                  <Form.Item
                    name="followupFeeTele"
                    label="Follow-up Teleconsultation Fee"
                  >
                    <Input type="number" placeholder="0.00" addonBefore="₹" />
                  </Form.Item>
                </Col>
              </Row>
            </>
          )}
        </Form>
      </Modal>
    </div>
  )
}
