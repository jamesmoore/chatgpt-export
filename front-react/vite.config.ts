import path from "path"
import tailwindcss from "@tailwindcss/vite"
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react(), tailwindcss()],
  resolve: {
    alias: {
      "@": path.resolve(__dirname, "./src"),
    },
  },
  build: {
    outDir: path.resolve(__dirname, "../ChatGpt.Archive.Api/wwwroot"),
    emptyOutDir: false,
  },
  server: {
    proxy: {
      '/conversations': {
        target: 'https://localhost:7181',
        changeOrigin: true,
        secure: false,
      },
      '/asset': {
        target: 'https://localhost:7181',
        changeOrigin: true,
        secure: false,
      },
    },
  },
})
