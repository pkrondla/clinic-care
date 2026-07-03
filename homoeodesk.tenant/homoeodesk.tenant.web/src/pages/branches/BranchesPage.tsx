import { useState } from 'react'
import { Button, Table, Tag, Space, Input, Modal, Form, message, Divider, Typography } from 'antd'
import { PlusOutlined, EditOutlined, SearchOutlined } from '@ant-design/icons'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { branchService, type Branch, OperatingHoursType } from '@core/services/branchService'
import { OperatingHoursForm } from '../../components/OperatingHoursForm'
import dayjs from 'dayjs'

const { Text } = Typography;

export const BranchesPage = () => {
  const [searchText, setSearchText] = useState('')
  const [isModalOpen, setIsModalOpen] = useState(false)
  const [editingBranch, setEditingBranch] = useState<Branch | null>(null)
  const [operatingHoursType, setOperatingHoursType] = useState<OperatingHoursType>(OperatingHoursType.SingleShift)
  const [form] = Form.useForm()
  const queryClient = useQueryClient()

  const { data: Branches, isLoading } = useQuery({
    queryKey: ['Branches'],
    queryFn: branchService.getAll
  })

  const createMutation = useMutation({
    mutationFn: branchService.create,
    onSuccess: () => {
      message.success('Branch created successfully')
      queryClient.invalidateQueries({ queryKey: ['Branches'] })
      setIsModalOpen(false)
      form.resetFields()
    },
    onError: () => {
      message.error('Failed to create clinic')
    }
  })

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: number; data: any }) => 
      branchService.update(id, data),
    onSuccess: () => {
      message.success('Branch selected successfully')
      queryClient.invalidateQueries({ queryKey: ['Branches'] })
      setIsModalOpen(false)
      setEditingBranch(null)
      form.resetFields()
    },
    onError: () => {
      message.error('Failed to update clinic')
    }
  })

  const handleCreate = () => {
    setEditingBranch(null)
    setOperatingHoursType(OperatingHoursType.SingleShift)
    form.resetFields()
    form.setFieldsValue({
      operatingHoursType: OperatingHoursType.SingleShift,
      fullDayStartTime: dayjs('10:00', 'HH:mm'),
      fullDayEndTime: dayjs('17:00', 'HH:mm'),
    })
    setIsModalOpen(true)
  }

  const handleEdit = (branch: Branch) => {
    setEditingBranch(branch)
    setOperatingHoursType(branch.operatingHoursType || OperatingHoursType.SingleShift)
    
    const formValues: Record<string, unknown> = {
      name: branch.name,
      code: branch.code,
      address: branch.address,
      phone: branch.phone,
      email: branch.email,
      operatingHoursType: branch.operatingHoursType || OperatingHoursType.SingleShift,
      isActive: branch.isActive,
    }

    // Set time values if they exist
    if (branch.fullDayStartTime) {
      formValues.fullDayStartTime = dayjs(branch.fullDayStartTime, 'HH:mm:ss')
    }
    if (branch.fullDayEndTime) {
      formValues.fullDayEndTime = dayjs(branch.fullDayEndTime, 'HH:mm:ss')
    }
    if (branch.morningStartTime) {
      formValues.morningStartTime = dayjs(branch.morningStartTime, 'HH:mm:ss')
    }
    if (branch.morningEndTime) {
      formValues.morningEndTime = dayjs(branch.morningEndTime, 'HH:mm:ss')
    }
    if (branch.eveningStartTime) {
      formValues.eveningStartTime = dayjs(branch.eveningStartTime, 'HH:mm:ss')
    }
    if (branch.eveningEndTime) {
      formValues.eveningEndTime = dayjs(branch.eveningEndTime, 'HH:mm:ss')
    }

    form.setFieldsValue(formValues)
    setIsModalOpen(true)
  }

  const handleSubmit = async () => {
    try {
      const values = await form.validateFields()
      
      // Format time values to HH:mm:ss
      const formattedValues: any = {
        name: values.name,
        code: values.code,
        address: values.address,
        phone: values.phone,
        email: values.email,
        operatingHoursType: values.operatingHoursType,
      }

      // Add time fields based on operating hours type
      if (values.operatingHoursType === OperatingHoursType.SingleShift) {
        formattedValues.fullDayStartTime = values.fullDayStartTime?.format('HH:mm:ss')
        formattedValues.fullDayEndTime = values.fullDayEndTime?.format('HH:mm:ss')
      } else {
        formattedValues.morningStartTime = values.morningStartTime?.format('HH:mm:ss')
        formattedValues.morningEndTime = values.morningEndTime?.format('HH:mm:ss')
        formattedValues.eveningStartTime = values.eveningStartTime?.format('HH:mm:ss')
        formattedValues.eveningEndTime = values.eveningEndTime?.format('HH:mm:ss')
      }

      if (editingBranch) {
        formattedValues.isActive = values.isActive
        updateMutation.mutate({ id: editingBranch.id, data: formattedValues })
      } else {
        createMutation.mutate(formattedValues)
      }
    } catch (error) {
      console.error('Validation failed:', error)
    }
  }

  const filteredBranches = Branches?.filter(branch =>
    branch.name.toLowerCase().includes(searchText.toLowerCase()) ||
    branch.code.toLowerCase().includes(searchText.toLowerCase())
  )

  const columns = [
    {
      title: 'Name',
      dataIndex: 'name',
      key: 'name',
      sorter: (a: Branch, b: Branch) => a.name.localeCompare(b.name)
    },
    {
      title: 'Code',
      dataIndex: 'code',
      key: 'code',
      render: (code: string) => <Tag color="blue">{code}</Tag>
    },
    {
      title: 'Address',
      dataIndex: 'address',
      key: 'address',
      ellipsis: true
    },
    {
      title: 'Phone',
      dataIndex: 'phone',
      key: 'phone'
    },
    {
      title: 'Email',
      dataIndex: 'email',
      key: 'email'
    },
    {
      title: 'Operating Hours',
      key: 'operatingHours',
      render: (_: unknown, record: Branch) => {
        if (record.operatingHoursType === OperatingHoursType.SingleShift) {
          return (
            <Space direction="vertical" size={0}>
              <Tag color="blue">Single Shift</Tag>
              {record.fullDayStartTime && record.fullDayEndTime && (
                <Text style={{ fontSize: '12px' }}>
                  {dayjs(record.fullDayStartTime, 'HH:mm:ss').format('hh:mm A')} - {dayjs(record.fullDayEndTime, 'HH:mm:ss').format('hh:mm A')}
                </Text>
              )}
            </Space>
          )
        } else {
          return (
            <Space direction="vertical" size={0}>
              <Tag color="purple">Split Shift</Tag>
              {record.morningStartTime && record.morningEndTime && (
                <Text style={{ fontSize: '11px' }}>
                  ☀️ {dayjs(record.morningStartTime, 'HH:mm:ss').format('hh:mm A')} - {dayjs(record.morningEndTime, 'HH:mm:ss').format('hh:mm A')}
                </Text>
              )}
              {record.eveningStartTime && record.eveningEndTime && (
                <Text style={{ fontSize: '11px' }}>
                  🌙 {dayjs(record.eveningStartTime, 'HH:mm:ss').format('hh:mm A')} - {dayjs(record.eveningEndTime, 'HH:mm:ss').format('hh:mm A')}
                </Text>
              )}
            </Space>
          )
        }
      }
    },
    {
      title: 'Status',
      dataIndex: 'isActive',
      key: 'isActive',
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
      render: (date: string) => dayjs(date).format('MMM DD, YYYY')
    },
    {
      title: 'Actions',
      key: 'actions',
      render: (_: unknown, record: Branch) => (
        <Space>
          <Button
            type="link"
            icon={<EditOutlined />}
            onClick={() => handleEdit(record)}
          >
            Edit
          </Button>
        </Space>
      )
    }
  ]

  return (
    <div style={{ padding: '24px' }}>
      <div style={{ marginBottom: '24px', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <div>
          <h1 style={{ margin: 0, fontSize: '24px', fontWeight: 600 }}>Branches</h1>
          <p style={{ margin: '8px 0 0 0', color: '#666' }}>
            Manage clinic locations and settings
          </p>
        </div>
        <Button
          type="primary"
          icon={<PlusOutlined />}
          onClick={handleCreate}
          size="large"
        >
          Add Branch
        </Button>
      </div>

      <div style={{ marginBottom: '16px' }}>
        <Input
          placeholder="Search by name or code..."
          prefix={<SearchOutlined />}
          value={searchText}
          onChange={(e) => setSearchText(e.target.value)}
          style={{ maxWidth: '400px' }}
          size="large"
        />
      </div>

      <Table
        columns={columns}
        dataSource={filteredBranches}
        loading={isLoading}
        rowKey="id"
        pagination={{
          pageSize: 10,
          showSizeChanger: true,
          showTotal: (total) => `Total ${total} Branches`
        }}
      />

      <Modal
        title={editingBranch ? 'Edit Branch' : 'Add New Branch'}
        open={isModalOpen}
        onOk={handleSubmit}
        onCancel={() => {
          setIsModalOpen(false)
          setEditingBranch(null)
          form.resetFields()
        }}
        confirmLoading={createMutation.isPending || updateMutation.isPending}
        width={800}
      >
        <Form
          form={form}
          layout="vertical"
          style={{ marginTop: '24px' }}
        >
          <Text strong style={{ fontSize: '16px' }}>Basic Information</Text>
          <Divider style={{ marginTop: 8, marginBottom: 16 }} />

          <Form.Item
            label="Branch Name"
            name="name"
            rules={[{ required: true, message: 'Please enter Branch Name' }]}
          >
            <Input placeholder="e.g., Downtown Branch" />
          </Form.Item>

          {!editingBranch && (
            <Form.Item
              label="Branch Code"
              name="code"
              rules={[{ required: true, message: 'Please enter Branch Code' }]}
            >
              <Input placeholder="e.g., DT001" />
            </Form.Item>
          )}

          <Form.Item
            label="Address"
            name="address"
          >
            <Input.TextArea 
              rows={2} 
              placeholder="Branch address"
            />
          </Form.Item>

          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '16px' }}>
            <Form.Item
              label="Phone"
              name="phone"
            >
              <Input placeholder="+1234567890" />
            </Form.Item>

            <Form.Item
              label="Email"
              name="email"
              rules={[{ type: 'email', message: 'Please enter a valid email' }]}
            >
              <Input placeholder="clinic@example.com" />
            </Form.Item>
          </div>

          <Text strong style={{ fontSize: '16px', marginTop: 16, display: 'block' }}>
            ⏰ Operating Hours
          </Text>
          <Text type="secondary" style={{ fontSize: '12px', display: 'block', marginBottom: 16 }}>
            Set the default working hours for this branch. Doctors assigned to this branch will automatically follow these hours.
          </Text>
          <Divider style={{ marginTop: 8, marginBottom: 16 }} />

          <OperatingHoursForm 
            operatingHoursType={operatingHoursType}
            onTypeChange={setOperatingHoursType}
          />

          {editingBranch && (
            <>
              <Divider />
              <Form.Item
                label="Status"
                name="isActive"
                valuePropName="checked"
              >
                <input type="checkbox" />
              </Form.Item>
            </>
          )}
        </Form>
      </Modal>
    </div>
  )
}

