import { useEffect, useState } from "react";
import { listarCategorias } from "../api/categorias";
import { listarPessoas } from "../api/pessoas";
import { relatorioPorPessoa } from "../api/relatorios";
import { listarTransacoes } from "../api/transacoes";

export default function DashboardPage() {
  const [totalReceitas, setTotalReceitas] = useState(0);
  const [totalDespesas, setTotalDespesas] = useState(0);
  const [saldoLiquido, setSaldoLiquido] = useState(0);
  const [nPessoas, setNPessoas] = useState(0);
  const [nCategorias, setNCategorias] = useState(0);
  const [nTransacoes, setNTransacoes] = useState(0);
  const [loading, setLoading] = useState(true);
  const [erro, setErro] = useState("");

  useEffect(() => {
    Promise.all([
      relatorioPorPessoa(),
      listarPessoas(),
      listarCategorias(),
      listarTransacoes(),
    ])
      .then(([rel, pessoas, categorias, transacoes]) => {
        setTotalReceitas(rel.totalGeralReceitas);
        setTotalDespesas(rel.totalGeralDespesas);
        setSaldoLiquido(rel.saldoLiquido);
        setNPessoas(pessoas.length);
        setNCategorias(categorias.length);
        setNTransacoes(transacoes.length);
      })
      .catch((err: unknown) => {
        setErro(err instanceof Error ? err.message : "Erro ao carregar o dashboard.");
      })
      .finally(() => setLoading(false));
  }, []);

  const fmt = (v: number) =>
    v.toLocaleString("pt-BR", { style: "currency", currency: "BRL" });

  return (
    <div>
      <div className="page-header">
        <h2>Dashboard</h2>
      </div>

      {erro && <div className="error-box">{erro}</div>}
      {loading ? (
        <p style={{ color: "#aaa" }}>Carregando...</p>
      ) : !erro ? (
        <>
          <div className="dashboard-grid">
            <div className="stat-card stat-receita">
              <div className="stat-label">Total Receitas</div>
              <div className="stat-value">{fmt(totalReceitas)}</div>
            </div>
            <div className="stat-card stat-despesa">
              <div className="stat-label">Total Despesas</div>
              <div className="stat-value">{fmt(totalDespesas)}</div>
            </div>
            <div className={`stat-card ${saldoLiquido >= 0 ? "stat-saldo-pos" : "stat-saldo-neg"}`}>
              <div className="stat-label">Saldo Líquido</div>
              <div className="stat-value">{fmt(saldoLiquido)}</div>
            </div>
          </div>

          <div className="dashboard-grid">
            <div className="stat-card stat-neutral">
              <div className="stat-label">Pessoas cadastradas</div>
              <div className="stat-value stat-count">{nPessoas}</div>
            </div>
            <div className="stat-card stat-neutral">
              <div className="stat-label">Categorias cadastradas</div>
              <div className="stat-value stat-count">{nCategorias}</div>
            </div>
            <div className="stat-card stat-neutral">
              <div className="stat-label">Transações registradas</div>
              <div className="stat-value stat-count">{nTransacoes}</div>
            </div>
          </div>
        </>
      ) : null}
    </div>
  );
}
