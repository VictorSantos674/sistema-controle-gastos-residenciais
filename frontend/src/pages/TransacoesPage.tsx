import { AlertTriangle, PlusCircle, Trash2 } from "lucide-react";
import { useEffect, useState } from "react";
import { listarCategorias } from "../api/categorias";
import { listarPessoas } from "../api/pessoas";
import { type TransacaoInput, criarTransacao, deletarTransacao, listarTransacoes } from "../api/transacoes";
import { Badge } from "../components/ui/badge";
import { Button } from "../components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "../components/ui/card";
import { Input } from "../components/ui/input";
import { Label } from "../components/ui/label";
import { Select } from "../components/ui/select";
import type { Categoria, Pessoa, Transacao } from "../types";

const hoje = () => new Date().toISOString().slice(0, 10);

const emptyForm: TransacaoInput = {
  descricao: "",
  valor: 0,
  valorReceita: 0,
  valorDespesa: 0,
  tipo: 1,
  categoriaId: 0,
  pessoaId: 0,
  data: hoje(),
};

function tipoBadge(tipo: string) {
  if (tipo === "Receita") return <Badge variant="receita">Receita</Badge>;
  if (tipo === "Ambas")   return <Badge variant="ambas">Ambas</Badge>;
  return <Badge variant="despesa">Despesa</Badge>;
}

