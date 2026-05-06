import React, { Suspense } from "react";
import { Navigate, Route, Routes } from "react-router-dom";
import ProtectedRoute from "./components/ProtectedRoute";
import Layout from "./components/Layout";
import { AuthProvider } from "./contexts/AuthContext";
import CategoriasPage from "./pages/CategoriasPage";
import DashboardPage from "./pages/DashboardPage";
import LoginPage from "./pages/LoginPage";
import PessoasPage from "./pages/PessoasPage";
import RegisterPage from "./pages/RegisterPage";

const RelatoriosPage = React.lazy(() => import("./pages/RelatoriosPage"));
const TransacoesPage = React.lazy(() => import("./pages/TransacoesPage"));

function PageFallback() {
  return (
    <div className="flex items-center gap-2 p-6 text-sm text-gray-400 dark:text-gray-500">
      <div className="h-4 w-4 animate-spin rounded-full border-2 border-blue-500 border-t-transparent" />
      Carregando...
    </div>
  );
}

export default function App() {
  return (
    <AuthProvider>
      <Routes>
        {/* Rotas públicas */}
        <Route path="/login"    element={<LoginPage />} />
        <Route path="/cadastro" element={<RegisterPage />} />

        {/* Rotas protegidas — exigem autenticação */}
        <Route element={<ProtectedRoute />}>
          <Route path="/" element={<Layout />}>
            <Route index element={<Navigate to="/dashboard" replace />} />
            <Route path="dashboard"  element={<DashboardPage />} />
            <Route path="pessoas"    element={<PessoasPage />} />
            <Route path="categorias" element={<CategoriasPage />} />
            <Route path="transacoes" element={<Suspense fallback={<PageFallback />}><TransacoesPage /></Suspense>} />
            <Route path="relatorios" element={<Suspense fallback={<PageFallback />}><RelatoriosPage /></Suspense>} />
          </Route>
        </Route>

        {/* Fallback */}
        <Route path="*" element={<Navigate to="/login" replace />} />
      </Routes>
    </AuthProvider>
  );
}
