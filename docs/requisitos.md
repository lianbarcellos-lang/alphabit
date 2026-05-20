# Requisitos do Sistema Alphabit

## Escopo implementado

O Alphabit possui uma API em .NET 9 com SQLite e Dapper, junto com uma interface Blazor Server. O sistema cobre cadastro e autenticacao de usuarios, recuperacao de senha por email, gestao administrativa de eventos, cupons, cidades e categorias geek, fluxo de compra com carrinho, tipos de ingresso, assentos, reservas e filtros de descoberta.

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
Como administrador, quero cadastrar eventos com nome, local, cidade, atracao principal, categoria geek, capacidade, data, preco e imagem, para publicar a agenda da plataforma.

### HU08 - Gestao de eventos
Como administrador, quero editar e excluir eventos, para manter a agenda atualizada.

### HU09 - Catalogo de cidades
Como administrador, quero selecionar, criar, editar e excluir cidades de um catalogo, para evitar digitacao repetida e erros.

### HU10 - Catalogo de categorias geek
Como administrador, quero selecionar, criar, editar e excluir categorias geek de um catalogo, para padronizar os eventos cadastrados.

### HU11 - Cadastro de cupons
Como administrador, quero cadastrar cupons promocionais com codigo, desconto e valor minimo, para criar campanhas.

### HU12 - Gestao de cupons
Como administrador, quero editar e excluir cupons, para ajustar campanhas promocionais.

### HU13 - Vitrine publica de eventos
Como cliente, quero visualizar os eventos disponiveis com imagem e informacoes principais, para escolher o que desejo comprar.

### HU14 - Filtros de descoberta
Como cliente, quero filtrar eventos por cidade, dia da semana, atracao e categoria geek, para encontrar opcoes mais relevantes.

### HU15 - Detalhe do evento
Como cliente, quero abrir uma pagina de detalhe do evento, para entender local, data, preco e seguir para os assentos.

### HU16 - Selecao de assentos
Como cliente, quero escolher assentos disponiveis antes de comprar, para montar meu pedido.

### HU17 - Carrinho e cupom
Como cliente, quero aplicar um cupom no carrinho e ver o desconto antes de finalizar, para saber o valor final da compra.

### HU18 - Finalizacao da compra
Como cliente, quero concluir a compra de ingressos, para garantir minha reserva.

### HU19 - Forma de pagamento no carrinho
Como cliente, quero escolher entre Pix, Cartao e Boleto no carrinho, para finalizar a compra com a forma de pagamento desejada.

### HU20 - Consulta de reservas
Como cliente, quero consultar minhas reservas, para acompanhar meus ingressos confirmados.

### HU21 - Relatorio de vendas do administrador
Como administrador, quero visualizar um dashboard com cliente, evento, data, pagamento, status, valor, quantidade, tipo de ingresso, pedido, check-ins, capacidade, cupons e avaliacoes, para acompanhar a operacao comercial.

### HU22 - Separacao de contextos no painel administrativo
Como administrador, quero alternar entre controle de ingressos e relatorio de vendas, para trabalhar com menos poluicao visual.

### HU23 - Protecao de perfil e reservas
Como sistema, quero restringir acesso a perfil e reservas ao proprio cliente ou administrador, para proteger dados pessoais.

### HU24 - Protecao de usuarios administrativos
Como sistema, quero proteger a listagem de usuarios para uso apenas administrativo, para evitar exposicao indevida.

### HU25 - Seguranca contra SQL Injection
Como sistema, quero usar Dapper com parametros nomeados, para reduzir o risco de SQL Injection.

### HU26 - Qualidade automatizada
Como desenvolvedor, quero manter testes automatizados com Assert, para validar o comportamento esperado do sistema.

### HU27 - Tipos de ingresso por evento
Como cliente, quero escolher entre tipos de ingresso com precos e beneficios diferentes, para comprar a experiencia desejada.

### HU28 - Atividades internas do evento
Como cliente, quero me inscrever em atividades internas com limite de vagas, para participar da programacao do evento.

### HU29 - Convidados do evento
Como cliente, quero visualizar convidados associados ao evento, para decidir se o evento e relevante para mim.

