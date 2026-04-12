import { AlertTriangle, Check, Eye, EyeOff, X } from "lucide-react";
import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { registrar as apiRegistrar } from "../api/auth";
import { Button } from "../components/ui/button";
import { Input } from "../components/ui/input";
import { Label } from "../components/ui/label";
import { useAuth } from "../contexts/AuthContext";

type ReqKey = "minLength" | "lowercase" | "uppercase" | "number" | "special";

interface Req {
  label: string;
  test: (v: string) => boolean;
}

const requisitos: Record<ReqKey, Req> = {
  minLength: { label: "Mínimo 8 caracteres",              test: (v) => v.length >= 8           },
  lowercase: { label: "Pelo menos uma letra minúscula",   test: (v) => /[a-z]/.test(v)         },
  uppercase: { label: "Pelo menos uma letra maiúscula",   test: (v) => /[A-Z]/.test(v)         },
  number:    { label: "Pelo menos um número",             test: (v) => /[0-9]/.test(v)         },
  special:   { label: "Pelo menos um caractere especial", test: (v) => /[^a-zA-Z0-9]/.test(v) },
};

export default function RegisterPage() {
  const { login } = useAuth();
  const navigate   = useNavigate();

  const [form, setForm]                       = useState({ login: "", senha: "", confirmar: "" });
  const [erro, setErro]                       = useState("");
  const [loading, setLoading]                 = useState(false);
  const [showSenha, setShowSenha]             = useState(false);
  const [showConfirmar, setShowConfirmar]     = useState(false);
  const [senhaInteragida, setSenhaInteragida] = useState(false);

  const reqStatus  = (key: ReqKey) => requisitos[key].test(form.senha);
  const allReqsMet = (Object.keys(requisitos) as ReqKey[]).every(reqStatus);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setErro("");
    if (!form.login.trim())            return setErro("Informe o login.");
    if (!allReqsMet)                   return setErro("A senha não atende aos requisitos de segurança.");
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

  const senhasDiferem = form.confirmar.length > 0 && form.confirmar !== form.senha;
  const senhasIguais  = form.confirmar.length > 0 && form.confirmar === form.senha;

  return (
    <div className="flex min-h-screen items-center justify-center bg-gray-900 px-4 py-8">
      <div className="w-full max-w-sm">
        {/* Brand */}
        <div className="mb-8 text-center">
          <h1 className="text-2xl font-bold text-gray-100">
            <span className="text-blue-400">Gastos</span> Residenciais
          </h1>
          <p className="mt-1 text-sm text-gray-400">
            Crie sua conta para começar
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
            {/* Login */}
            <div className="space-y-1.5">
              <Label htmlFor="login" className="text-gray-300">Login</Label>
              <Input
                id="login"
                autoComplete="username"
                placeholder="Escolha um login"
                value={form.login}
                onChange={(e) => setForm({ ...form, login: e.target.value })}
                className="bg-gray-700 border-gray-600 text-gray-100 placeholder:text-gray-500"
              />
            </div>

            {/* Senha */}
            <div className="space-y-1.5">
              <Label htmlFor="senha" className="text-gray-300">Senha</Label>
              <div className="relative">
                <Input
                  id="senha"
                  type={showSenha ? "text" : "password"}
                  autoComplete="new-password"
                  placeholder="Crie uma senha segura"
                  value={form.senha}
                  onFocus={() => setSenhaInteragida(true)}
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

              {/* Validador de força da senha */}
              {senhaInteragida && (
                <div className="mt-2 space-y-1.5 rounded-lg border border-gray-700 bg-gray-900/60 px-3 py-2.5">
                  {(Object.entries(requisitos) as [ReqKey, Req][]).map(([key, req]) => {
                    const met = reqStatus(key);
                    return (
                      <div key={key} className="flex items-center gap-2 text-xs">
                        {met
                          ? <Check size={12} className="shrink-0 text-green-400" />
                          : <X    size={12} className="shrink-0 text-red-400"   />
                        }
                        <span className={met ? "text-green-400" : "text-red-400"}>
                          {req.label}
                        </span>
                      </div>
                    );
                  })}
                </div>
              )}
            </div>

            {/* Confirmar senha */}
            <div className="space-y-1.5">
              <Label htmlFor="confirmar" className="text-gray-300">Confirmar senha</Label>
              <div className="relative">
                <Input
                  id="confirmar"
                  type={showConfirmar ? "text" : "password"}
                  autoComplete="new-password"
                  placeholder="Repita a senha"
                  value={form.confirmar}
                  onChange={(e) => setForm({ ...form, confirmar: e.target.value })}
                  className={[
                    "bg-gray-700 border-gray-600 text-gray-100 placeholder:text-gray-500 pr-10",
                    senhasDiferem && "border-red-500 ring-1 ring-red-500",
                    senhasIguais  && "border-green-500 ring-1 ring-green-500",
                  ].filter(Boolean).join(" ")}
                />
                <button
                  type="button"
                  onClick={() => setShowConfirmar(!showConfirmar)}
                  className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-200 transition-colors"
                >
                  {showConfirmar ? <EyeOff size={16} /> : <Eye size={16} />}
                </button>
              </div>
              {senhasDiferem && (
                <p className="text-xs text-red-400">As senhas não coincidem.</p>
              )}
            </div>

            <Button type="submit" className="w-full" disabled={loading}>
              {loading ? "Criando conta..." : "Criar conta"}
            </Button>
          </form>
        </div>

        <p className="mt-4 text-center text-sm text-gray-400">
          Já tem conta?{" "}
          <Link
            to="/login"
            className="font-medium text-blue-400 hover:underline"
          >
            Fazer login
          </Link>
        </p>
      </div>
    </div>
  );
}
