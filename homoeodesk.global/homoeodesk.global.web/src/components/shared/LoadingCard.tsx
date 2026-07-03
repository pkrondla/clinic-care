import { Card, Skeleton } from 'antd'
import { PropsWithChildren } from 'react'

interface LoadingCardProps {
  loading?: boolean
  title?: React.ReactNode
  extra?: React.ReactNode
}

export const LoadingCard = ({ 
  loading = true, 
  title, 
  extra, 
  children 
}: PropsWithChildren<LoadingCardProps>) => {
  return (
    <Card title={title} extra={extra}>
      {loading ? (
        <Skeleton active />
      ) : (
        children
      )}
    </Card>
  )
}