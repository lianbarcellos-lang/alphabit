# Integrantes:

João Victor Guimarães - 06006067
Juarez Costa - 06004451
Julio Henrique Oliveira Carvalho - 06010951
Maurício Alves de Barros Neto - 06014054
Raphael Leon Ramos da Silva - 06004469

# TicketPrime

API e interface Blazor para venda e gestao de ingressos de eventos musicais.

## Estrutura do repositorio

- `docs`: documentos de requisitos, arquitetura e operacao
- `db`: script SQL manual
- `src`: codigo-fonte da API e do Blazor
- `tests`: testes automatizados com xUnit

## Funcionalidades implementadas

- cadastro basico de usuario por `CPF`, `nome` e `email` em `POST /api/usuarios`
- cadastro e login de cliente com senha
- login administrativo com `admin / admin`
- cadastro, listagem, edicao e exclusao de eventos
- cadastro, listagem, edicao e exclusao de cupons
- carrinho de compras
- selecao de assentos por evento
- pre-visualizacao de cupom no carrinho com desconto e total final
- finalizacao de compra com reservas
- consulta de reservas por CPF
- protecao de acesso nas rotas sensiveis de usuarios e reservas
- validacao de capacidade do evento
- limite de 2 reservas por CPF no mesmo evento
- aplicacao de cupom com valor minimo
- filtros por cidade, dia da semana, artista e genero musical
- consultas com Dapper parametrizado
- testes automatizados com `Assert`

## Como rodar a API

Antes de subir a API com recuperacao de senha por Gmail, configure os segredos locais do projeto:

```powershell
dotnet user-secrets --project "C:\Users\rapha\OneDrive\Área de Trabalho\Curso_Video\TicketPrime (1)\TicketPrime\src\TicketPrime.API\TicketPrime.API.csproj" set "EmailSettings:SmtpHost" "smtp.gmail.com"
dotnet user-secrets --project "C:\Users\rapha\OneDrive\Área de Trabalho\Curso_Video\TicketPrime (1)\TicketPrime\src\TicketPrime.API\TicketPrime.API.csproj" set "EmailSettings:SmtpPort" "587"
dotnet user-secrets --project "C:\Users\rapha\OneDrive\Área de Trabalho\Curso_Video\TicketPrime (1)\TicketPrime\src\TicketPrime.API\TicketPrime.API.csproj" set "EmailSettings:SenderEmail" "ticketprimeshows@gmail.com"
dotnet user-secrets --project "C:\Users\rapha\OneDrive\Área de Trabalho\Curso_Video\TicketPrime (1)\TicketPrime\src\TicketPrime.API\TicketPrime.API.csproj" set "EmailSettings:AppPassword" "SUA_APP_PASSWORD_AQUI"
dotnet user-secrets --project "C:\Users\rapha\OneDrive\Área de Trabalho\Curso_Video\TicketPrime (1)\TicketPrime\src\TicketPrime.API\TicketPrime.API.csproj" set "EmailSettings:SenderName" "TicketPrime"
```

```powershell
dotnet run --project "C:\Users\rapha\OneDrive\Área de Trabalho\Curso_Video\TicketPrime (1)\TicketPrime\src\TicketPrime.API\TicketPrime.API.csproj"
```

API disponivel em:

- `http://localhost:5238`

## Como rodar o Blazor

```powershell
dotnet run --project "C:\Users\rapha\OneDrive\Área de Trabalho\Curso_Video\TicketPrime (1)\TicketPrime\src\TicketPrime.App\TicketPrime.App.csproj"
```

Site disponivel em:

- `http://localhost:5062`

## Como rodar os testes

Feche a API e o Blazor antes de executar os testes para evitar bloqueio dos arquivos.

```powershell
dotnet test "C:\Users\rapha\OneDrive\Área de Trabalho\Curso_Video\TicketPrime (1)\TicketPrime\tests\TicketPrime.Tests\TicketPrime.Tests.csproj" --no-restore
```

