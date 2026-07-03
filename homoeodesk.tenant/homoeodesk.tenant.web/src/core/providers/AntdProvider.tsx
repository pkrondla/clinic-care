import { ConfigProvider, theme } from 'antd'
import { useTheme } from '../stores/uiStore'

interface AntdProviderProps {
  children: React.ReactNode
}

export const AntdProvider = ({ children }: AntdProviderProps) => {
  const { theme: currentTheme } = useTheme()

  return (
    <ConfigProvider
      theme={{
        algorithm: currentTheme === 'dark' ? theme.darkAlgorithm : theme.defaultAlgorithm,
        token: {
          // Medical/Healthcare color scheme
          colorPrimary: '#1890ff', // Professional blue
          colorSuccess: '#52c41a', // Healthy green
          colorWarning: '#faad14', // Attention orange
          colorError: '#ff4d4f', // Alert red
          colorInfo: '#1890ff', // Information blue
          
          // Typography
          fontFamily: '"Segoe UI", "Roboto", "Helvetica Neue", Arial, sans-serif',
          fontSize: 14,
          fontSizeHeading1: 32,
          fontSizeHeading2: 24,
          fontSizeHeading3: 20,
          fontSizeHeading4: 16,
          
          // Layout
          borderRadius: 6,
          boxShadow: '0 2px 8px rgba(0, 0, 0, 0.15)',
          
          // Healthcare-specific colors
          colorBgContainer: currentTheme === 'dark' ? '#141414' : '#ffffff',
          colorBgLayout: currentTheme === 'dark' ? '#000000' : '#f5f5f5',
          colorBgElevated: currentTheme === 'dark' ? '#1f1f1f' : '#ffffff'
        },
        components: {
          Layout: {
            headerBg: currentTheme === 'dark' ? '#141414' : '#ffffff',
            siderBg: currentTheme === 'dark' ? '#141414' : '#ffffff',
            bodyBg: currentTheme === 'dark' ? '#000000' : '#f5f5f5'
          },
          Menu: {
            itemBg: 'transparent',
            subMenuItemBg: 'transparent',
            itemSelectedBg: '#e6f7ff',
            itemSelectedColor: '#1890ff'
          },
          Card: {
            headerBg: 'transparent',
            boxShadow: '0 1px 3px rgba(0, 0, 0, 0.12), 0 1px 2px rgba(0, 0, 0, 0.24)'
          },
          Table: {
            headerBg: currentTheme === 'dark' ? '#1f1f1f' : '#fafafa',
            rowHoverBg: currentTheme === 'dark' ? '#262626' : '#f5f5f5'
          },
          Button: {
            borderRadius: 6,
            fontWeight: 500
          },
          Input: {
            borderRadius: 6
          },
          Select: {
            borderRadius: 6
          },
          DatePicker: {
            borderRadius: 6
          }
        }
      }}
      // locale configuration can be added here if needed
    >
      {children}
    </ConfigProvider>
  )
}
