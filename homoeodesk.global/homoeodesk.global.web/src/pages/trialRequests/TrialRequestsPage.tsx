import { useState } from 'react'
import { Button, Select, Space, Table, Tag, message } from 'antd'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import dayjs from 'dayjs'
import { trialRequestService, type TrialRequest } from '@core/services/trialRequestService'

const statusColors: Record<string, string> = {
  New: 'blue',
  Contacted: 'orange',
  Converted: 'green',
  Closed: 'default',
}

export const TrialRequestsPage = () => {
  const [statusFilter, setStatusFilter] = useState<string | undefined>()
  const queryClient = useQueryClient()

  const { data, isLoading } = useQuery({
    queryKey: ['trial-requests', statusFilter],
    queryFn: () => trialRequestService.getAll(statusFilter),
  })

  const updateMutation = useMutation({
    mutationFn: ({ id, status }: { id: number; status: string }) =>
      trialRequestService.updateStatus(id, status),
    onSuccess: () => {
      message.success('Status updated')
      queryClient.invalidateQueries({ queryKey: ['trial-requests'] })
    },
    onError: () => message.error('Failed to update status'),
  })

  const columns = [
    {
      title: 'Created',
      dataIndex: 'createdAt',
      key: 'createdAt',
      render: (value: string) => dayjs(value).format('DD MMM YYYY HH:mm'),
      sorter: (a: TrialRequest, b: TrialRequest) =>
        dayjs(a.createdAt).unix() - dayjs(b.createdAt).unix(),
      defaultSortOrder: 'descend' as const,
    },
    { title: 'Clinic', dataIndex: 'clinicName', key: 'clinicName' },
    { title: 'Contact', dataIndex: 'fullName', key: 'fullName' },
    { title: 'Email', dataIndex: 'email', key: 'email' },
    { title: 'Phone', dataIndex: 'phone', key: 'phone' },
    { title: 'City', dataIndex: 'city', key: 'city' },
    { title: 'Volume', dataIndex: 'patientsPerWeek', key: 'patientsPerWeek' },
    { title: 'Source', dataIndex: 'source', key: 'source' },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      render: (status: string) => <Tag color={statusColors[status] || 'default'}>{status}</Tag>,
    },
    {
      title: 'Message',
      dataIndex: 'message',
      key: 'message',
      ellipsis: true,
    },
    {
      title: 'Actions',
      key: 'actions',
      render: (_: unknown, record: TrialRequest) => (
        <Space wrap>
          {['Contacted', 'Converted', 'Closed'].map((status) => (
            <Button
              key={status}
              size="small"
              disabled={record.status === status}
              loading={updateMutation.isPending}
              onClick={() => updateMutation.mutate({ id: record.id, status })}
            >
              Mark {status}
            </Button>
          ))}
        </Space>
      ),
    },
  ]

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 16 }}>
        <h2 style={{ margin: 0 }}>Trial Requests</h2>
        <Select
          allowClear
          placeholder="Filter status"
          style={{ width: 180 }}
          value={statusFilter}
          onChange={setStatusFilter}
          options={['New', 'Contacted', 'Converted', 'Closed'].map((s) => ({ value: s, label: s }))}
        />
      </div>
      <Table
        rowKey="id"
        loading={isLoading}
        dataSource={data}
        columns={columns}
        scroll={{ x: true }}
      />
    </div>
  )
}