export default function TransacoesPage() {
  const [transacoes, setTransacoes] = useState<Transacao[]>([]);
  const [pessoas, setPessoas] = useState<Pessoa[]>([]);
  const [categorias, setCategorias] = useState<Categoria[]>([]);
  const [form, setForm] = useState<TransacaoInput>(emptyForm);
  const [erro, setErro] = useState("");
  const [loading, setLoading] = useState(false);
  const [confirmDeleteId, setConfirmDeleteId] = useState<number | null>(null);

  const carregar = async () => {
    const [t, p, c] = await Promise.all([listarTransacoes(), listarPessoas(), listarCategorias()]);
    setTransacoes(t);
    setPessoas(p);
    setCategorias(c);
  };
  useEffect(() => { carregar(); }, []);

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

  const valorDisplay = (t: Transacao) => {
    const fmtBRL = (v: number) =>
      v.toLocaleString("pt-BR", { style: "currency", currency: "BRL" });
    if (t.tipo === "Ambas") {
      return (
        <span
          className="font-semibold text-violet-600"
          title={`Receita: ${fmtBRL(t.valorReceita ?? 0)} | Despesa: ${fmtBRL(t.valorDespesa ?? 0)}`}
        >
          {fmtBRL(t.valor)}
        </span>
      );
    }
    return (
      <span className={`font-semibold ${t.tipo === "Receita" ? "text-emerald-600" : "text-red-600"}`}>
        {fmtBRL(t.valor)}
      </span>
    );
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h1 className="text-2xl font-bold text-gray-900">Transações</h1>
        <p className="text-sm text-gray-500">Registre receitas e despesas</p>
      </div>

      {/* Form card */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <PlusCircle size={16} />
            Nova Transação
          </CardTitle>
        </CardHeader>
        <CardContent className="pt-4">
          {erro && (
            <div className="mb-4 flex items-center gap-2 rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
              <AlertTriangle size={15} className="shrink-0" />
              {erro}
            </div>
          )}
          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="grid gap-4 sm:grid-cols-2">
              {/* Pessoa */}
              <div className="space-y-1.5">
                <Label htmlFor="pessoa">Pessoa</Label>
                <Select
                  id="pessoa"
                  value={form.pessoaId}
                  onChange={(e) => handleChangePessoa(Number(e.target.value))}
                >
                  <option value={0}>Selecione uma pessoa...</option>
                  {pessoas.map((p) => (
                    <option key={p.id} value={p.id}>
                      {p.nome} — {p.idade} anos{p.idade < 18 ? " (menor)" : ""}
                    </option>
                  ))}
                </Select>
              </div>

              {/* Tipo */}
              <div className="space-y-1.5">
                <Label htmlFor="tipo">Tipo</Label>
                <Select
                  id="tipo"
                  value={form.tipo}
                  disabled={ehMenor}
                  onChange={(e) => handleChangeTipo(Number(e.target.value))}
                >
                  <option value={1}>Despesa</option>
                  {!ehMenor && <option value={2}>Receita</option>}
                  {!ehMenor && <option value={3}>Ambas</option>}
                </Select>
                {ehMenor && (
                  <p className="text-xs text-red-500">Menores de 18: apenas despesas permitidas.</p>
                )}
              </div>

              {/* Categoria */}
              <div className="space-y-1.5">
                <Label htmlFor="categoria">Categoria</Label>
                <Select
                  id="categoria"
                  value={form.categoriaId}
                  onChange={(e) => setForm({ ...form, categoriaId: Number(e.target.value) })}
                >
                  <option value={0}>Selecione uma categoria...</option>
                  {categoriasFiltradas.map((c) => (
                    <option key={c.id} value={c.id}>
                      {c.descricao}
                    </option>
                  ))}
                </Select>
              </div>

              {/* Data */}
              <div className="space-y-1.5">
                <Label htmlFor="data">Data</Label>
                <Input
                  id="data"
                  type="date"
                  value={form.data ?? hoje()}
                  onChange={(e) => setForm({ ...form, data: e.target.value })}
                />
              </div>
            </div>

            {/* Descrição */}
            <div className="space-y-1.5">
              <Label htmlFor="descricao">Descrição</Label>
              <Input
                id="descricao"
                maxLength={400}
                placeholder="Descreva a transação"
                value={form.descricao}
                onChange={(e) => setForm({ ...form, descricao: e.target.value })}
              />
            </div>

            {/* Valor(es) */}
            {ehAmbas ? (
              <div className="grid gap-4 sm:grid-cols-2">
                <div className="space-y-1.5">
                  <Label htmlFor="valorReceita">Valor Receita (R$)</Label>
                  <Input
                    id="valorReceita"
                    type="number"
                    min={0.01}
                    step={0.01}
                    placeholder="0,00"
                    value={form.valorReceita || ""}
                    onChange={(e) => setForm({ ...form, valorReceita: Number(e.target.value) })}
                  />
                </div>
                <div className="space-y-1.5">
                  <Label htmlFor="valorDespesa">Valor Despesa (R$)</Label>
                  <Input
                    id="valorDespesa"
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
              <div className="space-y-1.5">
                <Label htmlFor="valor">Valor (R$)</Label>
                <Input
                  id="valor"
                  className="max-w-[180px]"
                  type="number"
                  min={0.01}
                  step={0.01}
                  placeholder="0,00"
                  value={form.valor || ""}
                  onChange={(e) => setForm({ ...form, valor: Number(e.target.value) })}
                />
              </div>
            )}

            <Button type="submit" disabled={loading}>
              {loading ? "Registrando..." : "Registrar Transação"}
            </Button>
          </form>
        </CardContent>
      </Card>

      {/* Table card */}
      <Card>
        <CardContent className="p-0">
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b bg-gray-50">
                  {["#", "Data", "Pessoa", "Tipo", "Categoria", "Descrição", "Valor", "Ações"].map((h) => (
                    <th
                      key={h}
                      className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-gray-500"
                    >
                      {h}
                    </th>
                  ))}
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100">
                {transacoes.map((t) => (
                  <tr key={t.id} className="hover:bg-gray-50">
                    <td className="px-4 py-3 text-gray-400">{t.id}</td>
                    <td className="whitespace-nowrap px-4 py-3 text-gray-600">
                      {new Date(t.data + "T00:00:00").toLocaleDateString("pt-BR")}
                    </td>
                    <td className="px-4 py-3 font-medium text-gray-800">{t.pessoaNome}</td>
                    <td className="px-4 py-3">{tipoBadge(t.tipo)}</td>
                    <td className="px-4 py-3 text-gray-600">{t.categoriaDescricao}</td>
                    <td className="px-4 py-3 text-gray-700">{t.descricao}</td>
                    <td className="px-4 py-3">{valorDisplay(t)}</td>
                    <td className="px-4 py-3">
                      {confirmDeleteId === t.id ? (
                        <div className="flex flex-wrap items-center gap-2">
                          <span className="text-xs font-semibold text-red-600">Deletar?</span>
                          <Button size="sm" variant="destructive" onClick={() => handleDeletar(t.id)}>
                            Confirmar
                          </Button>
                          <Button size="sm" variant="outline" onClick={() => setConfirmDeleteId(null)}>
                            Cancelar
                          </Button>
                        </div>
                      ) : (
                        <Button size="sm" variant="destructive" onClick={() => setConfirmDeleteId(t.id)}>
                          <Trash2 size={13} /> Deletar
                        </Button>
                      )}
                    </td>
                  </tr>
                ))}
                {transacoes.length === 0 && (
                  <tr>
                    <td colSpan={8} className="px-4 py-10 text-center text-sm text-gray-400">
                      Nenhuma transação registrada.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
