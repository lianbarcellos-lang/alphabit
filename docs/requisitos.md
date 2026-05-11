# Requisitos do Sistema TicketPrime

## Escopo implementado

O TicketPrime possui uma API em .NET 9 com SQLite e Dapper, junto com uma interface Blazor Server. O sistema cobre cadastro e autenticacao de usuarios, recuperacao de senha por email, gestao administrativa de eventos, cupons, cidades e generos musicais, fluxo de compra com carrinho, assentos, reservas e filtros de descoberta.

## Historias de usuario implementadas

### HU01 - Cadastro basico de usuario
Como atendente ou integracao externa, quero cadastrar um usuario com CPF, nome e email, para iniciar o relacionamento com a plataforma.

### HU02 - Cadastro completo de cliente
Como cliente, quero criar minha conta com CPF, nome, email e senha, para acessar e comprar ingressos.

### HU03 - Login do cliente
Como cliente, quero entrar com email ou CPF e minha senha, para visualizar eventos e concluir compras.

### HU04 - Recuperacao de senha
Como cliente, quero receber um codigo por email para redefinir minha senha, para voltar a acessar minha conta quando eu esquecer a senha.

### HU05 - Protecao contra duplicidade
Como sistema, quero bloquear CPF e email ja cadastrados, para manter a base consistente.

### HU06 - Login administrativo
Como administrador, quero acessar uma area protegida, para gerenciar a operacao da plataforma.

### HU07 - Cadastro de evento
Como administrador, quero cadastrar eventos com nome, local, cidade, artista, genero musical, capacidade, data, preco e imagem, para publicar a agenda da plataforma.

### HU08 - Gestao de eventos
Como administrador, quero editar e excluir eventos, para manter a agenda atualizada.

### HU09 - Catalogo de cidades
Como administrador, quero selecionar, criar, editar e excluir cidades de um catalogo, para evitar digitacao repetida e erros.

### HU10 - Catalogo de generos musicais
Como administrador, quero selecionar, criar, editar e excluir generos musicais de um catalogo, para padronizar os eventos cadastrados.

### HU11 - Cadastro de cupons
Como administrador, quero cadastrar cupons promocionais com codigo, desconto e valor minimo, para criar campanhas.

### HU12 - Gestao de cupons
Como administrador, quero editar e excluir cupons, para ajustar campanhas promocionais.

### HU13 - Vitrine publica de eventos
Como cliente, quero visualizar os eventos disponiveis com imagem e informacoes principais, para escolher o que desejo comprar.

### HU14 - Filtros de descoberta
Como cliente, quero filtrar eventos por cidade, dia da semana, artista e genero musical, para encontrar opcoes mais relevantes.

### HU15 - Detalhe do evento
Como cliente, quero abrir uma pagina de detalhe do evento, para entender local, data, preco e seguir para os assentos.

### HU16 - Selecao de assentos
Como cliente, quero escolher assentos disponiveis antes de comprar, para montar meu pedido.

### HU17 - Carrinho e cupom
Como cliente, quero aplicar um cupom no carrinho e ver o desconto antes de finalizar, para saber o valor final da compra.

### HU18 - Finalizacao da compra
Como cliente, quero concluir a compra de ingressos, para garantir minha reserva.

### HU19 - Consulta de reservas
Como cliente, quero consultar minhas reservas, para acompanhar meus ingressos confirmados.

### HU20 - Protecao de perfil e reservas
Como sistema, quero restringir acesso a perfil e reservas ao proprio cliente ou administrador, para proteger dados pessoais.

### HU21 - Protecao de usuarios administrativos
Como sistema, quero proteger a listagem de usuarios para uso apenas administrativo, para evitar exposicao indevida.

### HU22 - Seguranca contra SQL Injection
Como sistema, quero usar Dapper com parametros nomeados, para reduzir o risco de SQL Injection.

### HU23 - Qualidade automatizada
Como desenvolvedor, quero manter testes automatizados com Assert, para validar o comportamento esperado do sistema.

## Criterios de aceitacao

### Cadastro e autenticacao

Cenario: criar conta com dados validos  
Dado que o CPF e o email ainda nao existem  
Quando o cliente envia os dados de cadastro  
Entao a conta deve ser criada com sucesso

Cenario: bloquear CPF ou email duplicados  
Dado que ja existe conta com o CPF ou email informado  
Quando um novo cadastro e enviado  
Entao a API deve retornar erro de validacao

Cenario: login com credenciais validas  
Dado que o cliente possui cadastro completo  
Quando ele informa CPF ou email e senha corretos  
Entao o sistema deve autenticar o acesso

Cenario: recuperar senha por email  
Dado que existe um usuario com email cadastrado  
Quando ele solicita redefinicao de senha  
Entao o sistema deve gerar um codigo, enviar por email e permitir a troca de senha com o codigo correto

### Administracao

Cenario: cadastrar evento  
Dado que o administrador esta autenticado  
Quando informa os dados do evento  
Entao o evento deve ser salvo e aparecer na agenda

Cenario: editar evento  
Dado que existe um evento cadastrado  
Quando o administrador altera seus dados  
Entao a atualizacao deve ser persistida

Cenario: excluir evento  
Dado que existe um evento cadastrado  
Quando o administrador solicita a exclusao  
Entao o evento deve sair da listagem administrativa e publica

