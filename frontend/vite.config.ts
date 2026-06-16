import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import dns from 'node:dns'

// Node.js prefers IPv6 by default. If the .NET backend is listening on IPv4 (127.0.0.1),
// Node will wait for an IPv6 connection to timeout (2+ seconds) before falling back to IPv4.
// This causes massive delays on every proxied API request. Setting this fixes it.
dns.setDefaultResultOrder('ipv4first')

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      // Proxy API calls to the app service
      '/api': {
        target: process.env.SERVER_HTTPS || process.env.SERVER_HTTP,
        changeOrigin: true
      },
      '/hubs': {
        target: process.env.SERVER_HTTPS || process.env.SERVER_HTTP,
        ws: true,
        changeOrigin: true
      }
    }
  }
})
