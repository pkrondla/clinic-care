import { Card, Typography, Tabs } from 'antd'
import { CollectionReportPage } from './CollectionReportPage'
import { PatientReportPage } from './PatientReportPage'
import { InventoryReportPage } from './InventoryReportPage'

const { Title } = Typography

export const ReportsPage = () => {
  const tabItems = [
    {
      key: 'collection',
      label: 'Collection Report',
      children: <CollectionReportPage />,
    },
    {
      key: 'patient',
      label: 'Patient Reports',
      children: <PatientReportPage />,
    },
    {
      key: 'inventory',
      label: 'Inventory Reports',
      children: <InventoryReportPage />,
    },
  ]

  return (
    <div>
      <Title level={2}>Reports</Title>
      <Card>
        <Tabs items={tabItems} />
      </Card>
    </div>
  )
}

