import { useState } from 'react'
import { Card, Form, Switch, Select, Button, Space, Typography, Divider, message, Descriptions } from 'antd'
import { SaveOutlined } from '@ant-design/icons'
import { useTheme } from '@core/stores/uiStore'
import { useAuth } from '@core/stores/authStore'

const { Title, Text } = Typography
const { Option } = Select

export const SettingsPage = () => {
  const { theme, setTheme } = useTheme()
  const { user } = useAuth()
  const [form] = Form.useForm()
  const [loading, setLoading] = useState(false)

  const handleSubmit = async (values: any) => {
    setLoading(true)
    try {
      // Update theme
      if (values.theme !== theme) {
        setTheme(values.theme)
        message.success('Settings saved successfully')
      } else {
        message.success('Settings saved successfully')
      }
      
      // Here you can add more settings to save (e.g., API call to backend)
      // await settingsService.updateSettings(values)
    } catch (error) {
      message.error('Failed to save settings')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div>
      <Title level={2} style={{ marginBottom: 24 }}>Settings</Title>

      <Card title="Appearance">
        <Form
          form={form}
          layout="vertical"
          onFinish={handleSubmit}
          initialValues={{
            theme: theme || 'light',
            notifications: true,
            emailNotifications: true
          }}
        >
          <Form.Item
            label="Theme"
            name="theme"
            tooltip="Choose between light and dark theme"
          >
            <Select>
              <Option value="light">Light</Option>
              <Option value="dark">Dark</Option>
            </Select>
          </Form.Item>

          <Divider />

          <Title level={4}>Notifications</Title>
          
          <Form.Item
            label="Enable Notifications"
            name="notifications"
            valuePropName="checked"
            tooltip="Receive in-app notifications"
          >
            <Switch />
          </Form.Item>

          <Form.Item
            label="Email Notifications"
            name="emailNotifications"
            valuePropName="checked"
            tooltip="Receive notifications via email"
          >
            <Switch />
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
            </Space>
          </Form.Item>
        </Form>
      </Card>

      <Card title="Preferences" style={{ marginTop: 24 }}>
        <Descriptions column={1} bordered>
          <Descriptions.Item label="Default Clinic">
            <Text>{user?.selectedClinicName || 'Not selected'}</Text>
          </Descriptions.Item>
          <Descriptions.Item label="Language">
            <Text>English</Text>
          </Descriptions.Item>
          <Descriptions.Item label="Date Format">
            <Text>MM/DD/YYYY</Text>
          </Descriptions.Item>
          <Descriptions.Item label="Time Format">
            <Text>12-hour</Text>
          </Descriptions.Item>
        </Descriptions>
      </Card>

      <Card title="About" style={{ marginTop: 24 }}>
        <Descriptions column={1} bordered>
          <Descriptions.Item label="Application Version">
            <Text>1.0.0</Text>
          </Descriptions.Item>
          <Descriptions.Item label="Organization">
            <Text>{user?.organizationName || '-'}</Text>
          </Descriptions.Item>
        </Descriptions>
      </Card>
    </div>
  )
}
