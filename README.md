# Integrantes:

Raphael Leon Ramos da Silva - 06004469
João Victor Guimarães - 06006067
Maurício Alves de Barros Neto - 06014054
Juarez Costa - 06004451
Julio Henrique Oliveira Carvalho - 06010951

# TicketPrime

API e interface Blazor para venda e gestao de ingressos de eventos musicais.

## Estrutura do repositorio

- `docs`: documentos de requisitos, arquitetura e operacao
- `db`: script SQL manual
- `src`: codigo-fonte da API e do Blazor
- `tests`: testes automatizados com xUnit

## Funcionalidades implementadas

- cadastro de usuario por `CPF`, `nome` e `email` em `POST /api/usuarios`
- cadastro e login de cliente com senha
- login administrativo com `admin / admin`
- cadastro, listagem, edicao e exclusao de eventos
- cadastro, listagem, edicao e exclusao de cupons
- carrinho de compras
- selecao de assentos por evento
- pre-visualizacao de cupom no carrinho com desconto e total final
- finalizacao de compra com reservas
- consulta de reservas por CPF
- validacao de capacidade do evento
- limite de 2 reservas por CPF no mesmo evento
- aplicacao de cupom com valor minimo
- filtros por cidade, dia da semana, artista e genero musical
- consultas com Dapper parametrizado
- testes automatizados com `Assert`

## Como rodar a API

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

- `10` testes aprovados
- `0` falhas

## Endpoints principais

### Usuarios e autenticacao

- `POST /api/usuarios`
- `POST /api/auth/usuarios/cadastro`
- `POST /api/auth/usuarios/login`
- `POST /api/auth/admin/login`
- `GET /api/usuarios`

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
- `GET /api/reservas/{cpf}`

## Regras de negocio implementadas

- nao cadastrar usuario com CPF duplicado
- impedir cupom duplicado
- validar existencia de usuario e evento antes da reserva
- limitar a 2 reservas por CPF no mesmo evento
- bloquear venda acima da capacidade do evento
- aplicar cupom somente quando o valor do evento atende ao minimo exigido
- mostrar pre-visualizacao do desconto antes da finalizacao da compra

## Credenciais de demonstracao

### Administrador

- login: `admin`
- senha: `admin`

### Cliente

- pode criar conta na tela inicial do sistema

## Documentacao complementar

- requisitos: `docs/requisitos.md`
- adr: `docs/adr.md`
- operacao: `docs/operacao.md`
- checklist final: `release_checklist_final.md`

## Observacoes da AV2

- `GET /api/reservas/{cpf}` utiliza `INNER JOIN` para retornar o nome do evento
- `POST /api/reservas` aplica integridade, limite por `CPF + EventoId`, capacidade e motor de cupons
- o token administrativo da API foi movido para `appsettings.json` e `appsettings.Development.json`
