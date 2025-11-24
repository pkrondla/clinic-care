import { Select, Typography, message } from 'antd'
import { BankOutlined } from '@ant-design/icons'
import { useSelectedClinic, useAvailableClinics, useSelectClinic } from '@core/stores/authStore'
import { api } from '@core/services/apiClient'

const { Text } = Typography

export const ClinicSelector = () => {
  const selectedClinic = useSelectedClinic()
  const availableClinics = useAvailableClinics()
  const selectClinic = useSelectClinic()

  // Don't show selector if user has only one clinic or no clinics
  if (availableClinics.length <= 1) {
    if (selectedClinic) {
      return (
        <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
          <BankOutlined />
          <Text type="secondary">{selectedClinic.name}</Text>
        </div>
      )
    }
    return null
  }

  const handleClinicChange = async (clinicId: number) => {
    try {
      // Update backend
      await api.post('/auth/select-clinic', { clinicId })
      
      // Find clinic in available clinics
      const clinic = availableClinics.find(c => c.id === clinicId)
      if (clinic) {
        selectClinic(clinic)
        message.success(`Switched to ${clinic.name}`)
      }
    } catch (error: any) {
      console.error('Failed to update clinic:', error)
      message.error(error?.response?.data?.message || 'Failed to switch clinic')
    }
  }

  return (
    <div style={{ display: 'flex', alignItems: 'center', gap: 8, minWidth: 200 }}>
      <BankOutlined style={{ color: '#1890ff' }} />
      <Select
        value={selectedClinic?.id}
        onChange={handleClinicChange}
        style={{ minWidth: 180 }}
        placeholder="Select Clinic"
        options={availableClinics.map(clinic => ({
          label: clinic.name,
          value: clinic.id
        }))}
        suffixIcon={<BankOutlined />}
      />
    </div>
  )
}

