import tailwindcss from '@tailwindcss/vite'
import vue from '@vitejs/plugin-vue'
import { defineConfig } from 'vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [vue(), tailwindcss()],
  server: {
    proxy: {
      // Same-origin in development: the API client can call '/api/...' directly,
      // Vite forwards it to the ASP.NET Core backend without CORS.
      '/api': {
        target: 'http://localhost:5233',
        changeOrigin: true,
        rewrite: (path) => path.replace(/^\/api/, ''),
      },
    },
  },
})
