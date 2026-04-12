import client from "./client";

export interface AuthInput {
  login: string;
  senha: string;
}

export interface AuthResponse {
  token: string;
  login: string;
}

export const registrar = (data: AuthInput): Promise<AuthResponse> =>
  client.post<AuthResponse>("/api/auth/registrar", data).then((r) => r.data);

export const login = (data: AuthInput): Promise<AuthResponse> =>
  client.post<AuthResponse>("/api/auth/login", data).then((r) => r.data);
