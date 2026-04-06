import { useEffect, useState } from "react";
import { listarCategorias } from "../api/categorias";
import { listarPessoas } from "../api/pessoas";
import { TransacaoInput, criarTransacao, listarTransacoes } from "../api/transacoes";
import { Categoria, Pessoa, Transacao } from "../types";

const emptyForm: TransacaoInput = {
  descricao: "",
  valor: 0,
  tipo: 1,
  categoriaId: 0,
  pessoaId: 0,
};

export default function TransacoesPage() {
  const [transacoes, setTransacoes] = useState<Transacao[]>([]);
  const [pessoas, setPessoas] = useState<Pessoa[]>([]);
  const [categorias, setCategorias] = useState<Categoria[]>([]);
  const [form, setForm] = useState<TransacaoInput>(emptyForm);
  const [erro, setErro] = useState("");
  const [loading, setLoading] = useState(false);

  const carregar = async () => {
    const [t, p, c] = await Promise.all([
      listarTransacoes(),
      listarPessoas(),
      listarCategorias(),
    ]);
    setTransacoes(t);
    setPessoas(p);
    setCategorias(c);
  };

  useEffect(() => {
    carregar();
  }, []);

  const pessoaSelecionada = pessoas.find((p) => p.id === form.pessoaId);

  const categoriasFiltradas = categorias.filter((c) => {
    if (form.tipo === 1) return c.finalidade === "Despesa" || c.finalidade === "Ambas";
    return c.finalidade === "Receita" || c.finalidade === "Ambas";
  });

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setErro("");
    if (!form.descricao.trim()) return setErro("A descrição é obrigatória.");
    if (form.valor <= 0) return setErro("O valor deve ser positivo.");
    if (!form.pessoaId) return setErro("Selecione uma pessoa.");
    if (!form.categoriaId) return setErro("Selecione uma categoria.");

    if (pessoaSelecionada && pessoaSelecionada.idade < 18 && form.tipo === 2) {
      return setErro("Menores de 18 anos só podem registrar transações do tipo Despesa.");
    }

    setLoading(true);
    try {
      await criarTransacao(form);
      setForm(emptyForm);
      await carregar();
    } catch (err: unknown) {
      const msg =
        typeof err === "object" &&
        err !== null &&
        "response" in err &&
        typeof (err as { response?: { data?: { mensagem?: string } } }).response?.data
          ?.mensagem === "string"
          ? (err as { response?: { data?: { mensagem?: string } } }).response?.data?.mensagem
          : undefined;

      setErro(msg ?? "Erro ao salvar transação.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div>
      <h2>Transações</h2>

      <form onSubmit={handleSubmit} style={styles.form}>
        <h3>Nova Transação</h3>
        {erro && <p style={styles.erro}>{erro}</p>}

        <div style={styles.row}>
          <label style={styles.label}>Pessoa</label>
          <select
            style={styles.input}
            value={form.pessoaId}
            onChange={(e) => setForm({ ...form, pessoaId: Number(e.target.value) })}
          >
            <option value={0}>Selecione...</option>
            {pessoas.map((p) => (
              <option key={p.id} value={p.id}>
                {p.nome} ({p.idade} anos)
              </option>
            ))}
          </select>
        </div>

        <div style={styles.row}>
          <label style={styles.label}>Tipo</label>
          <select
            style={styles.input}
            value={form.tipo}
            onChange={(e) =>
              setForm({ ...form, tipo: Number(e.target.value), categoriaId: 0 })
            }
            disabled={!!(pessoaSelecionada && pessoaSelecionada.idade < 18)}
          >
            <option value={1}>Despesa</option>
            {(!pessoaSelecionada || pessoaSelecionada.idade >= 18) && (
              <option value={2}>Receita</option>
            )}
          </select>
          {pessoaSelecionada && pessoaSelecionada.idade < 18 && (
            <span style={{ color: "#c0392b", fontSize: 13 }}>
              Menor de 18: somente Despesa
            </span>
          )}
        </div>

        <div style={styles.row}>
          <label style={styles.label}>Categoria</label>
          <select
            style={styles.input}
            value={form.categoriaId}
            onChange={(e) => setForm({ ...form, categoriaId: Number(e.target.value) })}
          >
            <option value={0}>Selecione...</option>
            {categoriasFiltradas.map((c) => (
              <option key={c.id} value={c.id}>
                {c.descricao} ({c.finalidade})
              </option>
            ))}
          </select>
        </div>

        <div style={styles.row}>
          <label style={styles.label}>Descrição</label>
          <input
            style={styles.input}
            maxLength={400}
            value={form.descricao}
            onChange={(e) => setForm({ ...form, descricao: e.target.value })}
          />
        </div>

        <div style={styles.row}>
          <label style={styles.label}>Valor (R$)</label>
          <input
            style={{ ...styles.input, width: 140, flex: "none" }}
            type="number"
            min={0.01}
            step={0.01}
            value={form.valor}
            onChange={(e) => setForm({ ...form, valor: Number(e.target.value) })}
          />
        </div>

        <button style={styles.btn} disabled={loading}>
          Registrar
        </button>
      </form>

      <table style={styles.table}>
        <thead>
          <tr>
            <th style={styles.th}>#</th>
            <th style={styles.th}>Pessoa</th>
            <th style={styles.th}>Tipo</th>
            <th style={styles.th}>Categoria</th>
            <th style={styles.th}>Descrição</th>
            <th style={styles.th}>Valor</th>
          </tr>
        </thead>
        <tbody>
          {transacoes.map((t) => (
            <tr key={t.id}>
              <td style={styles.td}>{t.id}</td>
              <td style={styles.td}>{t.pessoaNome}</td>
              <td style={styles.td}>
                <span
                  style={{
                    ...styles.badge,
                    background: t.tipo === "Receita" ? "#27ae60" : "#c0392b",
                  }}
                >
                  {t.tipo}
                </span>
              </td>
              <td style={styles.td}>{t.categoriaDescricao}</td>
              <td style={styles.td}>{t.descricao}</td>
              <td style={styles.td}>R$ {t.valor.toFixed(2)}</td>
            </tr>
          ))}
          {transacoes.length === 0 && (
            <tr>
              <td colSpan={6} style={{ ...styles.td, textAlign: "center", color: "#888" }}>
                Nenhuma transação registrada.
              </td>
            </tr>
          )}
        </tbody>
      </table>
    </div>
  );
}

const styles: Record<string, React.CSSProperties> = {
  form: {
    background: "#f5f5f5",
    padding: 20,
    borderRadius: 8,
    marginBottom: 24,
    maxWidth: 560,
  },
  row: { display: "flex", alignItems: "center", marginBottom: 12, gap: 12 },
  label: { width: 90, fontWeight: 600, flexShrink: 0 },
  input: { padding: "6px 10px", border: "1px solid #ccc", borderRadius: 4, flex: 1 },
  btn: {
    padding: "8px 20px",
    background: "#1a1a2e",
    color: "#fff",
    border: "none",
    borderRadius: 4,
    cursor: "pointer",
  },
  table: { width: "100%", borderCollapse: "collapse" },
  th: { background: "#1a1a2e", color: "#fff", padding: "10px 14px", textAlign: "left" },
  td: { padding: "10px 14px", borderBottom: "1px solid #e0e0e0" },
  erro: { color: "#c0392b", marginBottom: 8 },
  badge: {
    color: "#fff",
    padding: "2px 10px",
    borderRadius: 12,
    fontSize: 13,
    fontWeight: 600,
  },
};
