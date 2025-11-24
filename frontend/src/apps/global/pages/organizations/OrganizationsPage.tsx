import { useState } from 'react'
import { Button, Table, Tag, Space, Input, Modal, Form, message } from 'antd'
import { PlusOutlined, EditOutlined, SearchOutlined } from '@ant-design/icons'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { organizationService, type Organization, type CreateOrganizationRequest } from '@core/services/organizationService'
import dayjs from 'dayjs'

export const OrganizationsPage = () => {
  const [searchText, setSearchText] = useState('')
  const [isModalOpen, setIsModalOpen] = useState(false)
  const [editingOrg, setEditingOrg] = useState<Organization | null>(null)
  const [form] = Form.useForm()
  const queryClient = useQueryClient()

  const { data: organizations, isLoading } = useQuery({
    queryKey: ['organizations'],
    queryFn: organizationService.getAll
  })

  const createMutation = useMutation({
    mutationFn: organizationService.create,
    onSuccess: () => {
      message.success('Organization created successfully')
      queryClient.invalidateQueries({ queryKey: ['organizations'] })
      setIsModalOpen(false)
      form.resetFields()
    },
    onError: () => {
      message.error('Failed to create organization')
    }
  })

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: number; data: any }) => 
      organizationService.update(id, data),
    onSuccess: () => {
      message.success('Organization updated successfully')
      queryClient.invalidateQueries({ queryKey: ['organizations'] })
      setIsModalOpen(false)
      setEditingOrg(null)
      form.resetFields()
    },
    onError: () => {
      message.error('Failed to update organization')
    }
  })

  const handleCreate = () => {
    setEditingOrg(null)
    form.resetFields()
    setIsModalOpen(true)
  }

  const handleEdit = (org: Organization) => {
    setEditingOrg(org)
    form.setFieldsValue(org)
    setIsModalOpen(true)
  }

  const handleSubmit = async () => {
    try {
      const values = await form.validateFields()
      
      if (editingOrg) {
        updateMutation.mutate({ id: editingOrg.id, data: values })
      } else {
        createMutation.mutate({ ...values, createDatabase: true })
      }
    } catch (error) {
      console.error('Validation failed:', error)
    }
  }

  const filteredOrganizations = organizations?.filter(org =>
    org.name.toLowerCase().includes(searchText.toLowerCase()) ||
    org.subdomain.toLowerCase().includes(searchText.toLowerCase()) ||
    org.contactEmail.toLowerCase().includes(searchText.toLowerCase())
  )

  const columns = [
    {
      title: 'Name',
      dataIndex: 'name',
      key: 'name',
      sorter: (a: Organization, b: Organization) => a.name.localeCompare(b.name)
    },
    {
      title: 'Subdomain',
      dataIndex: 'subdomain',
      key: 'subdomain',
      render: (subdomain: string) => (
        <Tag color="blue">{subdomain}.cliniccare.com</Tag>
      )
    },
    {
      title: 'Contact Email',
      dataIndex: 'contactEmail',
      key: 'contactEmail'
    },
    {
      title: 'Subscription',
      dataIndex: 'subscriptionStatus',
      key: 'subscriptionStatus',
      render: (status: string) => {
        const color = status === 'Trial' ? 'orange' : status === 'Active' ? 'green' : 'red'
        return <Tag color={color}>{status}</Tag>
      }
    },
    {
      title: 'Trial End Date',
      dataIndex: 'trialEndDate',
      key: 'trialEndDate',
      render: (date: string) => date ? dayjs(date).format('MMM DD, YYYY') : '-'
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
      render: (_: any, record: Organization) => (
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
          <h1 style={{ margin: 0, fontSize: '24px', fontWeight: 600 }}>Organizations</h1>
          <p style={{ margin: '8px 0 0 0', color: '#666' }}>
            Manage clinic organizations and their subscriptions
          </p>
        </div>
        <Button
          type="primary"
          icon={<PlusOutlined />}
          onClick={handleCreate}
          size="large"
        >
          Create Organization
        </Button>
      </div>

      <div style={{ marginBottom: '16px' }}>
        <Input
          placeholder="Search by name, subdomain, or email..."
          prefix={<SearchOutlined />}
          value={searchText}
          onChange={(e) => setSearchText(e.target.value)}
          style={{ maxWidth: '400px' }}
          size="large"
        />
      </div>

      <Table
        columns={columns}
        dataSource={filteredOrganizations}
        loading={isLoading}
        rowKey="id"
        pagination={{
          pageSize: 10,
          showSizeChanger: true,
          showTotal: (total) => `Total ${total} organizations`
        }}
      />

      <Modal
        title={editingOrg ? 'Edit Organization' : 'Create New Organization'}
        open={isModalOpen}
        onOk={handleSubmit}
        onCancel={() => {
          setIsModalOpen(false)
          setEditingOrg(null)
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
            label="Organization Name"
            name="name"
            rules={[{ required: true, message: 'Please enter organization name' }]}
          >
            <Input placeholder="e.g., City Hospital Group" />
          </Form.Item>

          {!editingOrg && (
            <Form.Item
              label="Subdomain"
              name="subdomain"
              help="Leave empty to auto-generate from organization name"
            >
              <Input 
                placeholder="e.g., cityhospital" 
                addonAfter=".cliniccare.com"
              />
            </Form.Item>
          )}

          <Form.Item
            label="Contact Email"
            name="contactEmail"
            rules={[
              { required: true, message: 'Please enter contact email' },
              { type: 'email', message: 'Please enter a valid email' }
            ]}
          >
            <Input placeholder="admin@example.com" />
          </Form.Item>

          <Form.Item
            label="Contact Phone"
            name="contactPhone"
          >
            <Input placeholder="+1234567890" />
          </Form.Item>

          <Form.Item
            label="Address"
            name="address"
          >
            <Input.TextArea 
              rows={3} 
              placeholder="Organization headquarters address"
            />
          </Form.Item>

          {editingOrg && (
            <Form.Item
              label="Status"
              name="isActive"
              valuePropName="checked"
            >
              <Input type="checkbox" />
            </Form.Item>
          )}
        </Form>
      </Modal>
    </div>
  )
}