# TicketPrime

Sistema de venda e vitrine de eventos musicais com API em .NET 9, banco SQLite e interface em Blazor.

## O que o projeto entrega hoje

- cadastro de cliente com `CPF`, `nome`, `email` e `senha`
- login do cliente com `email ou CPF + senha`
- acesso administrativo no mesmo site
- painel ADM para cadastrar, listar, editar e excluir eventos
- cadastro de cupons
- vitrine visual de eventos com imagens
- eventos iniciais cadastrados automaticamente para demonstracao
- queries parametrizadas com Dapper nas operacoes principais
- testes automatizados com xUnit e `Assert`

## Estrutura da solucao

- `src/TicketPrime.API`: backend da aplicacao
- `src/TicketPrime.App`: interface Blazor
- `tests/TicketPrime.Tests`: testes automatizados
- `docs/requisitos.md`: requisitos atualizados do sistema

## Como rodar

Abra dois terminais.

### 1. Subir a API

```powershell
dotnet run --project "C:\Users\rapha\OneDrive\Área de Trabalho\Curso_Video\TicketPrime (1)\TicketPrime\src\TicketPrime.API\TicketPrime.API.csproj"
```

API disponivel em:

- `http://localhost:5238`

### 2. Subir o Blazor

```powershell
dotnet run --project "C:\Users\rapha\OneDrive\Área de Trabalho\Curso_Video\TicketPrime (1)\TicketPrime\src\TicketPrime.App\TicketPrime.App.csproj"
```

Site disponivel em:

- `http://localhost:5062`

## Acessos para demonstracao

### Cliente

O cliente pode:

- criar conta com `CPF`, `nome`, `email` e `senha`
- fazer login com `email ou CPF`
- acessar a pagina de eventos

### Administrador

O acesso ADM fica no mesmo formulario de login do cliente.

Credenciais:

- login: `admin`
- senha: `admin`

Ao entrar com esse acesso, o sistema libera o painel administrativo.

## Funcionalidades do painel ADM

- cadastrar novo evento
- listar eventos cadastrados
- editar evento existente
- excluir evento
- cadastrar cupom

## Endpoints principais da API

### Status

- `GET /`

### Autenticacao e usuarios

- `POST /api/auth/usuarios/cadastro`
- `POST /api/auth/usuarios/login`
- `POST /api/auth/admin/login`
- `GET /api/usuarios`

### Eventos

- `GET /api/eventos`
- `POST /api/eventos`
- `GET /api/admin/eventos`
- `PUT /api/admin/eventos/{id}`
- `DELETE /api/admin/eventos/{id}`

### Cupons

- `POST /api/cupons`

## Exemplo de cadastro de cliente

```json
{
  "Cpf": "12345678900",
  "Nome": "Raphael",
  "Email": "raphael@email.com",
  "Senha": "123456"
}
```

Endpoint:

```http
POST http://localhost:5238/api/auth/usuarios/cadastro
```

## Exemplo de login de cliente

```json
{
  "Login": "raphael@email.com",
  "Senha": "123456"
}
```

Endpoint:

```http
POST http://localhost:5238/api/auth/usuarios/login
```

## Exemplo de cadastro de evento

Para operacoes administrativas via API, envie o cabecalho:

```http
X-Admin-Token: ticketprime-admin-token
```

Exemplo de corpo:

```json
{
  "Nome": "Show de Rock",
  "CapacidadeTotal": 5000,
  "DataEvento": "2026-05-01T20:00:00",
  "PrecoPadrao": 150.00,
  "ImagemUrl": "https://images.unsplash.com/photo-1501386761578-eac5c94b800a?auto=format&fit=crop&w=1200&q=80"
}
```

Endpoint:

```http
POST http://localhost:5238/api/eventos
```

## Testes

Para executar os testes:

```powershell
dotnet test "C:\Users\rapha\OneDrive\Área de Trabalho\Curso_Video\TicketPrime (1)\TicketPrime\tests\TicketPrime.Tests\TicketPrime.Tests.csproj" --no-restore
```

Os testes cobrem regras como:

- hash de senha
- credencial administrativa
- validacao de cadastro
- validacao de evento
- validacao de cupom

## Observacoes tecnicas

- o banco utilizado e SQLite
- as senhas dos clientes sao armazenadas como hash SHA-256
- as operacoes principais usam Dapper com queries parametrizadas
- isso reduz o risco de SQL Injection nas rotas implementadas
- eventos demo sao inseridos automaticamente quando a tabela `Eventos` esta vazia

## Documentacao complementar

- requisitos atualizados: `docs/requisitos.md`
