import { useState } from 'react'
import { Button, Table, Tag, Space, Input, Modal, Form, message, Select, Popconfirm } from 'antd'
import { PlusOutlined, EditOutlined, DeleteOutlined, SearchOutlined } from '@ant-design/icons'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { globalMedicineService, type GlobalMedicine } from '@core/services/globalMedicineService'
import dayjs from 'dayjs'

const { Option } = Select

const MEDICINE_TYPES = ['Tablet', 'Capsule', 'Syrup', 'Injection', 'Cream', 'Drops', 'Inhaler', 'Other']

export const GlobalMedicinesPage = () => {
  const [searchText, setSearchText] = useState('')
  const [filterType, setFilterType] = useState<string>()
  const [isModalOpen, setIsModalOpen] = useState(false)
  const [editingMedicine, setEditingMedicine] = useState<GlobalMedicine | null>(null)
  const [form] = Form.useForm()
  const queryClient = useQueryClient()

  const { data: medicines, isLoading } = useQuery({
    queryKey: ['global-medicines', searchText, filterType],
    queryFn: () => globalMedicineService.getAll({ 
      searchTerm: searchText || undefined,
      type: filterType 
    })
  })

  const createMutation = useMutation({
    mutationFn: globalMedicineService.create,
    onSuccess: () => {
      message.success('Medicine created successfully')
      queryClient.invalidateQueries({ queryKey: ['global-medicines'] })
      setIsModalOpen(false)
      form.resetFields()
    },
    onError: () => {
      message.error('Failed to create medicine')
    }
  })

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: number; data: any }) => 
      globalMedicineService.update(id, data),
    onSuccess: () => {
      message.success('Medicine updated successfully')
      queryClient.invalidateQueries({ queryKey: ['global-medicines'] })
      setIsModalOpen(false)
      setEditingMedicine(null)
      form.resetFields()
    },
    onError: () => {
      message.error('Failed to update medicine')
    }
  })

  const deleteMutation = useMutation({
    mutationFn: globalMedicineService.delete,
    onSuccess: () => {
      message.success('Medicine deleted successfully')
      queryClient.invalidateQueries({ queryKey: ['global-medicines'] })
    },
    onError: () => {
      message.error('Failed to delete medicine')
    }
  })

  const handleCreate = () => {
    setEditingMedicine(null)
    form.resetFields()
    setIsModalOpen(true)
  }

  const handleEdit = (medicine: GlobalMedicine) => {
    setEditingMedicine(medicine)
    form.setFieldsValue(medicine)
    setIsModalOpen(true)
  }

  const handleDelete = (id: number) => {
    deleteMutation.mutate(id)
  }

  const handleSubmit = async () => {
    try {
      const values = await form.validateFields()
      
      if (editingMedicine) {
        updateMutation.mutate({ id: editingMedicine.id, data: values })
      } else {
        createMutation.mutate(values)
      }
    } catch (error) {
      console.error('Validation failed:', error)
    }
  }

  const columns = [
    {
      title: 'Name',
      dataIndex: 'name',
      key: 'name',
      sorter: (a: GlobalMedicine, b: GlobalMedicine) => a.name.localeCompare(b.name)
    },
    {
      title: 'Generic Name',
      dataIndex: 'genericName',
      key: 'genericName'
    },
    {
      title: 'Type',
      dataIndex: 'type',
      key: 'type',
      render: (type: string) => <Tag color="blue">{type}</Tag>
    },
    {
      title: 'Potency',
      dataIndex: 'potency',
      key: 'potency'
    },
    {
      title: 'Manufacturer',
      dataIndex: 'manufacturer',
      key: 'manufacturer'
    },
    {
      title: 'Price',
      dataIndex: 'price',
      key: 'price',
      render: (price: number) => `$${price.toFixed(2)}`,
      sorter: (a: GlobalMedicine, b: GlobalMedicine) => a.price - b.price
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
      render: (_: any, record: GlobalMedicine) => (
        <Space>
          <Button
            type="link"
            icon={<EditOutlined />}
            onClick={() => handleEdit(record)}
          >
            Edit
          </Button>
          <Popconfirm
            title="Delete Medicine"
            description="Are you sure you want to delete this medicine?"
            onConfirm={() => handleDelete(record.id)}
            okText="Yes"
            cancelText="No"
          >
            <Button
              type="link"
              danger
              icon={<DeleteOutlined />}
            >
              Delete
            </Button>
          </Popconfirm>
        </Space>
      )
    }
  ]

  return (
    <div style={{ padding: '24px' }}>
      <div style={{ marginBottom: '24px', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <div>
          <h1 style={{ margin: 0, fontSize: '24px', fontWeight: 600 }}>Global Medicines</h1>
          <p style={{ margin: '8px 0 0 0', color: '#666' }}>
            Manage global medicine catalog for all clinics
          </p>
        </div>
        <Button
          type="primary"
          icon={<PlusOutlined />}
          onClick={handleCreate}
          size="large"
        >
          Add Medicine
        </Button>
      </div>

      <div style={{ marginBottom: '16px', display: 'flex', gap: '16px' }}>
        <Input
          placeholder="Search by name, generic name, or manufacturer..."
          prefix={<SearchOutlined />}
          value={searchText}
          onChange={(e) => setSearchText(e.target.value)}
          style={{ maxWidth: '400px' }}
          size="large"
        />
        <Select
          placeholder="Filter by type"
          allowClear
          value={filterType}
          onChange={setFilterType}
          style={{ width: '200px' }}
          size="large"
        >
          {MEDICINE_TYPES.map(type => (
            <Option key={type} value={type}>{type}</Option>
          ))}
        </Select>
      </div>

      <Table
        columns={columns}
        dataSource={medicines}
        loading={isLoading}
        rowKey="id"
        pagination={{
          pageSize: 10,
          showSizeChanger: true,
          showTotal: (total) => `Total ${total} medicines`
        }}
      />

      <Modal
        title={editingMedicine ? 'Edit Medicine' : 'Add New Medicine'}
        open={isModalOpen}
        onOk={handleSubmit}
        onCancel={() => {
          setIsModalOpen(false)
          setEditingMedicine(null)
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
            label="Medicine Name"
            name="name"
            rules={[{ required: true, message: 'Please enter medicine name' }]}
          >
            <Input placeholder="e.g., Paracetamol" />
          </Form.Item>

          <Form.Item
            label="Generic Name"
            name="genericName"
            rules={[{ required: true, message: 'Please enter generic name' }]}
          >
            <Input placeholder="e.g., Acetaminophen" />
          </Form.Item>

          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '16px' }}>
            <Form.Item
              label="Type"
              name="type"
              rules={[{ required: true, message: 'Please select type' }]}
            >
              <Select placeholder="Select type">
                {MEDICINE_TYPES.map(type => (
                  <Option key={type} value={type}>{type}</Option>
                ))}
              </Select>
            </Form.Item>

            <Form.Item
              label="Potency"
              name="potency"
              rules={[{ required: true, message: 'Please enter potency' }]}
            >
              <Input placeholder="e.g., 500mg" />
            </Form.Item>
          </div>

          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '16px' }}>
            <Form.Item
              label="Manufacturer"
              name="manufacturer"
              rules={[{ required: true, message: 'Please enter manufacturer' }]}
            >
              <Input placeholder="e.g., GSK Pharmaceuticals" />
            </Form.Item>

            <Form.Item
              label="Price ($)"
              name="price"
              rules={[{ required: true, message: 'Please enter price' }]}
            >
              <Input type="number" step="0.01" placeholder="0.00" />
            </Form.Item>
          </div>

          <Form.Item
            label="Description"
            name="description"
          >
            <Input.TextArea 
              rows={3} 
              placeholder="Medicine description, usage, or notes"
            />
          </Form.Item>

          {editingMedicine && (
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