### HU30 - Check-in por QR Code
Como administrador, quero validar a entrada pelo QR Code da reserva, para controlar acesso e impedir uso duplicado.

### HU31 - Avaliacoes pos-evento
Como cliente, quero avaliar um evento reservado, para registrar minha experiencia e ajudar outros clientes.

### HU32 - Moderacao de avaliacoes
Como administrador, quero remover avaliacoes indevidas, para manter a vitrine confiavel.

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

Cenario: gerenciar catalogo de categorias geek  
Dado que o administrador deseja padronizar generos  
Quando ele cria, edita ou exclui um genero no catalogo  
Entao a lista deve ser atualizada e reutilizavel no cadastro de eventos

Cenario: cadastrar cupom  
Dado que o administrador esta autenticado  
Quando informa codigo, desconto e valor minimo  
Entao o cupom deve ser criado

Cenario: visualizar relatorio de vendas  
Dado que existem compras registradas  
Quando o administrador abre a visao comercial  
Entao o sistema deve mostrar totais, formas de pagamento, ranking de eventos e a tabela detalhada das compras

Cenario: alternar entre as visoes do painel  
Dado que o administrador esta no painel  
Quando ele clica em controle de ingressos ou relatorio de vendas  
Entao a tela deve exibir apenas os blocos do contexto selecionado

Cenario: acompanhar metricas da pivotagem  
Dado que existem reservas, cupons, check-ins e avaliacoes  
Quando o administrador abre o dashboard  
Entao o sistema deve mostrar receita, ingressos vendidos, capacidade restante, eventos populares, uso de cupons, check-ins e avaliacoes recentes

### Compra e reservas

Cenario: concluir compra valida  
Dado que o usuario existe  
E que o evento existe  
Quando a compra respeita capacidade, limite por CPF e regra de cupom  
Entao a reserva deve ser salva com sucesso

Cenario: registrar forma de pagamento na compra  
Dado que o cliente esta no carrinho  
Quando ele escolhe Pix, Cartao ou Boleto e finaliza  
Entao a reserva deve salvar a forma de pagamento, o status correspondente e o codigo do pedido

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

Cenario: reservar por tipo de ingresso  
Dado que o evento possui tipos de ingresso ativos  
Quando o cliente seleciona um tipo de ingresso  
Entao o valor final deve usar o preco do tipo escolhido e respeitar a disponibilidade dele

Cenario: validar entrada por QR Code  
Dado que uma reserva possui QR Code valido  
Quando o administrador realiza o check-in  
Entao o sistema deve marcar o QR Code como usado e bloquear uma segunda validacao

Cenario: avaliar evento uma unica vez  
Dado que o cliente possui reserva para o evento  
Quando ele envia uma avaliacao valida  
Entao a avaliacao deve ser salva e uma nova avaliacao do mesmo cliente para o mesmo evento deve ser rejeitada

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
- `GET /api/eventos/{id}/assentos-ocupados`
- `GET /api/eventos/{id}/tipos-ingresso`
- `GET /api/eventos/{id}/atividades`
- `GET /api/eventos/{id}/convidados`
- `GET /api/eventos/{id}/avaliacoes`
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
- `GET /api/dashboard`

### Atividades, convidados, avaliacoes e check-in

- `GET /api/convidados`
- `POST /api/convidados`
- `POST /api/eventos/{id}/convidados`
- `DELETE /api/eventos/{id}/convidados/{convidadoId}`
- `DELETE /api/admin/convidados/{id}`
- `POST /api/atividades`
- `POST /api/atividades/{id}/inscricao`
- `DELETE /api/admin/atividades/{id}`
- `POST /api/checkin`
- `POST /api/avaliacoes`
- `DELETE /api/admin/avaliacoes/{id}`

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
- `TipoIngressoId` INTEGER NULL
- `CupomUtilizado` TEXT
- `Assentos` TEXT NOT NULL
- `Quantidade` INTEGER NOT NULL
- `PrecoUnitario` REAL NOT NULL
- `ValorFinalPago` REAL NOT NULL
- `FormaPagamento` TEXT NOT NULL
- `StatusPagamento` TEXT NOT NULL
- `CodigoPedido` TEXT NOT NULL
- `DataReserva` TEXT NOT NULL

### Tabela `TiposIngresso`

