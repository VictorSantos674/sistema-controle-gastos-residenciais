import { useEffect, useState } from "react";
import { listarCategorias } from "../api/categorias";
import { listarPessoas } from "../api/pessoas";
import { TransacaoInput, criarTransacao, deletarTransacao, listarTransacoes } from "../api/transacoes";
import { Categoria, Pessoa, Transacao } from "../types";

const emptyForm: TransacaoInput = {
  descricao: "",
  valor: 0,
  valorReceita: 0,
  valorDespesa: 0,
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
  const [confirmDeleteId, setConfirmDeleteId] = useState<number | null>(null);

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
  const ehMenor = !!(pessoaSelecionada && pessoaSelecionada.idade < 18);
  const ehAmbas = form.tipo === 3;

  const categoriasFiltradas = categorias.filter((c) => {
    if (form.tipo === 1) return c.finalidade === "Despesa";
    if (form.tipo === 2) return c.finalidade === "Receita";
    return c.finalidade === "Ambas";
  });

  const handleChangePessoa = (id: number) => {
    const pessoa = pessoas.find((p) => p.id === id);
    const novoTipo = pessoa && pessoa.idade < 18 ? 1 : form.tipo;
    setForm({ ...form, pessoaId: id, tipo: novoTipo, categoriaId: 0 });
  };

  const handleChangeTipo = (tipo: number) => {
    setForm({ ...form, tipo, categoriaId: 0 });
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setErro("");
    if (!form.pessoaId) return setErro("Selecione uma pessoa.");
    if (!form.categoriaId) return setErro("Selecione uma categoria.");
    if (!form.descricao.trim()) return setErro("A descrição é obrigatória.");

    if (ehAmbas) {
      if (!form.valorReceita || form.valorReceita <= 0)
        return setErro("O valor de receita deve ser maior que zero.");
    } else {
      if (!form.valor || form.valor <= 0)
        return setErro("O valor deve ser maior que zero.");
      if (ehMenor && form.tipo === 2)
        return setErro("Menores de 18 anos só podem registrar transações do tipo Despesa.");
    }

    setLoading(true);
    try {
      await criarTransacao(form);
      setForm(emptyForm);
      await carregar();
    } catch (err: unknown) {
      setErro(err instanceof Error ? err.message : "Erro ao registrar transação.");
    } finally {
      setLoading(false);
    }
  };

  const handleDeletar = async (id: number) => {
    setErro("");
    try {
      await deletarTransacao(id);
      setConfirmDeleteId(null);
      await carregar();
    } catch (err: unknown) {
      setConfirmDeleteId(null);
      setErro(err instanceof Error ? err.message : "Erro ao deletar transação.");
    }
  };

  const tipoBadgeClass = (tipo: string) => {
    if (tipo === "Receita") return "badge badge-receita";
    if (tipo === "Ambas") return "badge badge-ambas";
    return "badge badge-despesa";
  };

  const valorStyle = (t: Transacao): React.CSSProperties => {
    if (t.tipo === "Ambas") return { fontWeight: 600, color: "#2980b9" };
    return { fontWeight: 600, color: t.tipo === "Receita" ? "#27ae60" : "#e74c3c" };
  };

  return (
    <div>
      <div className="page-header">
        <h2>Transações</h2>
      </div>

      <div className="card">
        <h3>Nova Transação</h3>
        {erro && <div className="error-box">{erro}</div>}
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label className="form-label">Pessoa</label>
            <select
              className="form-select"
              value={form.pessoaId}
              onChange={(e) => handleChangePessoa(Number(e.target.value))}
            >
              <option value={0}>Selecione uma pessoa...</option>
              {pessoas.map((p) => (
                <option key={p.id} value={p.id}>
                  {p.nome} — {p.idade} anos{p.idade < 18 ? " (menor de 18)" : ""}
                </option>
              ))}
            </select>
          </div>

          <div className="form-group">
            <label className="form-label">Tipo</label>
            <select
              className="form-select"
              style={{ maxWidth: 200 }}
              value={form.tipo}
              disabled={ehMenor}
              onChange={(e) => handleChangeTipo(Number(e.target.value))}
            >
              <option value={1}>Despesa</option>
              {!ehMenor && <option value={2}>Receita</option>}
              {!ehMenor && <option value={3}>Ambas</option>}
            </select>
            {ehMenor && (
              <p style={{ margin: "6px 0 0", fontSize: 13, color: "#e74c3c" }}>
                Menor de 18 anos: apenas despesas são permitidas.
              </p>
            )}
          </div>

          <div className="form-group">
            <label className="form-label">Categoria</label>
            <select
              className="form-select"
              value={form.categoriaId}
              onChange={(e) => setForm({ ...form, categoriaId: Number(e.target.value) })}
            >
              <option value={0}>Selecione uma categoria...</option>
              {categoriasFiltradas.map((c) => (
                <option key={c.id} value={c.id}>
                  {c.descricao} ({c.finalidade})
                </option>
              ))}
            </select>
          </div>

          <div className="form-group">
            <label className="form-label">Descrição</label>
            <input
              className="form-input"
              maxLength={400}
              placeholder="Descreva a transação"
              value={form.descricao}
              onChange={(e) => setForm({ ...form, descricao: e.target.value })}
            />
          </div>

          {ehAmbas ? (
            <div style={{ display: "flex", gap: 16 }}>
              <div className="form-group">
                <label className="form-label">Valor Receita (R$)</label>
                <input
                  className="form-input"
                  style={{ maxWidth: 160 }}
                  type="number"
                  min={0.01}
                  step={0.01}
                  placeholder="0,00"
                  value={form.valorReceita || ""}
                  onChange={(e) => setForm({ ...form, valorReceita: Number(e.target.value) })}
                />
              </div>
              <div className="form-group">
                <label className="form-label">Valor Despesa (R$)</label>
                <input
                  className="form-input"
                  style={{ maxWidth: 160 }}
                  type="number"
                  min={0.01}
                  step={0.01}
                  placeholder="0,00"
                  value={form.valorDespesa || ""}
                  onChange={(e) => setForm({ ...form, valorDespesa: Number(e.target.value) })}
                />
              </div>
            </div>
          ) : (
            <div className="form-group">
              <label className="form-label">Valor (R$)</label>
              <input
                className="form-input"
                style={{ maxWidth: 160 }}
                type="number"
                min={0.01}
                step={0.01}
                placeholder="0,00"
                value={form.valor || ""}
                onChange={(e) => setForm({ ...form, valor: Number(e.target.value) })}
              />
            </div>
          )}

          <button type="submit" className="btn btn-primary" disabled={loading}>
            {loading ? "Registrando..." : "Registrar Transação"}
          </button>
        </form>
      </div>

      <div className="card">
        <table className="data-table">
          <thead>
            <tr>
              <th>#</th>
              <th>Pessoa</th>
              <th>Tipo</th>
              <th>Categoria</th>
              <th>Descrição</th>
              <th>Valor</th>
              <th>Ações</th>
            </tr>
          </thead>
          <tbody>
            {transacoes.map((t) => (
              <tr key={t.id}>
                <td>{t.id}</td>
                <td>{t.pessoaNome}</td>
                <td>
                  <span className={tipoBadgeClass(t.tipo)}>{t.tipo}</span>
                </td>
                <td>{t.categoriaDescricao}</td>
                <td>{t.descricao}</td>
                <td style={valorStyle(t)}>
                  {t.tipo === "Ambas" ? (
                    <span
                      title={`Receita: ${(t.valorReceita ?? 0).toLocaleString("pt-BR", { style: "currency", currency: "BRL" })} | Despesa: ${(t.valorDespesa ?? 0).toLocaleString("pt-BR", { style: "currency", currency: "BRL" })}`}
                    >
                      {t.valor.toLocaleString("pt-BR", { style: "currency", currency: "BRL" })}
                    </span>
                  ) : (
                    t.valor.toLocaleString("pt-BR", { style: "currency", currency: "BRL" })
                  )}
                </td>
                <td>
                  {confirmDeleteId === t.id ? (
                    <div className="confirm-inline">
                      <span>Deletar?</span>
                      <button
                        type="button"
                        className="btn btn-danger btn-sm"
                        onClick={() => handleDeletar(t.id)}
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
                    <button
                      type="button"
                      className="btn btn-danger btn-sm"
                      onClick={() => setConfirmDeleteId(t.id)}
                    >
                      Deletar
                    </button>
                  )}
                </td>
              </tr>
            ))}
            {transacoes.length === 0 && (
              <tr>
                <td
                  colSpan={7}
                  style={{ textAlign: "center", color: "#aaa", padding: "32px", fontSize: 14 }}
                >
                  Nenhuma transação registrada.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
