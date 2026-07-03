import { useState, useEffect } from 'react';
import {
  Card,
  Form,
  Input,
  Switch,
  Select,
  Button,
  Space,
  Typography,
  Divider,
  message,
  Alert,
  Spin,
  InputNumber,
} from 'antd';
import { SaveOutlined, ReloadOutlined } from '@ant-design/icons';
import { useSmsSettings, useCreateOrUpdateSmsSettings } from '@core/hooks/queries/useSmsSettings';

const { Title, Text } = Typography;
const { Option } = Select;

export const SmsSettingsPage = () => {
  const [form] = Form.useForm();
  const [loading, setLoading] = useState(false);

  const { data: settings, isLoading, refetch } = useSmsSettings();
  const createOrUpdateMutation = useCreateOrUpdateSmsSettings();

  // Load settings into form when available
  useEffect(() => {
    if (settings) {
      form.setFieldsValue({
        isEnabled: settings.isEnabled,
        provider: settings.provider || 'Twilio',
        apiKey: settings.apiKey, // Will be decrypted from backend
        apiSecret: settings.apiSecret, // Will be decrypted from backend
        accountSid: settings.accountSid,
        authToken: settings.authToken, // Will be decrypted from backend
        fromPhoneNumber: settings.fromPhoneNumber,
        senderId: settings.senderId,
        apiUrl: settings.apiUrl,
        timeoutSeconds: settings.timeoutSeconds || 30,
      });
    }
  }, [settings, form]);

  const handleSubmit = async (values: any) => {
    setLoading(true);
    try {
      await createOrUpdateMutation.mutateAsync({
        isEnabled: values.isEnabled,
        provider: values.provider,
        apiKey: values.apiKey, // Will be encrypted on backend
        apiSecret: values.apiSecret, // Will be encrypted on backend
        accountSid: values.accountSid,
        authToken: values.authToken, // Will be encrypted on backend
        fromPhoneNumber: values.fromPhoneNumber,
        senderId: values.senderId,
        apiUrl: values.apiUrl,
        timeoutSeconds: values.timeoutSeconds,
      });
      await refetch();
    } catch (error) {
      // Error handled by mutation
    } finally {
      setLoading(false);
    }
  };

  if (isLoading) {
    return (
      <div style={{ textAlign: 'center', padding: '50px' }}>
        <Spin size="large" />
      </div>
    );
  }

  return (
    <div>
      <Title level={2} style={{ marginBottom: 24 }}>
        SMS Settings
      </Title>

      <Alert
        message="SMS Provider Configuration"
        description="Configure your SMS provider settings to enable SMS notifications. Your API keys and tokens will be encrypted and stored securely."
        type="info"
        showIcon
        style={{ marginBottom: 24 }}
      />

      <Card>
        <Form
          form={form}
          layout="vertical"
          onFinish={handleSubmit}
          initialValues={{
            isEnabled: settings?.isEnabled || false,
            provider: settings?.provider || 'Twilio',
            timeoutSeconds: 30,
          }}
        >
          <Form.Item name="isEnabled" valuePropName="checked">
            <Space>
              <Switch />
              <Text strong>Enable SMS Notifications</Text>
            </Space>
          </Form.Item>

          <Divider>Provider Configuration</Divider>

          <Form.Item
            label="Provider"
            name="provider"
            tooltip="Select your SMS service provider"
            rules={[{ required: true, message: 'Please select a provider' }]}
          >
            <Select>
              <Option value="Twilio">Twilio</Option>
              <Option value="AWS SNS">AWS SNS</Option>
              <Option value="Vonage">Vonage (Nexmo)</Option>
              <Option value="MessageBird">MessageBird</Option>
              <Option value="Other">Other</Option>
            </Select>
          </Form.Item>

          <Form.Item
            label="API URL"
            name="apiUrl"
            tooltip="Provider API endpoint URL (optional, some providers use default URLs)"
          >
            <Input placeholder="https://api.twilio.com/2010-04-01" />
          </Form.Item>

          <Divider>Authentication</Divider>

          <Form.Item
            label="Account SID"
            name="accountSid"
            tooltip="Your provider account SID (for Twilio) or Account ID"
          >
            <Input placeholder="ACxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx" />
          </Form.Item>

          <Form.Item
            label="API Key"
            name="apiKey"
            tooltip="Your provider API key (will be encrypted)"
          >
            <Input.Password placeholder="Enter API key" />
          </Form.Item>

          <Form.Item
            label="API Secret"
            name="apiSecret"
            tooltip="Your provider API secret (will be encrypted)"
          >
            <Input.Password placeholder="Enter API secret" />
          </Form.Item>

          <Form.Item
            label="Auth Token"
            name="authToken"
            tooltip="Your provider authentication token (for Twilio) - will be encrypted"
          >
            <Input.Password placeholder="Enter auth token" />
          </Form.Item>

          <Divider>Sender Information</Divider>

          <Form.Item
            label="From Phone Number"
            name="fromPhoneNumber"
            tooltip="Your SMS sender phone number (E.164 format: +1234567890)"
            rules={[{ required: true, message: 'Please enter from phone number' }]}
          >
            <Input placeholder="+1234567890" />
          </Form.Item>

          <Form.Item
            label="Sender ID"
            name="senderId"
            tooltip="Sender ID or alphanumeric sender name (provider-dependent)"
          >
            <Input placeholder="YourClinic" />
          </Form.Item>

          <Form.Item
            label="Timeout (seconds)"
            name="timeoutSeconds"
            tooltip="Request timeout in seconds"
          >
            <InputNumber min={5} max={300} style={{ width: '100%' }} placeholder="30" />
          </Form.Item>

          <Form.Item>
            <Space>
              <Button
                type="primary"
                htmlType="submit"
                icon={<SaveOutlined />}
                loading={loading}
              >
                Save Settings
              </Button>
              <Button icon={<ReloadOutlined />} onClick={() => refetch()}>
                Refresh
              </Button>
            </Space>
          </Form.Item>
        </Form>
      </Card>

      <Card title="Provider Setup Instructions" style={{ marginTop: 24 }}>
        <Space direction="vertical" style={{ width: '100%' }}>
          <div>
            <Text strong>Twilio:</Text>
            <ol>
              <li>Create account at <a href="https://www.twilio.com" target="_blank" rel="noopener noreferrer">twilio.com</a></li>
              <li>Get Account SID and Auth Token from dashboard</li>
              <li>Get a phone number or use trial number</li>
              <li>Enter credentials above</li>
            </ol>
          </div>
          <div>
            <Text strong>AWS SNS:</Text>
            <ol>
              <li>Create AWS account and configure SNS</li>
              <li>Get Access Key ID and Secret Access Key</li>
              <li>Configure IAM permissions for SNS</li>
            </ol>
          </div>
        </Space>
      </Card>
    </div>
  );
};

