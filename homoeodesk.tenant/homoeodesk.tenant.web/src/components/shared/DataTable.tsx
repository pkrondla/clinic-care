import { Table, TableProps } from 'antd'
import { LoadingCard } from './LoadingCard'

interface DataTableProps<T> extends Omit<TableProps<T>, 'loading' | 'title'> {
  loading?: boolean
  title?: React.ReactNode
  extra?: React.ReactNode
}

export function DataTable<T extends object>({ 
  loading = false, 
  title,
  extra,
  ...tableProps 
}: DataTableProps<T>) {
  return (
    <LoadingCard loading={loading} title={title} extra={extra}>
      <Table<T>
        {...tableProps}
        pagination={{
          showSizeChanger: true,
          showTotal: (total, range) => `${range[0]}-${range[1]} of ${total} items`,
          ...tableProps.pagination
        }}
      />
    </LoadingCard>
  )
}