import { useState } from 'react'
import { Button, Table, Tag, Space, Input, Modal, Form, message } from 'antd'
import { PlusOutlined, EditOutlined, SearchOutlined } from '@ant-design/icons'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { clinicService, type Clinic } from '@core/services/clinicService'
import dayjs from 'dayjs'

export const ClinicsPage = () => {
  const [searchText, setSearchText] = useState('')
  const [isModalOpen, setIsModalOpen] = useState(false)
  const [editingClinic, setEditingClinic] = useState<Clinic | null>(null)
  const [form] = Form.useForm()
  const queryClient = useQueryClient()

  const { data: clinics, isLoading } = useQuery({
    queryKey: ['clinics'],
    queryFn: clinicService.getAll
  })

  const createMutation = useMutation({
    mutationFn: clinicService.create,
    onSuccess: () => {
      message.success('Clinic created successfully')
      queryClient.invalidateQueries({ queryKey: ['clinics'] })
      setIsModalOpen(false)
      form.resetFields()
    },
    onError: () => {
      message.error('Failed to create clinic')
    }
  })

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: number; data: any }) => 
      clinicService.update(id, data),
    onSuccess: () => {
      message.success('Clinic updated successfully')
      queryClient.invalidateQueries({ queryKey: ['clinics'] })
      setIsModalOpen(false)
      setEditingClinic(null)
      form.resetFields()
    },
    onError: () => {
      message.error('Failed to update clinic')
    }
  })

  const handleCreate = () => {
    setEditingClinic(null)
    form.resetFields()
    setIsModalOpen(true)
  }

  const handleEdit = (clinic: Clinic) => {
    setEditingClinic(clinic)
    form.setFieldsValue(clinic)
    setIsModalOpen(true)
  }

  const handleSubmit = async () => {
    try {
      const values = await form.validateFields()
      
      if (editingClinic) {
        updateMutation.mutate({ id: editingClinic.id, data: values })
      } else {
        createMutation.mutate(values)
      }
    } catch (error) {
      console.error('Validation failed:', error)
    }
  }

  const filteredClinics = clinics?.filter(clinic =>
    clinic.name.toLowerCase().includes(searchText.toLowerCase()) ||
    clinic.code.toLowerCase().includes(searchText.toLowerCase())
  )

  const columns = [
    {
      title: 'Name',
      dataIndex: 'name',
      key: 'name',
      sorter: (a: Clinic, b: Clinic) => a.name.localeCompare(b.name)
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
      render: (_: any, record: Clinic) => (
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
          <h1 style={{ margin: 0, fontSize: '24px', fontWeight: 600 }}>Clinics</h1>
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
          Add Clinic
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
        dataSource={filteredClinics}
        loading={isLoading}
        rowKey="id"
        pagination={{
          pageSize: 10,
          showSizeChanger: true,
          showTotal: (total) => `Total ${total} clinics`
        }}
      />

      <Modal
        title={editingClinic ? 'Edit Clinic' : 'Add New Clinic'}
        open={isModalOpen}
        onOk={handleSubmit}
        onCancel={() => {
          setIsModalOpen(false)
          setEditingClinic(null)
          form.resetFields()
        }}
        confirmLoading={createMutation.isPending || updateMutation.isPending}
        width={600}
      >
        <Form
          form={form}
          layout="vertical"
          style={{ marginTop: '24px' }}
        >
          <Form.Item
            label="Clinic Name"
            name="name"
            rules={[{ required: true, message: 'Please enter clinic name' }]}
          >
            <Input placeholder="e.g., Downtown Clinic" />
          </Form.Item>

          {!editingClinic && (
            <Form.Item
              label="Clinic Code"
              name="code"
              rules={[{ required: true, message: 'Please enter clinic code' }]}
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
              placeholder="Clinic address"
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

          {editingClinic && (
            <Form.Item
              label="Active"
              name="isActive"
              valuePropName="checked"
            >
              <input type="checkbox" />
            </Form.Item>
          )}
        </Form>
      </Modal>
    </div>
  )
}

