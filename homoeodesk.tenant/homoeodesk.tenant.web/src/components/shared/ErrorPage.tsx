import { Button, Result } from 'antd'
import { useNavigate } from 'react-router-dom'

interface ErrorPageProps {
  title?: string
  subTitle?: string
  status?: 403 | 404 | 500
  extra?: React.ReactNode
}

export const ErrorPage = ({ 
  title = 'Page Not Found', 
  subTitle = 'Sorry, the page you visited does not exist.',
  status = 404,
  extra
}: ErrorPageProps) => {
  const navigate = useNavigate()

  return (
    <Result
      status={status}
      title={title}
      subTitle={subTitle}
      extra={extra || (
        <Button type="primary" onClick={() => navigate('/')}>
          Back Home
        </Button>
      )}
    />
  )
}