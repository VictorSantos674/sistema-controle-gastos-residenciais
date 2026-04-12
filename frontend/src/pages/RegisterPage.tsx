import { AlertTriangle } from "lucide-react";
import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { registrar as apiRegistrar } from "../api/auth";
import { Button } from "../components/ui/button";
import { Input } from "../components/ui/input";
import { Label } from "../components/ui/label";
import { useAuth } from "../contexts/AuthContext";

export default function RegisterPage() {
  const { login } = useAuth();
  const navigate   = useNavigate();

  const [form, setForm]     = useState({ login: "", senha: "", confirmar: "" });
  const [erro, setErro]     = useState("");
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setErro("");
    if (!form.login.trim())          return setErro("Informe o login.");
    if (form.senha.length < 6)       return setErro("A senha deve ter pelo menos 6 caracteres.");
    if (form.senha !== form.confirmar) return setErro("As senhas não coincidem.");

    setLoading(true);
    try {
      const resp = await apiRegistrar({ login: form.login, senha: form.senha });
      login(resp.token, resp.login);
      navigate("/dashboard", { replace: true });
    } catch (err: unknown) {
      setErro(err instanceof Error ? err.message : "Erro ao criar conta.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="flex min-h-screen items-center justify-center bg-gray-100 dark:bg-gray-900 px-4">
      <div className="w-full max-w-sm">
        {/* Brand */}
        <div className="mb-8 text-center">
          <h1 className="text-2xl font-bold text-gray-900 dark:text-gray-100">
            <span className="text-blue-600 dark:text-blue-400">Gastos</span> Residenciais
          </h1>
          <p className="mt-1 text-sm text-gray-500 dark:text-gray-400">
            Crie sua conta para começar
          </p>
        </div>

        {/* Card */}
        <div className="rounded-xl border bg-white dark:bg-gray-800 dark:border-gray-700 p-6 shadow-sm">
          {erro && (
            <div className="mb-4 flex items-center gap-2 rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700 dark:border-red-800 dark:bg-red-900/30 dark:text-red-300">
              <AlertTriangle size={15} className="shrink-0" />
              {erro}
            </div>
          )}

          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="space-y-1.5">
              <Label htmlFor="login">Login</Label>
              <Input
                id="login"
                autoComplete="username"
                placeholder="Escolha um login"
                value={form.login}
                onChange={(e) => setForm({ ...form, login: e.target.value })}
              />
            </div>
            <div className="space-y-1.5">
              <Label htmlFor="senha">Senha</Label>
              <Input
                id="senha"
                type="password"
                autoComplete="new-password"
                placeholder="Mínimo 6 caracteres"
                value={form.senha}
                onChange={(e) => setForm({ ...form, senha: e.target.value })}
              />
            </div>
            <div className="space-y-1.5">
              <Label htmlFor="confirmar">Confirmar senha</Label>
              <Input
                id="confirmar"
                type="password"
                autoComplete="new-password"
                placeholder="Repita a senha"
                value={form.confirmar}
                onChange={(e) => setForm({ ...form, confirmar: e.target.value })}
              />
            </div>
            <Button type="submit" className="w-full" disabled={loading}>
              {loading ? "Criando conta..." : "Criar conta"}
            </Button>
          </form>
        </div>

        <p className="mt-4 text-center text-sm text-gray-500 dark:text-gray-400">
          Já tem conta?{" "}
          <Link
            to="/login"
            className="font-medium text-blue-600 hover:underline dark:text-blue-400"
          >
            Fazer login
          </Link>
        </p>
      </div>
    </div>
  );
}
