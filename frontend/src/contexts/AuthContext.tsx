import { createContext, useContext, useEffect, useState, type ReactNode } from "react";
import { logout as apiLogout, refresh as apiRefresh } from "../api/auth";
import { setAccessToken, setSessionChangeHandler } from "../api/client";

interface AuthUser {
  login: string;
  token: string;
}

interface AuthContextValue {
  user: AuthUser | null;
  login: (token: string, login: string) => void;
  logout: () => Promise<void>;
  isAuthenticated: boolean;
  loading: boolean;
}

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    setSessionChangeHandler((session) => {
      setUser(session ? { token: session.token, login: session.login } : null);
    });

    apiRefresh()
      .then((session) => {
        setAccessToken(session.token);
        setUser({ token: session.token, login: session.login });
      })
      .catch(() => {
        setAccessToken(null);
        setUser(null);
      })
      .finally(() => setLoading(false));

    return () => setSessionChangeHandler(null);
  }, []);

  const login = (token: string, loginStr: string) => {
    setAccessToken(token);
    setUser({ token, login: loginStr });
  };

  const logout = async () => {
    try {
      await apiLogout();
    } finally {
      setAccessToken(null);
      setUser(null);
    }
  };

  return (
    <AuthContext.Provider value={{ user, login, logout, isAuthenticated: user !== null, loading }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth deve ser usado dentro de <AuthProvider>");
  return ctx;
}
