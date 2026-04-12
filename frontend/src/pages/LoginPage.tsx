import { AlertTriangle, Eye, EyeOff } from "lucide-react";
import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { login as apiLogin } from "../api/auth";
import { Button } from "../components/ui/button";
import { Input } from "../components/ui/input";
import { Label } from "../components/ui/label";
import { useAuth } from "../contexts/AuthContext";

export default function LoginPage() {
  const { login } = useAuth();
  const navigate   = useNavigate();

  const [form, setForm]           = useState({ login: "", senha: "" });
  const [erro, setErro]           = useState("");
  const [loading, setLoading]     = useState(false);
  const [showSenha, setShowSenha] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setErro("");
    if (!form.login.trim()) return setErro("Informe o login.");
    if (!form.senha)        return setErro("Informe a senha.");

    setLoading(true);
    try {
      const resp = await apiLogin({ login: form.login, senha: form.senha });
      login(resp.token, resp.login);
      navigate("/dashboard", { replace: true });
    } catch (err: unknown) {
      setErro(err instanceof Error ? err.message : "Erro ao fazer login.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="flex min-h-screen items-center justify-center bg-gray-900 px-4">
      <div className="w-full max-w-sm">
        {/* Brand */}
        <div className="mb-8 text-center">
          <h1 className="text-2xl font-bold text-gray-100">
            <span className="text-blue-400">Gastos</span> Residenciais
          </h1>
          <p className="mt-1 text-sm text-gray-400">
            Faça login para acessar sua conta
          </p>
        </div>

        {/* Card */}
        <div className="rounded-xl border border-gray-700 bg-gray-800 p-6 shadow-sm">
          {erro && (
            <div className="mb-4 flex items-center gap-2 rounded-lg border border-red-800 bg-red-900/30 px-4 py-3 text-sm text-red-300">
              <AlertTriangle size={15} className="shrink-0" />
              {erro}
            </div>
          )}

          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="space-y-1.5">
              <Label htmlFor="login" className="text-gray-300">Login</Label>
              <Input
                id="login"
                autoComplete="username"
                placeholder="Seu login"
                value={form.login}
                onChange={(e) => setForm({ ...form, login: e.target.value })}
                className="bg-gray-700 border-gray-600 text-gray-100 placeholder:text-gray-500"
              />
            </div>
            <div className="space-y-1.5">
              <Label htmlFor="senha" className="text-gray-300">Senha</Label>
              <div className="relative">
                <Input
                  id="senha"
                  type={showSenha ? "text" : "password"}
                  autoComplete="current-password"
                  placeholder="Sua senha"
                  value={form.senha}
                  onChange={(e) => setForm({ ...form, senha: e.target.value })}
                  className="bg-gray-700 border-gray-600 text-gray-100 placeholder:text-gray-500 pr-10"
                />
                <button
                  type="button"
                  onClick={() => setShowSenha(!showSenha)}
                  className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-200 transition-colors"
                >
                  {showSenha ? <EyeOff size={16} /> : <Eye size={16} />}
                </button>
              </div>
            </div>
            <Button type="submit" className="w-full" disabled={loading}>
              {loading ? "Entrando..." : "Entrar"}
            </Button>
          </form>
        </div>

        <p className="mt-4 text-center text-sm text-gray-400">
          Não tem conta?{" "}
          <Link
            to="/cadastro"
            className="font-medium text-blue-400 hover:underline"
          >
            Criar conta
          </Link>
        </p>
      </div>
    </div>
  );
}
