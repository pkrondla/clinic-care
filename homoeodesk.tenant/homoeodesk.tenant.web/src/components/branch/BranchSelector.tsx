import { Select, Typography, message } from 'antd'
import { BankOutlined } from '@ant-design/icons'
import { useSelectedBranch, useAvailableBranches, useSelectBranch } from '@core/stores/authStore'
import { api } from '@core/services/apiClient'

const { Text } = Typography

export const BranchSelector = () => {
  const selectedClinic = useSelectedBranch()
  const AvailableBranches = useAvailableBranches()
  const selectClinic = useSelectBranch()

  // Don't show selector if user has only one clinic or no Branches
  if (AvailableBranches.length <= 1) {
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

  const handleBranchChange = async (branchId: number) => {
    try {
      // Update backend
      await api.post('/auth/select-branch', { branchId })
      
      const branch = AvailableBranches.find(c => c.id === branchId)
      if (branch) {
        selectClinic(branch)
        message.success(`Switched to ${branch.name}`)
      }
    } catch (error: unknown) {
      console.error('Failed to update branch:', error)
      message.error('Failed to switch branch')
    }
  }

  return (
    <div style={{ display: 'flex', alignItems: 'center', gap: 8, minWidth: 200 }}>
      <BankOutlined style={{ color: '#1890ff' }} />
      <Select
        value={selectedClinic?.id}
        onChange={handleBranchChange}
        style={{ minWidth: 180 }}
        placeholder="Select Branch"
        options={AvailableBranches.map(clinic => ({
          label: clinic.name,
          value: clinic.id
        }))}
        suffixIcon={<BankOutlined />}
      />
    </div>
  )
}

