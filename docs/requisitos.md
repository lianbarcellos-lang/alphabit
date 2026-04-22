# Requisitos do Sistema TicketPrime

## Escopo implementado ate o momento

O projeto TicketPrime possui atualmente uma API em .NET 9 com SQLite e uma interface em Blazor para apresentacao do sistema. A solucao cobre cadastro de usuarios, login de clientes, acesso administrativo, gestao de eventos, cadastro de cupons, carrinho de compras, reservas, filtros de descoberta, protecao contra SQL Injection com Dapper parametrizado e testes automatizados.

## Historias implementadas

### HU01 - Cadastro de cliente com senha
Como cliente, quero me cadastrar com CPF, nome, email e senha, para acessar os eventos da plataforma.

### HU02 - Login do cliente
Como cliente, quero entrar com email ou CPF e minha senha, para visualizar os eventos disponiveis.

### HU03 - Validacao de CPF e email unicos
Como sistema, quero impedir cadastros duplicados por CPF ou email, para manter a base consistente.

### HU04 - Login administrativo
Como administrador, quero acessar uma area protegida com credenciais administrativas, para gerenciar eventos e cupons.

### HU05 - Cadastro de eventos
Como administrador, quero cadastrar eventos com nome, local, cidade, artista, genero, capacidade, data, preco e imagem, para disponibiliza-los na vitrine do sistema.

### HU06 - Listagem publica de eventos
Como cliente, quero visualizar os eventos disponiveis com informacoes principais e imagem, para escolher o que desejo comprar.

### HU07 - Gestao administrativa de eventos
Como administrador, quero listar, editar e excluir eventos, para manter a agenda atualizada.

### HU08 - Cadastro de cupons
Como administrador, quero cadastrar cupons com codigo, porcentagem de desconto e valor minimo, para criar campanhas promocionais.

### HU09 - Gestao administrativa de cupons
Como administrador, quero listar, editar e excluir cupons, para manter as campanhas promocionais atualizadas.

### HU10 - Compra de ingressos
Como cliente, quero adicionar eventos ao carrinho e finalizar a compra, para garantir meus ingressos.

### HU11 - Visualizacao previa de desconto
Como cliente, quero aplicar um cupom antes de finalizar a compra, para ver o desconto e o total final no carrinho.

### HU12 - Consulta de reservas
Como cliente, quero consultar minhas reservas pelo CPF, para acompanhar meus ingressos confirmados.

### HU13 - Filtros de descoberta
Como cliente, quero filtrar eventos por cidade, dia da semana, artista e genero musical, para encontrar opcoes que facam sentido para mim.

### HU14 - Seguranca contra SQL Injection
Como sistema, quero executar queries parametrizadas com Dapper, para reduzir o risco de SQL Injection.

### HU15 - Testes automatizados com oraculo
Como desenvolvedor, quero testes automatizados com Assert, para validar regras esperadas do sistema.

## Criterios de aceitacao

### Cadastro de cliente

Cenario: criar conta com dados validos  
Dado que o cliente ainda nao existe  
Quando ele envia CPF, nome, email e senha  
Entao a conta e criada com sucesso

Cenario: impedir duplicidade de CPF ou email  
Dado que ja existe um cadastro com o CPF ou email informado  
Quando um novo cadastro e enviado  
Entao a API retorna erro de validacao

### Login do cliente

Cenario: login com email e senha validos  
Dado que o cliente possui cadastro  
Quando ele informa email e senha corretos  
Entao o acesso e liberado

Cenario: login com CPF e senha validos  
Dado que o cliente possui cadastro  
Quando ele informa CPF e senha corretos  
Entao o acesso e liberado

### Login administrativo

Cenario: acesso ADM valido  
Dado que o login informado e `admin`  
E a senha informada e `admin`  
Quando o formulario de login e enviado  
Entao o sistema libera a area administrativa

### Eventos

Cenario: cadastrar evento  
Dado que o administrador esta autenticado  
Quando ele informa nome, local, cidade, artista, genero, capacidade, data, preco e imagem  
Entao o evento e salvo no banco

Cenario: listar eventos publicos  
Dado que existem eventos cadastrados  
Quando a vitrine de eventos e aberta  
Entao os eventos sao exibidos com dados principais e imagem

Cenario: editar evento  
Dado que o administrador esta autenticado  
Quando ele altera os dados de um evento existente  
Entao o sistema salva a atualizacao

Cenario: excluir evento  
Dado que o administrador esta autenticado  
Quando ele remove um evento existente  
Entao o evento deixa de aparecer na listagem

