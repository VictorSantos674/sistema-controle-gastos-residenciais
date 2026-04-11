import { RefreshCw } from "lucide-react";
import { useEffect, useState } from "react";
import {
  Bar,
  BarChart,
  CartesianGrid,
  Cell,
  Legend,
  Pie,
  PieChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from "recharts";
import { type FiltroRelatorio, relatorioPorCategoria, relatorioPorPessoa } from "../api/relatorios";
import { Badge } from "../components/ui/badge";
import { Button } from "../components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "../components/ui/card";
import { Select } from "../components/ui/select";
import type { Finalidade, RelatorioPorCategoria, RelatorioPorPessoa } from "../types";

const PIE_COLORS = ["#e74c3c", "#e67e22", "#f39c12", "#9b59b6", "#2980b9", "#16a085", "#27ae60"];

const MESES = [
  "Janeiro", "Fevereiro", "Março", "Abril", "Maio", "Junho",
  "Julho", "Agosto", "Setembro", "Outubro", "Novembro", "Dezembro",
];

const gerarAnos = () => {
  const atual = new Date().getFullYear();
  return Array.from({ length: 5 }, (_, i) => atual - 4 + i);
};

function finalidadeBadge(finalidade: string) {
  if (finalidade === "Receita") return <Badge variant="receita">Receita</Badge>;
  if (finalidade === "Despesa") return <Badge variant="despesa">Despesa</Badge>;
  return <Badge variant="ambas">Ambas</Badge>;
}

export default function RelatoriosPage() {
  const [porPessoa, setPorPessoa] = useState<RelatorioPorPessoa | null>(null);
  const [porCategoria, setPorCategoria] = useState<RelatorioPorCategoria | null>(null);
  const [loading, setLoading] = useState(false);
  const [erro, setErro] = useState("");
  const [filtroFinalidade, setFiltroFinalidade] = useState<"" | Finalidade>("");
  const [filtroMes, setFiltroMes] = useState<number | undefined>(undefined);
  const [filtroAno, setFiltroAno] = useState<number | undefined>(new Date().getFullYear());

  const carregar = async () => {
    setLoading(true);
    setErro("");
    try {
      const filtro: FiltroRelatorio = { mes: filtroMes, ano: filtroAno };
      const [p, c] = await Promise.all([relatorioPorPessoa(filtro), relatorioPorCategoria(filtro)]);
      setPorPessoa(p);
      setPorCategoria(c);
    } catch (err: unknown) {
      setErro(err instanceof Error ? err.message : "Erro ao carregar relatórios.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { carregar(); }, [filtroMes, filtroAno]);

  const fmt = (v: number) => v.toLocaleString("pt-BR", { style: "currency", currency: "BRL" });

  const saldoClass = (v: number) =>
    v >= 0 ? "font-semibold text-emerald-600" : "font-semibold text-red-600";

  const barData =
    porPessoa?.pessoas.map((p) => ({
      name: p.nomePessoa.split(" ")[0],
      Receitas: p.totalReceitas,
      Despesas: p.totalDespesas,
    })) ?? [];

  const pieData =
    porCategoria?.categorias
      .filter((c) => c.totalDespesas > 0)
      .map((c) => ({ name: c.descricaoCategoria, value: c.totalDespesas })) ?? [];

  const categoriasFiltradas =
    porCategoria?.categorias.filter(
      (c) => filtroFinalidade === "" || c.finalidade === filtroFinalidade
    ) ?? [];

  const labelPeriodo =
    filtroAno && filtroMes
      ? `${MESES[filtroMes - 1]} de ${filtroAno}`
      : filtroAno
      ? `Ano ${filtroAno}`
      : filtroMes
      ? MESES[filtroMes - 1]
      : "Todos os períodos";

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Relatórios</h1>
          <p className="text-sm text-gray-500">Análise financeira por pessoa e categoria</p>
        </div>
        <Button size="sm" variant="outline" onClick={carregar} disabled={loading}>
          <RefreshCw size={14} className={loading ? "animate-spin" : ""} />
          Atualizar
        </Button>
      </div>

      {/* Filtro de período */}
      <Card>
        <CardContent className="flex flex-wrap items-center gap-4 py-4">
          <span className="text-sm font-semibold text-gray-600">Período:</span>
          <div className="flex items-center gap-2">
            <label className="text-xs text-gray-500">Mês</label>
            <Select
              className="max-w-[150px]"
              value={filtroMes ?? ""}
              onChange={(e) =>
                setFiltroMes(e.target.value === "" ? undefined : Number(e.target.value))
              }
            >
              <option value="">Todos</option>
              {MESES.map((m, i) => (
                <option key={i + 1} value={i + 1}>{m}</option>
              ))}
            </Select>
          </div>
          <div className="flex items-center gap-2">
            <label className="text-xs text-gray-500">Ano</label>
            <Select
              className="max-w-[110px]"
              value={filtroAno ?? ""}
              onChange={(e) =>
                setFiltroAno(e.target.value === "" ? undefined : Number(e.target.value))
              }
            >
              <option value="">Todos</option>
              {gerarAnos().map((a) => (
                <option key={a} value={a}>{a}</option>
              ))}
            </Select>
          </div>
          <span className="text-xs italic text-gray-400">{labelPeriodo}</span>
        </CardContent>
      </Card>

      {erro && (
        <div className="rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
          {erro}
        </div>
      )}

      {/* Totais por Pessoa */}
      <Card>
        <CardHeader>
          <CardTitle>Totais por Pessoa</CardTitle>
        </CardHeader>
        <CardContent className="p-0">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b bg-gray-50">
                <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-gray-500">Pessoa</th>
                <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-emerald-600">Receitas</th>
                <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-red-500">Despesas</th>
                <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-gray-500">Saldo</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {porPessoa?.pessoas.map((p) => (
                <tr key={p.pessoaId} className="hover:bg-gray-50">
                  <td className="px-4 py-3 font-medium text-gray-800">{p.nomePessoa}</td>
                  <td className="px-4 py-3 text-emerald-600">{fmt(p.totalReceitas)}</td>
                  <td className="px-4 py-3 text-red-600">{fmt(p.totalDespesas)}</td>
                  <td className={`px-4 py-3 ${saldoClass(p.saldo)}`}>{fmt(p.saldo)}</td>
                </tr>
              ))}
              {porPessoa && (
                <tr className="bg-slate-800 text-white">
                  <td className="px-4 py-3 font-bold">Total Geral</td>
                  <td className="px-4 py-3 font-bold text-emerald-400">{fmt(porPessoa.totalGeralReceitas)}</td>
                  <td className="px-4 py-3 font-bold text-red-400">{fmt(porPessoa.totalGeralDespesas)}</td>
                  <td className={`px-4 py-3 font-bold ${porPessoa.saldoLiquido >= 0 ? "text-emerald-400" : "text-red-400"}`}>
                    {fmt(porPessoa.saldoLiquido)}
                  </td>
                </tr>
              )}
              {!porPessoa && (
                <tr>
                  <td colSpan={4} className="px-4 py-10 text-center text-sm text-gray-400">Carregando...</td>
                </tr>
              )}
            </tbody>
          </table>
        </CardContent>
      </Card>

      {/* Bar chart */}
      {barData.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle>Receitas vs Despesas por Pessoa</CardTitle>
          </CardHeader>
          <CardContent>
            <ResponsiveContainer width="100%" height={280}>
              <BarChart data={barData} margin={{ top: 4, right: 16, left: 8, bottom: 4 }}>
                <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
                <XAxis dataKey="name" tick={{ fontSize: 13 }} />
                <YAxis tickFormatter={(v: number) => `R$${v}`} tick={{ fontSize: 12 }} />
                <Tooltip formatter={(v) => (typeof v === "number" ? fmt(v) : v)} />
                <Legend />
                <Bar dataKey="Receitas" fill="#10b981" radius={[4, 4, 0, 0]} />
                <Bar dataKey="Despesas" fill="#ef4444" radius={[4, 4, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </CardContent>
        </Card>
      )}

      {/* Totais por Categoria */}
      <Card>
        <CardHeader>
          <div className="flex items-center justify-between gap-4">
            <CardTitle>Totais por Categoria</CardTitle>
            <div className="flex items-center gap-2">
              <label className="text-xs text-gray-500">Finalidade:</label>
              <Select
                className="max-w-[160px]"
                value={filtroFinalidade}
                onChange={(e) => setFiltroFinalidade(e.target.value as "" | Finalidade)}
              >
                <option value="">Todas</option>
                <option value="Despesa">Despesa</option>
                <option value="Receita">Receita</option>
                <option value="Ambas">Ambas</option>
              </Select>
            </div>
          </div>
        </CardHeader>
        <CardContent className="p-0">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b bg-gray-50">
                <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-gray-500">Categoria</th>
                <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-gray-500">Finalidade</th>
                <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-emerald-600">Receitas</th>
                <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-red-500">Despesas</th>
                <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-gray-500">Saldo</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {categoriasFiltradas.map((c) => (
                <tr key={c.categoriaId} className="hover:bg-gray-50">
                  <td className="px-4 py-3 font-medium text-gray-800">{c.descricaoCategoria}</td>
                  <td className="px-4 py-3">{finalidadeBadge(c.finalidade)}</td>
                  <td className={`px-4 py-3 ${c.finalidade === "Despesa" ? "text-gray-300" : "text-emerald-600"}`}>
                    {c.finalidade === "Despesa" ? "—" : fmt(c.totalReceitas)}
                  </td>
                  <td className={`px-4 py-3 ${c.finalidade === "Receita" ? "text-gray-300" : "text-red-600"}`}>
                    {c.finalidade === "Receita" ? "—" : fmt(c.totalDespesas)}
                  </td>
                  <td className={`px-4 py-3 ${saldoClass(c.saldo)}`}>{fmt(c.saldo)}</td>
                </tr>
              ))}
              {porCategoria && categoriasFiltradas.length === 0 && (
                <tr>
                  <td colSpan={5} className="px-4 py-10 text-center text-sm text-gray-400">
                    Nenhuma categoria encontrada para esta finalidade.
                  </td>
                </tr>
              )}
              {!porCategoria && (
                <tr>
                  <td colSpan={5} className="px-4 py-10 text-center text-sm text-gray-400">Carregando...</td>
                </tr>
              )}
            </tbody>
          </table>
        </CardContent>
      </Card>

      {/* Pie chart */}
      {pieData.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle>Distribuição de Despesas por Categoria</CardTitle>
          </CardHeader>
          <CardContent>
            <ResponsiveContainer width="100%" height={300}>
              <PieChart>
                <Pie
                  data={pieData}
                  dataKey="value"
                  nameKey="name"
                  cx="50%"
                  cy="50%"
                  outerRadius={110}
                  label={({ name, percent }: { name?: string; percent?: number }) =>
                    `${name ?? ""} ${((percent ?? 0) * 100).toFixed(0)}%`
                  }
                >
                  {pieData.map((_, i) => (
                    <Cell key={i} fill={PIE_COLORS[i % PIE_COLORS.length]} />
                  ))}
                </Pie>
                <Tooltip formatter={(v) => (typeof v === "number" ? fmt(v) : v)} />
              </PieChart>
            </ResponsiveContainer>
          </CardContent>
        </Card>
      )}
    </div>
  );
}
