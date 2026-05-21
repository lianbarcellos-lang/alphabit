# GeekTop

Projeto acadêmico de plataforma de eventos Geek / Anime / Gaming, construído sobre a base técnica original do Alphabit.

## Integrantes

- Raphael Leon Ramos da Silva - 06004469
- João Victor Guimarães - 06006067
- Mauricio Alves de Barros Neto - 06014054
- Juarez Costa - 06004451
- Julio Henrique Oliveira Carvalho - 06010951
- Lian da Silva - 06010870

## Stack do projeto

- .NET 9
- ASP.NET Core Minimal API
- Blazor Server / Razor Components
- Dapper
- SQLite
- xUnit

Não foram adicionados JWT, Swagger obrigatório, MariaDB, SQL Server, Docker, Entity Framework, CQRS, MediatR ou Clean Architecture.

## Estrutura

- `docs`: requisitos, roadmap, ADR, checklist e documentação da pivotagem
- `db`: script SQL manual
- `src/Alphabit.API`: API Minimal API
- `src/Alphabit.App`: interface Blazor
- `tests/Alphabit.Tests`: testes automatizados

Os nomes técnicos `Alphabit.*` foram mantidos nos projetos, namespaces e banco local para reduzir risco de quebra. A identidade exibida ao usuário é GeekTop.

## Funcionalidades atuais

- cadastro, login, perfil e recuperação de senha de usuário
- login administrativo
- listagem, filtros e detalhe de eventos
- categorias geek, cidades e eventos enriquecidos
- imagens e identidade visual voltadas a games, anime, cosplay e cultura geek
- tipos de ingresso com preço, benefícios e disponibilidade
- seleção de assentos, carrinho e finalização de reserva
- cupons promocionais
- histórico de reservas com QR Code ampliável para leitura
- check-in administrativo manual ou por câmera/webcam, com bloqueio de duplicidade e rejeição de reserva cancelada
- convidados e associação de convidados aos eventos
- atividades internas com limite de vagas e inscrição
- avaliações de eventos com bloqueio de duplicidade e média exibida junto ao preço
- dashboard administrativo com relatório de vendas, eventos populares, compras recentes, cupons, avaliações, check-ins e capacidade

## Como rodar

Na raiz do projeto:

```powershell
dotnet build .\Alphabit.sln
dotnet run --project .\src\Alphabit.API\Alphabit.API.csproj --urls http://localhost:5248
```

Em outro terminal:

```powershell
dotnet run --project .\src\Alphabit.App\Alphabit.App.csproj --urls http://localhost:5072
```

URLs locais:

- API: `http://localhost:5248`
- App: `http://localhost:5072`

## Como testar

Feche a API e o App antes de rodar os testes, para evitar bloqueio dos arquivos compilados.

```powershell
dotnet build .\Alphabit.sln /nr:false -p:BuildInParallel=false -m:1
dotnet test .\tests\Alphabit.Tests\Alphabit.Tests.csproj --no-restore
```

Última validação local:

- build aprovado com 0 erros e 0 avisos
- 32 testes aprovados
- 0 falhas

## Credenciais de demonstração

Administrador:

- login: `admin`
- senha: `admin`

Token administrativo da API:

- `alphabit-admin-token`

## Banco local

O banco SQLite de desenvolvimento fica em:

```text
%LOCALAPPDATA%\Alphabit\Alphabit.db
```

Esse caminho foi mantido por compatibilidade técnica com a base original.

## Endpoints principais

### Autenticação e usuários

- `POST /api/usuarios`
- `GET /api/usuarios`
- `GET /api/usuarios/{cpf}/perfil`
- `PUT /api/usuarios/{cpf}/perfil`
- `POST /api/auth/usuarios/cadastro`
- `POST /api/auth/usuarios/login`
- `POST /api/auth/usuarios/recuperar-senha`
- `POST /api/auth/usuarios/redefinir-senha`
- `POST /api/auth/admin/login`

### Eventos e catálogos

- `GET /api/eventos`
- `GET /api/eventos/{id}`
- `POST /api/eventos`
- `GET /api/admin/eventos`
- `PUT /api/admin/eventos/{id}`
- `DELETE /api/admin/eventos/{id}`
- `GET /api/admin/generos`
- `POST /api/admin/generos`
- `PUT /api/admin/generos/{nomeAtual}`
- `DELETE /api/admin/generos/{nomeAtual}`
- `GET /api/admin/cidades`
- `POST /api/admin/cidades`
- `PUT /api/admin/cidades/{nomeAtual}`
- `DELETE /api/admin/cidades/{nomeAtual}`

### Tipos de ingresso, atividades e convidados

- `GET /api/eventos/{id}/tipos-ingresso`
- `GET /api/eventos/{id}/atividades`
- `POST /api/atividades/{id}/inscricao`
- `GET /api/eventos/{id}/convidados`
- `GET /api/admin/convidados`
- `POST /api/admin/convidados`
- `POST /api/admin/eventos/convidados`

### Reservas, check-in e avaliações

- `POST /api/reservas`
- `GET /api/reservas/{cpf}`
- `POST /api/admin/checkins/validar`
- `GET /api/eventos/{id}/avaliacoes`
- `POST /api/avaliacoes`
- `DELETE /api/admin/avaliacoes/{id}`

### Administração

- `GET /api/admin/vendas/dashboard`
- `POST /api/cupons`
- `POST /api/cupons/preview`
- `GET /api/admin/cupons`
- `PUT /api/admin/cupons/{codigo}`
- `DELETE /api/admin/cupons/{codigo}`

## Documentação complementar

- [docs/visao.md](docs/visao.md)
- [docs/arquitetura.md](docs/arquitetura.md)
- [docs/specs.md](docs/specs.md)
- [docs/roadmap.md](docs/roadmap.md)
- [docs/historias-usuario.md](docs/historias-usuario.md)
- [docs/pivotagem.md](docs/pivotagem.md)
- [docs/roadmap-pivotagem.md](docs/roadmap-pivotagem.md)
- [docs/requisitos.md](docs/requisitos.md)
- [docs/checklist-entrega.md](docs/checklist-entrega.md)
- [docs/adr-pivotagem-geek-anime.md](docs/adr-pivotagem-geek-anime.md)
