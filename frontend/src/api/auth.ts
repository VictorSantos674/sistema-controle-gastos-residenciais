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
  client.post<AuthResponse>("/api/auth/registrar", data, { skipAuthRefresh: true } as never).then((r) => r.data);

export const login = (data: AuthInput): Promise<AuthResponse> =>
  client.post<AuthResponse>("/api/auth/login", data, { skipAuthRefresh: true } as never).then((r) => r.data);

export const refresh = (): Promise<AuthResponse> =>
  client.post<AuthResponse>("/api/auth/refresh", undefined, { skipAuthRefresh: true } as never).then((r) => r.data);

export const logout = (): Promise<void> =>
  client.post("/api/auth/logout", undefined, { skipAuthRefresh: true } as never).then(() => undefined);
