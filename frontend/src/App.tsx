import { Navigate, Route, Routes } from "react-router-dom";
import ProtectedRoute from "./components/ProtectedRoute";
import Layout from "./components/Layout";
import { AuthProvider } from "./contexts/AuthContext";
import CategoriasPage from "./pages/CategoriasPage";
import DashboardPage from "./pages/DashboardPage";
import LoginPage from "./pages/LoginPage";
import PessoasPage from "./pages/PessoasPage";
import RegisterPage from "./pages/RegisterPage";
import RelatoriosPage from "./pages/RelatoriosPage";
import TransacoesPage from "./pages/TransacoesPage";

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
            <Route path="transacoes" element={<TransacoesPage />} />
            <Route path="relatorios" element={<RelatoriosPage />} />
          </Route>
        </Route>

        {/* Fallback */}
        <Route path="*" element={<Navigate to="/login" replace />} />
      </Routes>
    </AuthProvider>
  );
}
