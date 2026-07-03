import { Result, Button, Typography } from 'antd'
import { MailOutlined } from '@ant-design/icons'
import { Link } from 'react-router-dom'

const { Paragraph, Text } = Typography

export const TrialExpiredPage = () => {
  return (
    <div
      style={{
        minHeight: '100vh',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        background: '#f5f5f5',
        padding: 24
      }}
    >
      <Result
        status="warning"
        title="Your trial has expired"
        subTitle="Your HomoeoDesk trial period has ended. Upgrade your subscription to continue using the clinic management features."
        extra={[
          <Button type="primary" key="contact" icon={<MailOutlined />} href="mailto:support@homoeodesk.com">
            Contact Sales
          </Button>,
          <Link to="/login" key="login">
            <Button>Back to Login</Button>
          </Link>
        ]}
      >
        <Paragraph>
          <Text type="secondary">
            Need help? Visit{' '}
            <a href="https://homoeodesk.com" target="_blank" rel="noreferrer">
              homoeodesk.com
            </a>{' '}
            or email support@homoeodesk.com.
          </Text>
        </Paragraph>
      </Result>
    </div>
  )
}
