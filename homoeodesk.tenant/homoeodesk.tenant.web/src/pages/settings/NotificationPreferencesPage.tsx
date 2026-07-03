import { useState, useEffect } from 'react';
import {
  Card,
  Table,
  Switch,
  Button,
  Space,
  Typography,
  message,
  Modal,
  Input,
  Spin,
  Tag,
  Tooltip,
} from 'antd';
import { SaveOutlined, EditOutlined, EyeOutlined } from '@ant-design/icons';
import {
  useNotificationPreferences,
  useUpdateNotificationPreferences,
} from '@core/hooks/queries/useNotificationPreferences';
import { NotificationPreference } from '@core/services/notificationPreferencesService';

const { Title, Text } = Typography;
const { TextArea } = Input;

export const NotificationPreferencesPage = () => {
  const [formData, setFormData] = useState<NotificationPreference[]>([]);
  const [editingTemplate, setEditingTemplate] = useState<number | null>(null);
  const [templatePreview, setTemplatePreview] = useState<string>('');
  const [previewVisible, setPreviewVisible] = useState(false);

  const { data: preferences, isLoading, refetch } = useNotificationPreferences();
  const updateMutation = useUpdateNotificationPreferences();

  useEffect(() => {
    if (preferences) {
      setFormData([...preferences]);
    }
  }, [preferences]);

  const handleToggle = (index: number, field: 'enableWhatsApp' | 'enableEmail' | 'enableSMS') => {
    const updated = [...formData];
    updated[index] = { ...updated[index], [field]: !updated[index][field] };
    setFormData(updated);
  };

  const handleTemplateChange = (index: number, template: string) => {
    const updated = [...formData];
    updated[index] = { ...updated[index], template };
    setFormData(updated);
  };

  const handleSave = async () => {
    try {
      await updateMutation.mutateAsync({
        preferences: formData.map((p) => ({
          notificationType: p.notificationType,
          enableWhatsApp: p.enableWhatsApp,
          enableEmail: p.enableEmail,
          enableSMS: p.enableSMS,
          template: p.template,
          isActive: p.isActive,
        })),
      });
      await refetch();
    } catch (error) {
      // Error handled by mutation
    }
  };

  const handlePreviewTemplate = (template: string) => {
    // Replace variables with sample data for preview
    const preview = template
      .replace(/\{\{PatientName\}\}/g, 'John Doe')
      .replace(/\{\{DoctorName\}\}/g, 'Dr. Smith')
      .replace(/\{\{ClinicName\}\}/g, 'Demo Clinic')
      .replace(/\{\{AppointmentDate\}\}/g, '25/12/2024')
      .replace(/\{\{AppointmentTime\}\}/g, '10:00 AM')
      .replace(/\{\{TokenNumber\}\}/g, '5')
      .replace(/\{\{PrescriptionNumber\}\}/g, 'PR-2024-001')
      .replace(/\{\{InvoiceNumber\}\}/g, 'INV-2024-001')
      .replace(/\{\{TotalAmount\}\}/g, '₹1,500.00')
      .replace(/\{\{AmountPaid\}\}/g, '₹1,500.00')
      .replace(/\{\{AmountDue\}\}/g, '₹0.00')
      .replace(/\{\{DocketNumber\}\}/g, 'COU-123456')
      .replace(/\{\{ClinicAddress\}\}/g, '123 Main Street, City')
      .replace(/\{\{ClinicHours\}\}/g, '9:00 AM - 6:00 PM')
      .replace(/\{\{PatientToken\}\}/g, '5')
      .replace(/\{\{CurrentToken\}\}/g, '3')
      .replace(/\{\{ConsultationDate\}\}/g, '24/12/2024')
      .replace(/\{\{PaymentDate\}\}/g, '25/12/2024')
      .replace(/\{\{InvoiceDate\}\}/g, '24/12/2024')
      .replace(/\{\{PrescriptionDate\}\}/g, '24/12/2024')
      .replace(/\{\{ExpectedDeliveryDate\}\}/g, '28/12/2024')
      .replace(/\{\{FollowUpDate\}\}/g, '01/01/2025');

    setTemplatePreview(preview);
    setPreviewVisible(true);
  };

  const columns = [
    {
      title: 'Notification Type',
      dataIndex: 'notificationTypeName',
      key: 'notificationTypeName',
      width: 250,
      fixed: 'left' as const,
    },
    {
      title: 'WhatsApp',
      dataIndex: 'enableWhatsApp',
      key: 'enableWhatsApp',
      width: 100,
      align: 'center' as const,
      render: (_: boolean, record: NotificationPreference, index: number) => (
        <Switch
          checked={formData[index]?.enableWhatsApp}
          onChange={() => handleToggle(index, 'enableWhatsApp')}
        />
      ),
    },
    {
      title: 'Email',
      dataIndex: 'enableEmail',
      key: 'enableEmail',
      width: 100,
      align: 'center' as const,
      render: (_: boolean, record: NotificationPreference, index: number) => (
        <Switch
          checked={formData[index]?.enableEmail}
          onChange={() => handleToggle(index, 'enableEmail')}
        />
      ),
    },
    {
      title: 'SMS',
      dataIndex: 'enableSMS',
      key: 'enableSMS',
      width: 100,
      align: 'center' as const,
      render: (_: boolean, record: NotificationPreference, index: number) => (
        <Switch
          checked={formData[index]?.enableSMS}
          onChange={() => handleToggle(index, 'enableSMS')}
        />
      ),
    },
    {
      title: 'Template',
      key: 'template',
      width: 150,
      render: (_: any, record: NotificationPreference, index: number) => {
        const hasCustomTemplate = formData[index]?.template && formData[index].template!.trim().length > 0;
        return (
          <Space>
            {hasCustomTemplate ? (
              <Tag color="blue">Custom</Tag>
            ) : (
              <Tag>Default</Tag>
            )}
            <Button
              type="link"
              size="small"
              icon={<EditOutlined />}
              onClick={() => setEditingTemplate(index)}
            >
              Edit
            </Button>
            {hasCustomTemplate && (
              <Button
                type="link"
                size="small"
                icon={<EyeOutlined />}
                onClick={() => handlePreviewTemplate(formData[index].template!)}
              >
                Preview
              </Button>
            )}
          </Space>
        );
      },
    },
  ];

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
        Notification Preferences
      </Title>

      <Card
        title="Notification Channel Preferences"
        extra={
          <Button
            type="primary"
            icon={<SaveOutlined />}
            onClick={handleSave}
            loading={updateMutation.isPending}
          >
            Save Preferences
          </Button>
        }
      >
        <Table
          dataSource={formData}
          columns={columns}
          rowKey="notificationType"
          pagination={false}
          scroll={{ x: 800 }}
        />
      </Card>

      {/* Template Editor Modal */}
      <Modal
        title={`Edit Template: ${editingTemplate !== null ? formData[editingTemplate]?.notificationTypeName : ''}`}
        open={editingTemplate !== null}
        onCancel={() => setEditingTemplate(null)}
        onOk={() => setEditingTemplate(null)}
        width={800}
        footer={[
          <Button key="cancel" onClick={() => setEditingTemplate(null)}>
            Cancel
          </Button>,
          <Button
            key="preview"
            onClick={() => {
              if (editingTemplate !== null && formData[editingTemplate]?.template) {
                handlePreviewTemplate(formData[editingTemplate].template!);
              }
            }}
          >
            Preview
          </Button>,
          <Button key="ok" type="primary" onClick={() => setEditingTemplate(null)}>
            Save
          </Button>,
        ]}
      >
        {editingTemplate !== null && (
          <div>
            <Text type="secondary" style={{ display: 'block', marginBottom: 8 }}>
              Use variables like {'{{PatientName}}'}, {'{{AppointmentDate}}'}, etc. Leave empty to use default template.
            </Text>
            <TextArea
              rows={15}
              value={formData[editingTemplate]?.template || ''}
              onChange={(e) => handleTemplateChange(editingTemplate, e.target.value)}
              placeholder="Enter custom template or leave empty for default..."
            />
            <div style={{ marginTop: 16 }}>
              <Text strong>Available Variables:</Text>
              <div style={{ marginTop: 8, fontSize: '12px', color: '#666' }}>
                Patient: {'{{PatientName}}'}, {'{{PatientCode}}'}, {'{{PatientPhone}}'}
                <br />
                Appointment: {'{{AppointmentDate}}'}, {'{{AppointmentTime}}'}, {'{{TokenNumber}}'}
                <br />
                Doctor: {'{{DoctorName}}'}
                <br />
                Clinic: {'{{ClinicName}}'}, {'{{ClinicAddress}}'}, {'{{ClinicHours}}'}
                <br />
                Prescription: {'{{PrescriptionNumber}}'}, {'{{PrescriptionDate}}'}
                <br />
                Invoice: {'{{InvoiceNumber}}'}, {'{{TotalAmount}}'}, {'{{AmountPaid}}'}, {'{{AmountDue}}'}, {'{{InvoiceDate}}'}
                <br />
                Courier: {'{{DocketNumber}}'}, {'{{CourierCompany}}'}, {'{{TrackingUrl}}'}, {'{{ExpectedDeliveryDate}}'}
                <br />
                Other: {'{{PaymentDate}}'}, {'{{ConsultationDate}}'}, {'{{PatientToken}}'}, {'{{CurrentToken}}'}, {'{{FollowUpDate}}'}
              </div>
            </div>
          </div>
        )}
      </Modal>

      {/* Template Preview Modal */}
      <Modal
        title="Template Preview"
        open={previewVisible}
        onCancel={() => setPreviewVisible(false)}
        footer={[
          <Button key="close" onClick={() => setPreviewVisible(false)}>
            Close
          </Button>,
        ]}
        width={600}
      >
        <div style={{ whiteSpace: 'pre-wrap', fontFamily: 'monospace', padding: '16px', background: '#f5f5f5', borderRadius: '4px' }}>
          {templatePreview}
        </div>
      </Modal>
    </div>
  );
};

