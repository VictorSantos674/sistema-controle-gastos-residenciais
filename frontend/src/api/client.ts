import axios, { type AxiosError, type InternalAxiosRequestConfig } from "axios";

interface AuthSession {
  token: string;
  login: string;
}

interface RetriableRequestConfig extends InternalAxiosRequestConfig {
  _retry?: boolean;
  skipAuthRefresh?: boolean;
}

let accessToken: string | null = null;
let refreshPromise: Promise<AuthSession> | null = null;
let onSessionChanged: ((session: AuthSession | null) => void) | null = null;

const client = axios.create({
  baseURL: import.meta.env.VITE_API_URL ?? "",
  headers: { "Content-Type": "application/json" },
  withCredentials: true,
});

export function setAccessToken(token: string | null) {
  accessToken = token;
}

export function setSessionChangeHandler(handler: ((session: AuthSession | null) => void) | null) {
  onSessionChanged = handler;
}

async function refreshSession(): Promise<AuthSession> {
  if (!refreshPromise) {
    refreshPromise = client
      .post<AuthSession>("/api/auth/refresh", undefined, { skipAuthRefresh: true } as RetriableRequestConfig)
      .then((response) => {
        setAccessToken(response.data.token);
        onSessionChanged?.(response.data);
        return response.data;
      })
      .finally(() => {
        refreshPromise = null;
      });
  }

  return refreshPromise;
}

client.interceptors.request.use((config) => {
  if (accessToken) config.headers.Authorization = `Bearer ${accessToken}`;
  return config;
});

client.interceptors.response.use(
  (res) => res,
  async (err: AxiosError<{ mensagem?: string }>) => {
    const original = err.config as RetriableRequestConfig | undefined;

    if (err.response?.status === 401 && original && !original._retry && !original.skipAuthRefresh) {
      original._retry = true;

      try {
        const session = await refreshSession();
        original.headers.Authorization = `Bearer ${session.token}`;
        return client(original);
      } catch {
        setAccessToken(null);
        onSessionChanged?.(null);
        window.location.href = "/login";
      }
    }

    const mensagem =
      err.response?.data?.mensagem ??
      (err.response ? `Erro ${err.response.status}` : "Sem conexão com o servidor.");
    return Promise.reject(new Error(mensagem));
  }
);

export default client;