Resultado validado:

- `19` testes aprovados
- `0` falhas

## Publicacao no Railway

Para publicar no Railway sem quebrar o sistema, configure:

- um servico para a API
- um servico para o Blazor
- um volume persistente ligado na API

### Volume da API

Monte um volume na API. O Railway fornece automaticamente a variavel:

- `RAILWAY_VOLUME_MOUNT_PATH`

A API ja usa esse caminho quando ele existir e salva o banco SQLite dentro dele.

### Variaveis da API no Railway

- `AdminAccess__Token`
- `AdminAccess__Login`
- `AdminAccess__Password`
- `EmailSettings__SmtpHost`
- `EmailSettings__SmtpPort`
- `EmailSettings__SenderEmail`
- `EmailSettings__AppPassword`
- `EmailSettings__SenderName`

### Variavel do Blazor no Railway

- `TicketPrimeApi__BaseUrl`

Exemplo:

- URL publica da API: `https://sua-api.up.railway.app/`
- valor no App: `TicketPrimeApi__BaseUrl=https://sua-api.up.railway.app/`

### Recuperacao de senha

O fluxo `Esqueci minha senha` funciona no Railway desde que:

- o Gmail SMTP esteja configurado nas variaveis da API
- a conta Gmail use `App Password`
- o banco esteja em volume persistente para guardar os codigos de redefinicao

## Endpoints principais

### Usuarios e autenticacao

- `POST /api/usuarios`
- `POST /api/auth/usuarios/cadastro`
- `POST /api/auth/usuarios/login`
- `POST /api/auth/admin/login`
- `GET /api/usuarios` - protegido para administrador
- `GET /api/usuarios/{cpf}/perfil`
- `PUT /api/usuarios/{cpf}/perfil`

### Eventos

- `GET /api/eventos`
- `GET /api/eventos/{id}`
- `POST /api/eventos`
- `GET /api/admin/eventos`
- `PUT /api/admin/eventos/{id}`
- `DELETE /api/admin/eventos/{id}`

### Cupons

- `POST /api/cupons`
- `GET /api/admin/cupons`
- `PUT /api/admin/cupons/{codigo}`
- `DELETE /api/admin/cupons/{codigo}`
- `POST /api/cupons/preview`

### Reservas

- `POST /api/reservas`
- `GET /api/reservas/{cpf}` - protegido para o proprio cliente ou administrador

## Regras de negocio implementadas

- nao cadastrar usuario com CPF duplicado
- nao cadastrar usuario com email duplicado
- impedir cupom duplicado
- validar existencia de usuario e evento antes da reserva
- limitar a 2 reservas por CPF no mesmo evento
- bloquear venda acima da capacidade do evento
- aplicar cupom somente quando o valor do evento atende ao minimo exigido
- mostrar pre-visualizacao do desconto antes da finalizacao da compra
- exigir autenticacao nas rotas sensiveis de perfil, usuarios e reservas

## Credenciais de demonstracao

### Administrador

- login: `admin`
- senha: `admin`

### Cliente

- pode criar conta completa na tela inicial do sistema
- pode concluir o primeiro acesso depois do cadastro basico da rota `POST /api/usuarios`

## Documentacao complementar

- requisitos: `docs/requisitos.md`
- adr: `docs/adr.md`
- operacao: `docs/operacao.md`
- checklist final: `release_checklist_final.md`

## Observacoes da AV2

- `GET /api/reservas/{cpf}` utiliza `INNER JOIN` para retornar o nome do evento
- `POST /api/reservas` aplica integridade, limite por `CPF + EventoId`, capacidade e motor de cupons
- credenciais e token administrativos sao lidos de `appsettings.json` e `appsettings.Development.json`, sem fallback sensivel no `.cs`
- as rotas sensiveis da API foram protegidas com verificacao de acesso por cliente ou administrador
