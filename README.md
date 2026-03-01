# 📦 LumenEstoque API

## 📌 Sobre o Projeto

API RESTful desenvolvida em .NET 8 para gerenciamento de estoque.

O projeto foi construído com foco em:

- Organização em camadas
- Autenticação segura com JWT
- Separação de responsabilidades
- Estrutura preparada para evolução futura

---

## 🚀 Tecnologias Utilizadas

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- MySQL
- ASP.NET Identity
- JWT Bearer Authentication
- Swagger (OpenAPI)
- DataAnnotations
- Middleware global de tratamento de exceções

---

## 🧱 Arquitetura

LumenEstoque
│
├── Context  
├── Controllers  
├── DTOs  
├── Enums  
├── Migrations  
├── Models  
├── Pagination  
├── Services  
├── Validations  
├── GlobalExceptionMiddleware.cs  
└── Program.cs  

### Padrões Aplicados

- Controller → Service
- DTO Pattern
- Paginação customizada (PagedList<T>)
- Middleware global de exceções
- Autorização baseada em roles

---

## 🔐 Autenticação e Autorização

A API utiliza:

- JWT Bearer
- ASP.NET Identity
- Controle de acesso por roles

### Roles disponíveis

- Admin
- User

### Funcionalidades

- Registro de usuário
- Login com geração de token JWT
- Atribuição de usuário a roles
- Proteção de endpoints com [Authorize]

---

## 📦 Funcionalidades do Módulo de Estoque

- CRUD completo de produtos
- Busca por ID
- Busca por SKU
- Paginação de resultados
- Filtro por estoque mínimo
- Ativação / Desativação de produto
- Controle de acesso por perfil

---

## 📊 Paginação

A API implementa paginação customizada.

Metadados são retornados via Header: X-Pagination

Exemplo:

{
  "totalCount": 100,
  "pageSize": 10,
  "currentPage": 1,
  "totalPages": 10,
  "hasNext": true,
  "hasPrevious": false
}

---

## 🗄 Banco de Dados

- MySQL
- Migrations via Entity Framework Core

### Atualizar banco

dotnet ef database update

---

## ▶️ Como Executar

1. Clonar o repositório

git clone https://github.com/DennisOSilva/LumenEstoqueAPI.git

2. Configurar conexão

Editar appsettings.json com sua string do MySQL.

3. Aplicar migrations

dotnet ef database update

4. Executar

dotnet run

5. Acessar Swagger

https://localhost:{porta}/swagger

---

## 🎯 Objetivo

Projeto desenvolvido com foco em:

- Consolidar conhecimentos em ASP.NET Core
- Aplicar autenticação segura com JWT
- Estruturar API seguindo boas práticas de mercado

---

## 🔮 Melhorias Futuras

- Seed automático de Admin
- Deploy em nuvem
- Testes automatizados
- Dockerização
- Logs estruturados
