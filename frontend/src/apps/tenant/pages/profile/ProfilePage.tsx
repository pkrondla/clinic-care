import { useState } from 'react'
import { Card, Form, Input, Button, Space, Descriptions, Avatar, message, Row, Col, Typography, Divider } from 'antd'
import { UserOutlined, SaveOutlined, EditOutlined } from '@ant-design/icons'
import { useAuth, useUser } from '@core/stores/authStore'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { userService, UpdateUserRequest } from '@core/services/userService'
import dayjs from 'dayjs'

const { Title, Text } = Typography

export const ProfilePage = () => {
  const auth = useAuth()
  const { user } = auth
  const currentUser = useUser()
  const [form] = Form.useForm()
  const [isEditing, setIsEditing] = useState(false)
  const queryClient = useQueryClient()

  const updateProfileMutation = useMutation({
    mutationFn: (data: UpdateUserRequest) => {
      if (!user?.id) throw new Error('User ID not found')
      return userService.updateUser(user.id, data)
    },
    onSuccess: (updatedUser) => {
      // Update auth store with new user data
      auth.updateUser({
        firstName: updatedUser.firstName,
        lastName: updatedUser.lastName,
        email: updatedUser.email,
        phone: updatedUser.phone,
        fullName: `${updatedUser.firstName} ${updatedUser.lastName}`.trim()
      })
      queryClient.invalidateQueries({ queryKey: ['user', user?.id] })
      message.success('Profile updated successfully')
      setIsEditing(false)
    },
    onError: (error: any) => {
      message.error(error?.response?.data?.message || 'Failed to update profile')
    }
  })

  const handleEdit = () => {
    if (user) {
      form.setFieldsValue({
        firstName: user.firstName,
        lastName: user.lastName,
        email: user.email,
        phone: user.phone || ''
      })
      setIsEditing(true)
    }
  }

  const handleCancel = () => {
    form.resetFields()
    setIsEditing(false)
  }

  const handleSubmit = async (values: any) => {
    if (!user) return

    const updateData: UpdateUserRequest = {
      email: values.email,
      firstName: values.firstName,
      lastName: values.lastName,
      phone: values.phone,
      role: user.role,
      isActive: user.isActive || true
    }

    await updateProfileMutation.mutateAsync(updateData)
  }

  if (!user) {
    return (
      <Card>
        <div style={{ textAlign: 'center', padding: '50px' }}>
          <Text>User information not available</Text>
        </div>
      </Card>
    )
  }

  return (
    <div>
      <Title level={2} style={{ marginBottom: 24 }}>My Profile</Title>

      <Row gutter={[24, 24]}>
        {/* Profile Information */}
        <Col xs={24} lg={16}>
          <Card
            title="Profile Information"
            extra={
              !isEditing && (
                <Button
                  type="primary"
                  icon={<EditOutlined />}
                  onClick={handleEdit}
                >
                  Edit Profile
                </Button>
              )
            }
          >
            {isEditing ? (
              <Form
                form={form}
                layout="vertical"
                onFinish={handleSubmit}
                initialValues={{
                  firstName: user.firstName,
                  lastName: user.lastName,
                  email: user.email,
                  phone: user.phone || ''
                }}
              >
                <Form.Item
                  label="First Name"
                  name="firstName"
                  rules={[{ required: true, message: 'Please enter first name' }]}
                >
                  <Input />
                </Form.Item>

                <Form.Item
                  label="Last Name"
                  name="lastName"
                  rules={[{ required: true, message: 'Please enter last name' }]}
                >
                  <Input />
                </Form.Item>

                <Form.Item
                  label="Email"
                  name="email"
                  rules={[
                    { required: true, message: 'Please enter email' },
                    { type: 'email', message: 'Please enter a valid email' }
                  ]}
                >
                  <Input disabled />
                </Form.Item>

                <Form.Item
                  label="Phone"
                  name="phone"
                >
                  <Input />
                </Form.Item>

                <Form.Item>
                  <Space>
                    <Button
                      type="primary"
                      htmlType="submit"
                      icon={<SaveOutlined />}
                      loading={updateProfileMutation.isPending}
                    >
                      Save Changes
                    </Button>
                    <Button onClick={handleCancel}>
                      Cancel
                    </Button>
                  </Space>
                </Form.Item>
              </Form>
            ) : (
              <Descriptions column={1} bordered>
                <Descriptions.Item label="Full Name">
                  <Text strong>{user.fullName || `${user.firstName} ${user.lastName}`.trim()}</Text>
                </Descriptions.Item>
                <Descriptions.Item label="Email">
                  <Text>{user.email}</Text>
                </Descriptions.Item>
                <Descriptions.Item label="Phone">
                  <Text>{user.phone || '-'}</Text>
                </Descriptions.Item>
                <Descriptions.Item label="Role">
                  <Text>{user.role}</Text>
                </Descriptions.Item>
                <Descriptions.Item label="Organization">
                  <Text>{user.organizationName || '-'}</Text>
                </Descriptions.Item>
                {user.selectedClinicName && (
                  <Descriptions.Item label="Selected Clinic">
                    <Text>{user.selectedClinicName}</Text>
                  </Descriptions.Item>
                )}
              </Descriptions>
            )}
          </Card>
        </Col>

        {/* Profile Picture */}
        <Col xs={24} lg={8}>
          <Card title="Profile Picture">
            <div style={{ textAlign: 'center', padding: '20px 0' }}>
              <Avatar
                size={120}
                icon={<UserOutlined />}
                style={{ backgroundColor: '#1890ff', marginBottom: 16 }}
              />
              <div>
                <Text type="secondary">Profile picture feature coming soon</Text>
              </div>
            </div>
          </Card>

          {/* Account Information */}
          <Card title="Account Information" style={{ marginTop: 24 }}>
            <Descriptions column={1} size="small">
              <Descriptions.Item label="Account Created">
                <Text>{user.createdAt ? dayjs(user.createdAt).format('MMM DD, YYYY') : '-'}</Text>
              </Descriptions.Item>
              <Descriptions.Item label="Last Login">
                <Text>{user.lastLoginAt ? dayjs(user.lastLoginAt).format('MMM DD, YYYY hh:mm A') : 'Never'}</Text>
              </Descriptions.Item>
              <Descriptions.Item label="Status">
                <Text>{user.isActive ? 'Active' : 'Inactive'}</Text>
              </Descriptions.Item>
            </Descriptions>
          </Card>
        </Col>
      </Row>
    </div>
  )
}

