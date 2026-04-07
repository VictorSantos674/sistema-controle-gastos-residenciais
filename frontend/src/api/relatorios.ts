import { RelatorioPorCategoria, RelatorioPorPessoa } from "../types";
import client from "./client";

export interface FiltroRelatorio {
  mes?: number;
  ano?: number;
}

export const relatorioPorPessoa = (filtro?: FiltroRelatorio): Promise<RelatorioPorPessoa> =>
  client.get<RelatorioPorPessoa>("/api/relatorios/por-pessoa", { params: filtro }).then((r) => r.data);

export const relatorioPorCategoria = (filtro?: FiltroRelatorio): Promise<RelatorioPorCategoria> =>
  client.get<RelatorioPorCategoria>("/api/relatorios/por-categoria", { params: filtro }).then((r) => r.data);
