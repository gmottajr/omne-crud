import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

const apiTarget = process.env.SERVER_HTTPS ?? process.env.SERVER_HTTP;

export default defineConfig({
  plugins: [react()],
  server: {
    proxy: apiTarget
      ? {
          '/products': {
            target: apiTarget,
            changeOrigin: true,
            secure: false
          }
        }
      : undefined
  }
});
