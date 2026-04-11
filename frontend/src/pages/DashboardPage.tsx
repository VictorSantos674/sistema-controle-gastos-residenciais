import { ArrowDownCircle, ArrowUpCircle, Layers, Tag, TrendingUp, Users } from "lucide-react";
import { useEffect, useState } from "react";
import { listarCategorias } from "../api/categorias";
import { listarPessoas } from "../api/pessoas";
import { relatorioPorPessoa } from "../api/relatorios";
import { listarTransacoes } from "../api/transacoes";
import { Card, CardContent } from "../components/ui/card";

interface StatCardProps {
  label: string;
  value: string;
  icon: React.ReactNode;
  colorClass: string;
  bgClass: string;
}

function StatCard({ label, value, icon, colorClass, bgClass }: Readonly<StatCardProps>) {
  return (
    <Card>
      <CardContent className="flex items-center gap-4 p-5">
        <div className={`rounded-xl p-3 ${bgClass}`}>
          <div className={colorClass}>{icon}</div>
        </div>
        <div>
          <p className="text-xs font-semibold uppercase tracking-wider text-gray-500">{label}</p>
          <p className="mt-0.5 text-xl font-bold text-gray-800">{value}</p>
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
        <div className="rounded-xl bg-slate-100 p-3 text-slate-600">{icon}</div>
        <div>
          <p className="text-xs font-semibold uppercase tracking-wider text-gray-500">{label}</p>
          <p className="mt-0.5 text-3xl font-bold text-gray-800">{count}</p>
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

  const saldoMesColor = saldoMes >= 0 ? "emerald" : "red";
  const saldoTotalColor = saldoLiquido >= 0 ? "emerald" : "red";

  return (
    <div className="space-y-8">
      {/* Header */}
      <div>
        <h1 className="text-2xl font-bold text-gray-900">Dashboard</h1>
        <p className="text-sm text-gray-500">Visão geral das suas finanças</p>
      </div>

      {erro && (
        <div className="rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
          {erro}
        </div>
      )}

      {loading ? (
        <div className="flex items-center gap-2 text-sm text-gray-400">
          <div className="h-4 w-4 animate-spin rounded-full border-2 border-blue-500 border-t-transparent" />
          Carregando...
        </div>
      ) : !erro ? (
        <>
          {/* Mês atual */}
          <section>
            <h2 className="mb-3 text-sm font-semibold uppercase tracking-wider text-gray-500 capitalize">
              {nomeMes} de {anoAtual}
            </h2>
            <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
              <StatCard
                label="Receitas do mês"
                value={fmt(receitasMes)}
                icon={<ArrowUpCircle size={20} />}
                colorClass="text-emerald-600"
                bgClass="bg-emerald-50"
              />
              <StatCard
                label="Despesas do mês"
                value={fmt(despesasMes)}
                icon={<ArrowDownCircle size={20} />}
                colorClass="text-red-600"
                bgClass="bg-red-50"
              />
              <StatCard
                label="Saldo do mês"
                value={fmt(saldoMes)}
                icon={<TrendingUp size={20} />}
                colorClass={`text-${saldoMesColor}-600`}
                bgClass={`bg-${saldoMesColor}-50`}
              />
            </div>
          </section>

          {/* Acumulado total */}
          <section>
            <h2 className="mb-3 text-sm font-semibold uppercase tracking-wider text-gray-500">
              Acumulado total
            </h2>
            <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
              <StatCard
                label="Total Receitas"
                value={fmt(totalReceitas)}
                icon={<ArrowUpCircle size={20} />}
                colorClass="text-emerald-600"
                bgClass="bg-emerald-50"
              />
              <StatCard
                label="Total Despesas"
                value={fmt(totalDespesas)}
                icon={<ArrowDownCircle size={20} />}
                colorClass="text-red-600"
                bgClass="bg-red-50"
              />
              <StatCard
                label="Saldo Líquido"
                value={fmt(saldoLiquido)}
                icon={<TrendingUp size={20} />}
                colorClass={`text-${saldoTotalColor}-600`}
                bgClass={`bg-${saldoTotalColor}-50`}
              />
            </div>
          </section>

          {/* Contadores */}
          <section>
            <h2 className="mb-3 text-sm font-semibold uppercase tracking-wider text-gray-500">
              Cadastros
            </h2>
            <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
              <CountCard label="Pessoas" count={nPessoas} icon={<Users size={20} />} />
              <CountCard label="Categorias" count={nCategorias} icon={<Tag size={20} />} />
              <CountCard label="Transações" count={nTransacoes} icon={<Layers size={20} />} />
            </div>
          </section>
        </>
      ) : null}
    </div>
  );
}
