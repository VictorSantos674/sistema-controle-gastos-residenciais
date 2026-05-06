export type Finalidade = "Despesa" | "Receita" | "Ambas";
export type TipoTransacao = "Despesa" | "Receita" | "Ambas";

export interface Pessoa {
  id: number;
  nome: string;
  idade: number;
}

export interface Categoria {
  id: number;
  descricao: string;
  finalidade: Finalidade;
}

export interface Transacao {
  id: number;
  descricao: string;
  valor: number;
  valorReceita?: number;
  valorDespesa?: number;
  tipo: TipoTransacao;
  categoriaId: number;
  categoriaDescricao: string;
  pessoaId: number;
  pessoaNome: string;
  data: string;
}

export interface PaginatedResponse<T> {
  data: T[];
  total: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface DashboardResumo {
  totalReceitas: number;
  totalDespesas: number;
  saldoLiquido: number;
  receitasMes: number;
  despesasMes: number;
  saldoMes: number;
  totalPessoas: number;
  totalCategorias: number;
  totalTransacoes: number;
}

export interface TotalPorPessoa {
  pessoaId: number;
  nomePessoa: string;
  totalReceitas: number;
  totalDespesas: number;
  saldo: number;
}

export interface RelatorioPorPessoa {
  pessoas: TotalPorPessoa[];
  totalGeralReceitas: number;
  totalGeralDespesas: number;
  saldoLiquido: number;
}

export interface TotalPorCategoria {
  categoriaId: number;
  descricaoCategoria: string;
  finalidade: Finalidade;
  totalReceitas: number;
  totalDespesas: number;
  saldo: number;
}

export interface RelatorioPorCategoria {
  categorias: TotalPorCategoria[];
  totalGeralReceitas: number;
  totalGeralDespesas: number;
  saldoLiquido: number;
}
