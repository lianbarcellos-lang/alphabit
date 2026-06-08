# GeekTop

Sistema de venda e vitrine de eventos geek/anime com API em .NET 9, banco SQLite e interface em Blazor.

## O que o projeto entrega hoje

- cadastro de cliente com `CPF`, `nome`, `email` e `senha`
- login do cliente com `email ou CPF + senha`
- acesso administrativo no mesmo site
- painel ADM para cadastrar, listar, editar e excluir eventos
- cadastro de cupons
- catalogos administrativos de cidades e categorias geek
- vitrine visual de eventos com imagens
- tipos de ingresso por evento
- atividades internas com inscrição
- convidados associados aos eventos
- mapa de stands e expositores por evento, com organização automática por grades e ajuste manual por drag/drop
- check-in administrativo com QR Code
- avaliações de eventos por clientes com reserva
- dashboard administrativo com receita, reservas, check-ins, capacidade, cupons e avaliações
- eventos iniciais cadastrados automaticamente para demonstração
- queries parametrizadas com Dapper nas operações principais
- testes automatizados com xUnit e `Assert`

## Estrutura da solução

- `src/Alphabit.API`: backend da aplicação
- `src/Alphabit.App`: interface Blazor
- `tests/Alphabit.Tests`: testes automatizados
- `docs/requisitos.md`: requisitos atualizados do sistema

## Como rodar

Todos os comandos devem ser executados a partir da raiz do projeto:

```powershell
cd "C:\Users\rapha\Downloads\Projeto\alphabit-main__tentar\alphabit-main"
```

Se o terminal estiver em `C:\Users\rapha`, os caminhos `.\src\...` não existem e o comando `dotnet run` falha com erro de projeto inexistente.

Abra dois terminais.

### 1. Subir a API

```powershell
cd "C:\Users\rapha\Downloads\Projeto\alphabit-main__tentar\alphabit-main"
dotnet run --project .\src\Alphabit.API\Alphabit.API.csproj --urls http://localhost:5248
```

API disponível em:

- `http://localhost:5248`

### 2. Subir o Blazor

```powershell
cd "C:\Users\rapha\Downloads\Projeto\alphabit-main__tentar\alphabit-main"
dotnet run --project .\src\Alphabit.App\Alphabit.App.csproj --urls http://localhost:5072
```

Site disponível em:

- `http://localhost:5072`

## Acessos para demonstração

### Cliente

O cliente pode:

- criar conta com `CPF`, `nome`, `email` e `senha`
- fazer login com `email ou CPF`
- acessar a página de eventos

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
- gerenciar cidades e categorias geek
- cadastrar atividades e convidados
- associar convidados aos eventos
- reservar/liberar stands para empresas, lojas, arenas e atrações
- organizar automaticamente os stands em grades 2x2, 3x3, 4x4, 5x5 ou 8x8 antes dos ajustes manuais
- validar check-in por QR Code
- acompanhar métricas do dashboard
- moderar avaliações recentes

## Endpoints principais da API

### Status

- `GET /`

### Autenticação e usuários

- `POST /api/auth/usuarios/cadastro`
- `POST /api/auth/usuarios/login`
- `POST /api/auth/admin/login`
- `GET /api/usuarios`

### Eventos

- `GET /api/eventos`
- `POST /api/eventos`
- `GET /api/eventos/{id}/tipos-ingresso`
- `GET /api/eventos/{id}/atividades`
- `GET /api/eventos/{id}/convidados`
- `GET /api/eventos/{id}/stands`
- `GET /api/admin/eventos`
- `PUT /api/admin/eventos/{id}`
- `DELETE /api/admin/eventos/{id}`

### Catálogos administrativos

- `GET /api/admin/generos`
- `POST /api/admin/generos`
- `PUT /api/admin/generos/{nomeAtual}`
- `DELETE /api/admin/generos/{nomeAtual}`
- `GET /api/admin/cidades`
- `POST /api/admin/cidades`
- `PUT /api/admin/cidades/{nomeAtual}`
- `DELETE /api/admin/cidades/{nomeAtual}`

### Convidados e atividades

- `GET /api/convidados`
- `POST /api/convidados`
- `POST /api/eventos/{id}/convidados`
- `DELETE /api/eventos/{id}/convidados/{convidadoId}`
- `DELETE /api/admin/convidados/{id}`
- `POST /api/atividades`
- `POST /api/atividades/{id}/inscricao`
- `DELETE /api/admin/atividades/{id}`

### Mapa de stands

- `GET /api/eventos/{id}/stands`
- `POST /api/admin/eventos/{id}/stands`
- `PUT /api/admin/eventos/{id}/mapa-imagem`
- `PUT /api/admin/eventos/{id}/stands/{standId}`
- `DELETE /api/admin/eventos/{id}/stands/{standId}`
- `POST /api/admin/eventos/{id}/stand-setores`
- `PUT /api/admin/eventos/{id}/stand-setores/{nomeAtual}`
- `DELETE /api/admin/eventos/{id}/stand-setores/{nome}`

### Reservas e check-in

- `POST /api/reservas`
- `GET /api/reservas/{cpf}`
- `POST /api/checkin`

### Avaliações

- `POST /api/avaliacoes`
- `GET /api/eventos/{id}/avaliacoes`
- `DELETE /api/admin/avaliacoes/{id}`

### Dashboard administrativo

- `GET /api/admin/vendas/dashboard`

### Cupons

- `POST /api/cupons`
- `POST /api/cupons/preview`
- `GET /api/admin/cupons`
- `PUT /api/admin/cupons/{codigo}`
- `DELETE /api/admin/cupons/{codigo}`

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
POST http://localhost:5248/api/auth/usuarios/cadastro
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
POST http://localhost:5248/api/auth/usuarios/login
```

## Exemplo de cadastro de evento

Para operações administrativas via API, envie o cabeçalho:

```http
X-Admin-Token: alphabit-admin-token
```

Exemplo de corpo:

```json
{
  "Nome": "Anime Friends Experience",
  "CapacidadeTotal": 5000,
  "DataEvento": "2026-05-01T20:00:00",
  "PrecoPadrao": 150.00,
  "ImagemUrl": "https://images.unsplash.com/photo-1501386761578-eac5c94b800a?auto=format&fit=crop&w=1200&q=80"
}
```

Endpoint:

```http
POST http://localhost:5248/api/eventos
```

## Testes

Para executar os testes:

```powershell
dotnet test .\tests\Alphabit.Tests\Alphabit.Tests.csproj --no-restore
```

Os testes cobrem regras como:

- hash de senha
- credencial administrativa
- validação de cadastro
- validação de evento
- validação de cupom
- tipos de ingresso
- atividades internas
- convidados
- mapa de stands
- check-in com QR Code
- avaliações de eventos

## Observações técnicas

- o banco utilizado é SQLite
- as senhas dos clientes são armazenadas como hash SHA-256
- as operações principais usam Dapper com queries parametrizadas
- isso reduz o risco de SQL Injection nas rotas implementadas
- eventos demo são inseridos automaticamente quando a tabela `Eventos` está vazia

## Documentação complementar

- visão do projeto: `docs/visao.md`
- arquitetura: `docs/arquitetura.md`
- specs: `docs/specs.md`
- histórias de usuário: `docs/historias-usuario.md`
- roadmap resumido: `docs/roadmap.md`
- requisitos atualizados: `docs/requisitos.md`
- pivotagem: `docs/pivotagem.md`
- roadmap da pivotagem: `docs/roadmap-pivotagem.md`
- checklist de entrega: `docs/checklist-entrega.md`
