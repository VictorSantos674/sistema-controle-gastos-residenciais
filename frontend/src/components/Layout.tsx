import { NavLink, Outlet } from "react-router-dom";

const navLinks = [
  { to: "/pessoas", label: "Pessoas" },
  { to: "/categorias", label: "Categorias" },
  { to: "/transacoes", label: "Transações" },
  { to: "/relatorios", label: "Relatórios" },
];

export default function Layout() {
  return (
    <div style={{ fontFamily: "sans-serif", minHeight: "100vh" }}>
      <nav
        style={{
          background: "#1a1a2e",
          padding: "0 24px",
          display: "flex",
          alignItems: "center",
          gap: 4,
        }}
      >
        <span
          style={{
            color: "#e94560",
            fontWeight: 700,
            fontSize: 18,
            marginRight: 24,
            padding: "16px 0",
          }}
        >
          Gastos Residenciais
        </span>
        {navLinks.map((link) => (
          <NavLink
            key={link.to}
            to={link.to}
            style={({ isActive }) => ({
              color: isActive ? "#e94560" : "#ccc",
              textDecoration: "none",
              padding: "16px 12px",
              fontWeight: isActive ? 700 : 400,
              borderBottom: isActive ? "2px solid #e94560" : "2px solid transparent",
            })}
          >
            {link.label}
          </NavLink>
        ))}
      </nav>
      <main style={{ padding: 32, maxWidth: 1100, margin: "0 auto" }}>
        <Outlet />
      </main>
    </div>
  );
}
