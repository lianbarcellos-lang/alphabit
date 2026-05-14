# Integrantes

- Raphael Leon Ramos da Silva - 06004469
- Joao Victor Guimaraes - 06006067
- Mauricio Alves de Barros Neto - 06014054
- Juarez Costa - 06004451
- Julio Henrique Oliveira Carvalho - 06010951
- Lian da Silva - 06010870

# TicketPrime

API em .NET 9 com Dapper, SQLite e interface Blazor Server para venda de ingressos de eventos musicais.

## Estrutura do repositorio

- `docs`: requisitos, operacao e arquitetura
- `db`: script SQL manual
- `src`: API e interface Blazor
- `tests`: testes automatizados com xUnit

## Funcionalidades atuais

- cadastro basico de usuario em `POST /api/usuarios`
- cadastro completo de cliente com senha
- login de cliente por email ou CPF
- login administrativo
- recuperacao de senha por codigo enviado por email
- perfil do cliente com dados pessoais
- listagem publica de eventos
- detalhe do evento
- selecao de assentos
- carrinho com pre-visualizacao de cupom
- selecao de forma de pagamento no carrinho
- finalizacao de compra
- consulta protegida de reservas
- painel administrativo com duas visoes: controle de ingressos e relatorio de vendas
- dashboard administrativo de vendas com cards, ranking de shows e ultimas compras
- cadastro, edicao e exclusao de eventos
- cadastro, edicao e exclusao de cupons
- catalogo administrativo de cidades
- catalogo administrativo de generos musicais
- filtros por cidade, artista, genero musical e dia da semana
- Dapper com parametros nomeados
- testes automatizados com Assert

## Como rodar a API localmente

Antes de subir a API com recuperacao de senha por Gmail, configure os segredos locais do projeto:

```powershell
dotnet user-secrets --project "C:\Users\rapha\OneDrive\Área de Trabalho\Curso_Video\TicketPrime (1)\TicketPrime\src\TicketPrime.API\TicketPrime.API.csproj" set "EmailSettings:SmtpHost" "smtp.gmail.com"
dotnet user-secrets --project "C:\Users\rapha\OneDrive\Área de Trabalho\Curso_Video\TicketPrime (1)\TicketPrime\src\TicketPrime.API\TicketPrime.API.csproj" set "EmailSettings:SmtpPort" "587"
dotnet user-secrets --project "C:\Users\rapha\OneDrive\Área de Trabalho\Curso_Video\TicketPrime (1)\TicketPrime\src\TicketPrime.API\TicketPrime.API.csproj" set "EmailSettings:SenderEmail" "ticketprimeshows@gmail.com"
dotnet user-secrets --project "C:\Users\rapha\OneDrive\Área de Trabalho\Curso_Video\TicketPrime (1)\TicketPrime\src\TicketPrime.API\TicketPrime.API.csproj" set "EmailSettings:AppPassword" "SUA_APP_PASSWORD_AQUI"
dotnet user-secrets --project "C:\Users\rapha\OneDrive\Área de Trabalho\Curso_Video\TicketPrime (1)\TicketPrime\src\TicketPrime.API\TicketPrime.API.csproj" set "EmailSettings:SenderName" "TicketPrime"
```

Depois rode:

```powershell
dotnet run --project "C:\Users\rapha\OneDrive\Área de Trabalho\Curso_Video\TicketPrime (1)\TicketPrime\src\TicketPrime.API\TicketPrime.API.csproj"
```

API disponivel em:

- `http://localhost:5238`

## Como rodar o Blazor localmente

```powershell
dotnet run --project "C:\Users\rapha\OneDrive\Área de Trabalho\Curso_Video\TicketPrime (1)\TicketPrime\src\TicketPrime.App\TicketPrime.App.csproj"
```

Site disponivel em:

- `http://localhost:5062`

## Como rodar os testes

Feche API e Blazor antes dos testes para evitar bloqueio de arquivos.

```powershell
dotnet test "C:\Users\rapha\OneDrive\Área de Trabalho\Curso_Video\TicketPrime (1)\TicketPrime\tests\TicketPrime.Tests\TicketPrime.Tests.csproj" --no-restore
```

