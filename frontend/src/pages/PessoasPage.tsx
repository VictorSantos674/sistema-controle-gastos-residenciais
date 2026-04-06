import { useEffect, useState } from "react";
import {
  PessoaInput,
  criarPessoa,
  deletarPessoa,
  editarPessoa,
  listarPessoas,
} from "../api/pessoas";
import { Pessoa } from "../types";

const emptyForm: PessoaInput = { nome: "", idade: 0 };

export default function PessoasPage() {
  const [pessoas, setPessoas] = useState<Pessoa[]>([]);
  const [form, setForm] = useState<PessoaInput>(emptyForm);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [erro, setErro] = useState("");
  const [loading, setLoading] = useState(false);

  const carregar = async () => {
    setPessoas(await listarPessoas());
  };

  useEffect(() => {
    carregar();
  }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setErro("");
    if (!form.nome.trim()) return setErro("O nome é obrigatório.");
    if (form.idade < 0) return setErro("A idade deve ser um número positivo.");
    setLoading(true);
    try {
      if (editingId !== null) {
        await editarPessoa(editingId, form);
      } else {
        await criarPessoa(form);
      }
      setForm(emptyForm);
      setEditingId(null);
      await carregar();
    } catch {
      setErro("Erro ao salvar pessoa.");
    } finally {
      setLoading(false);
    }
  };

  const handleEditar = (p: Pessoa) => {
    setEditingId(p.id);
    setForm({ nome: p.nome, idade: p.idade });
    setErro("");
  };

  const handleDeletar = async (id: number) => {
    if (!confirm("Deletar esta pessoa e todas as suas transações?")) return;
    await deletarPessoa(id);
    await carregar();
  };

  return (
    <div>
      <h2>Pessoas</h2>

      <form onSubmit={handleSubmit} style={styles.form}>
        <h3>{editingId !== null ? "Editar Pessoa" : "Nova Pessoa"}</h3>
        {erro && <p style={styles.erro}>{erro}</p>}
        <div style={styles.row}>
          <label style={styles.label}>Nome</label>
          <input
            style={styles.input}
            maxLength={200}
            value={form.nome}
            onChange={(e) => setForm({ ...form, nome: e.target.value })}
          />
        </div>
        <div style={styles.row}>
          <label style={styles.label}>Idade</label>
          <input
            style={{ ...styles.input, width: 100 }}
            type="number"
            min={0}
            value={form.idade}
            onChange={(e) => setForm({ ...form, idade: Number(e.target.value) })}
          />
        </div>
        <div style={{ display: "flex", gap: 8 }}>
          <button style={styles.btn} disabled={loading}>
            {editingId !== null ? "Salvar alterações" : "Cadastrar"}
          </button>
          {editingId !== null && (
            <button
              type="button"
              style={{ ...styles.btn, background: "#555" }}
              onClick={() => {
                setEditingId(null);
                setForm(emptyForm);
                setErro("");
              }}
            >
              Cancelar
            </button>
          )}
        </div>
      </form>

      <table style={styles.table}>
        <thead>
          <tr>
            <th style={styles.th}>#</th>
            <th style={styles.th}>Nome</th>
            <th style={styles.th}>Idade</th>
            <th style={styles.th}>Ações</th>
          </tr>
        </thead>
        <tbody>
          {pessoas.map((p) => (
            <tr key={p.id}>
              <td style={styles.td}>{p.id}</td>
              <td style={styles.td}>{p.nome}</td>
              <td style={styles.td}>{p.idade}</td>
              <td style={styles.td}>
                <button style={styles.btnSm} onClick={() => handleEditar(p)}>
                  Editar
                </button>
                <button
                  style={{ ...styles.btnSm, background: "#c0392b", marginLeft: 6 }}
                  onClick={() => handleDeletar(p.id)}
                >
                  Deletar
                </button>
              </td>
            </tr>
          ))}
          {pessoas.length === 0 && (
            <tr>
              <td colSpan={4} style={{ ...styles.td, textAlign: "center", color: "#888" }}>
                Nenhuma pessoa cadastrada.
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
    maxWidth: 500,
  },
  row: { display: "flex", alignItems: "center", marginBottom: 12, gap: 12 },
  label: { width: 60, fontWeight: 600 },
  input: { padding: "6px 10px", border: "1px solid #ccc", borderRadius: 4, flex: 1 },
  btn: {
    padding: "8px 20px",
    background: "#1a1a2e",
    color: "#fff",
    border: "none",
    borderRadius: 4,
    cursor: "pointer",
  },
  btnSm: {
    padding: "4px 12px",
    background: "#1a1a2e",
    color: "#fff",
    border: "none",
    borderRadius: 4,
    cursor: "pointer",
    fontSize: 13,
  },
  table: { width: "100%", borderCollapse: "collapse" },
  th: {
    background: "#1a1a2e",
    color: "#fff",
    padding: "10px 14px",
    textAlign: "left",
  },
  td: { padding: "10px 14px", borderBottom: "1px solid #e0e0e0" },
  erro: { color: "#c0392b", marginBottom: 8 },
};
