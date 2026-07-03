import { useState } from 'react'
import {
  Card,
  Table,
  Button,
  Input,
  InputNumber,
  Space,
  Tag,
  Typography,
  Row,
  Col,
  Modal,
  Form,
  message,
  Divider,
  Statistic,
  DatePicker,
} from 'antd'
import {
  CheckOutlined,
  ReloadOutlined,
  HistoryOutlined,
  FileSearchOutlined,
} from '@ant-design/icons'
import { useStockAuditHistory, usePerformStockAudit } from '@core/hooks/queries/useStockAudit'
import { inventoryService } from '@core/services/inventoryService'
import { useSelectedBranch } from '@core/stores/authStore'
import type { InventoryItem } from '@core/services/inventoryService'
import type { StockAuditItem } from '@core/services/stockAuditService'
import dayjs from 'dayjs'

const { Title } = Typography
const { TextArea } = Input

interface AuditItemForm extends StockAuditItem {
  medicineName: string
  systemStock: number
  variance: number
}

export const StockAuditPage = () => {
  const selectedClinic = useSelectedBranch()
  const [auditMode, setAuditMode] = useState<'audit' | 'history'>('audit')
  const [auditItems, setAuditItems] = useState<AuditItemForm[]>([])
  const [inventoryItems, setInventoryItems] = useState<InventoryItem[]>([])
  const [loadingInventory, setLoadingInventory] = useState(false)
  const [historyModalVisible, setHistoryModalVisible] = useState(false)
  const [form] = Form.useForm()

  const performAuditMutation = usePerformStockAudit()
  const { data: auditHistory = [], isLoading: loadingHistory } = useStockAuditHistory({
    BranchId: selectedClinic?.id,
  })

  const loadInventory = async () => {
    if (!selectedClinic?.id) {
      message.warning('Please select a clinic first')
      return
    }

    setLoadingInventory(true)
    try {
      const items = await inventoryService.getAll(selectedClinic.id)
      setInventoryItems(items)
      
      // Initialize audit items with current system stock
      const initialAuditItems: AuditItemForm[] = items.map(item => ({
        inventoryId: item.id,
        medicineName: item.medicineName,
        systemStock: item.currentStock,
        physicalStock: item.currentStock, // Default to system stock
        variance: 0,
        notes: '',
      }))
      setAuditItems(initialAuditItems)
    } catch (error) {
      message.error('Failed to load inventory')
    } finally {
      setLoadingInventory(false)
    }
  }

  const handlePhysicalStockChange = (inventoryId: number, physicalStock: number) => {
    setAuditItems(items =>
      items.map(item => {
        if (item.inventoryId === inventoryId) {
          const variance = physicalStock - item.systemStock
          return { ...item, physicalStock, variance }
        }
        return item
      })
    )
  }

  const handleNotesChange = (inventoryId: number, notes: string) => {
    setAuditItems(items =>
      items.map(item =>
        item.inventoryId === inventoryId ? { ...item, notes } : item
      )
    )
  }

  const handleSubmitAudit = async () => {
    if (!selectedClinic?.id) {
      message.error('Please select a clinic')
      return
    }

    if (auditItems.length === 0) {
      message.error('Please load inventory items first')
      return
    }

    try {
      const values = await form.validateFields()
      
      const request = {
        BranchId: selectedClinic.id,
        auditItems: auditItems.map(item => ({
          inventoryId: item.inventoryId,
          physicalStock: item.physicalStock,
          notes: item.notes,
        })),
        notes: values.notes,
      }

      const result = await performAuditMutation.mutateAsync(request)
      
      // Show success message with details
      message.success(
        `Stock audit completed! ${result.itemsWithVariance} item(s) adjusted out of ${result.totalItemsAudited} audited.`
      )
      
      // Reload inventory to reflect changes
      await loadInventory()
    } catch (error) {
      console.error('Audit submission failed:', error)
    }
  }

  const itemsWithVariance = auditItems.filter(item => item.variance !== 0).length
  const totalVariance = auditItems.reduce((sum, item) => sum + Math.abs(item.variance), 0)

  const auditColumns = [
    {
      title: 'Medicine',
      dataIndex: 'medicineName',
      key: 'medicineName',
      width: 200,
    },
    {
      title: 'System Stock',
      dataIndex: 'systemStock',
      key: 'systemStock',
      width: 120,
      align: 'right' as const,
      render: (stock: number) => <strong>{stock}</strong>,
    },
    {
      title: 'Physical Stock',
      key: 'physicalStock',
      width: 150,
      render: (_: any, record: AuditItemForm) => (
        <InputNumber
          min={0}
          value={record.physicalStock}
          onChange={(value) => handlePhysicalStockChange(record.inventoryId, value || 0)}
          style={{ width: '100%' }}
        />
      ),
    },
    {
      title: 'Variance',
      dataIndex: 'variance',
      key: 'variance',
      width: 120,
      align: 'right' as const,
      render: (variance: number) => {
        if (variance === 0) return <Tag color="green">0</Tag>
        if (variance > 0)
          return <Tag color="blue">+{variance}</Tag>
        return <Tag color="red">{variance}</Tag>
      },
    },
    {
      title: 'Notes',
      key: 'notes',
      render: (_: any, record: AuditItemForm) => (
        <Input
          placeholder="Optional notes"
          value={record.notes}
          onChange={(e) => handleNotesChange(record.inventoryId, e.target.value)}
          style={{ width: '100%' }}
        />
      ),
    },
  ]

  const historyColumns = [
    {
      title: 'Date',
      dataIndex: 'auditDate',
      key: 'auditDate',
      width: 150,
      render: (date: string) => dayjs(date).format('DD/MM/YYYY HH:mm'),
    },
    {
      title: 'Clinic',
      dataIndex: 'clinicName',
      key: 'clinicName',
    },
    {
      title: 'Medicine',
      dataIndex: 'medicineName',
      key: 'medicineName',
    },
    {
      title: 'System Stock',
      dataIndex: 'systemStock',
      key: 'systemStock',
      width: 120,
      align: 'right' as const,
    },
    {
      title: 'Physical Stock',
      dataIndex: 'physicalStock',
      key: 'physicalStock',
      width: 120,
      align: 'right' as const,
    },
    {
      title: 'Variance',
      dataIndex: 'variance',
      key: 'variance',
      width: 120,
      align: 'right' as const,
      render: (variance: number) => {
        if (variance === 0) return <Tag color="green">0</Tag>
        if (variance > 0)
          return <Tag color="blue">+{variance}</Tag>
        return <Tag color="red">{variance}</Tag>
      },
    },
  ]

  return (
    <div>
      <Card>
        <Row justify="space-between" align="middle" style={{ marginBottom: 16 }}>
          <Col>
            <Title level={2} style={{ margin: 0 }}>
              Stock Audit
            </Title>
          </Col>
          <Col>
            <Space>
              <Button
                icon={<HistoryOutlined />}
                onClick={() => setHistoryModalVisible(true)}
              >
                View History
              </Button>
              <Button
                icon={<ReloadOutlined />}
                onClick={loadInventory}
                loading={loadingInventory}
                disabled={!selectedClinic?.id}
              >
                Load Inventory
              </Button>
            </Space>
          </Col>
        </Row>

        {!selectedClinic && (
          <div style={{ textAlign: 'center', padding: '40px', color: '#999' }}>
            <p>Please select a clinic to perform stock audit</p>
          </div>
        )}

        {selectedClinic && auditItems.length > 0 && (
          <>
            {/* Statistics */}
            <Row gutter={16} style={{ marginBottom: 24 }}>
              <Col xs={24} sm={8}>
                <Card>
                  <Statistic
                    title="Total Items"
                    value={auditItems.length}
                    prefix={<FileSearchOutlined />}
                  />
                </Card>
              </Col>
              <Col xs={24} sm={8}>
                <Card>
                  <Statistic
                    title="Items with Variance"
                    value={itemsWithVariance}
                    valueStyle={{ color: itemsWithVariance > 0 ? '#faad14' : '#3f8600' }}
                  />
                </Card>
              </Col>
              <Col xs={24} sm={8}>
                <Card>
                  <Statistic
                    title="Total Variance"
                    value={totalVariance}
                    valueStyle={{ color: totalVariance > 0 ? '#faad14' : '#3f8600' }}
                  />
                </Card>
              </Col>
            </Row>

            {/* Audit Form */}
            <Form form={form} layout="vertical">
              <Table
                columns={auditColumns}
                dataSource={auditItems}
                rowKey="inventoryId"
                pagination={false}
                scroll={{ y: 400 }}
                summary={() => (
                  <Table.Summary fixed>
                    <Table.Summary.Row>
                      <Table.Summary.Cell index={0} colSpan={2} align="right">
                        <strong>Total Items: {auditItems.length}</strong>
                      </Table.Summary.Cell>
                      <Table.Summary.Cell index={1} align="right">
                        <strong>
                          {auditItems.reduce((sum, item) => sum + item.physicalStock, 0)}
                        </strong>
                      </Table.Summary.Cell>
                      <Table.Summary.Cell index={2} align="right">
                        <strong>
                          {auditItems.reduce((sum, item) => sum + item.variance, 0)}
                        </strong>
                      </Table.Summary.Cell>
                      <Table.Summary.Cell index={3} />
                    </Table.Summary.Row>
                  </Table.Summary>
                )}
              />

              <Divider />

              <Form.Item label="Audit Notes" name="notes">
                <TextArea rows={3} placeholder="Enter any additional notes about this audit" />
              </Form.Item>

              <Form.Item>
                <Space>
                  <Button
                    type="primary"
                    icon={<CheckOutlined />}
                    size="large"
                    onClick={handleSubmitAudit}
                    loading={performAuditMutation.isPending}
                  >
                    Complete Audit
                  </Button>
                  <Button onClick={() => loadInventory()}>Reset</Button>
                </Space>
              </Form.Item>
            </Form>
          </>
        )}

        {selectedClinic && auditItems.length === 0 && !loadingInventory && (
          <div style={{ textAlign: 'center', padding: '40px', color: '#999' }}>
            <p>Click "Load Inventory" to start the stock audit</p>
          </div>
        )}
      </Card>

      {/* Audit History Modal */}
      <Modal
        title="Stock Audit History"
        open={historyModalVisible}
        onCancel={() => setHistoryModalVisible(false)}
        width={1000}
        footer={null}
      >
        <Table
          columns={historyColumns}
          dataSource={auditHistory}
          rowKey="id"
          loading={loadingHistory}
          pagination={{
            pageSize: 10,
            showSizeChanger: true,
            showTotal: (total) => `Total ${total} audit records`,
          }}
        />
      </Modal>
    </div>
  )
}

