import { Card, Row, Col, Typography, Tag, Button, Space, Descriptions, Spin, Table } from 'antd'
import { ArrowLeftOutlined, DownloadOutlined, MedicineBoxOutlined, DollarOutlined, FileTextOutlined, PrinterOutlined } from '@ant-design/icons'
import { useParams, useNavigate } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { prescriptionService, Prescription } from '@core/services/prescriptionService'
import { useSelectedBranch } from '@core/stores/authStore'
import { message } from 'antd'
import dayjs from 'dayjs'

const { Title, Text } = Typography

const DISPENSING_FORMS: Record<number, string> = {
  1: 'Globules',
  2: 'Tablets',
  3: 'Packet',
  4: 'Liquid',
  5: 'Tonic'
}

export const PrescriptionDetailPage = () => {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const selectedClinic = useSelectedBranch()
  const { data: prescription, isLoading, refetch } = useQuery<Prescription>({
    queryKey: ['prescription', id],
    queryFn: () => prescriptionService.getById(Number(id!)),
    enabled: !!id && !isNaN(Number(id))
  })

  const handleCreateInvoice = () => {
    if (!prescription) return
    // Navigate to invoice form with prescription ID (same as when saving prescription)
    navigate(`/invoices/new?prescriptionId=${prescription.id}`)
  }

  const handleDownloadPdf = async (includeMedicineNames: boolean = true) => {
    if (!prescription) return
    try {
      const blob = await prescriptionService.downloadPdf(prescription.id, includeMedicineNames)
      const url = window.URL.createObjectURL(blob)
      const link = document.createElement('a')
      link.href = url
      link.download = `Prescription_${prescription.prescriptionNumber}_${includeMedicineNames ? 'Internal' : 'Patient'}.pdf`
      document.body.appendChild(link)
      link.click()
      document.body.removeChild(link)
      window.URL.revokeObjectURL(url)
    } catch (error) {
      console.error('Failed to download PDF:', error)
    }
  }

  const formatDispensedQuantity = (dispensedQuantity: number | undefined, dispensingForm: number): string => {
    if (dispensedQuantity === undefined) return '-'
    // Globules: show in drops
    if (dispensingForm === 1) {
      return `${Math.round(dispensedQuantity)} drops`
    }
    // Packets: show in drops (1 Packet = 5 drops)
    if (dispensingForm === 3) {
      return `${Math.round(dispensedQuantity)} drops`
    }
    // Liquid: show in ml
    if (dispensingForm === 4) {
      return `${dispensedQuantity} ml`
    }
    // Tonic, Tablets: show as count
    return Math.round(dispensedQuantity).toString()
  }

  const formatQuantity = (quantity: number | undefined, dispensingForm: number): string => {
    if (quantity === undefined) return '-'
    if (dispensingForm === 4 || dispensingForm === 5) {
      return `${quantity} ml`
    }
    return quantity.toString()
  }

  const handlePrintLabels = () => {
    if (!prescription || !prescription.medicines) return
    
    const clinicName = selectedClinic?.name || 'Clinic'
    
    // Create a print window with label layout
    const printWindow = window.open('', '_blank', 'width=800,height=600')
    if (!printWindow) return
    
    let labelsHtml = ''
    
    prescription.medicines.forEach((medicine, index) => {
      const serialNo = index + 1
      const quantity = medicine.quantity || 1
      
      // Generate labels based on quantity
      for (let i = 0; i < quantity; i++) {
        labelsHtml += `
          <div class="label">
            <div class="clinic-name">${clinicName}</div>
            <div class="medicine-info">#${serialNo}, ${medicine.dosage || '-'}, ${medicine.frequency || '-'}, ${medicine.timing || '-'}</div>
          </div>
        `
      }
    })
    
    printWindow.document.write(`
      <!DOCTYPE html>
      <html>
        <head>
          <title>Medicine Labels - ${prescription.prescriptionNumber}</title>
          <style>
            @page {
              size: 1.5in 1in;
              margin: 0;
            }
            
            body {
              margin: 0;
              padding: 0;
              font-family: Arial, sans-serif;
            }
            
            .label {
              width: 1.5in;
              height: 1in;
              padding: 4px 6px;
              box-sizing: border-box;
              page-break-after: always;
              border: 1px solid #ddd;
              display: flex;
              flex-direction: column;
              justify-content: center;
            }
            
            .clinic-name {
              font-size: 10px;
              font-weight: bold;
              margin-bottom: 3px;
              text-align: center;
              border-bottom: 1px solid #000;
              padding-bottom: 2px;
            }
            
            .medicine-info {
              font-size: 9px;
              line-height: 1.3;
              text-align: center;
            }
            
            @media print {
              .label {
                border: none;
              }
            }
          </style>
        </head>
        <body>
          ${labelsHtml}
        </body>
      </html>
    `)
    
    printWindow.document.close()
    
    // Trigger print after content loads
    printWindow.onload = () => {
      printWindow.focus()
      printWindow.print()
    }
  }

  const medicineColumns = [
    {
      title: 'S.No',
      key: 'serialNumber',
      width: 60,
      align: 'center' as const,
      render: (_: any, __: any, index: number) => index + 1
    },
    {
      title: 'Medicine Name',
      dataIndex: 'medicineName',
      key: 'medicineName',
      render: (name: string, record: Prescription['medicines'][0]) => (
        <div>
          <div style={{ fontWeight: 500 }}>{name}</div>
          <div style={{ fontSize: '12px', color: '#666' }}>
            {DISPENSING_FORMS[record.dispensingForm] || 'Unknown'} 
            {record.containerSize && record.dispensingForm === 1 && ` (${record.containerSize} dram)`}
          </div>
        </div>
      )
    },
    {
      title: 'Dosage',
      dataIndex: 'dosage',
      key: 'dosage',
      width: 120
    },
    {
      title: 'Frequency',
      dataIndex: 'frequency',
      key: 'frequency',
      width: 150
    },
    {
      title: 'Duration',
      dataIndex: 'duration',
      key: 'duration',
      width: 120
    },
    {
      title: 'Timing',
      dataIndex: 'timing',
      key: 'timing',
      width: 120
    },
    {
      title: 'Quantity',
      dataIndex: 'quantity',
      key: 'quantity',
      width: 100,
      render: (quantity: number | undefined, record: Prescription['medicines'][0]) => 
        formatQuantity(quantity, record.dispensingForm)
    },
    {
      title: 'Dispensed Qty',
      dataIndex: 'dispensedQuantity',
      key: 'dispensedQuantity',
      width: 120,
      render: (dispensedQuantity: number | undefined, record: Prescription['medicines'][0]) =>
        formatDispensedQuantity(dispensedQuantity, record.dispensingForm)
    },
    {
      title: 'Instructions',
      dataIndex: 'instructions',
      key: 'instructions',
      ellipsis: true,
      render: (instructions: string) => instructions || '-'
    }
  ]

  if (isLoading) {
    return (
      <div style={{ textAlign: 'center', padding: '50px' }}>
        <Spin size="large" />
      </div>
    )
  }

  if (!prescription) {
    return (
      <Card>
        <div style={{ textAlign: 'center', padding: '50px' }}>
          <Title level={4}>Prescription not found</Title>
          <Button onClick={() => navigate('/prescriptions')}>
            Back to Prescriptions
          </Button>
        </div>
      </Card>
    )
  }

  return (
    <div>
      <div style={{ marginBottom: 24 }}>
        <Space>
          <Button
            icon={<ArrowLeftOutlined />}
            onClick={() => navigate('/prescriptions')}
          >
            Back
          </Button>
          <Button
            icon={<MedicineBoxOutlined />}
            onClick={() => navigate(`/consultations/${prescription.consultationId}`)}
          >
            View Consultation
          </Button>
          {prescription.hasInvoice && prescription.invoiceId ? (
            <Button
              type="primary"
              icon={<FileTextOutlined />}
              onClick={() => navigate(`/invoices/${prescription.invoiceId}`)}
            >
              View Invoice
            </Button>
          ) : (
            <Button
              type="primary"
              icon={<DollarOutlined />}
              onClick={handleCreateInvoice}
            >
              Create Invoice
            </Button>
          )}
          <Button
            icon={<DownloadOutlined />}
            onClick={() => handleDownloadPdf(true)}
          >
            Download PDF (Internal)
          </Button>
          <Button
            icon={<DownloadOutlined />}
            onClick={() => handleDownloadPdf(false)}
          >
            Download PDF (Patient)
          </Button>
          <Button
            icon={<PrinterOutlined />}
            onClick={handlePrintLabels}
            type="default"
          >
            Print Labels
          </Button>
        </Space>
      </div>

      <Row gutter={[24, 24]}>
        {/* Prescription Details */}
        <Col xs={24}>
          <Card title="Prescription Details" size="small">
            <Descriptions column={2} bordered size="small">
              <Descriptions.Item label="Prescription Number">
                <Tag color="blue" style={{ fontSize: '13px', padding: '2px 8px' }}>
                  {prescription.prescriptionNumber}
                </Tag>
              </Descriptions.Item>
              <Descriptions.Item label="Date">
                <Text>{dayjs(prescription.prescriptionDate).format('MMM DD, YYYY hh:mm A')}</Text>
              </Descriptions.Item>
              <Descriptions.Item label="Patient">
                <Space>
                  <Text strong>{prescription.patientName}</Text>
                  <Button
                    type="link"
                    size="small"
                    onClick={() => navigate(`/patients/${prescription.patientId}`)}
                    style={{ padding: 0, height: 'auto' }}
                  >
                    View Profile
                  </Button>
                </Space>
              </Descriptions.Item>
              <Descriptions.Item label="Doctor">
                <Text strong>{prescription.doctorName}</Text>
              </Descriptions.Item>
              {prescription.notes && (
                <Descriptions.Item label="Notes" span={2}>
                  <Text style={{ whiteSpace: 'pre-wrap', fontSize: '13px' }}>{prescription.notes}</Text>
                </Descriptions.Item>
              )}
            </Descriptions>
          </Card>
        </Col>
      </Row>

      {/* Medicines Card */}
      <Row gutter={[24, 24]} style={{ marginTop: 24 }}>
        <Col xs={24}>
          <Card title="Medicines">
            <Table
              dataSource={prescription.medicines || []}
              columns={medicineColumns}
              rowKey={(record, index) => `${record.medicineId}-${index}`}
              pagination={false}
              size="small"
            />
          </Card>
        </Col>
      </Row>
    </div>
  )
}

