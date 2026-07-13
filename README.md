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

Não foram adicionados JWT, Swagger obrigatório, MariaDB, SQL Server, Entity Framework, CQRS, MediatR ou Clean Architecture. O `Dockerfile` existente é usado apenas como empacotamento do deploy no Railway, sem alterar a stack da aplicação.

## Estrutura

- `docs`: requisitos, roadmap, ADR, checklist e documentação da pivotagem
- `db`: script SQL manual
- `src/Alphabit.API`: API Minimal API
- `src/Alphabit.App`: interface Blazor
- `tests/Alphabit.Tests`: testes automatizados

Os nomes técnicos `Alphabit.*` foram mantidos nos projetos, namespaces e banco local para reduzir risco de quebra. A identidade exibida ao usuário é GeekTop.

## Funcionalidades atuais

- cadastro, login e perfil de usuário
- login administrativo
- listagem, filtros e detalhe de eventos
- categorias geek, cidades e eventos enriquecidos
- imagens e identidade visual voltadas a games, anime, cosplay e cultura geek
- tipos de ingresso com preço, benefícios e disponibilidade
- escolha de tipo/quantidade de ingresso, carrinho e finalização de reserva
- cupons promocionais
- histórico de reservas com QR Code ampliável para leitura
- check-in administrativo manual ou por câmera/webcam, com bloqueio de duplicidade e rejeição de reserva cancelada
- convidados e associação de convidados aos eventos
- atividades internas com limite de vagas e inscrição
- mapa de stands e expositores por evento, com imagem de planta enviada pelo administrador, organização automática por grades 3x3 e 4x4, posicionamento manual por drag/drop, alocação administrativa por linhas/setores e visualização no detalhe/ingressos
- avaliações de eventos com bloqueio de duplicidade e média exibida junto ao preço
- dashboard administrativo com relatório de vendas, eventos populares, compras recentes, cupons, avaliações, check-ins e capacidade

## Como rodar

### Deploy no Railway

O guia completo esta em [docs/railway.md](docs/railway.md).

No Railway atual, o deploy usa o `Dockerfile` da raiz. Ele publica a API e o App no mesmo serviço: a API roda internamente na porta `8081` e o App Blazor fica exposto pela porta pública do Railway.

Configure no serviço as variáveis `AdminAccess__Token`, `AdminAccess__Login` e `AdminAccess__Password`. Para manter o SQLite depois de redeploy/restart, adicione um volume ao serviço. O domínio de apresentação configurado é `https://geektop.store`.

### Rodar localmente

Primeiro entre na pasta raiz do projeto. Os comandos abaixo precisam ser executados dentro desta pasta:

```powershell
cd "C:\Users\rapha\Downloads\Projeto\alphabit-main__tentar\alphabit-main"
```

Se o comando for executado em `C:\Users\rapha`, o .NET não encontra `.\src\Alphabit.API\Alphabit.API.csproj` e retorna erro de projeto inexistente.

Depois, no primeiro terminal, suba a API:

```powershell
dotnet build .\Alphabit.sln
dotnet run --project .\src\Alphabit.API\Alphabit.API.csproj --urls http://localhost:5248
```

Em outro terminal, entre na mesma pasta e suba o App:

```powershell
cd "C:\Users\rapha\Downloads\Projeto\alphabit-main__tentar\alphabit-main"
dotnet run --project .\src\Alphabit.App\Alphabit.App.csproj --urls http://localhost:5072
```

URLs locais:

- API: `http://localhost:5248`
- App: `http://localhost:5072`

## Como testar

Feche a API e o App antes de rodar os testes, para evitar bloqueio dos arquivos compilados.

```powershell
cd "C:\Users\rapha\Downloads\Projeto\alphabit-main__tentar\alphabit-main"
dotnet build .\Alphabit.sln /nr:false -p:BuildInParallel=false -m:1
dotnet test .\tests\Alphabit.Tests\Alphabit.Tests.csproj --no-restore
```

Última validação local:

- build aprovado com 0 erros e 0 avisos
- 41 testes aprovados
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

### Tipos de ingresso, atividades, convidados e stands

- `GET /api/eventos/{id}/tipos-ingresso`
- `GET /api/eventos/{id}/atividades`
- `POST /api/atividades/{id}/inscricao`
- `GET /api/eventos/{id}/convidados`
- `GET /api/convidados`
- `POST /api/convidados`
- `POST /api/eventos/{id}/convidados`
- `GET /api/eventos/{id}/stands`
- `POST /api/admin/eventos/{id}/stands`
- `PUT /api/admin/eventos/{id}/mapa-imagem`
- `PUT /api/admin/eventos/{id}/stands/{standId}`
- `DELETE /api/admin/eventos/{id}/stands/{standId}`
- `POST /api/admin/eventos/{id}/stand-setores`
- `PUT /api/admin/eventos/{id}/stand-setores/{nomeAtual}`
- `DELETE /api/admin/eventos/{id}/stand-setores/{nome}`

### Reservas, check-in e avaliações

- `POST /api/reservas`
- `GET /api/reservas/{cpf}`
- `POST /api/checkin`
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
- [docs/analise_arquitetura.md](docs/analise_arquitetura.md)
- [docs/violacoes_arquiteturais.md](docs/violacoes_arquiteturais.md)
- [docs/adrs](docs/adrs)
- [docs/specs.md](docs/specs.md)
- [docs/roadmap.md](docs/roadmap.md)
- [docs/historias-usuario.md](docs/historias-usuario.md)
- [docs/pivotagem.md](docs/pivotagem.md)
- [docs/roadmap-pivotagem.md](docs/roadmap-pivotagem.md)
- [docs/requisitos.md](docs/requisitos.md)
- [docs/divida_tecnica.md](docs/divida_tecnica.md)
- [docs/priorizacao_divida.md](docs/priorizacao_divida.md)
- [docs/classificacao_manutencao.md](docs/classificacao_manutencao.md)
- [docs/pipeline_liberacao.md](docs/pipeline_liberacao.md)
- [docs/plano_iteracao.md](docs/plano_iteracao.md)
- [docs/kanban_wip.md](docs/kanban_wip.md)
- [docs/metricas_dora.md](docs/metricas_dora.md)
- [docs/metricas_qualidade.md](docs/metricas_qualidade.md)
- [docs/slo.md](docs/slo.md)
- [docs/error_budget_policy.md](docs/error_budget_policy.md)
- [docs/ssdf.md](docs/ssdf.md)
- [docs/threat_model_e_gates.md](docs/threat_model_e_gates.md)
- [docs/topologia_times.md](docs/topologia_times.md)
- [docs/definition_of_done.md](docs/definition_of_done.md)
- [docs/checklist-entrega.md](docs/checklist-entrega.md)
- [docs/adr-pivotagem-geek-anime.md](docs/adr-pivotagem-geek-anime.md)