- `Id` INTEGER PRIMARY KEY AUTOINCREMENT
- `EventoId` INTEGER NOT NULL
- `Nome` TEXT NOT NULL
- `Beneficios` TEXT NOT NULL
- `Preco` REAL NOT NULL
- `QuantidadeDisponivel` INTEGER NOT NULL

### Tabela `Atividades`

- `Id` INTEGER PRIMARY KEY AUTOINCREMENT
- `EventoId` INTEGER NOT NULL
- `Nome` TEXT NOT NULL
- `Horario` TEXT NOT NULL
- `Tipo` TEXT NOT NULL
- `LimiteParticipantes` INTEGER NOT NULL

### Tabela `InscricoesAtividades`

- `Id` INTEGER PRIMARY KEY AUTOINCREMENT
- `AtividadeId` INTEGER NOT NULL
- `UsuarioCpf` TEXT NOT NULL
- `CriadoEm` TEXT NOT NULL

### Tabela `Convidados`

- `Id` INTEGER PRIMARY KEY AUTOINCREMENT
- `Nome` TEXT NOT NULL
- `Tipo` TEXT NOT NULL
- `Descricao` TEXT NOT NULL
- `ImagemUrl` TEXT

### Tabela `EventoConvidados`

- `EventoId` INTEGER NOT NULL
- `ConvidadoId` INTEGER NOT NULL

### Tabela `Checkins`

- `Id` INTEGER PRIMARY KEY AUTOINCREMENT
- `ReservaId` INTEGER NOT NULL
- `QrCode` TEXT NOT NULL
- `DataCheckin` TEXT NULL
- `Status` TEXT NOT NULL

### Tabela `Avaliacoes`

- `Id` INTEGER PRIMARY KEY AUTOINCREMENT
- `EventoId` INTEGER NOT NULL
- `UsuarioCpf` TEXT NOT NULL
- `Nota` INTEGER NOT NULL
- `Comentario` TEXT
- `CriadoEm` TEXT NOT NULL

### Tabela `RecuperacoesSenha`

- `Id` INTEGER PRIMARY KEY AUTOINCREMENT
- `UsuarioCpf` TEXT NOT NULL
- `CodigoHash` TEXT NOT NULL
- `EmailDestino` TEXT NOT NULL
- `ExpiraEm` TEXT NOT NULL
- `TentativasInvalidas` INTEGER NOT NULL
- `UsadoEm` TEXT NULL
- `CriadoEm` TEXT NOT NULL

## Observacoes tecnicas

- o acesso a dados principal utiliza Dapper com parametros nomeados
- as queries de negocio estao parametrizadas
- senhas de usuarios sao armazenadas como hash SHA-256
- a API possui fluxo de recuperacao de senha por email com codigo temporario
- o app Blazor possui modal de login, cadastro, redefinicao de senha e exibicao de senha
- a vitrine publica possui filtros por cidade, dia da semana, atracao e categoria geek
- o carrinho mostra pre-visualizacao do desconto antes da finalizacao e permite escolher a forma de pagamento
- a rota `GET /api/reservas/{cpf}` usa `INNER JOIN` para retornar o nome do evento
- a rota `POST /api/reservas` aplica validacao de integridade, limite por CPF, capacidade e cupom
- o painel administrativo tem uma visao separada de relatorio comercial
- o dashboard comercial mostra totais, formas de pagamento, eventos mais vendidos e ultimas compras
- o dashboard administrativo tambem mostra check-ins, capacidade, uso de cupons e avaliacoes recentes
- o check-in usa QR Code unico por reserva e impede reutilizacao
- as avaliacoes exigem reserva do usuario, nota valida e bloqueiam duplicidade por usuario/evento
- as datas de compra do historico e do relatorio sao convertidas para o horario de Brasilia
- as rotas sensiveis de usuarios e reservas exigem autorizacao do proprio cliente ou do administrador
- o projeto esta preparado para deploy no Railway com configuracao por variavel de ambiente e volume persistente para o banco SQLite
- a API suporta envio de email tanto por Gmail SMTP quanto por API HTTP compativel com endpoint `/emails`
- existem testes automatizados com xUnit cobrindo regras principais e riscos de seguranca