Resultado validado:

- `19` testes aprovados
- `0` falhas

## Deploy no Railway

O projeto foi ajustado para deploy no Railway sem prender a aplicacao ao `localhost`.

### API no Railway

Configure estas variaveis:

- `AdminAccess__Token`
- `AdminAccess__Login`
- `AdminAccess__Password`
- `EmailSettings__SmtpHost`
- `EmailSettings__SmtpPort`
- `EmailSettings__SenderEmail`
- `EmailSettings__AppPassword`
- `EmailSettings__SenderName`

Se for usar envio por API HTTP em vez de SMTP:

- `EmailApiSettings__BaseUrl`
- `EmailApiSettings__ApiKey`
- `EmailApiSettings__SenderEmail`
- `EmailApiSettings__SenderName`

A API suporta volume persistente automaticamente por `RAILWAY_VOLUME_MOUNT_PATH`.

### App no Railway

Configure:

- `TicketPrimeApi__BaseUrl`

Exemplo:

- `TicketPrimeApi__BaseUrl=https://sua-api.up.railway.app/`

### Recuperacao de senha em producao

O fluxo `Esqueci minha senha` funciona em producao desde que:

- o Gmail SMTP esteja configurado nas variaveis da API
- ou exista um provedor HTTP configurado em `EmailApiSettings__...`
- a API esteja com volume persistente para manter banco e codigos de redefinicao

## Endpoints principais

### Usuarios e autenticacao

- `POST /api/usuarios`
- `GET /api/usuarios`
- `GET /api/usuarios/{cpf}/perfil`
- `PUT /api/usuarios/{cpf}/perfil`
- `POST /api/auth/usuarios/cadastro`
- `POST /api/auth/usuarios/login`
- `POST /api/auth/usuarios/recuperar-senha`
- `POST /api/auth/usuarios/redefinir-senha`
- `POST /api/auth/admin/login`

### Eventos e catalogos administrativos

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
- `GET /api/admin/vendas/dashboard`

### Cupons

- `POST /api/cupons`
- `POST /api/cupons/preview`
- `GET /api/admin/cupons`
- `PUT /api/admin/cupons/{codigo}`
- `DELETE /api/admin/cupons/{codigo}`

### Reservas

- `POST /api/reservas`
- `GET /api/reservas/{cpf}`

## Regras de negocio principais

- nao cadastrar usuario com CPF duplicado
- nao cadastrar usuario com email duplicado
- impedir cupom duplicado
- validar existencia de usuario e evento antes da reserva
- limitar a 2 reservas por CPF no mesmo evento
- bloquear venda acima da capacidade do evento
- aplicar cupom apenas quando o valor minimo for atendido
- registrar forma de pagamento, status e codigo do pedido em cada compra
- proteger perfil, usuarios e reservas por autenticacao
- manter codigo de redefinicao temporario, com expiracao e bloqueio por tentativas
- converter datas de compra para o horario de Brasilia no historico e no relatorio

## Credenciais de demonstracao

### Administrador

- login: `admin`
- senha: `admin`

### Cliente

- pode criar conta pelo fluxo de cadastro da tela inicial
- pode concluir o primeiro acesso depois do cadastro basico de `POST /api/usuarios`

## Documentacao complementar

- [requisitos.md](C:\Users\rapha\OneDrive\Área%20de%20Trabalho\Curso_Video\TicketPrime%20(1)\TicketPrime\docs\requisitos.md)
- [adr.md](C:\Users\rapha\OneDrive\Área%20de%20Trabalho\Curso_Video\TicketPrime%20(1)\TicketPrime\docs\adr.md)
- [operacao.md](C:\Users\rapha\OneDrive\Área%20de%20Trabalho\Curso_Video\TicketPrime%20(1)\TicketPrime\docs\operacao.md)
- [release_checklist_final.md](C:\Users\rapha\OneDrive\Área%20de%20Trabalho\Curso_Video\TicketPrime%20(1)\TicketPrime\release_checklist_final.md)
