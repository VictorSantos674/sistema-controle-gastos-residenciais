# Sistema de Controle de Gastos Residenciais

Sistema web para controle de gastos residenciais, composto por uma Web API em C#/.NET e um front-end em React com TypeScript.

## Tecnologias

| Camada    | Tecnologia                                        |
|-----------|---------------------------------------------------|
| Back-end  | C# / ASP.NET Core 10 Web API                      |
| Banco     | SQLite via Entity Framework Core (EnsureCreated)  |
| Front-end | React 18 + TypeScript + Vite 4                    |
| HTTP      | Axios                                             |
| Roteamento| React Router v6                                   |

## Estrutura do projeto

```text
sistema-controle-gastos-residenciais/
├── backend/
│   └── GastosResidenciais.Api/
│       ├── Controllers/      # Endpoints RESTful
│       ├── Data/             # DbContext (SQLite)
│       ├── DTOs/             # Objetos de transferência de dados
│       ├── Models/           # Entidades de domínio + Enums
│       ├── Services/         # Regras de negócio
│       └── Program.cs        # Configuração da aplicação
└── frontend/
    └── src/
        ├── api/              # Módulos de acesso à API (Axios)
        ├── components/       # Layout e navegação
        ├── pages/            # Páginas por funcionalidade
        └── types/            # Tipos TypeScript compartilhados
```

## Como executar

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

## Funcionalidades

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
- A listagem exibe o nome da pessoa e a descrição da categoria.

### Relatórios

- **Por Pessoa**: total de receitas, despesas e saldo por pessoa + totais gerais.
- **Por Categoria**: total de receitas, despesas e saldo por categoria + totais gerais.

## Endpoints da API

| Método | Rota                          | Descrição                         |
|--------|-------------------------------|-----------------------------------|
| GET    | /api/pessoas                  | Lista todas as pessoas            |
| GET    | /api/pessoas/{id}             | Obtém pessoa por ID               |
| POST   | /api/pessoas                  | Cria nova pessoa                  |
| PUT    | /api/pessoas/{id}             | Edita pessoa                      |
| DELETE | /api/pessoas/{id}             | Deleta pessoa (+ transações)      |
| GET    | /api/categorias               | Lista todas as categorias         |
| POST   | /api/categorias               | Cria nova categoria               |
| GET    | /api/transacoes               | Lista todas as transações         |
| POST   | /api/transacoes               | Cria nova transação               |
| GET    | /api/relatorios/por-pessoa    | Relatório de totais por pessoa    |
| GET    | /api/relatorios/por-categoria | Relatório de totais por categoria |
