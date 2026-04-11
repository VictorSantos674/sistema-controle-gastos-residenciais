import { useEffect, useState } from "react";

/**
 * Hook para gerenciar o tema claro/escuro da aplicação.
 *
 * - Persiste a preferência do usuário no localStorage.
 * - Respeita a preferência do sistema operacional na primeira visita.
 * - Aplica/remove a classe "dark" no elemento <html> para acionar o
 *   modo escuro do Tailwind CSS (class-based dark mode).
 */
export function useDarkMode() {
  const [isDark, setIsDark] = useState<boolean>(() => {
    // Lê preferência salva; se não houver, usa a preferência do SO
    const stored = localStorage.getItem("theme");
    if (stored !== null) return stored === "dark";
    return window.matchMedia("(prefers-color-scheme: dark)").matches;
  });

  useEffect(() => {
    const root = document.documentElement;
    if (isDark) {
      root.classList.add("dark");
      localStorage.setItem("theme", "dark");
    } else {
      root.classList.remove("dark");
      localStorage.setItem("theme", "light");
    }
  }, [isDark]);

  const toggle = () => setIsDark((prev) => !prev);

  return { isDark, toggle };
}
