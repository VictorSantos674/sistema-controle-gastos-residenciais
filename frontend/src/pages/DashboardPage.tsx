import { useEffect, useState } from "react";
import { listarCategorias } from "../api/categorias";
import { listarPessoas } from "../api/pessoas";
import { relatorioPorPessoa } from "../api/relatorios";
import { listarTransacoes } from "../api/transacoes";

export default function DashboardPage() {
  const [totalReceitas, setTotalReceitas] = useState(0);
  const [totalDespesas, setTotalDespesas] = useState(0);
  const [saldoLiquido, setSaldoLiquido] = useState(0);

  // Totais do mês atual
  const [receitasMes, setReceitasMes] = useState(0);
  const [despesasMes, setDespesasMes] = useState(0);
  const [saldoMes, setSaldoMes] = useState(0);

  // Contadores gerais
  const [nPessoas, setNPessoas] = useState(0);
  const [nCategorias, setNCategorias] = useState(0);
  const [nTransacoes, setNTransacoes] = useState(0);

  const [loading, setLoading] = useState(true);
  const [erro, setErro] = useState("");

  const mesAtual = new Date().getMonth() + 1;
  const anoAtual = new Date().getFullYear();

  useEffect(() => {
    Promise.all([
      relatorioPorPessoa(),                               // Acumulado total
      relatorioPorPessoa({ mes: mesAtual, ano: anoAtual }), // Filtrado pelo mês corrente
      listarPessoas(),
      listarCategorias(),
      listarTransacoes(),
    ])
      .then(([relTotal, relMes, pessoas, categorias, transacoes]) => {
        setTotalReceitas(relTotal.totalGeralReceitas);
        setTotalDespesas(relTotal.totalGeralDespesas);
        setSaldoLiquido(relTotal.saldoLiquido);

        setReceitasMes(relMes.totalGeralReceitas);
        setDespesasMes(relMes.totalGeralDespesas);
        setSaldoMes(relMes.saldoLiquido);

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

  const nomeMesAtual = new Date().toLocaleString("pt-BR", { month: "long" });

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
          {/* Mês atual */}
          <h3 style={{ margin: "0 0 12px", fontSize: 15, color: "#555", textTransform: "capitalize" }}>
            {nomeMesAtual} de {anoAtual}
          </h3>
          <div className="dashboard-grid">
            <div className="stat-card stat-receita">
              <div className="stat-label">Receitas do mês</div>
              <div className="stat-value">{fmt(receitasMes)}</div>
            </div>
            <div className="stat-card stat-despesa">
              <div className="stat-label">Despesas do mês</div>
              <div className="stat-value">{fmt(despesasMes)}</div>
            </div>
            <div className={`stat-card ${saldoMes >= 0 ? "stat-saldo-pos" : "stat-saldo-neg"}`}>
              <div className="stat-label">Saldo do mês</div>
              <div className="stat-value">{fmt(saldoMes)}</div>
            </div>
          </div>

          {/* Acumulado total */}
          <h3 style={{ margin: "24px 0 12px", fontSize: 15, color: "#555" }}>
            Acumulado total
          </h3>
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

          {/* Contadores */}
          <div className="dashboard-grid" style={{ marginTop: 24 }}>
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
