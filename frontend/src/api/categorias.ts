import { Categoria } from "../types";
import client from "./client";

export interface CategoriaInput {
  descricao: string;
  finalidade: number;
}

export const listarCategorias = (): Promise<Categoria[]> =>
  client.get<Categoria[]>("/api/categorias").then((r) => r.data);

export const criarCategoria = (data: CategoriaInput): Promise<Categoria> =>
  client.post<Categoria>("/api/categorias", data).then((r) => r.data);
