import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import { VitePWA } from 'vite-plugin-pwa'
import path from 'path'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [
    react(),
    VitePWA({
      registerType: 'autoUpdate',
      workbox: {
        globPatterns: ['**/*.{js,css,html,ico,png,svg}']
      },
      manifest: {
        name: 'ClinicCare',
        short_name: 'ClinicCare',
        description: 'Homoeopathy Clinic Management System',
        theme_color: '#1890ff',
        background_color: '#ffffff',
        display: 'standalone',
        icons: [
          {
            src: 'icon-192.png',
            sizes: '192x192',
            type: 'image/png'
          },
          {
            src: 'icon-512.png',
            sizes: '512x512',
            type: 'image/png'
          }
        ]
      }
    })
  ],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
      '@core': path.resolve(__dirname, './src/core'),
      '@components': path.resolve(__dirname, './src/components'),
      '@shared': path.resolve(__dirname, './src/components/shared'),
      '@auth': path.resolve(__dirname, './src/components/auth'),
      '@apps': path.resolve(__dirname, './src/apps'),
      '@hooks': path.resolve(__dirname, './src/core/hooks'),
      '@services': path.resolve(__dirname, './src/core/services'),
      '@stores': path.resolve(__dirname, './src/core/stores'),
      '@types': path.resolve(__dirname, './src/core/types'),
      '@providers': path.resolve(__dirname, './src/core/providers')
    }
  },
  server: {
    port: 3000,
    proxy: {
      '/api': {
        target: 'http://localhost:51537',
        changeOrigin: true,
        secure: false
      },
      '/queueHub': {
        target: 'ws://localhost:51537',
        changeOrigin: true,
        secure: false,
        ws: true
      }
    }
  }
})