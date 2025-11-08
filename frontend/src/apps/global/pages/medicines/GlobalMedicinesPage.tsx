import { Card, Row, Col, Button, Space } from 'antd'
import { PlusOutlined } from '@ant-design/icons'
import { useGlobalMedicines } from '@core/hooks/queries/useGlobalMedicines'
import { LoadingCard } from '@components/shared/LoadingCard'
import { DataTable } from '@components/shared/DataTable'
import { GlobalMedicineTable } from './GlobalMedicineTable'

export const GlobalMedicinesPage = () => {
  const { data: medicines, isLoading } = useGlobalMedicines()

  return (
    <Row gutter={[24, 24]}>
      <Col span={24}>
        <Card 
          title="Global Medicine Database"
          extra={
            <Space>
              <Button type="primary" icon={<PlusOutlined />}>
                Add Medicine
              </Button>
            </Space>
          }
        >
          <LoadingCard loading={isLoading}>
            <DataTable
              dataSource={medicines}
              columns={GlobalMedicineTable}
              rowKey="id"
              scroll={{ x: true }}
            />
          </LoadingCard>
        </Card>
      </Col>
    </Row>
  )
}