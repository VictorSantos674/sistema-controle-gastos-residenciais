import axios from "axios";

const client = axios.create({
  baseURL: import.meta.env.VITE_API_URL ?? "",
  headers: { "Content-Type": "application/json" },
});

client.interceptors.response.use(
  (res) => res,
  (err) => {
    const mensagem =
      err?.response?.data?.mensagem ??
      (err?.response ? `Erro ${err.response.status}` : "Sem conexão com o servidor.");
    return Promise.reject(new Error(mensagem));
  }
);

export default client;
