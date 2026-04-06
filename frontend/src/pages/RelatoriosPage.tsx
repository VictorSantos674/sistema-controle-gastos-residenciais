import { useEffect, useState } from "react";
import { relatorioPorCategoria, relatorioPorPessoa } from "../api/relatorios";
import { RelatorioPorCategoria, RelatorioPorPessoa } from "../types";

export default function RelatoriosPage() {
  const [porPessoa, setPorPessoa] = useState<RelatorioPorPessoa | null>(null);
  const [porCategoria, setPorCategoria] = useState<RelatorioPorCategoria | null>(null);

  useEffect(() => {
    Promise.all([relatorioPorPessoa(), relatorioPorCategoria()]).then(([p, c]) => {
      setPorPessoa(p);
      setPorCategoria(c);
    });
  }, []);

  const fmt = (v: number) => `R$ ${v.toFixed(2)}`;

  return (
    <div>
      <h2>Relatórios</h2>

      <h3>Totais por Pessoa</h3>
      <table style={styles.table}>
        <thead>
          <tr>
            <th style={styles.th}>Pessoa</th>
            <th style={{ ...styles.th, color: "#27ae60" }}>Receitas</th>
            <th style={{ ...styles.th, color: "#e74c3c" }}>Despesas</th>
            <th style={styles.th}>Saldo</th>
          </tr>
        </thead>
        <tbody>
          {porPessoa?.pessoas.map((p) => (
            <tr key={p.pessoaId}>
              <td style={styles.td}>{p.nomePessoa}</td>
              <td style={{ ...styles.td, color: "#27ae60" }}>{fmt(p.totalReceitas)}</td>
              <td style={{ ...styles.td, color: "#c0392b" }}>{fmt(p.totalDespesas)}</td>
              <td style={{ ...styles.td, fontWeight: 600, color: p.saldo >= 0 ? "#27ae60" : "#c0392b" }}>
                {fmt(p.saldo)}
              </td>
            </tr>
          ))}
          {porPessoa && (
            <tr style={{ background: "#1a1a2e" }}>
              <td style={{ ...styles.td, color: "#fff", fontWeight: 700 }}>Total Geral</td>
              <td style={{ ...styles.td, color: "#2ecc71", fontWeight: 700 }}>
                {fmt(porPessoa.totalGeralReceitas)}
              </td>
              <td style={{ ...styles.td, color: "#e74c3c", fontWeight: 700 }}>
                {fmt(porPessoa.totalGeralDespesas)}
              </td>
              <td
                style={{
                  ...styles.td,
                  fontWeight: 700,
                  color: porPessoa.saldoLiquido >= 0 ? "#2ecc71" : "#e74c3c",
                }}
              >
                {fmt(porPessoa.saldoLiquido)}
              </td>
            </tr>
          )}
        </tbody>
      </table>

      <h3 style={{ marginTop: 40 }}>Totais por Categoria</h3>
      <table style={styles.table}>
        <thead>
          <tr>
            <th style={styles.th}>Categoria</th>
            <th style={styles.th}>Finalidade</th>
            <th style={{ ...styles.th, color: "#27ae60" }}>Receitas</th>
            <th style={{ ...styles.th, color: "#e74c3c" }}>Despesas</th>
            <th style={styles.th}>Saldo</th>
          </tr>
        </thead>
        <tbody>
          {porCategoria?.categorias.map((c) => (
            <tr key={c.categoriaId}>
              <td style={styles.td}>{c.descricaoCategoria}</td>
              <td style={styles.td}>{c.finalidade}</td>
              <td style={{ ...styles.td, color: "#27ae60" }}>{fmt(c.totalReceitas)}</td>
              <td style={{ ...styles.td, color: "#c0392b" }}>{fmt(c.totalDespesas)}</td>
              <td style={{ ...styles.td, fontWeight: 600, color: c.saldo >= 0 ? "#27ae60" : "#c0392b" }}>
                {fmt(c.saldo)}
              </td>
            </tr>
          ))}
          {porCategoria && (
            <tr style={{ background: "#1a1a2e" }}>
              <td style={{ ...styles.td, color: "#fff", fontWeight: 700 }} colSpan={2}>
                Total Geral
              </td>
              <td style={{ ...styles.td, color: "#2ecc71", fontWeight: 700 }}>
                {fmt(porCategoria.totalGeralReceitas)}
              </td>
              <td style={{ ...styles.td, color: "#e74c3c", fontWeight: 700 }}>
                {fmt(porCategoria.totalGeralDespesas)}
              </td>
              <td
                style={{
                  ...styles.td,
                  fontWeight: 700,
                  color: porCategoria.saldoLiquido >= 0 ? "#2ecc71" : "#e74c3c",
                }}
              >
                {fmt(porCategoria.saldoLiquido)}
              </td>
            </tr>
          )}
        </tbody>
      </table>
    </div>
  );
}

const styles: Record<string, React.CSSProperties> = {
  table: { width: "100%", borderCollapse: "collapse", marginBottom: 16 },
  th: { background: "#1a1a2e", color: "#fff", padding: "10px 14px", textAlign: "left" },
  td: { padding: "10px 14px", borderBottom: "1px solid #e0e0e0" },
};
