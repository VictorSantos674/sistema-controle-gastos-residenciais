import { useEffect, useState } from "react";
import { CategoriaInput, criarCategoria, listarCategorias } from "../api/categorias";
import { Categoria } from "../types";

const emptyForm: CategoriaInput = { descricao: "", finalidade: 1 };

export default function CategoriasPage() {
  const [categorias, setCategorias] = useState<Categoria[]>([]);
  const [form, setForm] = useState<CategoriaInput>(emptyForm);
  const [erro, setErro] = useState("");
  const [loading, setLoading] = useState(false);

  const carregar = async () => setCategorias(await listarCategorias());

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
    } catch (err: unknown) {
      setErro(err instanceof Error ? err.message : "Erro ao salvar categoria.");
    } finally {
      setLoading(false);
    }
  };

  const badgeClass = (finalidade: string) => {
    if (finalidade === "Receita") return "badge badge-receita";
    if (finalidade === "Despesa") return "badge badge-despesa";
    return "badge badge-ambas";
  };

  return (
    <div>
      <div className="page-header">
        <h2>Categorias</h2>
      </div>

      <div className="card">
        <h3>Nova Categoria</h3>
        {erro && <div className="error-box">{erro}</div>}
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label className="form-label">Descrição</label>
            <input
              className="form-input"
              maxLength={400}
              placeholder="Ex: Alimentação, Salário..."
              value={form.descricao}
              onChange={(e) => setForm({ ...form, descricao: e.target.value })}
            />
          </div>
          <div className="form-group">
            <label className="form-label">Finalidade</label>
            <select
              className="form-select"
              style={{ maxWidth: 200 }}
              value={form.finalidade}
              onChange={(e) => setForm({ ...form, finalidade: Number(e.target.value) })}
            >
              <option value={1}>Despesa</option>
              <option value={2}>Receita</option>
              <option value={3}>Ambas</option>
            </select>
          </div>
          <button type="submit" className="btn btn-primary" disabled={loading}>
            {loading ? "Salvando..." : "Cadastrar"}
          </button>
        </form>
      </div>

      <div className="card">
        <table className="data-table">
          <thead>
            <tr>
              <th>#</th>
              <th>Descrição</th>
              <th>Finalidade</th>
            </tr>
          </thead>
          <tbody>
            {categorias.map((c) => (
              <tr key={c.id}>
                <td>{c.id}</td>
                <td>{c.descricao}</td>
                <td>
                  <span className={badgeClass(c.finalidade)}>{c.finalidade}</span>
                </td>
              </tr>
            ))}
            {categorias.length === 0 && (
              <tr>
                <td
                  colSpan={3}
                  style={{ textAlign: "center", color: "#aaa", padding: "32px", fontSize: 14 }}
                >
                  Nenhuma categoria cadastrada.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
