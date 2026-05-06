import { PaginatedResponse, Transacao } from "../types";
import client from "./client";

export interface TransacaoInput {
  descricao: string;
  valor?: number;
  valorReceita?: number;
  valorDespesa?: number;
  tipo: number;
  categoriaId: number;
  pessoaId: number;
  data?: string;
}

export interface TransacaoPaginationParams {
  page?: number;
  pageSize?: number;
}

export const listarTransacoes = (
  params: TransacaoPaginationParams = {}
): Promise<PaginatedResponse<Transacao>> =>
  client.get<PaginatedResponse<Transacao>>("/api/transacoes", { params }).then((r) => r.data);

export const criarTransacao = (data: TransacaoInput): Promise<Transacao> =>
  client.post<Transacao>("/api/transacoes", data).then((r) => r.data);

export const editarTransacao = (id: number, data: TransacaoInput): Promise<Transacao> =>
  client.put<Transacao>(`/api/transacoes/${id}`, data).then((r) => r.data);

export const deletarTransacao = (id: number): Promise<void> =>
  client.delete(`/api/transacoes/${id}`).then(() => undefined);
