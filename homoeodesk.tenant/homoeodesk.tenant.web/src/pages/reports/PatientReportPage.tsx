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
  Typography,
  Space,
  Tabs,
  Tag,
  Descriptions,
  Empty,
} from 'antd';
import {
  UserOutlined,
  CalendarOutlined,
  FileTextOutlined,
  MedicineBoxOutlined,
  DollarOutlined,
  ReloadOutlined,
  DownloadOutlined,
} from '@ant-design/icons';
import { usePatientReport } from '@core/hooks/queries/useReports';
import { usePatients } from '@core/hooks/queries/usePatients';
import { useBranches } from '@core/hooks/queries/useBranches';
import { useDoctors } from '@core/hooks/queries/useDoctors';
import { useSelectedBranch } from '@core/stores/authStore';
import dayjs from 'dayjs';
import type { 
  PatientVisitDto, 
  TreatmentSummaryDto, 
  MedicationHistoryDto, 
  PaymentHistoryDto 
} from '@core/services/reportsService';

const { Title } = Typography;
const { RangePicker } = DatePicker;

export const PatientReportPage = () => {
  const selectedClinic = useSelectedBranch();
  const [filters, setFilters] = useState({
    patientId: undefined as number | undefined,
    BranchId: selectedClinic?.id as number | undefined,
    doctorId: undefined as number | undefined,
    startDate: dayjs().subtract(3, 'months').format('YYYY-MM-DD'),
    endDate: dayjs().format('YYYY-MM-DD'),
  });

  const { data: report, isLoading, refetch } = usePatientReport({
    patientId: filters.patientId,
    BranchId: filters.BranchId,
    doctorId: filters.doctorId,
    startDate: filters.startDate,
    endDate: filters.endDate,
  });

  const { data: patients = [] } = usePatients({ search: '' });
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

  const visitColumns = [
    {
      title: 'Visit Date',
      dataIndex: 'visitDate',
      key: 'visitDate',
      render: (date: string) => dayjs(date).format('MMM DD, YYYY'),
    },
    {
      title: 'Clinic',
      dataIndex: 'clinicName',
      key: 'clinicName',
    },
    {
      title: 'Doctor',
      dataIndex: 'doctorName',
      key: 'doctorName',
    },
    {
      title: 'Type',
      dataIndex: 'appointmentType',
      key: 'appointmentType',
    },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      render: (status: string) => <Tag>{status}</Tag>,
    },
  ];

  const treatmentColumns = [
    {
      title: 'Date',
      dataIndex: 'consultationDate',
      key: 'consultationDate',
      render: (date: string) => dayjs(date).format('MMM DD, YYYY'),
    },
    {
      title: 'Doctor',
      dataIndex: 'doctorName',
      key: 'doctorName',
    },
    {
      title: 'Diagnosis',
      dataIndex: 'diagnosis',
      key: 'diagnosis',
      ellipsis: true,
    },
    {
      title: 'Treatment Plan',
      dataIndex: 'treatmentPlan',
      key: 'treatmentPlan',
      ellipsis: true,
    },
  ];

  const medicationColumns = [
    {
      title: 'Date',
      dataIndex: 'prescriptionDate',
      key: 'prescriptionDate',
      render: (date: string) => dayjs(date).format('MMM DD, YYYY'),
    },
    {
      title: 'Prescription #',
      dataIndex: 'prescriptionNumber',
      key: 'prescriptionNumber',
    },
    {
      title: 'Doctor',
      dataIndex: 'doctorName',
      key: 'doctorName',
    },
    {
      title: 'Medicines',
      dataIndex: 'medicineCount',
      key: 'medicineCount',
    },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      render: (status: string) => <Tag>{status}</Tag>,
    },
  ];

  const paymentColumns = [
    {
      title: 'Date',
      dataIndex: 'invoiceDate',
      key: 'invoiceDate',
      render: (date: string) => dayjs(date).format('MMM DD, YYYY'),
    },
    {
      title: 'Invoice #',
      dataIndex: 'invoiceNumber',
      key: 'invoiceNumber',
    },
    {
      title: 'Total',
      dataIndex: 'totalAmount',
      key: 'totalAmount',
      render: (amount: number) => `₹${amount.toFixed(2)}`,
    },
    {
      title: 'Paid',
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
      title: 'Payment Method',
      dataIndex: 'paymentMethod',
      key: 'paymentMethod',
    },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      render: (status: string) => <Tag>{status}</Tag>,
    },
  ];

  const tabItems = [
    {
      key: 'summary',
      label: 'Summary',
      children: (
        <div>
          {report?.patientId && (
            <Card style={{ marginBottom: 16 }}>
              <Descriptions title="Patient Information" bordered>
                <Descriptions.Item label="Patient Code">
                  {report.patientCode}
                </Descriptions.Item>
                <Descriptions.Item label="Patient Name">
                  {report.patientName}
                </Descriptions.Item>
              </Descriptions>
            </Card>
          )}
          <Row gutter={16}>
            <Col span={6}>
              <Card>
                <Statistic
                  title="Total Visits"
                  value={report?.totalVisits || 0}
                  prefix={<CalendarOutlined />}
                />
              </Card>
            </Col>
            <Col span={6}>
              <Card>
                <Statistic
                  title="Consultations"
                  value={report?.totalConsultations || 0}
                  prefix={<FileTextOutlined />}
                />
              </Card>
            </Col>
            <Col span={6}>
              <Card>
                <Statistic
                  title="Prescriptions"
                  value={report?.totalPrescriptions || 0}
                  prefix={<MedicineBoxOutlined />}
                />
              </Card>
            </Col>
            <Col span={6}>
              <Card>
                <Statistic
                  title="Invoices"
                  value={report?.totalInvoices || 0}
                  prefix={<DollarOutlined />}
                />
              </Card>
            </Col>
          </Row>
          <Row gutter={16} style={{ marginTop: 16 }}>
            <Col span={12}>
              <Card>
                <Statistic
                  title="Total Paid"
                  value={report?.totalAmountPaid || 0}
                  prefix="₹"
                  precision={2}
                  valueStyle={{ color: '#3f8600' }}
                />
              </Card>
            </Col>
            <Col span={12}>
              <Card>
                <Statistic
                  title="Pending Amount"
                  value={report?.totalAmountPending || 0}
                  prefix="₹"
                  precision={2}
                  valueStyle={{ color: '#cf1322' }}
                />
              </Card>
            </Col>
          </Row>
        </div>
      ),
    },
    {
      key: 'visits',
      label: 'Visit History',
      children: (
        <Table
          columns={visitColumns}
          dataSource={report?.visitHistory || []}
          rowKey={(record, index) => `${record.visitDate}-${index}`}
          loading={isLoading}
          pagination={{ pageSize: 10 }}
        />
      ),
    },
    {
      key: 'treatment',
      label: 'Treatment Summary',
      children: (
        <Table
          columns={treatmentColumns}
          dataSource={report?.treatmentSummary || []}
          rowKey={(record, index) => `${record.consultationDate}-${index}`}
          loading={isLoading}
          pagination={{ pageSize: 10 }}
          expandable={{
            expandedRowRender: (record: TreatmentSummaryDto) => (
              <div style={{ padding: '16px' }}>
                <p><strong>Notes:</strong> {record.notes || 'N/A'}</p>
              </div>
            ),
          }}
        />
      ),
    },
    {
      key: 'medication',
      label: 'Medication History',
      children: (
        <Table
          columns={medicationColumns}
          dataSource={report?.medicationHistory || []}
          rowKey={(record, index) => `${record.prescriptionNumber}-${index}`}
          loading={isLoading}
          pagination={{ pageSize: 10 }}
          expandable={{
            expandedRowRender: (record: MedicationHistoryDto) => (
              <div style={{ padding: '16px' }}>
                <Table
                  columns={[
                    { title: 'Medicine', dataIndex: 'medicineName', key: 'medicineName' },
                    { title: 'Dosage', dataIndex: 'dosage', key: 'dosage' },
                    { title: 'Frequency', dataIndex: 'frequency', key: 'frequency' },
                    { 
                      title: 'Duration', 
                      key: 'duration',
                      render: (_: any, item: any) => `${item.duration} ${item.durationUnit}`,
                    },
                    { title: 'Instructions', dataIndex: 'instructions', key: 'instructions' },
                  ]}
                  dataSource={record.medications}
                  rowKey="medicineName"
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
      key: 'payment',
      label: 'Payment History',
      children: (
        <Table
          columns={paymentColumns}
          dataSource={report?.paymentHistory || []}
          rowKey="invoiceNumber"
          loading={isLoading}
          pagination={{ pageSize: 10 }}
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
              Patient Report
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
              <Select
                placeholder="Select Patient"
                style={{ width: '100%' }}
                allowClear
                showSearch
                value={filters.patientId}
                onChange={(value) => setFilters({ ...filters, patientId: value })}
                filterOption={(input, option) =>
                  (option?.label ?? '').toLowerCase().includes(input.toLowerCase())
                }
                options={patients.map((p) => ({
                  value: p.id,
                  label: `${p.patientCode} - ${p.firstName} ${p.lastName}`,
                }))}
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
              <RangePicker
                style={{ width: '100%' }}
                value={[
                  filters.startDate ? dayjs(filters.startDate) : null,
                  filters.endDate ? dayjs(filters.endDate) : null,
                ]}
                onChange={handleDateRangeChange}
              />
            </Col>
          </Row>
        </Card>

        {report && (
          <Tabs items={tabItems} />
        )}

        {!report && !isLoading && (
          <Card>
            <Empty description="No data available. Please select filters and try again." />
          </Card>
        )}
      </Card>
    </div>
  );
};

