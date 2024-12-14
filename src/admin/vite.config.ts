import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

export default defineConfig({
  plugins: [vue()],
  server: {
    port: 5270,
    proxy: {
      '/api': {
        target: 'http://localhost:5269',
        changeOrigin: true
      }
    }
  }
})
