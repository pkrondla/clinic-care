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
  Radio,
  Input,
  Tag,
  Popconfirm,
  Row,
  Col,
  Typography,
  Alert,
  Divider,
} from 'antd';
import {
  PlusOutlined,
  EditOutlined,
  DeleteOutlined,
  CalendarOutlined,
  InfoCircleOutlined,
} from '@ant-design/icons';
import { useDoctorAvailability, useCreateDoctorAvailability, useUpdateDoctorAvailability, useDeleteDoctorAvailability } from '@core/hooks/queries/useDoctorAvailability';
import { useDoctors } from '@core/hooks/queries/useDoctors';
import { useClinics } from '@core/hooks/queries/useClinics';
import { useSelectedClinic } from '@core/stores/authStore';
import { AvailabilityType, type DoctorAvailability } from '@core/services/doctorAvailabilityService';
import type { Clinic, OperatingHoursType } from '@core/services/clinicService';
import dayjs, { Dayjs } from 'dayjs';
import isBetween from 'dayjs/plugin/isBetween';

dayjs.extend(isBetween);

const { Title, Text, Paragraph } = Typography;
const { TextArea } = Input;
const { RangePicker } = DatePicker;

export const DoctorSchedulePage = () => {
  const selectedClinic = useSelectedClinic();
  const [modalVisible, setModalVisible] = useState(false);
  const [editingRecord, setEditingRecord] = useState<DoctorAvailability | null>(null);
  const [selectedAvailabilityType, setSelectedAvailabilityType] = useState<AvailabilityType>(AvailabilityType.Regular);
  const [form] = Form.useForm();

  // Filters
  const [filters, setFilters] = useState({
    doctorId: undefined as number | undefined,
    clinicId: selectedClinic?.id,
    startDate: dayjs().startOf('week').format('YYYY-MM-DD'),
    endDate: dayjs().endOf('week').add(3, 'week').format('YYYY-MM-DD'),
  });

  const { data: availability = [], isLoading, refetch } = useDoctorAvailability(filters);
  const { data: doctors = [] } = useDoctors({ clinicId: selectedClinic?.id });
  const { data: clinics = [] } = useClinics();
  const createMutation = useCreateDoctorAvailability();
  const updateMutation = useUpdateDoctorAvailability();
  const deleteMutation = useDeleteDoctorAvailability();

  // Get selected doctor's base clinic info
  const selectedDoctor = filters.doctorId ? doctors.find(d => d.id === filters.doctorId) : null;
  const baseClinic = selectedDoctor?.baseClinicId 
    ? clinics.find(c => c.id === selectedDoctor.baseClinicId) 
    : null;

  const handleAdd = () => {
    setEditingRecord(null);
    setSelectedAvailabilityType(AvailabilityType.DifferentClinic);
    form.resetFields();
    form.setFieldsValue({
      availabilityType: AvailabilityType.DifferentClinic,
    });
    setModalVisible(true);
  };

  const handleMarkLeave = () => {
    setEditingRecord(null);
    setSelectedAvailabilityType(AvailabilityType.Leave);
    form.resetFields();
    form.setFieldsValue({
      availabilityType: AvailabilityType.Leave,
    });
    setModalVisible(true);
  };

  const handleEdit = (record: DoctorAvailability) => {
    setEditingRecord(record);
    setSelectedAvailabilityType(record.availabilityType);
    
    const formValues: any = {
      doctorId: record.doctorId,
      clinicId: record.clinicId,
      availabilityType: record.availabilityType,
      notes: record.notes,
      isActive: record.isActive,
    };

    // Handle date range for leaves
    if (record.availabilityType === AvailabilityType.Leave && record.endDate) {
      formValues.leavePeriod = [dayjs(record.availableDate), dayjs(record.endDate)];
    } else {
      formValues.availableDate = dayjs(record.availableDate);
    }

    // Handle time fields (not for leaves)
    if (record.availabilityType !== AvailabilityType.Leave) {
      formValues.startTime = dayjs(record.startTime, 'HH:mm:ss');
      formValues.endTime = dayjs(record.endTime, 'HH:mm:ss');
    }

    form.setFieldsValue(formValues);
    setModalVisible(true);
  };

  const handleDelete = async (id: number) => {
    await deleteMutation.mutateAsync(id);
  };

  const handleSubmit = async () => {
    try {
      const values = await form.validateFields();
      
      const request: any = {
        doctorId: values.doctorId,
        availabilityType: values.availabilityType,
        notes: values.notes,
      };

      // Handle leave date range
      if (values.availabilityType === AvailabilityType.Leave) {
        if (!values.leavePeriod || !values.leavePeriod[0] || !values.leavePeriod[1]) {
          throw new Error('Please select leave period');
        }
        request.availableDate = values.leavePeriod[0].format('YYYY-MM-DD');
        request.endDate = values.leavePeriod[1].format('YYYY-MM-DD');
        request.startTime = '00:00:00';
        request.endTime = '23:59:59';
        request.clinicId = values.clinicId || selectedClinic?.id;
      } else {
        request.clinicId = values.clinicId;
        request.availableDate = values.availableDate.format('YYYY-MM-DD');
        request.startTime = values.startTime.format('HH:mm:ss');
        request.endTime = values.endTime.format('HH:mm:ss');
      }

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

  const getAvailabilityTypeConfig = (type: AvailabilityType) => {
    const configs = {
      [AvailabilityType.Regular]: { color: 'green', text: 'Regular', icon: '✓' },
      [AvailabilityType.DifferentClinic]: { color: 'blue', text: 'Different Clinic', icon: '📍' },
      [AvailabilityType.Leave]: { color: 'red', text: 'Leave', icon: '🏖' },
      [AvailabilityType.ModifiedHours]: { color: 'orange', text: 'Modified Hours', icon: '⏰' },
    };
    return configs[type];
  };

  const columns = [
    {
      title: 'Date',
      dataIndex: 'availableDate',
      key: 'availableDate',
      render: (date: string, record: DoctorAvailability) => {
        if (record.endDate && record.availabilityType === AvailabilityType.Leave) {
          return (
            <Space direction="vertical" size={0}>
              <Text strong>{dayjs(date).format('MMM DD, YYYY')}</Text>
              <Text type="secondary" style={{ fontSize: '12px' }}>
                to {dayjs(record.endDate).format('MMM DD, YYYY')}
              </Text>
            </Space>
          );
        }
        return dayjs(date).format('MMM DD, YYYY');
      },
      sorter: (a: DoctorAvailability, b: DoctorAvailability) => 
        dayjs(a.availableDate).unix() - dayjs(b.availableDate).unix(),
    },
    {
      title: 'Doctor',
      dataIndex: 'doctorName',
      key: 'doctorName',
      sorter: (a: DoctorAvailability, b: DoctorAvailability) => a.doctorName.localeCompare(b.doctorName),
    },
    {
      title: 'Type',
      dataIndex: 'availabilityType',
      key: 'availabilityType',
      render: (type: AvailabilityType) => {
        const config = getAvailabilityTypeConfig(type);
        return (
          <Tag color={config.color}>
            {config.icon} {config.text}
          </Tag>
        );
      },
      filters: [
        { text: 'Regular', value: AvailabilityType.Regular },
        { text: 'Different Clinic', value: AvailabilityType.DifferentClinic },
        { text: 'Leave', value: AvailabilityType.Leave },
        { text: 'Modified Hours', value: AvailabilityType.ModifiedHours },
      ],
      onFilter: (value: any, record: DoctorAvailability) => record.availabilityType === value,
    },
    {
      title: 'Clinic',
      dataIndex: 'clinicName',
      key: 'clinicName',
      sorter: (a: DoctorAvailability, b: DoctorAvailability) => a.clinicName.localeCompare(b.clinicName),
    },
    {
      title: 'Time',
      key: 'time',
      render: (_: any, record: DoctorAvailability) => {
        if (record.availabilityType === AvailabilityType.Leave) {
          return <Text type="secondary">Not Available</Text>;
        }
        return (
          <span>
            {dayjs(record.startTime, 'HH:mm:ss').format('hh:mm A')} - {dayjs(record.endTime, 'HH:mm:ss').format('hh:mm A')}
          </span>
        );
      },
    },
    {
      title: 'Notes',
      dataIndex: 'notes',
      key: 'notes',
      render: (notes?: string) => notes ? <Text>{notes}</Text> : <Text type="secondary">-</Text>,
      ellipsis: true,
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
            title="Are you sure you want to delete this entry?"
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
              <CalendarOutlined /> Doctor Schedule Management
            </Title>
            <Text type="secondary">
              Manage doctor availability, leaves, and clinic visits
            </Text>
          </Col>
          <Col>
            <Space>
              <Button
                icon={<PlusOutlined />}
                onClick={handleMarkLeave}
              >
                Mark Leave
              </Button>
              <Button
                type="primary"
                icon={<PlusOutlined />}
                onClick={handleAdd}
              >
                Add Exception
              </Button>
            </Space>
          </Col>
        </Row>

        {/* Info Alert about default schedule */}
        {selectedDoctor && baseClinic && (
          <Alert
            message="Default Schedule"
            description={
              <div>
                <Text strong>{selectedDoctor.doctorName}</Text> is normally available at <Text strong>{baseClinic.name}</Text> during clinic operating hours.
                <br />
                Add exceptions below for different clinics, leaves, or modified hours.
              </div>
            }
            type="info"
            icon={<InfoCircleOutlined />}
            showIcon
            style={{ marginBottom: 16 }}
          />
        )}

        {/* Filters */}
        <Card size="small" style={{ marginBottom: 16 }}>
          <Row gutter={16} align="middle">
            <Col span={6}>
              <Text strong>Doctor:</Text>
              <Select
                placeholder="Select Doctor"
                style={{ width: '100%', marginTop: 8 }}
                allowClear
                value={filters.doctorId}
                onChange={(value) => setFilters({ ...filters, doctorId: value })}
              >
                {doctors.map((doctor) => (
                  <Select.Option key={doctor.id} value={doctor.id}>
                    {doctor.doctorName}
                  </Select.Option>
                ))}
              </Select>
            </Col>
            <Col span={6}>
              <Text strong>Clinic:</Text>
              <Select
                placeholder="All Clinics"
                style={{ width: '100%', marginTop: 8 }}
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
              <Text strong>Date Range:</Text>
              <DatePicker.RangePicker
                style={{ width: '100%', marginTop: 8 }}
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
            <Col span={4}>
              <Button type="primary" onClick={() => refetch()} style={{ marginTop: 24 }}>
                Search
              </Button>
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
            showTotal: (total) => `Total ${total} schedule entries`,
          }}
        />
      </Card>

      {/* Add/Edit Modal */}
      <Modal
        title={
          editingRecord 
            ? 'Edit Schedule Entry' 
            : (selectedAvailabilityType === AvailabilityType.Leave ? 'Mark Doctor Leave' : 'Add Schedule Exception')
        }
        open={modalVisible}
        onOk={handleSubmit}
        onCancel={() => {
          setModalVisible(false);
          form.resetFields();
          setEditingRecord(null);
        }}
        confirmLoading={createMutation.isPending || updateMutation.isPending}
        width={700}
        okText={editingRecord ? 'Update' : 'Save'}
      >
        <Form
          form={form}
          layout="vertical"
          initialValues={{
            clinicId: selectedClinic?.id,
            availabilityType: AvailabilityType.DifferentClinic,
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
                  <Space>
                    <span>{doctor.doctorName}</span>
                    {doctor.baseClinicName && (
                      <Text type="secondary" style={{ fontSize: '12px' }}>
                        (Base: {doctor.baseClinicName})
                      </Text>
                    )}
                  </Space>
                </Select.Option>
              ))}
            </Select>
          </Form.Item>

          <Divider />

          <Form.Item
            label="Schedule Type"
            name="availabilityType"
            rules={[{ required: true, message: 'Please select schedule type' }]}
          >
            <Radio.Group
              onChange={(e) => setSelectedAvailabilityType(e.target.value)}
              disabled={!!editingRecord}
            >
              <Space direction="vertical">
                <Radio value={AvailabilityType.DifferentClinic}>
                  <Space direction="vertical" size={0}>
                    <Text strong>📍 Different Clinic</Text>
                    <Text type="secondary" style={{ fontSize: '12px' }}>
                      Doctor will be working at another clinic
                    </Text>
                  </Space>
                </Radio>
                <Radio value={AvailabilityType.ModifiedHours}>
                  <Space direction="vertical" size={0}>
                    <Text strong>⏰ Modified Hours</Text>
                    <Text type="secondary" style={{ fontSize: '12px' }}>
                      Arriving late, leaving early, or custom hours for specific date
                    </Text>
                  </Space>
                </Radio>
                <Radio value={AvailabilityType.Leave}>
                  <Space direction="vertical" size={0}>
                    <Text strong>🏖 Leave</Text>
                    <Text type="secondary" style={{ fontSize: '12px' }}>
                      Doctor is not available (supports date ranges)
                    </Text>
                  </Space>
                </Radio>
              </Space>
            </Radio.Group>
          </Form.Item>

          <Divider />

          {/* Conditional fields based on availability type */}
          {selectedAvailabilityType === AvailabilityType.Leave ? (
            <>
              <Form.Item
                label="Leave Period"
                name="leavePeriod"
                rules={[{ required: true, message: 'Please select leave period' }]}
              >
                <RangePicker
                  style={{ width: '100%' }}
                  format="YYYY-MM-DD"
                  placeholder={['Start Date', 'End Date']}
                />
              </Form.Item>

              <Form.Item
                label="Clinic (for reference)"
                name="clinicId"
              >
                <Select placeholder="Select clinic" disabled>
                  {clinics.map((clinic) => (
                    <Select.Option key={clinic.id} value={clinic.id}>
                      {clinic.name}
                    </Select.Option>
                  ))}
                </Select>
              </Form.Item>
            </>
          ) : (
            <>
              <Form.Item
                label="Date"
                name="availableDate"
                rules={[{ required: true, message: 'Please select a date' }]}
              >
                <DatePicker style={{ width: '100%' }} disabled={!!editingRecord} />
              </Form.Item>

              <Form.Item
                label={
                  selectedAvailabilityType === AvailabilityType.DifferentClinic 
                    ? 'Visiting Clinic' 
                    : 'Clinic'
                }
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

              <Row gutter={16}>
                <Col span={12}>
                  <Form.Item
                    label="Start Time"
                    name="startTime"
                    rules={[{ required: true, message: 'Please select start time' }]}
                  >
                    <TimePicker style={{ width: '100%' }} format="HH:mm" use12Hours />
                  </Form.Item>
                </Col>
                <Col span={12}>
                  <Form.Item
                    label="End Time"
                    name="endTime"
                    rules={[{ required: true, message: 'Please select end time' }]}
                  >
                    <TimePicker style={{ width: '100%' }} format="HH:mm" use12Hours />
                  </Form.Item>
                </Col>
              </Row>
            </>
          )}

          <Form.Item
            label="Notes"
            name="notes"
          >
            <TextArea
              rows={3}
              placeholder="Additional information (e.g., reason for leave, special instructions)"
              maxLength={500}
              showCount
            />
          </Form.Item>

          {editingRecord && (
            <Form.Item
              label="Status"
              name="isActive"
            >
              <Radio.Group>
                <Radio value={true}>Active</Radio>
                <Radio value={false}>Inactive</Radio>
              </Radio.Group>
            </Form.Item>
          )}
        </Form>
      </Modal>
    </div>
  );
};
