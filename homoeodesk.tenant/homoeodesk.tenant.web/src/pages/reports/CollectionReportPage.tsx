import { useState } from 'react';
import {
  Card,
  Row,
  Col,
  Statistic,
  DatePicker,
  Select,
  Button,
  Table,
  Tag,
  Typography,
  Space,
  Divider,
} from 'antd';
import {
  DollarOutlined,
  FileTextOutlined,
  CheckCircleOutlined,
  ClockCircleOutlined,
  DownloadOutlined,
  ReloadOutlined,
} from '@ant-design/icons';
import { useCollectionReport } from '@core/hooks/queries/useReports';
import { useBranches } from '@core/hooks/queries/useBranches';
import { useDoctors } from '@core/hooks/queries/useDoctors';
import { useSelectedBranch } from '@core/stores/authStore';
import dayjs from 'dayjs';
import type { CollectionReportItemDto, PaymentMethodBreakdownDto, DailyCollectionDto } from '@core/services/reportsService';
import {
  LineChart,
  Line,
  BarChart,
  Bar,
  PieChart,
  Pie,
  Cell,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
} from 'recharts';

const { Title } = Typography;
const { RangePicker } = DatePicker;

const COLORS = ['#0088FE', '#00C49F', '#FFBB28', '#FF8042', '#8884d8', '#82ca9d'];

