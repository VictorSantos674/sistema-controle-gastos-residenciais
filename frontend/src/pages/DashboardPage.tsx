import { ArrowDownCircle, ArrowUpCircle, Layers, Tag, TrendingUp, Users } from "lucide-react";
import { useEffect, useState } from "react";
import { obterResumoDashboard } from "../api/dashboard";
import { Card, CardContent } from "../components/ui/card";
import type { DashboardResumo } from "../types";

type StatColor = "emerald" | "red";

interface StatCardProps {
  label: string;
  value: string;
  icon: React.ReactNode;
  color: StatColor;
}

const STAT_COLORS: Record<StatColor, { icon: string; bg: string }> = {
  emerald: {
    icon: "text-emerald-600 dark:text-emerald-400",
    bg: "bg-emerald-50 dark:bg-emerald-900/30",
  },
  red: {
    icon: "text-red-600 dark:text-red-400",
    bg: "bg-red-50 dark:bg-red-900/30",
  },
};

function StatCard({ label, value, icon, color }: Readonly<StatCardProps>) {
  const { icon: iconClass, bg: bgClass } = STAT_COLORS[color];
  return (
    <Card>
      <CardContent className="flex items-center gap-4 p-5">
        <div className={`rounded-xl p-3 ${bgClass}`}>
          <div className={iconClass}>{icon}</div>
        </div>
        <div>
          <p className="text-xs font-semibold uppercase tracking-wider text-gray-500 dark:text-gray-400">{label}</p>
          <p className="mt-0.5 text-xl font-bold text-gray-800 dark:text-gray-200">{value}</p>
        </div>
      </CardContent>
    </Card>
  );
}

interface CountCardProps {
  label: string;
  count: number;
  icon: React.ReactNode;
}

function CountCard({ label, count, icon }: Readonly<CountCardProps>) {
  return (
    <Card>
      <CardContent className="flex items-center gap-4 p-5">
        <div className="rounded-xl bg-slate-100 p-3 text-slate-600 dark:bg-slate-700 dark:text-slate-300">{icon}</div>
        <div>
          <p className="text-xs font-semibold uppercase tracking-wider text-gray-500 dark:text-gray-400">{label}</p>
          <p className="mt-0.5 text-3xl font-bold text-gray-800 dark:text-gray-200">{count}</p>
        </div>
      </CardContent>
    </Card>
  );
}

function SkeletonCard() {
  return (
    <Card>
      <CardContent className="flex items-center gap-4 p-5">
        <div className="h-11 w-11 animate-pulse rounded-xl bg-gray-200 dark:bg-gray-700" />
        <div className="flex-1 space-y-2">
          <div className="h-3 w-28 animate-pulse rounded bg-gray-200 dark:bg-gray-700" />
          <div className="h-6 w-36 animate-pulse rounded bg-gray-200 dark:bg-gray-700" />
        </div>
      </CardContent>
    </Card>
  );
}

export default function DashboardPage() {
  const [resumo, setResumo] = useState<DashboardResumo | null>(null);
  const [loading, setLoading] = useState(true);
  const [erro, setErro] = useState("");

  const anoAtual = new Date().getFullYear();

  useEffect(() => {
    obterResumoDashboard()
      .then(setResumo)
      .catch((err: unknown) => {
        setErro(err instanceof Error ? err.message : "Erro ao carregar o dashboard.");
      })
      .finally(() => setLoading(false));
  }, []);

  const fmt = (v: number) =>
    v.toLocaleString("pt-BR", { style: "currency", currency: "BRL" });

  const nomeMes = new Date().toLocaleString("pt-BR", { month: "long" });

  const saldoMesColor: StatColor = (resumo?.saldoMes ?? 0) >= 0 ? "emerald" : "red";
  const saldoTotalColor: StatColor = (resumo?.saldoLiquido ?? 0) >= 0 ? "emerald" : "red";

  const renderSkeletonGrid = () => (
    <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
      <SkeletonCard />
      <SkeletonCard />
      <SkeletonCard />
    </div>
  );

  return (
    <div className="space-y-8">
      <div>
        <h1 className="text-2xl font-bold text-gray-900 dark:text-gray-100">Dashboard</h1>
        <p className="text-sm text-gray-500 dark:text-gray-400">Visão geral das suas finanças</p>
      </div>

      {erro && (
        <div className="rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700 dark:border-red-800 dark:bg-red-900/30 dark:text-red-300">
          {erro}
        </div>
      )}

      <section>
        <h2 className="mb-3 text-sm font-semibold uppercase tracking-wider text-gray-500 dark:text-gray-400 capitalize">
          {nomeMes} de {anoAtual}
        </h2>
        {loading ? renderSkeletonGrid() : resumo ? (
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
            <StatCard label="Receitas do mês" value={fmt(resumo.receitasMes)} icon={<ArrowUpCircle size={20} />} color="emerald" />
            <StatCard label="Despesas do mês" value={fmt(resumo.despesasMes)} icon={<ArrowDownCircle size={20} />} color="red" />
            <StatCard label="Saldo do mês" value={fmt(resumo.saldoMes)} icon={<TrendingUp size={20} />} color={saldoMesColor} />
          </div>
        ) : null}
      </section>

      <section>
        <h2 className="mb-3 text-sm font-semibold uppercase tracking-wider text-gray-500 dark:text-gray-400">
          Acumulado total
        </h2>
        {loading ? renderSkeletonGrid() : resumo ? (
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
            <StatCard label="Total Receitas" value={fmt(resumo.totalReceitas)} icon={<ArrowUpCircle size={20} />} color="emerald" />
            <StatCard label="Total Despesas" value={fmt(resumo.totalDespesas)} icon={<ArrowDownCircle size={20} />} color="red" />
            <StatCard label="Saldo Líquido" value={fmt(resumo.saldoLiquido)} icon={<TrendingUp size={20} />} color={saldoTotalColor} />
          </div>
        ) : null}
      </section>

      <section>
        <h2 className="mb-3 text-sm font-semibold uppercase tracking-wider text-gray-500 dark:text-gray-400">
          Cadastros
        </h2>
        {loading ? renderSkeletonGrid() : resumo ? (
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
            <CountCard label="Pessoas" count={resumo.totalPessoas} icon={<Users size={20} />} />
            <CountCard label="Categorias" count={resumo.totalCategorias} icon={<Tag size={20} />} />
            <CountCard label="Transações" count={resumo.totalTransacoes} icon={<Layers size={20} />} />
          </div>
        ) : null}
      </section>
    </div>
  );
}
