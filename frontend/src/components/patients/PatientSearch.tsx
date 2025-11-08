import { useState, useEffect } from 'react'
import { AutoComplete, Avatar, Empty } from 'antd'
import { UserOutlined, SearchOutlined } from '@ant-design/icons'
import { useSearchPatients } from '../../hooks/queries/usePatients'
import type { PatientSearch as PatientSearchType } from '../../types/patient'

interface PatientSearchProps {
  onSelect?: (patient: PatientSearchType) => void
  placeholder?: string
  style?: React.CSSProperties
  disabled?: boolean
}

export const PatientSearch = ({ 
  onSelect, 
  placeholder = "Search patients...", 
  style,
  disabled = false 
}: PatientSearchProps) => {
  const [searchTerm, setSearchTerm] = useState('')
  const [options, setOptions] = useState<any[]>([])

  const { data: searchResults } = useSearchPatients({
    searchTerm,
    limit: 10
  })

  useEffect(() => {
    if (searchResults && searchResults.length > 0) {
      const autoCompleteOptions = searchResults.map(patient => ({
        value: `${patient.fullName} (${patient.patientCode})`,
        label: (
          <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
            <Avatar size="small" icon={<UserOutlined />} />
            <div>
              <div style={{ fontWeight: 500 }}>{patient.fullName}</div>
              <div style={{ fontSize: 12, color: '#666' }}>
                {patient.patientCode} • {patient.age} years • {patient.gender}
                {patient.bloodGroup && ` • ${patient.bloodGroup}`}
              </div>
            </div>
          </div>
        ),
        patient
      }))
      setOptions(autoCompleteOptions)
    } else if (searchTerm.length >= 2) {
      setOptions([{
        value: searchTerm,
        label: (
          <div style={{ textAlign: 'center', padding: 16 }}>
            <Empty 
              image={Empty.PRESENTED_IMAGE_SIMPLE}
              description="No patients found"
            />
          </div>
        ),
        disabled: true
      }])
    } else {
      setOptions([])
    }
  }, [searchResults, searchTerm])

  const handleSearch = (value: string) => {
    setSearchTerm(value)
  }

  const handleSelect = (_value: string, option: any) => {
    if (option.patient && onSelect) {
      onSelect(option.patient)
    }
  }

  const handleChange = (value: string) => {
    setSearchTerm(value)
  }

  return (
    <AutoComplete
      style={style}
      options={options}
      onSearch={handleSearch}
      onSelect={handleSelect}
      onChange={handleChange}
      placeholder={placeholder}
      disabled={disabled}
      notFoundContent={
        searchTerm.length < 2 ? (
          <div style={{ textAlign: 'center', padding: 16 }}>
            <SearchOutlined style={{ fontSize: 24, color: '#ccc', marginBottom: 8 }} />
            <div style={{ color: '#999' }}>Type at least 2 characters to search</div>
          </div>
        ) : null
      }
      filterOption={false}
    />
  )
}
