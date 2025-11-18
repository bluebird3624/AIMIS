import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    watch: {
      ignored: ['**/AppData/**'],
      
    },
    allowedHosts: ['.ngrok-free.dev', "https://unweighty-rosella-knowable.ngrok-free.dev"]
  },
})