### Cupons

Cenario: criar cupom valido  
Dado que o administrador esta autenticado  
Quando ele informa codigo, desconto e valor minimo  
Entao o cupom e registrado com sucesso

Cenario: impedir cupom duplicado  
Dado que o codigo do cupom ja existe  
Quando o administrador tenta cadastrar novamente  
Entao a API retorna erro

Cenario: editar ou excluir cupom  
Dado que o administrador esta autenticado  
Quando ele altera ou remove um cupom existente  
Entao o sistema deve refletir a mudanca na listagem administrativa

### Reservas

Cenario: concluir compra valida  
Dado que o usuario existe  
E que o evento existe  
Quando a compra respeita capacidade, limite por CPF e cupom valido  
Entao a reserva e salva com sucesso

Cenario: bloquear excesso por CPF no mesmo evento  
Dado que o mesmo CPF ja possui 2 reservas para o mesmo evento  
Quando uma nova tentativa e enviada  
Entao a API retorna erro de validacao

Cenario: aplicar cupom no carrinho antes da compra  
Dado que o cliente adicionou ingressos ao carrinho  
Quando ele informa um cupom valido e clica em aplicar  
Entao o sistema deve mostrar o desconto e o total final antes da finalizacao

### Filtros

Cenario: filtrar por cidade  
Dado que existem eventos cadastrados em cidades diferentes  
Quando o cliente seleciona uma cidade  
Entao a vitrine deve mostrar apenas os eventos daquela cidade

Cenario: filtrar por dia da semana  
Dado que existem eventos em dias diferentes  
Quando o cliente seleciona um dia da semana  
Entao a vitrine deve mostrar apenas os eventos compativeis

## Endpoints implementados

### Autenticacao e usuarios

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

## Banco de dados atual

### Tabela `Usuarios`

- `Cpf` TEXT PRIMARY KEY
- `Nome` TEXT NOT NULL
- `Email` TEXT NOT NULL
- `SenhaHash` TEXT NOT NULL

### Tabela `Eventos`

- `Id` INTEGER PRIMARY KEY AUTOINCREMENT
- `Nome` TEXT NOT NULL
- `LocalEvento` TEXT NOT NULL
- `CidadeEvento` TEXT NOT NULL
- `Artista` TEXT NOT NULL
- `GeneroMusical` TEXT NOT NULL
- `CapacidadeTotal` INTEGER NOT NULL
- `DataEvento` TEXT NOT NULL
- `PrecoPadrao` REAL NOT NULL
- `ImagemUrl` TEXT

### Tabela `Cupons`

- `Codigo` TEXT PRIMARY KEY
- `PorcentagemDesconto` REAL NOT NULL
- `ValorMinimoRegra` REAL NOT NULL

### Tabela `Reservas`

- `Id` INTEGER PRIMARY KEY AUTOINCREMENT
- `UsuarioCpf` TEXT NOT NULL
- `EventoId` INTEGER NOT NULL
- `CupomUtilizado` TEXT
- `Quantidade` INTEGER NOT NULL
- `PrecoUnitario` REAL NOT NULL
- `ValorFinalPago` REAL NOT NULL
- `DataReserva` TEXT NOT NULL

## Observacoes tecnicas

- O acesso a dados principal utiliza Dapper com parametros nomeados.
- As queries implementadas estao parametrizadas para reduzir risco de SQL Injection.
- Nao ha concatenacao com `+` nem interpolacao `$""` nas queries de negocio da API.
- Senhas de usuarios sao armazenadas como hash SHA-256.
- O administrador usa credenciais fixas para demonstracao: `admin / admin`.
- O token administrativo da API e lido de configuracao em `appsettings.json` e `appsettings.Development.json`.
- Existem eventos iniciais semeados automaticamente para exibicao na vitrine.
- O projeto possui infraestrutura de testes com xUnit.
- Existem testes com Assert cobrindo regras principais em `tests/TicketPrime.Tests`.
- A rota `POST /api/reservas` aplica validacao de integridade, limite por CPF no mesmo evento, controle de capacidade e regra de cupom.
- A rota `POST /api/cupons/preview` calcula o desconto no carrinho sem gravar a compra.
- A interface Blazor permite adicionar eventos ao carrinho, finalizar compra e consultar reservas.
- A interface Blazor permite selecionar assentos, aplicar cupom e ver o total final antes da compra.
- A vitrine publica possui filtros por cidade, dia da semana, artista e genero musical.
