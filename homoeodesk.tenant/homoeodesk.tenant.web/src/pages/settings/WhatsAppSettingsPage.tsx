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
} from 'antd';
import { SaveOutlined, CheckCircleOutlined, ReloadOutlined } from '@ant-design/icons';
import { useWhatsAppSettings, useCreateOrUpdateWhatsAppSettings, useTestWhatsAppConnection } from '@core/hooks/queries/useWhatsAppSettings';

const { Title, Text } = Typography;
const { Option } = Select;
const { TextArea } = Input;

export const WhatsAppSettingsPage = () => {
  const [form] = Form.useForm();
  const [loading, setLoading] = useState(false);
  const [testing, setTesting] = useState(false);

  const { data: settings, isLoading, refetch } = useWhatsAppSettings();
  const createOrUpdateMutation = useCreateOrUpdateWhatsAppSettings();
  const testConnectionMutation = useTestWhatsAppConnection();

  // Load settings into form when available
  useEffect(() => {
    if (settings) {
      form.setFieldsValue({
        isEnabled: settings.isEnabled,
        provider: settings.provider,
        phoneNumberId: settings.phoneNumberId,
        businessAccountId: settings.businessAccountId,
        apiVersion: settings.apiVersion || 'v18.0',
        fromPhoneNumber: settings.fromPhoneNumber,
        webhookUrl: settings.webhookUrl,
        webhookVerifyToken: settings.webhookVerifyToken,
      });
    }
  }, [settings, form]);

  const handleSubmit = async (values: any) => {
    setLoading(true);
    try {
      await createOrUpdateMutation.mutateAsync({
        isEnabled: values.isEnabled,
        provider: values.provider,
        phoneNumberId: values.phoneNumberId,
        businessAccountId: values.businessAccountId,
        accessToken: values.accessToken, // Will be encrypted on backend
        apiVersion: values.apiVersion,
        fromPhoneNumber: values.fromPhoneNumber,
        webhookUrl: values.webhookUrl,
        webhookSecret: values.webhookSecret, // Will be encrypted on backend
        webhookVerifyToken: values.webhookVerifyToken,
      });
      await refetch();
    } catch (error) {
      // Error handled by mutation
    } finally {
      setLoading(false);
    }
  };

  const handleTestConnection = async () => {
    setTesting(true);
    try {
      await testConnectionMutation.mutateAsync();
    } catch (error) {
      // Error handled by mutation
    } finally {
      setTesting(false);
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
        WhatsApp Business Settings
      </Title>

      <Alert
        message="WhatsApp Business API Configuration"
        description="Configure your WhatsApp Business API credentials to enable WhatsApp notifications. Your sensitive data (access tokens, API keys) will be encrypted and stored securely."
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
            provider: settings?.provider || 'Meta',
            apiVersion: settings?.apiVersion || 'v18.0',
          }}
        >
          <Form.Item name="isEnabled" valuePropName="checked">
            <Space>
              <Switch />
              <Text strong>Enable WhatsApp Notifications</Text>
            </Space>
          </Form.Item>

          <Divider />

          <Form.Item
            label="Provider"
            name="provider"
            rules={[{ required: true, message: 'Please select a provider' }]}
          >
            <Select>
              <Option value="Meta">Meta WhatsApp Business API</Option>
              <Option value="Twilio" disabled>Twilio (Coming Soon)</Option>
              <Option value="Dialog360" disabled>360dialog (Coming Soon)</Option>
            </Select>
          </Form.Item>

          <Form.Item
            label="Phone Number ID"
            name="phoneNumberId"
            tooltip="Your WhatsApp Business Phone Number ID from Meta"
            rules={[{ required: true, message: 'Please enter Phone Number ID' }]}
          >
            <Input placeholder="e.g., 123456789012345" />
          </Form.Item>

          <Form.Item
            label="Business Account ID"
            name="businessAccountId"
            tooltip="Your WhatsApp Business Account ID (optional)"
          >
            <Input placeholder="e.g., 123456789012345" />
          </Form.Item>

          <Form.Item
            label="Access Token"
            name="accessToken"
            tooltip="Your WhatsApp Business API Access Token (will be encrypted)"
            rules={[{ required: true, message: 'Please enter Access Token' }]}
          >
            <Input.Password placeholder="Enter your access token" />
          </Form.Item>

          <Form.Item
            label="API Version"
            name="apiVersion"
            tooltip="Meta WhatsApp API version (default: v18.0)"
          >
            <Input placeholder="v18.0" />
          </Form.Item>

          <Form.Item
            label="From Phone Number"
            name="fromPhoneNumber"
            tooltip="Your WhatsApp Business phone number (E.164 format: +1234567890)"
          >
            <Input placeholder="+1234567890" />
          </Form.Item>

          <Divider>Webhook Configuration (Optional)</Divider>

          <Form.Item
            label="Webhook URL"
            name="webhookUrl"
            tooltip="URL where Meta will send webhook events"
          >
            <Input placeholder="https://your-domain.com/api/webhooks/whatsapp" />
          </Form.Item>

          <Form.Item
            label="Webhook Secret"
            name="webhookSecret"
            tooltip="Secret for verifying webhook requests (will be encrypted)"
          >
            <Input.Password placeholder="Enter webhook secret" />
          </Form.Item>

          <Form.Item
            label="Webhook Verify Token"
            name="webhookVerifyToken"
            tooltip="Token for webhook verification"
          >
            <Input placeholder="Enter verify token" />
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
              <Button
                icon={<CheckCircleOutlined />}
                onClick={handleTestConnection}
                loading={testing}
                disabled={!form.getFieldValue('isEnabled')}
              >
                Test Connection
              </Button>
              <Button icon={<ReloadOutlined />} onClick={() => refetch()}>
                Refresh
              </Button>
            </Space>
          </Form.Item>
        </Form>
      </Card>

      <Card title="Setup Instructions" style={{ marginTop: 24 }}>
        <ol>
          <li>
            <Text strong>Create Meta Business Account:</Text> Go to{' '}
            <a href="https://business.facebook.com" target="_blank" rel="noopener noreferrer">
              business.facebook.com
            </a>{' '}
            and create a business account
          </li>
          <li>
            <Text strong>Create WhatsApp Business App:</Text> In Meta Business Suite, create a
            WhatsApp Business App
          </li>
          <li>
            <Text strong>Get Access Token:</Text> Generate a temporary or permanent access token
            from the App Dashboard
          </li>
          <li>
            <Text strong>Get Phone Number ID:</Text> Find your Phone Number ID in the WhatsApp
            Business API settings
          </li>
          <li>
            <Text strong>Configure Webhook (Optional):</Text> Set up webhook URL for delivery
            status updates
          </li>
          <li>
            <Text strong>Test Connection:</Text> Click "Test Connection" to verify your
            configuration
          </li>
        </ol>
      </Card>
    </div>
  );
};

