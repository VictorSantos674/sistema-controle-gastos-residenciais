# Sistema de Controle de Gastos Residenciais

Sistema web para controle de gastos residenciais com autenticação por usuário, isolamento de dados por perfil, relatórios financeiros e exportação em PDF. Composto por uma Web API em C#/.NET e um front-end em React com TypeScript.

## Tecnologias

| Camada | Tecnologia |
|--------|------------|
| Back-end | ASP.NET Core 10 Web API |
| Banco | PostgreSQL via Entity Framework Core migrations |
| Auth | JWT Bearer + BCrypt |
| Segurança | Rate limiter nativo para endpoints de autenticação |
| Front-end | React 18 + TypeScript + Vite |
| UI | Tailwind CSS v4 + shadcn/ui + lucide-react |
| HTTP | Axios com interceptor JWT |
| PDF | jsPDF + jspdf-autotable |

## Estrutura do projeto

```text
backend/
  GastosResidenciais.Api/
    Controllers/      # Endpoints REST
    Data/             # AppDbContext + factory design-time
    DTOs/             # DTOs de entrada/saída
    Migrations/       # Migration inicial PostgreSQL
    Models/           # Entidades e enums
    Services/         # Regras de negócio
  GastosResidenciais.Tests/
    *.cs              # Testes xUnit unitários e integração
frontend/
  src/
    api/              # Cliente Axios por recurso
    components/       # Layout, ProtectedRoute e UI
    contexts/         # AuthContext
    pages/            # Telas da aplicação
    types/            # Tipos TypeScript
```

## Variáveis de ambiente

### Back-end

| Variável | Descrição | Obrigatória em prod |
|----------|-----------|---------------------|
| `DATABASE_URL` | URL PostgreSQL no formato Railway, ex. `postgresql://user:pass@host:5432/db` | Sim |
| `JWT_SECRET` | Chave secreta para assinar tokens JWT, mínimo 32 caracteres | Sim |
| `PORT` | Porta injetada pelo Railway | Sim no Railway |
| `CORS_ORIGINS` | Origens permitidas separadas por vírgula | Sim |

Em desenvolvimento, se `DATABASE_URL` não existir, a API usa `ConnectionStrings:Default` do `appsettings.json`. Se `JWT_SECRET` não existir, há fallback inseguro apenas para desenvolvimento.

### Front-end

| Variável | Descrição | Obrigatória em prod |
|----------|-----------|---------------------|
| `VITE_API_URL` | URL base da API, ex. `https://api.up.railway.app` | Sim |

## Banco e migrations

A aplicação usa PostgreSQL com EF Core migrations. No startup, o `Program.cs` executa `db.Database.MigrateAsync()` para aplicar migrations pendentes automaticamente, o que atende o deploy no Railway sem rodar comandos manuais no container.

Comandos úteis:

```bash
cd backend/GastosResidenciais.Api
dotnet ef migrations add InitialCreate
dotnet ef database update
```

Para produção no Railway, configure `DATABASE_URL`, `JWT_SECRET` e `CORS_ORIGINS`. O health check pode ser monitorado em:

```text
GET /health
```

## Como executar localmente

### Back-end

```bash
cd backend/GastosResidenciais.Api
dotnet restore
dotnet run --launch-profile http
```

A API sobe em `http://localhost:5000` por padrão.

### Front-end

```bash
cd frontend
npm install
npm run dev
```

O front-end fica em `http://localhost:5173`.

### Testes

```bash
dotnet test sistema-controle-gastos-residenciais.sln
npm run build --prefix frontend
```

## Deploy

| Serviço | Camada | Root directory |
|---------|--------|----------------|
| Railway | Back-end | `backend/GastosResidenciais.Api` |
| Vercel | Front-end | `frontend/` |

O back-end usa o `Dockerfile` do projeto da API. As migrations são aplicadas no startup via `MigrateAsync()`.

## Funcionalidades

- Autenticação com registro/login, JWT no `localStorage` e redirecionamento em 401.
- Rate limiting em `POST /api/auth/login` e `POST /api/auth/registrar`: 5 tentativas por IP por minuto, com `429` e header `Retry-After`.
- CRUD de pessoas.
- CRUD de categorias, com bloqueio de deleção quando houver transações vinculadas.
- CRUD parcial de transações: criar, editar, deletar e listar com paginação.
- Dashboard com resumo em chamada única.
- Relatórios por pessoa e categoria com filtros aplicados no SQL.
- Exportação de relatórios em PDF.

## Endpoints principais

| Método | Rota | Descrição | Auth |
|--------|------|-----------|------|
| POST | `/api/auth/registrar` | Cria usuário e retorna token | Não |
| POST | `/api/auth/login` | Autentica usuário e retorna token | Não |
| GET | `/api/pessoas` | Lista pessoas | Sim |
| POST | `/api/pessoas` | Cria pessoa | Sim |
| PUT | `/api/pessoas/{id}` | Edita pessoa | Sim |
| DELETE | `/api/pessoas/{id}` | Remove pessoa e suas transações | Sim |
| GET | `/api/categorias` | Lista categorias | Sim |
| POST | `/api/categorias` | Cria categoria | Sim |
| PUT | `/api/categorias/{id}` | Edita categoria | Sim |
| DELETE | `/api/categorias/{id}` | Remove categoria, se não houver transações | Sim |
| GET | `/api/transacoes?page=1&pageSize=20` | Lista transações paginadas | Sim |
| POST | `/api/transacoes` | Cria transação | Sim |
| PUT | `/api/transacoes/{id}` | Edita transação | Sim |
| DELETE | `/api/transacoes/{id}` | Remove transação | Sim |
| GET | `/api/dashboard/resumo` | Resumo financeiro do dashboard | Sim |
| GET | `/api/relatorios/por-pessoa` | Totais por pessoa, com `mes`/`ano` opcionais | Sim |
| GET | `/api/relatorios/por-categoria` | Totais por categoria, com `mes`/`ano` opcionais | Sim |
| GET | `/health` | Health check da API e banco | Não |
