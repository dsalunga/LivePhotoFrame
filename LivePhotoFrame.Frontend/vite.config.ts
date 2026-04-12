import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig({
  plugins: [react()],
  build: {
    outDir: '../LivePhotoFrame.WebApp/wwwroot/app',
    emptyOutDir: true,
  },
  server: {
    proxy: {
      '/api': {
        target: 'https://localhost:5001',
        changeOrigin: true,
        secure: false,
      },
    },
  },
});
