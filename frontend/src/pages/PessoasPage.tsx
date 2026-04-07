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
  const [confirmDeleteId, setConfirmDeleteId] = useState<number | null>(null);
  const [erro, setErro] = useState("");
  const [loading, setLoading] = useState(false);

  const carregar = async () => setPessoas(await listarPessoas());

  useEffect(() => {
    carregar();
  }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setErro("");
    if (!form.nome.trim()) return setErro("O nome é obrigatório.");
    if (form.idade < 0 || form.idade > 150) return setErro("Idade inválida.");
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
    } catch (err: unknown) {
      setErro(err instanceof Error ? err.message : "Erro ao salvar pessoa.");
    } finally {
      setLoading(false);
    }
  };

  const handleEditar = (p: Pessoa) => {
    setEditingId(p.id);
    setForm({ nome: p.nome, idade: p.idade });
    setConfirmDeleteId(null);
    setErro("");
    window.scrollTo({ top: 0, behavior: "smooth" });
  };

  const handleDeletar = async (id: number) => {
    setErro("");
    try {
      await deletarPessoa(id);
      setConfirmDeleteId(null);
      if (editingId === id) {
        setEditingId(null);
        setForm(emptyForm);
      }
      await carregar();
    } catch (err: unknown) {
      setConfirmDeleteId(null);
      setErro(err instanceof Error ? err.message : "Erro ao deletar pessoa.");
    }
  };

  const cancelarEdicao = () => {
    setEditingId(null);
    setForm(emptyForm);
    setErro("");
  };

  return (
    <div>
      <div className="page-header">
        <h2>Pessoas</h2>
      </div>

      <div className="card">
        <h3>{editingId !== null ? "Editar Pessoa" : "Nova Pessoa"}</h3>
        {erro && <div className="error-box">{erro}</div>}
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label className="form-label">Nome</label>
            <input
              className="form-input"
              maxLength={200}
              placeholder="Nome completo"
              value={form.nome}
              onChange={(e) => setForm({ ...form, nome: e.target.value })}
            />
          </div>
          <div className="form-group">
            <label className="form-label">Idade</label>
            <input
              className="form-input"
              style={{ maxWidth: 120 }}
              type="number"
              min={0}
              max={150}
              placeholder="0"
              value={form.idade || ""}
              onChange={(e) => setForm({ ...form, idade: Number(e.target.value) })}
            />
          </div>
          <div className="btn-row">
            <button type="submit" className="btn btn-primary" disabled={loading}>
              {loading ? "Salvando..." : editingId !== null ? "Salvar alterações" : "Cadastrar"}
            </button>
            {editingId !== null && (
              <button type="button" className="btn btn-secondary" onClick={cancelarEdicao}>
                Cancelar
              </button>
            )}
          </div>
        </form>
      </div>

      <div className="card">
        <table className="data-table">
          <thead>
            <tr>
              <th>#</th>
              <th>Nome</th>
              <th>Idade</th>
              <th>Ações</th>
            </tr>
          </thead>
          <tbody>
            {pessoas.map((p) => (
              <tr key={p.id}>
                <td>{p.id}</td>
                <td>{p.nome}</td>
                <td>{p.idade} anos</td>
                <td>
                  {confirmDeleteId === p.id ? (
                    <div className="confirm-inline">
                      <span>Deletar e todas as transações?</span>
                      <button
                        type="button"
                        className="btn btn-danger btn-sm"
                        onClick={() => handleDeletar(p.id)}
                      >
                        Confirmar
                      </button>
                      <button
                        type="button"
                        className="btn btn-secondary btn-sm"
                        onClick={() => setConfirmDeleteId(null)}
                      >
                        Cancelar
                      </button>
                    </div>
                  ) : (
                    <div className="btn-row">
                      <button
                        type="button"
                        className="btn btn-primary btn-sm"
                        onClick={() => handleEditar(p)}
                      >
                        Editar
                      </button>
                      <button
                        type="button"
                        className="btn btn-danger btn-sm"
                        onClick={() => setConfirmDeleteId(p.id)}
                      >
                        Deletar
                      </button>
                    </div>
                  )}
                </td>
              </tr>
            ))}
            {pessoas.length === 0 && (
              <tr>
                <td
                  colSpan={4}
                  style={{ textAlign: "center", color: "#aaa", padding: "32px", fontSize: 14 }}
                >
                  Nenhuma pessoa cadastrada.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
