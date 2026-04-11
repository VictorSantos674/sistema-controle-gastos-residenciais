import { AlertTriangle, Pencil, Trash2, UserPlus } from "lucide-react";
import { useEffect, useState } from "react";
import {
  type PessoaInput,
  criarPessoa,
  deletarPessoa,
  editarPessoa,
  listarPessoas,
} from "../api/pessoas";
import { Badge } from "../components/ui/badge";
import { Button } from "../components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "../components/ui/card";
import { Input } from "../components/ui/input";
import { Label } from "../components/ui/label";
import type { Pessoa } from "../types";

const emptyForm: PessoaInput = { nome: "", idade: 0 };

export default function PessoasPage() {
  const [pessoas, setPessoas] = useState<Pessoa[]>([]);
  const [form, setForm] = useState<PessoaInput>(emptyForm);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [confirmDeleteId, setConfirmDeleteId] = useState<number | null>(null);
  const [erro, setErro] = useState("");
  const [loading, setLoading] = useState(false);

  const carregar = async () => setPessoas(await listarPessoas());
  useEffect(() => { carregar(); }, []);

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
      if (editingId === id) { setEditingId(null); setForm(emptyForm); }
      await carregar();
    } catch (err: unknown) {
      setConfirmDeleteId(null);
      setErro(err instanceof Error ? err.message : "Erro ao deletar pessoa.");
    }
  };

  const cancelarEdicao = () => { setEditingId(null); setForm(emptyForm); setErro(""); };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h1 className="text-2xl font-bold text-gray-900">Pessoas</h1>
        <p className="text-sm text-gray-500">Gerencie as pessoas do sistema</p>
      </div>

      {/* Form card */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <UserPlus size={16} />
            {editingId !== null ? "Editar Pessoa" : "Nova Pessoa"}
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
            <div className="space-y-1.5">
              <Label htmlFor="nome">Nome</Label>
              <Input
                id="nome"
                maxLength={200}
                placeholder="Nome completo"
                value={form.nome}
                onChange={(e) => setForm({ ...form, nome: e.target.value })}
              />
            </div>
            <div className="space-y-1.5">
              <Label htmlFor="idade">Idade</Label>
              <Input
                id="idade"
                className="max-w-[120px]"
                type="number"
                min={0}
                max={150}
                placeholder="0"
                value={form.idade || ""}
                onChange={(e) => setForm({ ...form, idade: Number(e.target.value) })}
              />
            </div>
            <div className="flex gap-2">
              <Button type="submit" disabled={loading}>
                {loading ? "Salvando..." : editingId !== null ? "Salvar alterações" : "Cadastrar"}
              </Button>
              {editingId !== null && (
                <Button type="button" variant="outline" onClick={cancelarEdicao}>
                  Cancelar
                </Button>
              )}
            </div>
          </form>
        </CardContent>
      </Card>

      {/* Table card */}
      <Card>
        <CardContent className="p-0">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b bg-gray-50">
                <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-gray-500">#</th>
                <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-gray-500">Nome</th>
                <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-gray-500">Idade</th>
                <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-gray-500">Ações</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {pessoas.map((p) => (
                <tr key={p.id} className="hover:bg-gray-50">
                  <td className="px-4 py-3 text-gray-400">{p.id}</td>
                  <td className="px-4 py-3 font-medium text-gray-800">
                    {p.nome}
                    {p.idade < 18 && (
                      <Badge variant="despesa" className="ml-2">Menor</Badge>
                    )}
                  </td>
                  <td className="px-4 py-3 text-gray-600">{p.idade} anos</td>
                  <td className="px-4 py-3">
                    {confirmDeleteId === p.id ? (
                      <div className="flex flex-wrap items-center gap-2">
                        <span className="text-xs font-semibold text-red-600">
                          Deletar e todas as transações?
                        </span>
                        <Button size="sm" variant="destructive" onClick={() => handleDeletar(p.id)}>
                          Confirmar
                        </Button>
                        <Button size="sm" variant="outline" onClick={() => setConfirmDeleteId(null)}>
                          Cancelar
                        </Button>
                      </div>
                    ) : (
                      <div className="flex gap-2">
                        <Button size="sm" variant="outline" onClick={() => handleEditar(p)}>
                          <Pencil size={13} /> Editar
                        </Button>
                        <Button size="sm" variant="destructive" onClick={() => setConfirmDeleteId(p.id)}>
                          <Trash2 size={13} /> Deletar
                        </Button>
                      </div>
                    )}
                  </td>
                </tr>
              ))}
              {pessoas.length === 0 && (
                <tr>
                  <td colSpan={4} className="px-4 py-10 text-center text-sm text-gray-400">
                    Nenhuma pessoa cadastrada.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </CardContent>
      </Card>
    </div>
  );
}
