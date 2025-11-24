import { useState } from 'react';
import {
  Card,
  Row,
  Col,
  Statistic,
  Select,
  Button,
  Table,
  Typography,
  Space,
  Tabs,
  Tag,
  Alert,
  Switch,
  Empty,
} from 'antd';
import {
  MedicineBoxOutlined,
  WarningOutlined,
  DollarOutlined,
  ReloadOutlined,
  DownloadOutlined,
  ShopOutlined,
} from '@ant-design/icons';
import { useInventoryReport } from '@core/hooks/queries/useReports';
import { useClinics } from '@core/hooks/queries/useClinics';
import { useSelectedClinic } from '@core/stores/authStore';
import dayjs from 'dayjs';
import type { 
  CombinedInventoryItemDto,
  ClinicInventoryDto,
  LowStockAlertDto,
  StockMovementDto,
  ClinicStockDto
} from '@core/services/reportsService';

const { Title } = Typography;

export const InventoryReportPage = () => {
  const selectedClinic = useSelectedClinic();
  const [filters, setFilters] = useState({
    clinicId: undefined as number | undefined,
    lowStockOnly: false,
  });

  const { data: report, isLoading, refetch } = useInventoryReport({
    clinicId: filters.clinicId,
    lowStockOnly: filters.lowStockOnly || undefined,
  });

  const { data: clinics = [] } = useClinics();

  const combinedInventoryColumns = [
    {
      title: 'Medicine',
      dataIndex: 'medicineName',
      key: 'medicineName',
      sorter: (a: CombinedInventoryItemDto, b: CombinedInventoryItemDto) => 
        a.medicineName.localeCompare(b.medicineName),
    },
    {
      title: 'Code',
      dataIndex: 'medicineCode',
      key: 'medicineCode',
    },
    {
      title: 'Total Qty',
      dataIndex: 'totalQuantity',
      key: 'totalQuantity',
      render: (qty: number, record: CombinedInventoryItemDto) => `${qty} ${record.unit}`,
    },
    {
      title: 'Available',
      dataIndex: 'availableQuantity',
      key: 'availableQuantity',
      render: (qty: number, record: CombinedInventoryItemDto) => `${qty} ${record.unit}`,
    },
    {
      title: 'Clinics',
      dataIndex: 'clinicCount',
      key: 'clinicCount',
    },
    {
      title: 'Avg Price',
      dataIndex: 'averagePrice',
      key: 'averagePrice',
      render: (price: number) => `₹${price.toFixed(2)}`,
    },
    {
      title: 'Total Value',
      dataIndex: 'totalValue',
      key: 'totalValue',
      render: (value: number) => `₹${value.toFixed(2)}`,
      sorter: (a: CombinedInventoryItemDto, b: CombinedInventoryItemDto) => 
        a.totalValue - b.totalValue,
    },
  ];

  const clinicInventoryColumns = [
    {
      title: 'Clinic',
      dataIndex: 'clinicName',
      key: 'clinicName',
    },
    {
      title: 'Medicines',
      dataIndex: 'medicineCount',
      key: 'medicineCount',
    },
    {
      title: 'Total Value',
      dataIndex: 'totalValue',
      key: 'totalValue',
      render: (value: number) => `₹${value.toFixed(2)}`,
      sorter: (a: ClinicInventoryDto, b: ClinicInventoryDto) => 
        a.totalValue - b.totalValue,
    },
    {
      title: 'Low Stock',
      dataIndex: 'lowStockCount',
      key: 'lowStockCount',
      render: (count: number) => (
        <Tag color="orange">{count}</Tag>
      ),
    },
    {
      title: 'Out of Stock',
      dataIndex: 'outOfStockCount',
      key: 'outOfStockCount',
      render: (count: number) => (
        <Tag color="red">{count}</Tag>
      ),
    },
  ];

  const lowStockColumns = [
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
      title: 'Code',
      dataIndex: 'medicineCode',
      key: 'medicineCode',
    },
    {
      title: 'Current Stock',
      dataIndex: 'currentStock',
      key: 'currentStock',
      render: (stock: number, record: LowStockAlertDto) => (
        <span style={{ color: stock === 0 ? '#ff4d4f' : '#faad14' }}>
          {stock} {record.unit}
        </span>
      ),
    },
    {
      title: 'Reorder Level',
      dataIndex: 'reorderLevel',
      key: 'reorderLevel',
      render: (level: number, record: LowStockAlertDto) => `${level} ${record.unit}`,
    },
    {
      title: 'Required Qty',
      dataIndex: 'requiredQuantity',
      key: 'requiredQuantity',
      render: (qty: number, record: LowStockAlertDto) => (
        <Tag color="blue">{qty} {record.unit}</Tag>
      ),
    },
  ];

  const stockMovementColumns = [
    {
      title: 'Date',
      dataIndex: 'date',
      key: 'date',
      render: (date: string) => dayjs(date).format('MMM DD, YYYY HH:mm'),
    },
    {
      title: 'Type',
      dataIndex: 'transactionType',
      key: 'transactionType',
      render: (type: string) => <Tag>{type}</Tag>,
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
      title: 'Quantity',
      key: 'quantity',
      render: (_: any, record: StockMovementDto) => {
        const isPositive = record.transactionType === 'Purchase' || record.transactionType === 'Transfer';
        return (
          <span style={{ color: isPositive ? '#52c41a' : '#ff4d4f' }}>
            {isPositive ? '+' : '-'}{record.quantity} {record.unit}
          </span>
        );
      },
    },
    {
      title: 'Unit Price',
      dataIndex: 'unitPrice',
      key: 'unitPrice',
      render: (price: number) => `₹${price.toFixed(2)}`,
    },
    {
      title: 'Total Value',
      dataIndex: 'totalValue',
      key: 'totalValue',
      render: (value: number) => `₹${value.toFixed(2)}`,
    },
    {
      title: 'Reference',
      dataIndex: 'reference',
      key: 'reference',
      ellipsis: true,
    },
  ];

  const tabItems = [
    {
      key: 'summary',
      label: 'Summary',
      children: (
        <div>
          <Row gutter={16} style={{ marginBottom: 24 }}>
            <Col span={6}>
              <Card>
                <Statistic
                  title="Total Inventory Value"
                  value={report?.totalInventoryValue || 0}
                  prefix={<DollarOutlined />}
                  precision={2}
                  valueStyle={{ color: '#3f8600' }}
                />
              </Card>
            </Col>
            <Col span={6}>
              <Card>
                <Statistic
                  title="Total Medicines"
                  value={report?.totalMedicines || 0}
                  prefix={<MedicineBoxOutlined />}
                />
              </Card>
            </Col>
            <Col span={6}>
              <Card>
                <Statistic
                  title="Low Stock Items"
                  value={report?.lowStockItems || 0}
                  prefix={<WarningOutlined />}
                  valueStyle={{ color: '#faad14' }}
                />
              </Card>
            </Col>
            <Col span={6}>
              <Card>
                <Statistic
                  title="Out of Stock"
                  value={report?.outOfStockItems || 0}
                  prefix={<WarningOutlined />}
                  valueStyle={{ color: '#cf1322' }}
                />
              </Card>
            </Col>
          </Row>

          {report && report.lowStockItems > 0 && (
            <Alert
              message="Low Stock Alert"
              description={`${report.lowStockItems} items are below reorder level. Please review the Low Stock Alerts tab.`}
              type="warning"
              showIcon
              style={{ marginBottom: 16 }}
            />
          )}

          <Card title="Inventory by Clinic">
            <Table
              columns={clinicInventoryColumns}
              dataSource={report?.clinicInventory || []}
              rowKey="clinicId"
              loading={isLoading}
              pagination={false}
              expandable={{
                expandedRowRender: (record: ClinicInventoryDto) => (
                  <Table
                    columns={[
                      { title: 'Medicine', dataIndex: 'medicineName', key: 'medicineName' },
                      { 
                        title: 'Available', 
                        key: 'available',
                        render: (_: any, stock: ClinicStockDto) => `${stock.availableQuantity} ${stock.unitPrice > 0 ? 'units' : ''}`,
                      },
                      { title: 'Unit Price', dataIndex: 'unitPrice', key: 'unitPrice', render: (p: number) => `₹${p.toFixed(2)}` },
                      { title: 'Value', dataIndex: 'totalValue', key: 'totalValue', render: (v: number) => `₹${v.toFixed(2)}` },
                      { 
                        title: 'Status', 
                        key: 'status',
                        render: (_: any, stock: ClinicStockDto) => (
                          stock.isLowStock ? <Tag color="orange">Low Stock</Tag> : <Tag color="green">In Stock</Tag>
                        ),
                      },
                    ]}
                    dataSource={record.stocks}
                    rowKey={(stock, index) => `${stock.clinicId}-${index}`}
                    pagination={false}
                    size="small"
                  />
                ),
              }}
            />
          </Card>
        </div>
      ),
    },
    {
      key: 'combined',
      label: 'Combined Inventory',
      children: (
        <Table
          columns={combinedInventoryColumns}
          dataSource={report?.combinedInventory || []}
          rowKey="medicineId"
          loading={isLoading}
          pagination={{ pageSize: 20 }}
          expandable={{
            expandedRowRender: (record: CombinedInventoryItemDto) => (
              <div style={{ padding: '16px' }}>
                <h4>Stock by Clinic:</h4>
                <Table
                  columns={[
                    { title: 'Clinic', dataIndex: 'clinicName', key: 'clinicName' },
                    { title: 'Available', dataIndex: 'availableQuantity', key: 'availableQuantity', render: (q: number) => `${q} units` },
                    { title: 'Unit Price', dataIndex: 'unitPrice', key: 'unitPrice', render: (p: number) => `₹${p.toFixed(2)}` },
                    { title: 'Value', dataIndex: 'totalValue', key: 'totalValue', render: (v: number) => `₹${v.toFixed(2)}` },
                    { 
                      title: 'Status', 
                      key: 'status',
                      render: (_: any, stock: ClinicStockDto) => (
                        stock.isLowStock ? <Tag color="orange">Low Stock</Tag> : <Tag color="green">In Stock</Tag>
                      ),
                    },
                  ]}
                  dataSource={record.clinicStocks}
                  rowKey="clinicId"
                  pagination={false}
                  size="small"
                />
              </div>
            ),
          }}
        />
      ),
    },
    {
      key: 'lowStock',
      label: 'Low Stock Alerts',
      children: (
        <div>
          {report && report.lowStockAlerts.length > 0 ? (
            <Table
              columns={lowStockColumns}
              dataSource={report.lowStockAlerts}
              rowKey={(record, index) => `${record.clinicId}-${record.medicineId}-${index}`}
              loading={isLoading}
              pagination={{ pageSize: 20 }}
            />
          ) : (
            <Empty description="No low stock items found" />
          )}
        </div>
      ),
    },
    {
      key: 'movements',
      label: 'Stock Movements',
      children: (
        <Table
          columns={stockMovementColumns}
          dataSource={report?.stockMovements || []}
          rowKey={(record, index) => `${record.date}-${record.medicineName}-${index}`}
          loading={isLoading}
          pagination={{ pageSize: 20 }}
        />
      ),
    },
  ];

  return (
    <div>
      <Card>
        <Row justify="space-between" align="middle" style={{ marginBottom: 16 }}>
          <Col>
            <Title level={2} style={{ margin: 0 }}>
              Inventory Report
            </Title>
          </Col>
          <Col>
            <Space>
              <Button icon={<ReloadOutlined />} onClick={() => refetch()}>
                Refresh
              </Button>
              <Button icon={<DownloadOutlined />} type="primary">
                Export
              </Button>
            </Space>
          </Col>
        </Row>

        {/* Filters */}
        <Card size="small" style={{ marginBottom: 16 }}>
          <Row gutter={16}>
            <Col span={8}>
              <Select
                placeholder="Select Clinic (All if not selected)"
                style={{ width: '100%' }}
                allowClear
                value={filters.clinicId}
                onChange={(value) => setFilters({ ...filters, clinicId: value })}
              >
                {clinics.map((clinic) => (
                  <Select.Option key={clinic.id} value={clinic.id}>
                    {clinic.name}
                  </Select.Option>
                ))}
              </Select>
            </Col>
            <Col span={8}>
              <Space>
                <span>Low Stock Only:</span>
                <Switch
                  checked={filters.lowStockOnly}
                  onChange={(checked) => setFilters({ ...filters, lowStockOnly: checked })}
                />
              </Space>
            </Col>
          </Row>
        </Card>

        {report && (
          <Tabs items={tabItems} />
        )}

        {!report && !isLoading && (
          <Card>
            <Empty description="No inventory data available" />
          </Card>
        )}
      </Card>
    </div>
  );
};

