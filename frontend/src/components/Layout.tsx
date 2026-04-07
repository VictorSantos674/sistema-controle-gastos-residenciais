import { NavLink, Outlet } from "react-router-dom";

const navLinks = [
  { to: "/dashboard", label: "Dashboard", icon: "⊞" },
  { to: "/pessoas", label: "Pessoas", icon: "◎" },
  { to: "/categorias", label: "Categorias", icon: "◈" },
  { to: "/transacoes", label: "Transações", icon: "◉" },
  { to: "/relatorios", label: "Relatórios", icon: "◫" },
];

export default function Layout() {
  return (
    <div className="app-shell">
      <aside className="sidebar">
        <div className="sidebar-brand">
          <span className="sidebar-brand-accent">Gastos</span> Residenciais
        </div>
        <nav className="sidebar-nav">
          {navLinks.map((link) => (
            <NavLink
              key={link.to}
              to={link.to}
              className={({ isActive }) => `sidebar-link${isActive ? " active" : ""}`}
            >
              <span className="sidebar-icon">{link.icon}</span>
              {link.label}
            </NavLink>
          ))}
        </nav>
      </aside>
      <main className="main-content">
        <Outlet />
      </main>
    </div>
  );
}
