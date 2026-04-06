import { RelatorioPorCategoria, RelatorioPorPessoa } from "../types";
import client from "./client";

export const relatorioPorPessoa = (): Promise<RelatorioPorPessoa> =>
  client.get<RelatorioPorPessoa>("/api/relatorios/por-pessoa").then((r) => r.data);

export const relatorioPorCategoria = (): Promise<RelatorioPorCategoria> =>
  client.get<RelatorioPorCategoria>("/api/relatorios/por-categoria").then((r) => r.data);
