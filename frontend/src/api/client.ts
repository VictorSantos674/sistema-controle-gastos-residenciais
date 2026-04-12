import axios from "axios";

const STORAGE_KEY = "gastos_auth";

const client = axios.create({
  baseURL: import.meta.env.VITE_API_URL ?? "",
  headers: { "Content-Type": "application/json" },
});

/// Injeta o token JWT em todas as requisições, se disponível.
client.interceptors.request.use((config) => {
  try {
    const stored = localStorage.getItem(STORAGE_KEY);
    if (stored) {
      const { token } = JSON.parse(stored) as { token: string };
      if (token) config.headers.Authorization = `Bearer ${token}`;
    }
  } catch {
    // localStorage indisponível ou JSON inválido — ignora silenciosamente
  }
  return config;
});

/// Trata erros de resposta: extrai mensagem amigável e redireciona para /login em caso de 401.
client.interceptors.response.use(
  (res) => res,
  (err) => {
    if (err?.response?.status === 401) {
      localStorage.removeItem(STORAGE_KEY);
      window.location.href = "/login";
    }
    const mensagem =
      err?.response?.data?.mensagem ??
      (err?.response ? `Erro ${err.response.status}` : "Sem conexão com o servidor.");
    return Promise.reject(new Error(mensagem));
  }
);

export default client;
