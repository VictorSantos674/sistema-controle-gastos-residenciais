import tailwindcss from "@tailwindcss/vite";
import react from "@vitejs/plugin-react";
import { defineConfig } from "vite";

/**
 * Configuração do Vite para o frontend aprimorado (Projeto 2).
 *
 * Plugins:
 *   - tailwindcss: integração nativa do Tailwind CSS v4 com Vite (sem PostCSS).
 *   - react: suporte a JSX/TSX com Fast Refresh.
 *
 * O proxy redireciona /api/* para o backend .NET em desenvolvimento,
 * eliminando problemas de CORS.
 */
export default defineConfig({
  plugins: [tailwindcss(), react()],
  server: {
    port: 5173,
    proxy: {
      "/api": {
        target: "http://localhost:5000",
        changeOrigin: true,
      },
    },
  },
});
