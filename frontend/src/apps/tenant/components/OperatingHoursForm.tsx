import { Form, Radio, TimePicker, Row, Col, Typography, Space, Card } from 'antd';
import dayjs, { Dayjs } from 'dayjs';
import { OperatingHoursType } from '@core/services/clinicService';

const { Text } = Typography;

interface OperatingHoursFormProps {
  operatingHoursType: OperatingHoursType;
  onTypeChange: (type: OperatingHoursType) => void;
}

export const OperatingHoursForm: React.FC<OperatingHoursFormProps> = ({
  operatingHoursType,
  onTypeChange,
}) => {
  return (
    <>
      <Form.Item
        label="Operating Hours Type"
        name="operatingHoursType"
        rules={[{ required: true }]}
      >
        <Radio.Group onChange={(e) => onTypeChange(e.target.value)}>
          <Space direction="vertical">
            <Radio value={OperatingHoursType.SingleShift}>
              <Space direction="vertical" size={0}>
                <Text strong>Single Shift</Text>
                <Text type="secondary" style={{ fontSize: '12px' }}>
                  One continuous shift (e.g., 10 AM - 5 PM)
                </Text>
              </Space>
            </Radio>
            <Radio value={OperatingHoursType.SplitShift}>
              <Space direction="vertical" size={0}>
                <Text strong>Split Shift</Text>
                <Text type="secondary" style={{ fontSize: '12px' }}>
                  Morning and evening sessions (e.g., 10 AM - 2 PM, 5 PM - 8 PM)
                </Text>
              </Space>
            </Radio>
          </Space>
        </Radio.Group>
      </Form.Item>

      {operatingHoursType === OperatingHoursType.SingleShift && (
        <Card size="small" style={{ backgroundColor: '#f5f5f5', marginBottom: 16 }}>
          <Text strong style={{ display: 'block', marginBottom: 12 }}>
            Full Day Timings
          </Text>
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item
                label="Start Time"
                name="fullDayStartTime"
                rules={[{ required: true, message: 'Please select start time' }]}
              >
                <TimePicker 
                  style={{ width: '100%' }} 
                  format="HH:mm" 
                  use12Hours
                  placeholder="e.g., 10:00 AM"
                />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item
                label="End Time"
                name="fullDayEndTime"
                rules={[{ required: true, message: 'Please select end time' }]}
              >
                <TimePicker 
                  style={{ width: '100%' }} 
                  format="HH:mm" 
                  use12Hours
                  placeholder="e.g., 05:00 PM"
                />
              </Form.Item>
            </Col>
          </Row>
        </Card>
      )}

      {operatingHoursType === OperatingHoursType.SplitShift && (
        <>
          <Card size="small" style={{ backgroundColor: '#f0f5ff', marginBottom: 16 }}>
            <Text strong style={{ display: 'block', marginBottom: 12 }}>
              ☀️ Morning Shift
            </Text>
            <Row gutter={16}>
              <Col span={12}>
                <Form.Item
                  label="Start Time"
                  name="morningStartTime"
                  rules={[{ required: true, message: 'Please select morning start time' }]}
                >
                  <TimePicker 
                    style={{ width: '100%' }} 
                    format="HH:mm" 
                    use12Hours
                    placeholder="e.g., 10:00 AM"
                  />
                </Form.Item>
              </Col>
              <Col span={12}>
                <Form.Item
                  label="End Time"
                  name="morningEndTime"
                  rules={[{ required: true, message: 'Please select morning end time' }]}
                >
                  <TimePicker 
                    style={{ width: '100%' }} 
                    format="HH:mm" 
                    use12Hours
                    placeholder="e.g., 02:00 PM"
                  />
                </Form.Item>
              </Col>
            </Row>
          </Card>

          <Card size="small" style={{ backgroundColor: '#fff7e6', marginBottom: 16 }}>
            <Text strong style={{ display: 'block', marginBottom: 12 }}>
              🌙 Evening Shift
            </Text>
            <Row gutter={16}>
              <Col span={12}>
                <Form.Item
                  label="Start Time"
                  name="eveningStartTime"
                  rules={[{ required: true, message: 'Please select evening start time' }]}
                >
                  <TimePicker 
                    style={{ width: '100%' }} 
                    format="HH:mm" 
                    use12Hours
                    placeholder="e.g., 05:00 PM"
                  />
                </Form.Item>
              </Col>
              <Col span={12}>
                <Form.Item
                  label="End Time"
                  name="eveningEndTime"
                  rules={[{ required: true, message: 'Please select evening end time' }]}
                >
                  <TimePicker 
                    style={{ width: '100%' }} 
                    format="HH:mm" 
                    use12Hours
                    placeholder="e.g., 08:00 PM"
                  />
                </Form.Item>
              </Col>
            </Row>
          </Card>
        </>
      )}
    </>
  );
};

