import { defineConfig } from 'vite';
import basicSsl from '@vitejs/plugin-basic-ssl';

/** WebXR exige HTTPS (salvo localhost). basic-ssl genera certificado de desarrollo. */
export default defineConfig({
  plugins: [basicSsl()],
  server: {
    port: 5173,
    strictPort: true,
    proxy: {
      '/desing-stl': {
        target: 'https://localhost:44384',
        changeOrigin: true,
        secure: false,
        rewrite: (path) => path.replace(/^\/desing-stl/, '')
      }
    }
  },
  build: {
    outDir: 'dist',
    emptyOutDir: true
  }
});
