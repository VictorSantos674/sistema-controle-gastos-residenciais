# Sistema de Controle de Gastos Residenciais

Sistema web para controle de gastos residenciais com autenticação por usuário, isolamento de dados por perfil, relatórios financeiros e exportação em PDF. Composto por uma Web API em C#/.NET e um front-end em React com TypeScript.

## Tecnologias

| Camada     | Tecnologia                                                      |
|------------|-----------------------------------------------------------------|
| Back-end   | C# / ASP.NET Core 10 Web API                                    |
| Banco      | SQLite via Entity Framework Core (`EnsureCreated`)              |
| Auth       | JWT Bearer (`Microsoft.AspNetCore.Authentication.JwtBearer`)    |
| Segurança  | BCrypt (`BCrypt.Net-Next`) para hash de senhas                  |
| Front-end  | React 18 + TypeScript + Vite                                    |
| UI         | Tailwind CSS v4 + shadcn/ui + lucide-react                      |
| HTTP       | Axios (com interceptors para JWT e redirecionamento 401)        |
| Roteamento | React Router v6                                                 |
| PDF        | jsPDF + jspdf-autotable                                         |

## Estrutura do projeto

```text
sistema-controle-gastos-residenciais/
├── backend/
│   └── GastosResidenciais.Api/
│       ├── Controllers/      # Endpoints RESTful
│       ├── Data/             # DbContext (SQLite)
│       ├── DTOs/             # Objetos de transferência de dados
│       ├── Models/           # Entidades de domínio + Enums
│       ├── Services/         # Regras de negócio + AuthService
│       ├── Dockerfile        # Build para deploy (Railway)
│       └── Program.cs        # Configuração da aplicação + JWT
└── frontend/
    ├── src/
    │   ├── api/              # Módulos de acesso à API (Axios)
    │   ├── components/       # Layout, navegação e ProtectedRoute
    │   ├── contexts/         # AuthContext (token JWT + estado de sessão)
    │   ├── hooks/            # useDarkMode
    │   ├── pages/            # Páginas por funcionalidade
    │   └── types/            # Tipos TypeScript compartilhados
    └── vercel.json           # Rewrite para SPA routing no Vercel
```

## Variáveis de ambiente

### Variáveis do back-end

| Variável          | Descrição                                               | Obrigatória em prod |
|-------------------|---------------------------------------------------------|---------------------|
| `JWT_SECRET`      | Chave secreta para assinar tokens JWT (mínimo 32 chars) | Sim                 |
| `ASPNETCORE_URLS` | URL de escuta da API (ex: `http://+:8080`)              | Sim (Railway)       |

> Em desenvolvimento, se `JWT_SECRET` não estiver definida, é usada uma chave padrão insegura. **Nunca use o valor padrão em produção.**

### Variáveis do front-end

| Variável       | Descrição                                          | Obrigatória em prod |
|----------------|----------------------------------------------------|---------------------|
| `VITE_API_URL` | URL base da API (ex: `https://api.up.railway.app`) | Sim                 |

> Variáveis `VITE_*` são embutidas no bundle no momento do build. Defina-as antes de buildar (Vercel: Settings → Environment Variables).

## Como executar localmente

### Back-end

```bash
cd backend/GastosResidenciais.Api
dotnet run --launch-profile http
```

A API estará disponível em `http://localhost:5000`. O banco de dados `gastos.db` é criado automaticamente na primeira execução.

### Front-end

```bash
cd frontend
npm install
npm run dev
```

O front-end estará disponível em `http://localhost:5173`.

> O Vite proxy redireciona `/api/*` para `http://localhost:5000`, eliminando problemas de CORS em desenvolvimento.

## Deploy

| Serviço | Camada     | Root directory                   |
|---------|------------|----------------------------------|
| Railway | Back-end   | `backend/GastosResidenciais.Api` |
| Vercel  | Front-end  | `frontend/`                      |

O back-end usa um `Dockerfile` customizado (sem Nixpacks) para evitar erros de build secret no Railway. O front-end usa `vercel.json` com rewrite universal para suportar SPA routing.

## Funcionalidades

### Autenticação

- Cadastro com login e senha (mínimo 8 caracteres, letras maiúsculas/minúsculas, número e caractere especial).
- Login com geração de token JWT (validade de 7 dias).
- Cada usuário acessa apenas seus próprios dados (isolamento completo por perfil).
- Token armazenado no `localStorage`; requisições protegidas por interceptor Axios.
- Redirecionamento automático para `/login` em caso de token expirado ou inválido (401).

### Pessoas (CRUD completo)

- Criar, editar, deletar e listar pessoas.
- Ao deletar uma pessoa, todas as transações vinculadas são removidas automaticamente (cascade).

### Categorias (Criar e Listar)

- Cada categoria possui uma **finalidade**: `Despesa`, `Receita` ou `Ambas`.
- A finalidade determina quais tipos de transação podem usar a categoria.

### Transações (Criar e Listar)

- **Menores de 18 anos**: somente transações do tipo `Despesa` são aceitas.
- **Compatibilidade de categoria**: o tipo da transação deve ser compatível com a finalidade da categoria escolhida.
  - `Despesa` → categorias com finalidade `Despesa` ou `Ambas`.
  - `Receita` → categorias com finalidade `Receita` ou `Ambas`.
- Campo de descrição opcional; campos obrigatórios sinalizados com asterisco e destacados em caso de erro.

### Relatórios

- **Por Pessoa**: total de receitas, despesas e saldo por pessoa + totais gerais.
- **Por Categoria**: total de receitas, despesas e saldo por categoria + totais gerais.
- **Exportação em PDF**: gera um relatório personalizado por usuário via download direto no navegador.

## Endpoints da API

### Endpoints de autenticação

| Método | Rota                    | Descrição                          | Auth |
|--------|-------------------------|------------------------------------|------|
| POST   | /api/auth/registrar     | Cria novo usuário e retorna token  | Não  |
| POST   | /api/auth/login         | Autentica usuário e retorna token  | Não  |

### Pessoas

| Método | Rota                | Descrição                        | Auth |
|--------|---------------------|----------------------------------|------|
| GET    | /api/pessoas        | Lista pessoas do usuário         | Sim  |
| GET    | /api/pessoas/{id}   | Obtém pessoa por ID              | Sim  |
| POST   | /api/pessoas        | Cria nova pessoa                 | Sim  |
| PUT    | /api/pessoas/{id}   | Edita pessoa                     | Sim  |
| DELETE | /api/pessoas/{id}   | Deleta pessoa (+ transações)     | Sim  |

### Categorias

| Método | Rota                  | Descrição                        | Auth |
|--------|-----------------------|----------------------------------|------|
| GET    | /api/categorias       | Lista categorias do usuário      | Sim  |
| POST   | /api/categorias       | Cria nova categoria              | Sim  |

### Transações

| Método | Rota                | Descrição                        | Auth |
|--------|---------------------|----------------------------------|------|
| GET    | /api/transacoes     | Lista transações do usuário      | Sim  |
| POST   | /api/transacoes     | Cria nova transação              | Sim  |

### Endpoints de relatórios

| Método | Rota                          | Descrição                         | Auth |
|--------|-------------------------------|-----------------------------------|------|
| GET    | /api/relatorios/por-pessoa    | Relatório de totais por pessoa    | Sim  |
| GET    | /api/relatorios/por-categoria | Relatório de totais por categoria | Sim  |
