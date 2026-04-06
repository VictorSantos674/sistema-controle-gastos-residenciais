import { Navigate, Route, Routes } from "react-router-dom";
import Layout from "./components/Layout";
import CategoriasPage from "./pages/CategoriasPage";
import PessoasPage from "./pages/PessoasPage";
import RelatoriosPage from "./pages/RelatoriosPage";
import TransacoesPage from "./pages/TransacoesPage";

export default function App() {
  return (
    <Routes>
      <Route path="/" element={<Layout />}>
        <Route index element={<Navigate to="/pessoas" replace />} />
        <Route path="pessoas" element={<PessoasPage />} />
        <Route path="categorias" element={<CategoriasPage />} />
        <Route path="transacoes" element={<TransacoesPage />} />
        <Route path="relatorios" element={<RelatoriosPage />} />
      </Route>
    </Routes>
  );
}
