import { BarChart3, FolderOpen, Home, LayoutList, LogOut, Moon, Sun, Users } from "lucide-react";
import { NavLink, Outlet } from "react-router-dom";
import { useAuth } from "../contexts/AuthContext";
import { useDarkMode } from "../hooks/useDarkMode";
import { cn } from "../lib/utils";

const navLinks = [
  { to: "/dashboard",  label: "Dashboard",   Icon: Home        },
  { to: "/pessoas",    label: "Pessoas",      Icon: Users       },
  { to: "/categorias", label: "Categorias",   Icon: FolderOpen  },
  { to: "/transacoes", label: "Transações",   Icon: LayoutList  },
  { to: "/relatorios", label: "Relatórios",   Icon: BarChart3   },
];

export default function Layout() {
  const { isDark, toggle } = useDarkMode();
  const { user, logout }   = useAuth();

  return (
    <div className="flex min-h-screen bg-gray-100 dark:bg-gray-900">
      {/* ── Sidebar ─────────────────────────────── */}
      <aside className="fixed inset-y-0 left-0 z-50 flex w-56 flex-col bg-slate-900">
        {/* Brand */}
        <div className="border-b border-slate-700/60 px-5 py-5">
          <span className="text-sm font-bold leading-tight text-white">
            <span className="text-blue-400">Gastos</span> Residenciais
          </span>
        </div>

        {/* Nav links */}
        <nav className="flex flex-1 flex-col gap-0.5 px-2 py-3">
          {navLinks.map(({ to, label, Icon }) => (
            <NavLink
              key={to}
              to={to}
              className={({ isActive }) =>
                cn(
                  "flex items-center gap-3 rounded-md px-3 py-2.5 text-sm font-medium transition-colors",
                  isActive
                    ? "bg-blue-600 text-white"
                    : "text-slate-400 hover:bg-slate-800 hover:text-slate-100"
                )
              }
            >
              <Icon size={16} className="shrink-0" />
              {label}
            </NavLink>
          ))}
        </nav>

        {/* Footer — usuário logado + tema + logout */}
        <div className="border-t border-slate-700/60 px-4 py-3 space-y-2">
          {user && (
            <p className="truncate text-xs font-medium text-slate-300" title={user.login}>
              {user.login}
            </p>
          )}
          <div className="flex items-center justify-between">
            <p className="text-xs text-slate-500">v1.0.0</p>
            <div className="flex items-center gap-1">
              <button
                onClick={toggle}
                title={isDark ? "Modo claro" : "Modo escuro"}
                className="rounded-md p-1.5 text-slate-400 transition-colors hover:bg-slate-800 hover:text-slate-100"
              >
                {isDark ? <Sun size={15} /> : <Moon size={15} />}
              </button>
              <button
                onClick={logout}
                title="Sair"
                className="rounded-md p-1.5 text-slate-400 transition-colors hover:bg-slate-800 hover:text-red-400"
              >
                <LogOut size={15} />
              </button>
            </div>
          </div>
        </div>
      </aside>

      {/* ── Main content ────────────────────────── */}
      <main className="ml-56 flex-1 p-8">
        <Outlet />
      </main>
    </div>
  );
}
