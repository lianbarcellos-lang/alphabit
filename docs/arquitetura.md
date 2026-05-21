# Arquitetura do Projeto - GeekTop

## Visao geral

O GeekTop usa uma arquitetura simples e direta, adequada ao tamanho do projeto e a stack definida. A solucao e dividida em tres partes principais:

- `src/Alphabit.API`: backend em ASP.NET Core Minimal API.
- `src/Alphabit.App`: frontend em Blazor Server / Razor Components.
- `tests/Alphabit.Tests`: testes automatizados com xUnit.

## Backend

O backend fica em `src/Alphabit.API` e concentra:

- definicao das rotas HTTP;
- inicializacao e migracao simples do banco SQLite;
- regras de negocio principais;
- consultas SQL com Dapper;
- validacoes de seguranca e integridade;
- endpoints administrativos e publicos.

As rotas principais estao em `Program.cs`. O projeto usa Minimal API para manter a implementacao objetiva e compativel com a base existente.

## Frontend

O frontend fica em `src/Alphabit.App` e usa Blazor Server / Razor Components.

Principais telas:

- Home;
- Eventos;
- Detalhe do evento;
- Assentos;
- Carrinho;
- Reservas;
- Minha conta;
- Painel administrativo.

O app consome a API por meio de `AlphabitApiClient` e guarda estado local de usuario/carrinho com servicos como `UserSessionState` e `CartState`.

## Banco de dados

O banco utilizado e SQLite. Em ambiente local, o arquivo fica em:

`%LOCALAPPDATA%\Alphabit\Alphabit.db`

O banco e criado/atualizado no startup da API por comandos SQL. O acesso a dados usa Dapper com parametros nomeados.

## Principais tabelas

- `Usuarios`
- `Eventos`
- `Reservas`
- `Cupons`
- `GenerosMusicais`
- `CidadesEventos`
- `TiposIngresso`
- `Atividades`
- `InscricoesAtividades`
- `Convidados`
- `EventoConvidados`
- `Checkins`
- `Avaliacoes`
- `RecuperacoesSenha`

## Modulos funcionais

### Usuarios e autenticacao

Responsavel por cadastro, login, perfil, protecao de reservas e recuperacao de senha.

### Eventos

Responsavel por cadastro, edicao, exclusao, listagem publica, filtros, imagens, categorias e cidades.

### Compra e reservas

Responsavel por selecao de assentos, carrinho, cupom, forma de pagamento, pedido e historico.

### Tipos de ingresso

Permite diferentes experiencias dentro do mesmo evento, como Normal, VIP, Premium e Meet and Greet.

### Atividades

Permite programacao interna do evento, com inscricao e limite de participantes.

### Convidados

Permite cadastro de convidados e associacao deles aos eventos.

### Check-in

Gera QR Code unico por reserva e permite validacao administrativa por codigo manual ou camera.

### Avaliacoes

Permite avaliacao pos-reserva, impede duplicidade e mostra media do evento junto ao preco.

### Dashboard administrativo

Mostra relatorio comercial com reservas, receita, ranking de eventos, formas de pagamento, capacidade, cupons, check-ins e avaliacoes.

## Padroes adotados

- Minimal API no backend.
- Dapper para persistencia.
- SQL parametrizado.
- Blazor Server para interface.
- Servicos de estado no frontend para sessao e carrinho.
- Testes xUnit para regras principais.

## Decisoes importantes

- Manter SQLite em vez de migrar para MariaDB ou SQL Server.
- Manter Dapper em vez de Entity Framework.
- Manter rotas e nomes existentes quando isso reduz risco.
- Adaptar `GenerosMusicais` como catalogo de categorias geek.
- Usar `ImagemUrl` como banner/capa do evento.
- Guardar QR Code em `Checkins`, vinculado a `Reservas`.

## Como rodar

API:

```powershell
dotnet run --project .\src\Alphabit.API\Alphabit.API.csproj --urls http://localhost:5248
```

App:

```powershell
dotnet run --project .\src\Alphabit.App\Alphabit.App.csproj --urls http://localhost:5072
```

Testes:

```powershell
dotnet test .\tests\Alphabit.Tests\Alphabit.Tests.csproj --no-restore
```

