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
import { FiltroRelatorio, relatorioPorCategoria, relatorioPorPessoa } from "../api/relatorios";
import { Finalidade, RelatorioPorCategoria, RelatorioPorPessoa } from "../types";

const PIE_COLORS = ["#e74c3c", "#e67e22", "#f39c12", "#9b59b6", "#2980b9", "#16a085", "#27ae60"];

const MESES = [
  "Janeiro", "Fevereiro", "Março", "Abril", "Maio", "Junho",
  "Julho", "Agosto", "Setembro", "Outubro", "Novembro", "Dezembro",
];

const gerarAnos = () => {
  const atual = new Date().getFullYear();
  return Array.from({ length: 5 }, (_, i) => atual - 4 + i);
};

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

  useEffect(() => {
    carregar();
  }, [filtroMes, filtroAno]);

  const fmt = (v: number) =>
    v.toLocaleString("pt-BR", { style: "currency", currency: "BRL" });
  const saldoColor = (v: number): React.CSSProperties => ({
    color: v >= 0 ? "#27ae60" : "#e74c3c",
    fontWeight: 600,
  });

  const badgeClass = (finalidade: string) => {
    if (finalidade === "Receita") return "badge badge-receita";
    if (finalidade === "Despesa") return "badge badge-despesa";
    return "badge badge-ambas";
  };

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
    <div>
      <div className="page-header">
        <h2>Relatórios</h2>
        <button className="btn btn-primary btn-sm" onClick={carregar} disabled={loading}>
          {loading ? "Atualizando..." : "Atualizar"}
        </button>
      </div>

      {/* Filtro de período */}
      <div className="card" style={{ paddingTop: 16, paddingBottom: 16 }}>
        <div style={{ display: "flex", alignItems: "center", gap: 16, flexWrap: "wrap" }}>
          <span style={{ fontSize: 14, fontWeight: 600, color: "#555" }}>Período:</span>

          <div style={{ display: "flex", alignItems: "center", gap: 6 }}>
            <label style={{ fontSize: 13, color: "#666" }}>Mês</label>
            <select
              className="form-select"
              style={{ maxWidth: 150 }}
              value={filtroMes ?? ""}
              onChange={(e) =>
                setFiltroMes(e.target.value === "" ? undefined : Number(e.target.value))
              }
            >
              <option value="">Todos</option>
              {MESES.map((m, i) => (
                <option key={i + 1} value={i + 1}>{m}</option>
              ))}
            </select>
          </div>

          <div style={{ display: "flex", alignItems: "center", gap: 6 }}>
            <label style={{ fontSize: 13, color: "#666" }}>Ano</label>
            <select
              className="form-select"
              style={{ maxWidth: 110 }}
              value={filtroAno ?? ""}
              onChange={(e) =>
                setFiltroAno(e.target.value === "" ? undefined : Number(e.target.value))
              }
            >
              <option value="">Todos</option>
              {gerarAnos().map((a) => (
                <option key={a} value={a}>{a}</option>
              ))}
            </select>
          </div>

          <span style={{ fontSize: 13, color: "#888", fontStyle: "italic" }}>
            {labelPeriodo}
          </span>
        </div>
      </div>

      {erro && <div className="error-box">{erro}</div>}

      <div className="card">
        <h3>Totais por Pessoa</h3>
        <table className="data-table">
          <thead>
            <tr>
              <th>Pessoa</th>
              <th style={{ color: "#2ecc71" }}>Receitas</th>
              <th style={{ color: "#e74c3c" }}>Despesas</th>
              <th>Saldo</th>
            </tr>
          </thead>
          <tbody>
            {porPessoa?.pessoas.map((p) => (
              <tr key={p.pessoaId}>
                <td>{p.nomePessoa}</td>
                <td style={{ color: "#27ae60" }}>{fmt(p.totalReceitas)}</td>
                <td style={{ color: "#e74c3c" }}>{fmt(p.totalDespesas)}</td>
                <td style={saldoColor(p.saldo)}>{fmt(p.saldo)}</td>
              </tr>
            ))}
            {porPessoa && (
              <tr className="total-row">
                <td style={{ color: "#fff" }}>Total Geral</td>
                <td style={{ color: "#2ecc71" }}>{fmt(porPessoa.totalGeralReceitas)}</td>
                <td style={{ color: "#e74c3c" }}>{fmt(porPessoa.totalGeralDespesas)}</td>
                <td style={saldoColor(porPessoa.saldoLiquido)}>
                  {fmt(porPessoa.saldoLiquido)}
                </td>
              </tr>
            )}
            {!porPessoa && (
              <tr>
                <td
                  colSpan={4}
                  style={{ textAlign: "center", color: "#aaa", padding: "32px", fontSize: 14 }}
                >
                  Carregando...
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>

      {barData.length > 0 && (
        <div className="card">
          <h3>Receitas vs Despesas por Pessoa</h3>
          <ResponsiveContainer width="100%" height={280}>
            <BarChart data={barData} margin={{ top: 4, right: 16, left: 8, bottom: 4 }}>
              <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
              <XAxis dataKey="name" tick={{ fontSize: 13 }} />
              <YAxis tickFormatter={(v: number) => `R$${v}`} tick={{ fontSize: 12 }} />
              <Tooltip formatter={(v) => (typeof v === "number" ? fmt(v) : v)} />
              <Legend />
              <Bar dataKey="Receitas" fill="#27ae60" radius={[4, 4, 0, 0]} />
              <Bar dataKey="Despesas" fill="#e74c3c" radius={[4, 4, 0, 0]} />
            </BarChart>
          </ResponsiveContainer>
        </div>
      )}

      <div className="card">
        <div style={{ display: "flex", alignItems: "center", justifyContent: "space-between", marginBottom: 16 }}>
          <h3 style={{ margin: 0 }}>Totais por Categoria</h3>
          <div style={{ display: "flex", alignItems: "center", gap: 8 }}>
            <label style={{ fontSize: 13, color: "#666" }}>Filtrar por finalidade:</label>
            <select
              className="form-select"
              style={{ maxWidth: 160 }}
              value={filtroFinalidade}
              onChange={(e) => setFiltroFinalidade(e.target.value as "" | Finalidade)}
            >
              <option value="">Todas</option>
              <option value="Despesa">Despesa</option>
              <option value="Receita">Receita</option>
              <option value="Ambas">Ambas</option>
            </select>
          </div>
        </div>
        <table className="data-table">
          <thead>
            <tr>
              <th>Categoria</th>
              <th>Finalidade</th>
              <th style={{ color: "#2ecc71" }}>Receitas</th>
              <th style={{ color: "#e74c3c" }}>Despesas</th>
              <th>Saldo</th>
            </tr>
          </thead>
          <tbody>
            {categoriasFiltradas.map((c) => (
              <tr key={c.categoriaId}>
                <td>{c.descricaoCategoria}</td>
                <td>
                  <span className={badgeClass(c.finalidade)}>{c.finalidade}</span>
                </td>
                <td style={{ color: c.finalidade === "Despesa" ? "#999" : "#27ae60" }}>
                  {c.finalidade === "Despesa" ? "—" : fmt(c.totalReceitas)}
                </td>
                <td style={{ color: c.finalidade === "Receita" ? "#999" : "#e74c3c" }}>
                  {c.finalidade === "Receita" ? "—" : fmt(c.totalDespesas)}
                </td>
                <td style={saldoColor(c.saldo)}>{fmt(c.saldo)}</td>
              </tr>
            ))}
            {porCategoria && categoriasFiltradas.length === 0 && (
              <tr>
                <td
                  colSpan={5}
                  style={{ textAlign: "center", color: "#aaa", padding: "24px", fontSize: 14 }}
                >
                  Nenhuma categoria encontrada para esta finalidade.
                </td>
              </tr>
            )}
            {!porCategoria && (
              <tr>
                <td
                  colSpan={5}
                  style={{ textAlign: "center", color: "#aaa", padding: "32px", fontSize: 14 }}
                >
                  Carregando...
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>

      {pieData.length > 0 && (
        <div className="card">
          <h3>Distribuição de Despesas por Categoria</h3>
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
                labelLine={true}
              >
                {pieData.map((_, i) => (
                  <Cell key={i} fill={PIE_COLORS[i % PIE_COLORS.length]} />
                ))}
              </Pie>
              <Tooltip formatter={(v) => (typeof v === "number" ? fmt(v) : v)} />
            </PieChart>
          </ResponsiveContainer>
        </div>
      )}
    </div>
  );
}