Cenario: gerenciar catalogo de cidades  
Dado que o administrador deseja padronizar cidades  
Quando ele cria, edita ou exclui uma cidade no catalogo  
Entao a lista deve ser atualizada e reutilizavel no cadastro de eventos

Cenario: gerenciar catalogo de generos musicais  
Dado que o administrador deseja padronizar generos  
Quando ele cria, edita ou exclui um genero no catalogo  
Entao a lista deve ser atualizada e reutilizavel no cadastro de eventos

Cenario: cadastrar cupom  
Dado que o administrador esta autenticado  
Quando informa codigo, desconto e valor minimo  
Entao o cupom deve ser criado

### Compra e reservas

Cenario: concluir compra valida  
Dado que o usuario existe  
E que o evento existe  
Quando a compra respeita capacidade, limite por CPF e regra de cupom  
Entao a reserva deve ser salva com sucesso

Cenario: bloquear excesso por CPF  
Dado que o mesmo CPF ja possui 2 reservas para o mesmo evento  
Quando uma nova tentativa e enviada  
Entao a API deve retornar erro

Cenario: aplicar cupom no carrinho  
Dado que o cliente adicionou ingressos ao carrinho  
Quando ele informa um cupom valido e clica em aplicar  
Entao o sistema deve mostrar desconto e total final antes da compra

Cenario: proteger reservas  
Dado que a rota de reservas pertence a um CPF especifico  
Quando outro usuario tenta consultar o historico sem permissao  
Entao a API deve retornar acesso nao autorizado

### Descoberta

Cenario: listar eventos publicos  
Dado que existem eventos cadastrados  
Quando a vitrine e aberta  
Entao os eventos devem ser exibidos com suas informacoes principais

Cenario: filtrar eventos  
Dado que existem eventos com cidades, artistas, dias e generos diferentes  
Quando o cliente aplica filtros  
Entao a vitrine deve exibir apenas os eventos compativeis

## Endpoints implementados

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

### Eventos, cidades e generos

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

### Cupons

- `POST /api/cupons`
- `POST /api/cupons/preview`
- `GET /api/admin/cupons`
- `PUT /api/admin/cupons/{codigo}`
- `DELETE /api/admin/cupons/{codigo}`

### Reservas

- `POST /api/reservas`
- `GET /api/reservas/{cpf}`

## Banco de dados atual

### Tabela `Usuarios`

- `Cpf` TEXT PRIMARY KEY
- `Nome` TEXT NOT NULL
- `Email` TEXT NOT NULL
- `SenhaHash` TEXT NOT NULL
- `Sobrenome` TEXT NOT NULL
- `PaisResidencia` TEXT NOT NULL
- `TipoDocumento` TEXT NOT NULL
- `CodigoPais` TEXT NOT NULL
- `Telefone` TEXT NOT NULL
- `DataNascimento` TEXT NULL
- `Sexo` TEXT NOT NULL

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

### Tabela `GenerosMusicais`

- `Id` INTEGER PRIMARY KEY AUTOINCREMENT
- `Nome` TEXT NOT NULL UNIQUE

### Tabela `CidadesEventos`

- `Id` INTEGER PRIMARY KEY AUTOINCREMENT
- `Nome` TEXT NOT NULL UNIQUE

### Tabela `Reservas`

- `Id` INTEGER PRIMARY KEY AUTOINCREMENT
- `UsuarioCpf` TEXT NOT NULL
- `EventoId` INTEGER NOT NULL
- `CupomUtilizado` TEXT
- `Assentos` TEXT NOT NULL
- `Quantidade` INTEGER NOT NULL
- `PrecoUnitario` REAL NOT NULL
- `ValorFinalPago` REAL NOT NULL
- `DataReserva` TEXT NOT NULL

### Tabela `RecuperacoesSenha`

- `Id` INTEGER PRIMARY KEY AUTOINCREMENT
- `UsuarioCpf` TEXT NOT NULL
- `CodigoHash` TEXT NOT NULL
- `EmailDestino` TEXT NOT NULL
- `ExpiraEm` TEXT NOT NULL
- `Utilizado` INTEGER NOT NULL
- `TentativasInvalidas` INTEGER NOT NULL
- `Bloqueado` INTEGER NOT NULL
- `CriadoEm` TEXT NOT NULL

## Observacoes tecnicas

- o acesso a dados principal utiliza Dapper com parametros nomeados
- as queries de negocio estao parametrizadas
- senhas de usuarios sao armazenadas como hash SHA-256
- a API possui fluxo de recuperacao de senha por email com codigo temporario
- o app Blazor possui modal de login, cadastro, redefinicao de senha e exibicao de senha
- a vitrine publica possui filtros por cidade, dia da semana, artista e genero musical
- o carrinho mostra pre-visualizacao do desconto antes da finalizacao
- a rota `GET /api/reservas/{cpf}` usa `INNER JOIN` para retornar o nome do evento
- a rota `POST /api/reservas` aplica validacao de integridade, limite por CPF, capacidade e cupom
- as rotas sensiveis de usuarios e reservas exigem autorizacao do proprio cliente ou do administrador
- o projeto esta preparado para deploy no Railway com configuracao por variavel de ambiente e volume persistente para o banco SQLite
- a API usa Gmail SMTP em producao quando as variaveis de ambiente forem configuradas corretamente
- existem testes automatizados com xUnit cobrindo regras principais e riscos de seguranca
