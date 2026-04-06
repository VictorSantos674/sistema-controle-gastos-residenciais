import { Pessoa } from "../types";
import client from "./client";

export interface PessoaInput {
  nome: string;
  idade: number;
}

export const listarPessoas = (): Promise<Pessoa[]> =>
  client.get<Pessoa[]>("/api/pessoas").then((r) => r.data);

export const criarPessoa = (data: PessoaInput): Promise<Pessoa> =>
  client.post<Pessoa>("/api/pessoas", data).then((r) => r.data);

export const editarPessoa = (id: number, data: PessoaInput): Promise<Pessoa> =>
  client.put<Pessoa>(`/api/pessoas/${id}`, data).then((r) => r.data);

export const deletarPessoa = (id: number): Promise<void> =>
  client.delete(`/api/pessoas/${id}`).then(() => undefined);
