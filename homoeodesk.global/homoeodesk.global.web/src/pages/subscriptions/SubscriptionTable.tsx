import type { TableProps } from 'antd'
import type { Subscription } from '@core/types/subscription'

export const SubscriptionTable: TableProps<Subscription>['columns'] = [
  {
    title: 'Organization',
    dataIndex: ['organization', 'name'],
    sorter: true
  },
  {
    title: 'Plan',
    dataIndex: 'plan',
    sorter: true
  },
  {
    title: 'Status',
    dataIndex: 'status',
    sorter: true
  },
  {
    title: 'Start Date',
    dataIndex: 'startDate',
    render: (date: string) => new Date(date).toLocaleDateString()
  },
  {
    title: 'End Date',
    dataIndex: 'endDate',
    render: (date: string) => new Date(date).toLocaleDateString()
  },
  {
    title: 'Price',
    dataIndex: 'price',
    align: 'right',
    render: (price: number) => `$${price.toFixed(2)}`
  }
]