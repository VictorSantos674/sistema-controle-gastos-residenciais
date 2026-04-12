import autoTable from "jspdf-autotable";
import jsPDF from "jspdf";
import { Download, RefreshCw } from "lucide-react";
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
    v >= 0 ? "font-semibold text-emerald-600 dark:text-emerald-400" : "font-semibold text-red-600 dark:text-red-400";

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

  const exportarPDF = () => {
    const doc = new jsPDF();
    const agora = new Date().toLocaleDateString("pt-BR");

    // Cabeçalho
    doc.setFontSize(18);
    doc.setFont("helvetica", "bold");
    doc.text("Relatório Financeiro", 14, 20);

    doc.setFontSize(11);
    doc.setFont("helvetica", "normal");
    doc.text(`Período: ${labelPeriodo}`, 14, 29);

    doc.setFontSize(9);
    doc.setTextColor(130);
    doc.text(`Gerado em: ${agora}`, 14, 36);
    doc.setTextColor(0);

    let y = 46;

    // Totais por Pessoa
    if (porPessoa && porPessoa.pessoas.length > 0) {
      doc.setFontSize(12);
      doc.setFont("helvetica", "bold");
      doc.text("Totais por Pessoa", 14, y);

      const pessoaBody = porPessoa.pessoas.map((p) => [
        p.nomePessoa,
        fmt(p.totalReceitas),
        fmt(p.totalDespesas),
        fmt(p.saldo),
      ]);
      pessoaBody.push([
        "Total Geral",
        fmt(porPessoa.totalGeralReceitas),
        fmt(porPessoa.totalGeralDespesas),
        fmt(porPessoa.saldoLiquido),
      ]);

      autoTable(doc, {
        startY: y + 4,
        head: [["Pessoa", "Receitas", "Despesas", "Saldo"]],
        body: pessoaBody,
        headStyles: { fillColor: [30, 64, 175], textColor: 255, fontStyle: "bold" },
        styles: { fontSize: 10 },
        alternateRowStyles: { fillColor: [245, 247, 250] },
        didParseCell: (data) => {
          if (data.section === "body" && data.row.index === pessoaBody.length - 1) {
            data.cell.styles.fontStyle = "bold";
            data.cell.styles.fillColor = [30, 41, 59];
            data.cell.styles.textColor = 255;
          }
        },
      });

      y = (doc as jsPDF & { lastAutoTable: { finalY: number } }).lastAutoTable.finalY + 14;
    }

    // Totais por Categoria
    if (porCategoria && porCategoria.categorias.length > 0) {
      if (y > 230) { doc.addPage(); y = 20; }

      doc.setFontSize(12);
      doc.setFont("helvetica", "bold");
      doc.text("Totais por Categoria", 14, y);

      const catBody = porCategoria.categorias.map((c) => [
        c.descricaoCategoria,
        c.finalidade,
        c.finalidade === "Despesa" ? "—" : fmt(c.totalReceitas),
        c.finalidade === "Receita" ? "—" : fmt(c.totalDespesas),
        fmt(c.saldo),
      ]);

      autoTable(doc, {
        startY: y + 4,
        head: [["Categoria", "Finalidade", "Receitas", "Despesas", "Saldo"]],
        body: catBody,
        headStyles: { fillColor: [30, 64, 175], textColor: 255, fontStyle: "bold" },
        styles: { fontSize: 10 },
        alternateRowStyles: { fillColor: [245, 247, 250] },
      });
    }

    const nomeArquivo = `relatorio-${labelPeriodo.toLowerCase().replace(/\s+/g, "-").replace(/[^a-z0-9-]/g, "")}.pdf`;
    doc.save(nomeArquivo);
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold text-gray-900 dark:text-gray-100">Relatórios</h1>
          <p className="text-sm text-gray-500 dark:text-gray-400">Análise financeira por pessoa e categoria</p>
        </div>
        <div className="flex gap-2">
          <Button
            size="sm"
            variant="outline"
            onClick={exportarPDF}
            disabled={loading || (!porPessoa && !porCategoria)}
          >
            <Download size={14} />
            Exportar PDF
          </Button>
          <Button size="sm" variant="outline" onClick={carregar} disabled={loading}>
            <RefreshCw size={14} className={loading ? "animate-spin" : ""} />
            Atualizar
          </Button>
        </div>
      </div>

      {/* Filtro de período */}
      <Card>
        <CardContent className="flex flex-wrap items-center gap-4 py-4">
          <span className="text-sm font-semibold text-gray-600 dark:text-gray-300">Período:</span>
          <div className="flex items-center gap-2">
            <label className="text-xs text-gray-500 dark:text-gray-400">Mês</label>
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
            <label className="text-xs text-gray-500 dark:text-gray-400">Ano</label>
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
          <span className="text-xs italic text-gray-400 dark:text-gray-500">{labelPeriodo}</span>
        </CardContent>
      </Card>

      {erro && (
        <div className="rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700 dark:border-red-800 dark:bg-red-900/30 dark:text-red-300">
          {erro}
        </div>
      )}

      {/* Totais por Pessoa */}
      <Card>
        <CardHeader className="pb-4">
          <CardTitle>Totais por Pessoa</CardTitle>
        </CardHeader>
        <CardContent className="p-0">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b bg-gray-50 dark:bg-gray-700/50">
                <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-gray-500 dark:text-gray-400">Pessoa</th>
                <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-emerald-600 dark:text-emerald-400">Receitas</th>
                <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-red-500 dark:text-red-400">Despesas</th>
                <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-gray-500 dark:text-gray-400">Saldo</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100 dark:divide-gray-700">
              {porPessoa?.pessoas.map((p) => (
                <tr key={p.pessoaId} className="hover:bg-gray-50 dark:hover:bg-gray-700/30">
                  <td className="px-4 py-3 font-medium text-gray-800 dark:text-gray-200">{p.nomePessoa}</td>
                  <td className="px-4 py-3 text-emerald-600 dark:text-emerald-400">{fmt(p.totalReceitas)}</td>
                  <td className="px-4 py-3 text-red-600 dark:text-red-400">{fmt(p.totalDespesas)}</td>
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
                  <td colSpan={4} className="px-4 py-10 text-center text-sm text-gray-400 dark:text-gray-500">Carregando...</td>
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
        <CardHeader className="pb-4">
          <div className="flex items-center justify-between gap-4">
            <CardTitle>Totais por Categoria</CardTitle>
            <div className="flex items-center gap-2">
              <label className="text-xs text-gray-500 dark:text-gray-400">Finalidade:</label>
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
              <tr className="border-b bg-gray-50 dark:bg-gray-700/50">
                <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-gray-500 dark:text-gray-400">Categoria</th>
                <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-gray-500 dark:text-gray-400">Finalidade</th>
                <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-emerald-600 dark:text-emerald-400">Receitas</th>
                <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-red-500 dark:text-red-400">Despesas</th>
                <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-gray-500 dark:text-gray-400">Saldo</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100 dark:divide-gray-700">
              {categoriasFiltradas.map((c) => (
                <tr key={c.categoriaId} className="hover:bg-gray-50 dark:hover:bg-gray-700/30">
                  <td className="px-4 py-3 font-medium text-gray-800 dark:text-gray-200">{c.descricaoCategoria}</td>
                  <td className="px-4 py-3">{finalidadeBadge(c.finalidade)}</td>
                  <td className={`px-4 py-3 ${c.finalidade === "Despesa" ? "text-gray-300 dark:text-gray-600" : "text-emerald-600 dark:text-emerald-400"}`}>
                    {c.finalidade === "Despesa" ? "—" : fmt(c.totalReceitas)}
                  </td>
                  <td className={`px-4 py-3 ${c.finalidade === "Receita" ? "text-gray-300 dark:text-gray-600" : "text-red-600 dark:text-red-400"}`}>
                    {c.finalidade === "Receita" ? "—" : fmt(c.totalDespesas)}
                  </td>
                  <td className={`px-4 py-3 ${saldoClass(c.saldo)}`}>{fmt(c.saldo)}</td>
                </tr>
              ))}
              {porCategoria && categoriasFiltradas.length === 0 && (
                <tr>
                  <td colSpan={5} className="px-4 py-10 text-center text-sm text-gray-400 dark:text-gray-500">
                    Nenhuma categoria encontrada para esta finalidade.
                  </td>
                </tr>
              )}
              {!porCategoria && (
                <tr>
                  <td colSpan={5} className="px-4 py-10 text-center text-sm text-gray-400 dark:text-gray-500">Carregando...</td>
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
