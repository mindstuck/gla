import tailwindcss from '@tailwindcss/vite'
import { alphaTab } from '@coderline/alphatab-vite'
import vue from '@vitejs/plugin-vue'
import { defineConfig } from 'vite'

// https://vite.dev/config/
export default defineConfig({
  // alphaTab: the plugin wires up Web Workers + Audio Worklets and copies the
  // bundled Bravura font and SONiVOX soundfont to /font and /soundfont.
  plugins: [vue(), tailwindcss(), alphaTab()],
  server: {
    // Listen on all interfaces so the dev app is reachable from other devices
    // on the LAN via this machine's IP (localhost keeps working).
    host: true,
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
