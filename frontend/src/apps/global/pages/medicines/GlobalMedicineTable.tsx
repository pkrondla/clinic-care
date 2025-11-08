import type { TableProps } from 'antd'
import type { Medicine } from '@core/types/medicine'

export const GlobalMedicineTable: TableProps<Medicine>['columns'] = [
  {
    title: 'Name',
    dataIndex: 'name',
    sorter: true
  },
  {
    title: 'Generic Name',
    dataIndex: 'genericName',
    sorter: true
  },
  {
    title: 'Manufacturer',
    dataIndex: 'manufacturer',
    sorter: true
  },
  {
    title: 'Dosage Form',
    dataIndex: 'dosageForm'
  },
  {
    title: 'Strength',
    dataIndex: 'strength'
  },
  {
    title: 'Package Size',
    dataIndex: 'packageSize'
  },
  {
    title: 'Price',
    dataIndex: 'price',
    align: 'right',
    render: (price: number) => `$${price.toFixed(2)}`
  }
]