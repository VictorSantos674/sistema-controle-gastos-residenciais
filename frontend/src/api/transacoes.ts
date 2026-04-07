import { Transacao } from "../types";
import client from "./client";

export interface TransacaoInput {
  descricao: string;
  valor?: number;
  valorReceita?: number;
  valorDespesa?: number;
  tipo: number;
  categoriaId: number;
  pessoaId: number;
}

export const listarTransacoes = (): Promise<Transacao[]> =>
  client.get<Transacao[]>("/api/transacoes").then((r) => r.data);

export const criarTransacao = (data: TransacaoInput): Promise<Transacao> =>
  client.post<Transacao>("/api/transacoes", data).then((r) => r.data);

export const deletarTransacao = (id: number): Promise<void> =>
  client.delete(`/api/transacoes/${id}`).then(() => undefined);
