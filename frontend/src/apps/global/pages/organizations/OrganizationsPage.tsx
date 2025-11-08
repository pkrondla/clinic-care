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
  Tooltip,
  Popconfirm,
  Modal,
  Form,
  Select,
  message
} from 'antd'
import { 
  PlusOutlined, 
  EditOutlined, 
  DeleteOutlined, 
  GlobalOutlined,
  ReloadOutlined
} from '@ant-design/icons'
import { globalApi } from '../../../../services/globalApi'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import type { Organization, CreateOrganizationDto, SubscriptionPlan } from '../../../../types/organization'
import dayjs from 'dayjs'

const { Title } = Typography
const { Search } = Input

export const OrganizationsPage = () => {
  const [searchText, setSearchText] = useState('')
  const [modalVisible, setModalVisible] = useState(false)
  const [editingOrg, setEditingOrg] = useState<Organization | null>(null)
  const [form] = Form.useForm()
  const queryClient = useQueryClient()

  // Get organizations
  const { data: orgsData, isLoading, refetch } = useQuery<Organization[]>({
    queryKey: ['organizations'],
    queryFn: async () => {
      const response = await globalApi.organizations.getAll()
      return response.data
    }
  })

  // Get subscription plans
  const { data: subscriptionPlans } = useQuery<SubscriptionPlan[]>({
    queryKey: ['subscription-plans'],
    queryFn: async () => {
      const response = await globalApi.subscriptions.getAll()
      return response.data
    }
  })

  // Create organization mutation
  const createMutation = useMutation({
    mutationFn: (data: CreateOrganizationDto) => globalApi.organizations.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['organizations'] })
      message.success('Organization created successfully')
      setModalVisible(false)
      form.resetFields()
    },
    onError: (error: Error) => {
      message.error(error.message || 'Failed to create organization')
    }
  })

  // Delete organization mutation
  const deleteMutation = useMutation({
    mutationFn: (id: string) => globalApi.organizations.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['organizations'] })
      message.success('Organization deleted successfully')
    },
    onError: (error: Error) => {
      message.error(error.message || 'Failed to delete organization')
    }
  })

  const handleSearch = (value: string) => {
    setSearchText(value)
  }

  const handleCreate = async (values: CreateOrganizationDto) => {
    await createMutation.mutateAsync(values)
  }

  const handleDelete = async (id: string) => {
    await deleteMutation.mutateAsync(id)
  }

  const columns = [
    {
      title: 'Organization',
      dataIndex: 'name',
      key: 'name',
      render: (name: string, record: Organization) => (
        <div>
          <div style={{ fontWeight: 500 }}>{name}</div>
          <div style={{ fontSize: 12, color: '#666' }}>
            <GlobalOutlined style={{ marginRight: 8 }} />
            {record.subdomain}.yourapp.com
          </div>
        </div>
      )
    },
    {
      title: 'Contact',
      key: 'contact',
      render: (record: Organization) => (
        <div>
          <div>{record.contactEmail}</div>
          <div style={{ fontSize: 12, color: '#666' }}>{record.contactPhone}</div>
        </div>
      )
    },
    {
      title: 'Subscription',
      dataIndex: ['subscription', 'name'],
      key: 'subscription',
      render: (plan: string) => <Tag color="blue">{plan}</Tag>
    },
    {
      title: 'Status',
      dataIndex: 'isActive',
      key: 'status',
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
      width: 120,
      render: (record: Organization) => (
        <Space size="small">
          <Tooltip title="Edit">
            <Button
              type="text"
              icon={<EditOutlined />}
              onClick={() => {
                setEditingOrg(record)
                setModalVisible(true)
              }}
            />
          </Tooltip>
          <Tooltip title="Delete">
            <Popconfirm
              title="Delete Organization"
              description="Are you sure? This will delete all associated data."
              onConfirm={() => handleDelete(record.id.toString())}
              okText="Yes"
              cancelText="No"
            >
              <Button
                type="text"
                danger
                icon={<DeleteOutlined />}
                loading={deleteMutation.isPending}
              />
            </Popconfirm>
          </Tooltip>
        </Space>
      )
    }
  ]

  const filteredOrgs = (orgsData || []).filter(org =>
    org.name.toLowerCase().includes(searchText.toLowerCase()) ||
    org.subdomain.toLowerCase().includes(searchText.toLowerCase())
  )
  
  return (
    <div>
      <div style={{ marginBottom: 24, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <Title level={2} style={{ margin: 0 }}>Organizations</Title>
        <Button
          type="primary"
          icon={<PlusOutlined />}
          onClick={() => {
            setEditingOrg(null)
            setModalVisible(true)
          }}
        >
          Add Organization
        </Button>
      </div>

      <Card>
        <Row gutter={[16, 16]} style={{ marginBottom: 16 }}>
          <Col flex="1">
            <Search
              placeholder="Search organizations..."
              allowClear
              onSearch={handleSearch}
              style={{ width: '100%' }}
            />
          </Col>
          <Col>
            <Button
              icon={<ReloadOutlined />}
              onClick={() => refetch()}
              loading={isLoading}
            >
              Refresh
            </Button>
          </Col>
        </Row>

        <Table
          columns={columns}
          dataSource={filteredOrgs}
          rowKey="id"
          loading={isLoading}
        />
      </Card>

      <Modal
        title={editingOrg ? 'Edit Organization' : 'Add Organization'}
        open={modalVisible}
        onCancel={() => {
          setModalVisible(false)
          form.resetFields()
          setEditingOrg(null)
        }}
        footer={null}
      >
        <Form
          form={form}
          layout="vertical"
          onFinish={handleCreate}
          initialValues={editingOrg || {}}
        >
          <Form.Item
            name="name"
            label="Organization Name"
            rules={[{ required: true, message: 'Please enter organization name' }]}
          >
            <Input />
          </Form.Item>

          <Form.Item
            name="subdomain"
            label="Subdomain"
            rules={[
              { required: true, message: 'Please enter subdomain' },
              { pattern: /^[a-z0-9-]+$/, message: 'Only lowercase letters, numbers, and hyphens allowed' }
            ]}
          >
            <Input addonAfter=".yourapp.com" />
          </Form.Item>

          <Form.Item
            name="subscriptionPlanId"
            label="Subscription Plan"
            rules={[{ required: true, message: 'Please select a subscription plan' }]}
          >
            <Select>
              {subscriptionPlans?.map(plan => (
                <Select.Option key={plan.id} value={plan.id}>
                  {plan.name}
                </Select.Option>
              ))}
            </Select>
          </Form.Item>

          <Form.Item
            name="contactEmail"
            label="Contact Email"
            rules={[
              { required: true, message: 'Please enter contact email' },
              { type: 'email', message: 'Please enter a valid email' }
            ]}
          >
            <Input />
          </Form.Item>

          <Form.Item name="contactPhone" label="Contact Phone">
            <Input />
          </Form.Item>

          <Form.Item style={{ marginBottom: 0 }}>
            <Space>
              <Button type="primary" htmlType="submit" loading={createMutation.isPending}>
                {editingOrg ? 'Update' : 'Create'}
              </Button>
              <Button onClick={() => {
                setModalVisible(false)
                form.resetFields()
                setEditingOrg(null)
              }}>
                Cancel
              </Button>
            </Space>
          </Form.Item>
        </Form>
      </Modal>
    </div>
  )
}