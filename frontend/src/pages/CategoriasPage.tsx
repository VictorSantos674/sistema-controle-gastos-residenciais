import { useEffect, useState } from "react";
import { CategoriaInput, criarCategoria, listarCategorias } from "../api/categorias";
import { Categoria } from "../types";

const emptyForm: CategoriaInput = { descricao: "", finalidade: 1 };

export default function CategoriasPage() {
  const [categorias, setCategorias] = useState<Categoria[]>([]);
  const [form, setForm] = useState<CategoriaInput>(emptyForm);
  const [erro, setErro] = useState("");
  const [loading, setLoading] = useState(false);

  const carregar = async () => {
    setCategorias(await listarCategorias());
  };

  useEffect(() => {
    carregar();
  }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setErro("");
    if (!form.descricao.trim()) return setErro("A descrição é obrigatória.");
    setLoading(true);
    try {
      await criarCategoria(form);
      setForm(emptyForm);
      await carregar();
    } catch {
      setErro("Erro ao salvar categoria.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div>
      <h2>Categorias</h2>

      <form onSubmit={handleSubmit} style={styles.form}>
        <h3>Nova Categoria</h3>
        {erro && <p style={styles.erro}>{erro}</p>}
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
          <label style={styles.label}>Finalidade</label>
          <select
            style={styles.input}
            value={form.finalidade}
            onChange={(e) => setForm({ ...form, finalidade: Number(e.target.value) })}
          >
            <option value={1}>Despesa</option>
            <option value={2}>Receita</option>
            <option value={3}>Ambas</option>
          </select>
        </div>
        <button style={styles.btn} disabled={loading}>
          Cadastrar
        </button>
      </form>

      <table style={styles.table}>
        <thead>
          <tr>
            <th style={styles.th}>#</th>
            <th style={styles.th}>Descrição</th>
            <th style={styles.th}>Finalidade</th>
          </tr>
        </thead>
        <tbody>
          {categorias.map((c) => (
            <tr key={c.id}>
              <td style={styles.td}>{c.id}</td>
              <td style={styles.td}>{c.descricao}</td>
              <td style={styles.td}>
                <span style={{ ...styles.badge, ...badgeColor(c.finalidade) }}>
                  {c.finalidade}
                </span>
              </td>
            </tr>
          ))}
          {categorias.length === 0 && (
            <tr>
              <td colSpan={3} style={{ ...styles.td, textAlign: "center", color: "#888" }}>
                Nenhuma categoria cadastrada.
              </td>
            </tr>
          )}
        </tbody>
      </table>
    </div>
  );
}

function badgeColor(finalidade: string): React.CSSProperties {
  if (finalidade === "Receita") return { background: "#27ae60" };
  if (finalidade === "Despesa") return { background: "#c0392b" };
  return { background: "#2980b9" };
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
  label: { width: 80, fontWeight: 600 },
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