export const CollectionReportPage = () => {
  const selectedClinic = useSelectedBranch();
  const [filters, setFilters] = useState({
    BranchId: selectedClinic?.id as number | undefined,
    doctorId: undefined as number | undefined,
    startDate: dayjs().startOf('month').format('YYYY-MM-DD'),
    endDate: dayjs().endOf('month').format('YYYY-MM-DD'),
    groupBy: 'day' as 'day' | 'week' | 'month' | 'clinic' | 'doctor' | 'paymentMethod',
  });

  const { data: report, isLoading, refetch } = useCollectionReport({
    BranchId: filters.BranchId,
    doctorId: filters.doctorId,
    startDate: filters.startDate,
    endDate: filters.endDate,
    groupBy: filters.groupBy,
  });

  const { data: Branches = [] } = useBranches();
  const { data: doctors = [] } = useDoctors({ BranchId: filters.BranchId });

  const handleDateRangeChange = (dates: any) => {
    if (dates && dates[0] && dates[1]) {
      setFilters({
        ...filters,
        startDate: dates[0].format('YYYY-MM-DD'),
        endDate: dates[1].format('YYYY-MM-DD'),
      });
    }
  };

  const itemColumns = [
    {
      title: 'Period',
      dataIndex: 'groupKey',
      key: 'groupKey',
    },
    {
      title: 'Total Amount',
      dataIndex: 'totalAmount',
      key: 'totalAmount',
      render: (amount: number) => `₹${amount.toFixed(2)}`,
    },
    {
      title: 'Paid Amount',
      dataIndex: 'paidAmount',
      key: 'paidAmount',
      render: (amount: number) => <span style={{ color: '#52c41a' }}>₹{amount.toFixed(2)}</span>,
    },
    {
      title: 'Balance',
      dataIndex: 'balanceAmount',
      key: 'balanceAmount',
      render: (amount: number) => (
        <span style={{ color: amount > 0 ? '#ff4d4f' : '#52c41a' }}>
          ₹{amount.toFixed(2)}
        </span>
      ),
    },
    {
      title: 'Invoices',
      dataIndex: 'invoiceCount',
      key: 'invoiceCount',
    },
  ];

  const paymentMethodColumns = [
    {
      title: 'Payment Method',
      dataIndex: 'paymentMethod',
      key: 'paymentMethod',
    },
    {
      title: 'Amount',
      dataIndex: 'amount',
      key: 'amount',
      render: (amount: number) => `₹${amount.toFixed(2)}`,
    },
    {
      title: 'Count',
      dataIndex: 'count',
      key: 'count',
    },
    {
      title: 'Percentage',
      dataIndex: 'percentage',
      key: 'percentage',
      render: (percentage: number) => `${percentage.toFixed(1)}%`,
    },
  ];

  return (
    <div>
      <Card>
        <Row justify="space-between" align="middle" style={{ marginBottom: 16 }}>
          <Col>
            <Title level={2} style={{ margin: 0 }}>
              Collection Report
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
            <Col span={6}>
              <RangePicker
                style={{ width: '100%' }}
                value={[
                  filters.startDate ? dayjs(filters.startDate) : null,
                  filters.endDate ? dayjs(filters.endDate) : null,
                ]}
                onChange={handleDateRangeChange}
              />
            </Col>
            <Col span={6}>
              <Select
                placeholder="Select Clinic"
                style={{ width: '100%' }}
                allowClear
                value={filters.BranchId}
                onChange={(value) => setFilters({ ...filters, BranchId: value })}
              >
                {Branches.map((clinic) => (
                  <Select.Option key={clinic.id} value={clinic.id}>
                    {clinic.name}
                  </Select.Option>
                ))}
              </Select>
            </Col>
            <Col span={6}>
              <Select
                placeholder="Select Doctor"
                style={{ width: '100%' }}
                allowClear
                value={filters.doctorId}
                onChange={(value) => setFilters({ ...filters, doctorId: value })}
              >
                {doctors.map((doctor) => (
                  <Select.Option key={doctor.id} value={doctor.id}>
                    {doctor.name}
                  </Select.Option>
                ))}
              </Select>
            </Col>
            <Col span={6}>
              <Select
                placeholder="Group By"
                style={{ width: '100%' }}
                value={filters.groupBy}
                onChange={(value) => setFilters({ ...filters, groupBy: value })}
              >
                <Select.Option value="day">Day</Select.Option>
                <Select.Option value="week">Week</Select.Option>
                <Select.Option value="month">Month</Select.Option>
                <Select.Option value="clinic">Clinic</Select.Option>
                <Select.Option value="doctor">Doctor</Select.Option>
              </Select>
            </Col>
          </Row>
        </Card>

        {/* Summary Statistics */}
        {report && (
          <Row gutter={16} style={{ marginBottom: 24 }}>
            <Col span={6}>
              <Card>
                <Statistic
                  title="Total Collection"
                  value={report.totalCollection}
                  prefix={<DollarOutlined />}
                  precision={2}
                  valueStyle={{ color: '#3f8600' }}
                />
              </Card>
            </Col>
            <Col span={6}>
              <Card>
                <Statistic
                  title="Pending Amount"
                  value={report.totalPending}
                  prefix={<ClockCircleOutlined />}
                  precision={2}
                  valueStyle={{ color: '#cf1322' }}
                />
              </Card>
            </Col>
            <Col span={6}>
              <Card>
                <Statistic
                  title="Total Invoices"
                  value={report.totalInvoices}
                  prefix={<FileTextOutlined />}
                />
              </Card>
            </Col>
            <Col span={6}>
              <Card>
                <Statistic
                  title="Paid Invoices"
                  value={report.paidInvoices}
                  prefix={<CheckCircleOutlined />}
                  valueStyle={{ color: '#3f8600' }}
                />
              </Card>
            </Col>
          </Row>
        )}

        {/* Charts */}
        {report && (
          <Row gutter={16} style={{ marginBottom: 24 }}>
            <Col span={12}>
              <Card title="Daily Collections" style={{ height: 400 }}>
                <ResponsiveContainer width="100%" height={300}>
                  <LineChart data={report.dailyCollections}>
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis
                      dataKey="date"
                      tickFormatter={(value) => dayjs(value).format('MMM DD')}
                    />
                    <YAxis />
                    <Tooltip
                      formatter={(value: number) => `₹${value.toFixed(2)}`}
                      labelFormatter={(label) => dayjs(label).format('MMM DD, YYYY')}
                    />
                    <Legend />
                    <Line
                      type="monotone"
                      dataKey="collection"
                      stroke="#1890ff"
                      strokeWidth={2}
                      name="Collection"
                    />
                  </LineChart>
                </ResponsiveContainer>
              </Card>
            </Col>
            <Col span={12}>
              <Card title="Payment Method Breakdown" style={{ height: 400 }}>
                <ResponsiveContainer width="100%" height={300}>
                  <PieChart>
                    <Pie
                      data={report.paymentMethodBreakdown}
                      cx="50%"
                      cy="50%"
                      labelLine={false}
                      label={({ name, percentage }) => `${name}: ${percentage.toFixed(1)}%`}
                      outerRadius={80}
                      fill="#8884d8"
                      dataKey="amount"
                    >
                      {report.paymentMethodBreakdown.map((entry, index) => (
                        <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                      ))}
                    </Pie>
                    <Tooltip formatter={(value: number) => `₹${value.toFixed(2)}`} />
                  </PieChart>
                </ResponsiveContainer>
              </Card>
            </Col>
          </Row>
        )}

        {/* Grouped Items Table */}
        {report && report.items.length > 0 && (
          <Card title={`Collection by ${filters.groupBy.charAt(0).toUpperCase() + filters.groupBy.slice(1)}`} style={{ marginBottom: 24 }}>
            <Table
              columns={itemColumns}
              dataSource={report.items}
              rowKey="groupKey"
              pagination={false}
              loading={isLoading}
            />
          </Card>
        )}

        {/* Payment Method Breakdown Table */}
        {report && report.paymentMethodBreakdown.length > 0 && (
          <Card title="Payment Method Breakdown">
            <Table
              columns={paymentMethodColumns}
              dataSource={report.paymentMethodBreakdown}
              rowKey="paymentMethod"
              pagination={false}
              loading={isLoading}
            />
          </Card>
        )}

        {report && report.totalInvoices === 0 && (
          <Card>
            <div style={{ textAlign: 'center', padding: '40px', color: '#999' }}>
              <p>No invoices found for the selected period</p>
            </div>
          </Card>
        )}
      </Card>
    </div>
  );
};

