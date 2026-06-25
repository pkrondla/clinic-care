import { useState, useEffect } from 'react';
import {
  Card,
  Form,
  Input,
  Switch,
  InputNumber,
  Button,
  Space,
  Typography,
  Divider,
  message,
  Alert,
  Spin,
} from 'antd';
import { SaveOutlined, ReloadOutlined } from '@ant-design/icons';
import { useEmailSettings, useCreateOrUpdateEmailSettings } from '@core/hooks/queries/useEmailSettings';

const { Title, Text } = Typography;

export const EmailSettingsPage = () => {
  const [form] = Form.useForm();
  const [loading, setLoading] = useState(false);

  const { data: settings, isLoading, refetch } = useEmailSettings();
  const createOrUpdateMutation = useCreateOrUpdateEmailSettings();

  // Load settings into form when available
  useEffect(() => {
    if (settings) {
      form.setFieldsValue({
        isEnabled: settings.isEnabled,
        smtpServer: settings.smtpServer,
        smtpPort: settings.smtpPort || 587,
        useSsl: settings.useSsl ?? true,
        useTls: settings.useTls ?? true,
        smtpUsername: settings.smtpUsername,
        smtpPassword: settings.smtpPassword, // Will be decrypted from backend
        fromEmail: settings.fromEmail,
        fromName: settings.fromName,
        replyToEmail: settings.replyToEmail,
        timeoutSeconds: settings.timeoutSeconds || 30,
      });
    }
  }, [settings, form]);

  const handleSubmit = async (values: any) => {
    setLoading(true);
    try {
      await createOrUpdateMutation.mutateAsync({
        isEnabled: values.isEnabled,
        smtpServer: values.smtpServer,
        smtpPort: values.smtpPort,
        useSsl: values.useSsl ?? true,
        useTls: values.useTls ?? true,
        smtpUsername: values.smtpUsername,
        smtpPassword: values.smtpPassword, // Will be encrypted on backend
        fromEmail: values.fromEmail,
        fromName: values.fromName,
        replyToEmail: values.replyToEmail,
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
        Email Settings
      </Title>

      <Alert
        message="SMTP Configuration"
        description="Configure your SMTP server settings to enable email notifications. Your password will be encrypted and stored securely."
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
            smtpPort: 587,
            useSsl: true,
            useTls: true,
            timeoutSeconds: 30,
          }}
        >
          <Form.Item name="isEnabled" valuePropName="checked">
            <Space>
              <Switch />
              <Text strong>Enable Email Notifications</Text>
            </Space>
          </Form.Item>

          <Divider>SMTP Server Configuration</Divider>

          <Form.Item
            label="SMTP Server"
            name="smtpServer"
            tooltip="Your SMTP server address (e.g., smtp.gmail.com, smtp.outlook.com)"
            rules={[{ required: true, message: 'Please enter SMTP server' }]}
          >
            <Input placeholder="smtp.gmail.com" />
          </Form.Item>

          <Form.Item
            label="SMTP Port"
            name="smtpPort"
            tooltip="SMTP port (usually 587 for TLS, 465 for SSL, 25 for unencrypted)"
            rules={[{ required: true, message: 'Please enter SMTP port' }]}
          >
            <InputNumber min={1} max={65535} style={{ width: '100%' }} placeholder="587" />
          </Form.Item>

          <Form.Item name="useSsl" valuePropName="checked">
            <Space>
              <Switch />
              <Text>Use SSL</Text>
            </Space>
          </Form.Item>

          <Form.Item name="useTls" valuePropName="checked">
            <Space>
              <Switch />
              <Text>Use TLS</Text>
            </Space>
          </Form.Item>

          <Divider>Authentication</Divider>

          <Form.Item
            label="SMTP Username"
            name="smtpUsername"
            tooltip="Your SMTP username (usually your email address)"
            rules={[{ required: true, message: 'Please enter SMTP username' }]}
          >
            <Input placeholder="your-email@example.com" />
          </Form.Item>

          <Form.Item
            label="SMTP Password"
            name="smtpPassword"
            tooltip="Your SMTP password or app-specific password (will be encrypted)"
            rules={[{ required: true, message: 'Please enter SMTP password' }]}
          >
            <Input.Password placeholder="Enter your SMTP password" />
          </Form.Item>

          <Divider>Sender Information</Divider>

          <Form.Item
            label="From Email"
            name="fromEmail"
            tooltip="Email address that will appear as sender"
            rules={[
              { required: true, message: 'Please enter from email' },
              { type: 'email', message: 'Please enter a valid email address' },
            ]}
          >
            <Input placeholder="noreply@yourclinic.com" />
          </Form.Item>

          <Form.Item
            label="From Name"
            name="fromName"
            tooltip="Display name for the sender"
          >
            <Input placeholder="Your Clinic Name" />
          </Form.Item>

          <Form.Item
            label="Reply-To Email"
            name="replyToEmail"
            tooltip="Email address for replies (optional)"
            rules={[{ type: 'email', message: 'Please enter a valid email address' }]}
          >
            <Input placeholder="support@yourclinic.com" />
          </Form.Item>

          <Form.Item
            label="Timeout (seconds)"
            name="timeoutSeconds"
            tooltip="Connection timeout in seconds"
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

      <Card title="Common SMTP Settings" style={{ marginTop: 24 }}>
        <Space direction="vertical" style={{ width: '100%' }}>
          <div>
            <Text strong>Gmail:</Text>
            <ul>
              <li>Server: smtp.gmail.com</li>
              <li>Port: 587 (TLS) or 465 (SSL)</li>
              <li>Use App Password (not regular password)</li>
            </ul>
          </div>
          <div>
            <Text strong>Outlook/Hotmail:</Text>
            <ul>
              <li>Server: smtp-mail.outlook.com</li>
              <li>Port: 587 (TLS)</li>
            </ul>
          </div>
          <div>
            <Text strong>Yahoo:</Text>
            <ul>
              <li>Server: smtp.mail.yahoo.com</li>
              <li>Port: 587 (TLS) or 465 (SSL)</li>
            </ul>
          </div>
        </Space>
      </Card>
    </div>
  );
};

