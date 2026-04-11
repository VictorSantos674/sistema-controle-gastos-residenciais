import { AlertTriangle, FolderPlus, Trash2 } from "lucide-react";
import { useEffect, useState } from "react";
import { type CategoriaInput, criarCategoria, deletarCategoria, listarCategorias } from "../api/categorias";
import { Badge } from "../components/ui/badge";
import { Button } from "../components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "../components/ui/card";
import { Input } from "../components/ui/input";
import { Label } from "../components/ui/label";
import { Select } from "../components/ui/select";
import type { Categoria } from "../types";

const emptyForm: CategoriaInput = { descricao: "", finalidade: 1 };

function finalidadeBadge(finalidade: string) {
  if (finalidade === "Receita") return <Badge variant="receita">Receita</Badge>;
  if (finalidade === "Despesa") return <Badge variant="despesa">Despesa</Badge>;
  return <Badge variant="ambas">Ambas</Badge>;
}

export default function CategoriasPage() {
  const [categorias, setCategorias] = useState<Categoria[]>([]);
  const [form, setForm] = useState<CategoriaInput>(emptyForm);
  const [erro, setErro] = useState("");
  const [loading, setLoading] = useState(false);
  const [confirmDeleteId, setConfirmDeleteId] = useState<number | null>(null);

  const carregar = async () => setCategorias(await listarCategorias());
  useEffect(() => { carregar(); }, []);

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

  const handleDeletar = async (id: number) => {
    setErro("");
    try {
      await deletarCategoria(id);
      setConfirmDeleteId(null);
      await carregar();
    } catch (err: unknown) {
      setConfirmDeleteId(null);
      setErro(err instanceof Error ? err.message : "Erro ao deletar categoria.");
    }
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h1 className="text-2xl font-bold text-gray-900">Categorias</h1>
        <p className="text-sm text-gray-500">Gerencie as categorias de transações</p>
      </div>

      {/* Form card */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <FolderPlus size={16} />
            Nova Categoria
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
              <Label htmlFor="descricao">Descrição</Label>
              <Input
                id="descricao"
                maxLength={400}
                placeholder="Ex: Alimentação, Salário..."
                value={form.descricao}
                onChange={(e) => setForm({ ...form, descricao: e.target.value })}
              />
            </div>
            <div className="space-y-1.5">
              <Label htmlFor="finalidade">Finalidade</Label>
              <Select
                id="finalidade"
                className="max-w-[200px]"
                value={form.finalidade}
                onChange={(e) => setForm({ ...form, finalidade: Number(e.target.value) })}
              >
                <option value={1}>Despesa</option>
                <option value={2}>Receita</option>
                <option value={3}>Ambas</option>
              </Select>
            </div>
            <Button type="submit" disabled={loading}>
              {loading ? "Salvando..." : "Cadastrar"}
            </Button>
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
                <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-gray-500">Descrição</th>
                <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-gray-500">Finalidade</th>
                <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-gray-500">Ações</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {categorias.map((c) => (
                <tr key={c.id} className="hover:bg-gray-50">
                  <td className="px-4 py-3 text-gray-400">{c.id}</td>
                  <td className="px-4 py-3 font-medium text-gray-800">{c.descricao}</td>
                  <td className="px-4 py-3">{finalidadeBadge(c.finalidade)}</td>
                  <td className="px-4 py-3">
                    {confirmDeleteId === c.id ? (
                      <div className="flex flex-wrap items-center gap-2">
                        <span className="text-xs font-semibold text-red-600">Deletar categoria?</span>
                        <Button size="sm" variant="destructive" onClick={() => handleDeletar(c.id)}>
                          Confirmar
                        </Button>
                        <Button size="sm" variant="outline" onClick={() => setConfirmDeleteId(null)}>
                          Cancelar
                        </Button>
                      </div>
                    ) : (
                      <Button size="sm" variant="destructive" onClick={() => setConfirmDeleteId(c.id)}>
                        <Trash2 size={13} /> Deletar
                      </Button>
                    )}
                  </td>
                </tr>
              ))}
              {categorias.length === 0 && (
                <tr>
                  <td colSpan={4} className="px-4 py-10 text-center text-sm text-gray-400">
                    Nenhuma categoria cadastrada.
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
