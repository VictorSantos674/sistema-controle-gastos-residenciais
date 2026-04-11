import { ArrowDownCircle, ArrowUpCircle, Layers, Tag, TrendingUp, Users } from "lucide-react";
import { useEffect, useState } from "react";
import { listarCategorias } from "../api/categorias";
import { listarPessoas } from "../api/pessoas";
import { relatorioPorPessoa } from "../api/relatorios";
import { listarTransacoes } from "../api/transacoes";
import { Card, CardContent } from "../components/ui/card";

type StatColor = "emerald" | "red";

interface StatCardProps {
  label: string;
  value: string;
  icon: React.ReactNode;
  color: StatColor;
}

// Mapeamento de cor para classes Tailwind (light + dark) — precisa ser literal para o Tailwind detectar
const STAT_COLORS: Record<StatColor, { icon: string; bg: string }> = {
  emerald: {
    icon: "text-emerald-600 dark:text-emerald-400",
    bg:   "bg-emerald-50 dark:bg-emerald-900/30",
  },
  red: {
    icon: "text-red-600 dark:text-red-400",
    bg:   "bg-red-50 dark:bg-red-900/30",
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

export default function DashboardPage() {
  const [totalReceitas, setTotalReceitas] = useState(0);
  const [totalDespesas, setTotalDespesas] = useState(0);
  const [saldoLiquido, setSaldoLiquido] = useState(0);
  const [receitasMes, setReceitasMes] = useState(0);
  const [despesasMes, setDespesasMes] = useState(0);
  const [saldoMes, setSaldoMes] = useState(0);
  const [nPessoas, setNPessoas] = useState(0);
  const [nCategorias, setNCategorias] = useState(0);
  const [nTransacoes, setNTransacoes] = useState(0);
  const [loading, setLoading] = useState(true);
  const [erro, setErro] = useState("");

  const mesAtual = new Date().getMonth() + 1;
  const anoAtual = new Date().getFullYear();

  useEffect(() => {
    Promise.all([
      relatorioPorPessoa(),
      relatorioPorPessoa({ mes: mesAtual, ano: anoAtual }),
      listarPessoas(),
      listarCategorias(),
      listarTransacoes(),
    ])
      .then(([relTotal, relMes, pessoas, categorias, transacoes]) => {
        setTotalReceitas(relTotal.totalGeralReceitas);
        setTotalDespesas(relTotal.totalGeralDespesas);
        setSaldoLiquido(relTotal.saldoLiquido);
        setReceitasMes(relMes.totalGeralReceitas);
        setDespesasMes(relMes.totalGeralDespesas);
        setSaldoMes(relMes.saldoLiquido);
        setNPessoas(pessoas.length);
        setNCategorias(categorias.length);
        setNTransacoes(transacoes.length);
      })
      .catch((err: unknown) => {
        setErro(err instanceof Error ? err.message : "Erro ao carregar o dashboard.");
      })
      .finally(() => setLoading(false));
  }, []);

  const fmt = (v: number) =>
    v.toLocaleString("pt-BR", { style: "currency", currency: "BRL" });

  const nomeMes = new Date().toLocaleString("pt-BR", { month: "long" });

  const saldoMesColor:   StatColor = saldoMes    >= 0 ? "emerald" : "red";
  const saldoTotalColor: StatColor = saldoLiquido >= 0 ? "emerald" : "red";

  return (
    <div className="space-y-8">
      {/* Header */}
      <div>
        <h1 className="text-2xl font-bold text-gray-900 dark:text-gray-100">Dashboard</h1>
        <p className="text-sm text-gray-500 dark:text-gray-400">Visão geral das suas finanças</p>
      </div>

      {erro && (
        <div className="rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700 dark:border-red-800 dark:bg-red-900/30 dark:text-red-300">
          {erro}
        </div>
      )}

      {loading ? (
        <div className="flex items-center gap-2 text-sm text-gray-400 dark:text-gray-500">
          <div className="h-4 w-4 animate-spin rounded-full border-2 border-blue-500 border-t-transparent" />
          Carregando...
        </div>
      ) : !erro ? (
        <>
          {/* Mês atual */}
          <section>
            <h2 className="mb-3 text-sm font-semibold uppercase tracking-wider text-gray-500 dark:text-gray-400 capitalize">
              {nomeMes} de {anoAtual}
            </h2>
            <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
              <StatCard label="Receitas do mês"  value={fmt(receitasMes)}  icon={<ArrowUpCircle size={20} />}   color="emerald" />
              <StatCard label="Despesas do mês"  value={fmt(despesasMes)}  icon={<ArrowDownCircle size={20} />} color="red" />
              <StatCard label="Saldo do mês"     value={fmt(saldoMes)}     icon={<TrendingUp size={20} />}      color={saldoMesColor} />
            </div>
          </section>

          {/* Acumulado total */}
          <section>
            <h2 className="mb-3 text-sm font-semibold uppercase tracking-wider text-gray-500 dark:text-gray-400">
              Acumulado total
            </h2>
            <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
              <StatCard label="Total Receitas" value={fmt(totalReceitas)} icon={<ArrowUpCircle size={20} />}   color="emerald" />
              <StatCard label="Total Despesas" value={fmt(totalDespesas)} icon={<ArrowDownCircle size={20} />} color="red" />
              <StatCard label="Saldo Líquido"  value={fmt(saldoLiquido)} icon={<TrendingUp size={20} />}      color={saldoTotalColor} />
            </div>
          </section>

          {/* Contadores */}
          <section>
            <h2 className="mb-3 text-sm font-semibold uppercase tracking-wider text-gray-500 dark:text-gray-400">
              Cadastros
            </h2>
            <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
              <CountCard label="Pessoas"    count={nPessoas}    icon={<Users size={20} />}  />
              <CountCard label="Categorias" count={nCategorias} icon={<Tag size={20} />}    />
              <CountCard label="Transações" count={nTransacoes} icon={<Layers size={20} />} />
            </div>
          </section>
        </>
      ) : null}
    </div>
  );
}
