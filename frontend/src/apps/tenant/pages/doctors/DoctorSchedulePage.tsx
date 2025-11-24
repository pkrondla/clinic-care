import { useState } from 'react';
import {
  Card,
  Table,
  Button,
  Space,
  Modal,
  Form,
  DatePicker,
  TimePicker,
  Select,
  message,
  Tag,
  Popconfirm,
  Row,
  Col,
  Typography,
} from 'antd';
import {
  PlusOutlined,
  EditOutlined,
  DeleteOutlined,
  CalendarOutlined,
} from '@ant-design/icons';
import { useDoctorAvailability, useCreateDoctorAvailability, useUpdateDoctorAvailability, useDeleteDoctorAvailability } from '@core/hooks/queries/useDoctorAvailability';
import { useDoctors } from '@core/hooks/queries/useDoctors';
import { useClinics } from '@core/hooks/queries/useClinics';
import { useSelectedClinic } from '@core/stores/authStore';
import type { DoctorAvailability } from '@core/services/doctorAvailabilityService';
import dayjs, { Dayjs } from 'dayjs';

const { Title } = Typography;

export const DoctorSchedulePage = () => {
  const selectedClinic = useSelectedClinic();
  const [modalVisible, setModalVisible] = useState(false);
  const [editingRecord, setEditingRecord] = useState<DoctorAvailability | null>(null);
  const [form] = Form.useForm();

  // Filters
  const [filters, setFilters] = useState({
    doctorId: undefined as number | undefined,
    clinicId: selectedClinic?.id,
    startDate: dayjs().startOf('week').format('YYYY-MM-DD'),
    endDate: dayjs().endOf('week').format('YYYY-MM-DD'),
  });

  const { data: availability = [], isLoading, refetch } = useDoctorAvailability(filters);
  const { data: doctors = [] } = useDoctors({ clinicId: selectedClinic?.id });
  const { data: clinics = [] } = useClinics();
  const createMutation = useCreateDoctorAvailability();
  const updateMutation = useUpdateDoctorAvailability();
  const deleteMutation = useDeleteDoctorAvailability();

  const handleAdd = () => {
    setEditingRecord(null);
    form.resetFields();
    setModalVisible(true);
  };

  const handleEdit = (record: DoctorAvailability) => {
    setEditingRecord(record);
    form.setFieldsValue({
      doctorId: record.doctorId,
      clinicId: record.clinicId,
      availableDate: dayjs(record.availableDate),
      startTime: dayjs(record.startTime, 'HH:mm:ss'),
      endTime: dayjs(record.endTime, 'HH:mm:ss'),
      isActive: record.isActive,
    });
    setModalVisible(true);
  };

  const handleDelete = async (id: number) => {
    await deleteMutation.mutateAsync(id);
  };

  const handleSubmit = async () => {
    try {
      const values = await form.validateFields();
      
      const request = {
        doctorId: values.doctorId,
        clinicId: values.clinicId,
        availableDate: values.availableDate.format('YYYY-MM-DD'),
        startTime: values.startTime.format('HH:mm:ss'),
        endTime: values.endTime.format('HH:mm:ss'),
      };

      if (editingRecord) {
        await updateMutation.mutateAsync({
          id: editingRecord.id,
          ...request,
          isActive: values.isActive ?? true,
        });
      } else {
        await createMutation.mutateAsync(request);
      }

      setModalVisible(false);
      form.resetFields();
      setEditingRecord(null);
    } catch (error) {
      console.error('Validation failed:', error);
    }
  };

  const columns = [
    {
      title: 'Doctor',
      dataIndex: 'doctorName',
      key: 'doctorName',
      sorter: (a: DoctorAvailability, b: DoctorAvailability) => a.doctorName.localeCompare(b.doctorName),
    },
    {
      title: 'Clinic',
      dataIndex: 'clinicName',
      key: 'clinicName',
      sorter: (a: DoctorAvailability, b: DoctorAvailability) => a.clinicName.localeCompare(b.clinicName),
    },
    {
      title: 'Date',
      dataIndex: 'availableDate',
      key: 'availableDate',
      render: (date: string) => dayjs(date).format('MMM DD, YYYY'),
      sorter: (a: DoctorAvailability, b: DoctorAvailability) => 
        dayjs(a.availableDate).unix() - dayjs(b.availableDate).unix(),
    },
    {
      title: 'Time',
      key: 'time',
      render: (_: any, record: DoctorAvailability) => (
        <span>
          {dayjs(record.startTime, 'HH:mm:ss').format('hh:mm A')} - {dayjs(record.endTime, 'HH:mm:ss').format('hh:mm A')}
        </span>
      ),
    },
    {
      title: 'Status',
      dataIndex: 'isActive',
      key: 'isActive',
      render: (isActive: boolean) => (
        <Tag color={isActive ? 'green' : 'red'}>
          {isActive ? 'Active' : 'Inactive'}
        </Tag>
      ),
    },
    {
      title: 'Actions',
      key: 'actions',
      render: (_: any, record: DoctorAvailability) => (
        <Space>
          <Button
            type="link"
            icon={<EditOutlined />}
            onClick={() => handleEdit(record)}
            size="small"
          >
            Edit
          </Button>
          <Popconfirm
            title="Are you sure you want to delete this availability?"
            onConfirm={() => handleDelete(record.id)}
            okText="Yes"
            cancelText="No"
          >
            <Button
              type="link"
              danger
              icon={<DeleteOutlined />}
              size="small"
            >
              Delete
            </Button>
          </Popconfirm>
        </Space>
      ),
    },
  ];

  return (
    <div>
      <Card>
        <Row justify="space-between" align="middle" style={{ marginBottom: 16 }}>
          <Col>
            <Title level={2} style={{ margin: 0 }}>
              Doctor Schedule Management
            </Title>
          </Col>
          <Col>
            <Button
              type="primary"
              icon={<PlusOutlined />}
              onClick={handleAdd}
            >
              Add Availability
            </Button>
          </Col>
        </Row>

        {/* Filters */}
        <Card size="small" style={{ marginBottom: 16 }}>
          <Row gutter={16}>
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
                placeholder="Select Clinic"
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
            <Col span={6}>
              <DatePicker.RangePicker
                style={{ width: '100%' }}
                value={[
                  filters.startDate ? dayjs(filters.startDate) : null,
                  filters.endDate ? dayjs(filters.endDate) : null,
                ]}
                onChange={(dates) => {
                  if (dates && dates[0] && dates[1]) {
                    setFilters({
                      ...filters,
                      startDate: dates[0].format('YYYY-MM-DD'),
                      endDate: dates[1].format('YYYY-MM-DD'),
                    });
                  }
                }}
              />
            </Col>
            <Col span={6}>
              <Button onClick={() => refetch()}>Refresh</Button>
            </Col>
          </Row>
        </Card>

        <Table
          columns={columns}
          dataSource={availability}
          loading={isLoading}
          rowKey="id"
          pagination={{
            pageSize: 20,
            showSizeChanger: true,
            showTotal: (total) => `Total ${total} availability records`,
          }}
        />
      </Card>

      {/* Add/Edit Modal */}
      <Modal
        title={editingRecord ? 'Edit Doctor Availability' : 'Add Doctor Availability'}
        open={modalVisible}
        onOk={handleSubmit}
        onCancel={() => {
          setModalVisible(false);
          form.resetFields();
          setEditingRecord(null);
        }}
        confirmLoading={createMutation.isPending || updateMutation.isPending}
        width={600}
      >
        <Form
          form={form}
          layout="vertical"
          initialValues={{
            clinicId: selectedClinic?.id,
            isActive: true,
          }}
        >
          <Form.Item
            label="Doctor"
            name="doctorId"
            rules={[{ required: true, message: 'Please select a doctor' }]}
          >
            <Select placeholder="Select doctor" disabled={!!editingRecord}>
              {doctors.map((doctor) => (
                <Select.Option key={doctor.id} value={doctor.id}>
                  {doctor.name}
                </Select.Option>
              ))}
            </Select>
          </Form.Item>

          <Form.Item
            label="Clinic"
            name="clinicId"
            rules={[{ required: true, message: 'Please select a clinic' }]}
          >
            <Select placeholder="Select clinic" disabled={!!editingRecord}>
              {clinics.map((clinic) => (
                <Select.Option key={clinic.id} value={clinic.id}>
                  {clinic.name}
                </Select.Option>
              ))}
            </Select>
          </Form.Item>

          <Form.Item
            label="Available Date"
            name="availableDate"
            rules={[{ required: true, message: 'Please select a date' }]}
          >
            <DatePicker style={{ width: '100%' }} />
          </Form.Item>

          <Row gutter={16}>
            <Col span={12}>
              <Form.Item
                label="Start Time"
                name="startTime"
                rules={[{ required: true, message: 'Please select start time' }]}
              >
                <TimePicker style={{ width: '100%' }} format="HH:mm" />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item
                label="End Time"
                name="endTime"
                rules={[{ required: true, message: 'Please select end time' }]}
              >
                <TimePicker style={{ width: '100%' }} format="HH:mm" />
              </Form.Item>
            </Col>
          </Row>

          {editingRecord && (
            <Form.Item
              label="Status"
              name="isActive"
            >
              <Select>
                <Select.Option value={true}>Active</Select.Option>
                <Select.Option value={false}>Inactive</Select.Option>
              </Select>
            </Form.Item>
          )}
        </Form>
      </Modal>
    </div>
  );
};

