# Roadmap da Pivotagem - Alphabit Geek / Anime

Este roadmap organiza as especificacoes necessarias para transformar o Alphabit em uma plataforma de eventos Geek / Anime / Gaming sem trocar a stack atual do projeto.

## Stack mantida

- C# e .NET 9
- ASP.NET Core Minimal API
- Blazor Server / Razor Components
- SQLite
- Dapper
- xUnit

## Fora do escopo tecnico

Nao implementar nesta pivotagem:

- Entity Framework
- JWT
- Swagger obrigatorio
- MariaDB
- SQL Server
- Docker
- Clean Architecture
- Microservices
- CQRS
- MediatR

## Fase 0 - Preparacao e seguranca

Objetivo: garantir que a base atual esteja consistente antes de iniciar mudancas funcionais.

Entregas:

- confirmar nomes do projeto como Alphabit
- manter portas locais separadas de outros projetos
- revisar documentos de pivotagem
- manter backup ou copia do estado atual
- rodar build e testes antes de cada fase relevante

Criterios de aceite:

- `dotnet build .\Alphabit.sln` executa com sucesso
- `dotnet test .\tests\Alphabit.Tests\Alphabit.Tests.csproj` executa com sucesso
- documentacao da pivotagem existe em `docs`

## Fase 1 - Rebranding funcional

Objetivo: adaptar o sistema atual para o contexto Geek / Anime sem mudar a arquitetura.

Backend:

- revisar textos, mensagens e dados de exemplo para eventos geek/anime
- manter rotas atuais funcionando
- preservar usuarios, eventos, reservas e cupons
- adaptar nomes de campos visiveis quando fizer sentido

Frontend:

- atualizar textos da interface para a nova proposta
- adaptar cards de eventos para cultura pop, anime, games e cosplay
- destacar categoria, cidade, data e preco
- manter fluxo de compra/reserva existente

Banco:

- preservar tabelas atuais
- adicionar colunas somente quando necessario e com migracao simples no startup

Testes:

- garantir que testes atuais continuem passando
- atualizar testes que dependem de textos antigos

Criterios de aceite:

- usuario consegue listar eventos
- usuario consegue reservar ingresso
- cupons continuam funcionando
- interface ja comunica a nova proposta Geek / Anime

## Fase 2 - Categorias geek e eventos enriquecidos

Objetivo: ampliar o cadastro de eventos para suportar classificacao e detalhes do novo dominio.

Status atual: parcialmente implementado com adaptacao ao schema existente.

Tabelas/colunas previstas:

- `CategoriasGeek` ou catalogo equivalente `GenerosMusicais`
- `Eventos.CategoriaId`, `Eventos.Categoria` ou campo equivalente `GeneroMusical`
- `Eventos.FaixaEtaria` ainda pendente se a entrega exigir o campo literal
- `Eventos.BannerUrl` atendido por `ImagemUrl`
- `Eventos.Organizadora` ainda pendente se a entrega exigir o campo literal

Endpoints previstos:

- `GET /api/categorias` ou equivalente administrativo `GET /api/admin/generos`
- `POST /api/categorias` ou equivalente administrativo `POST /api/admin/generos`
- `GET /api/eventos`
- `GET /api/eventos/{id}`
- `POST /api/eventos`
- `PUT /api/eventos/{id}` ou equivalente administrativo `PUT /api/admin/eventos/{id}`

Regras:

- eventos devem ter categoria valida
- eventos cancelados nao devem aceitar novas reservas
- filtros por categoria e cidade devem funcionar

Frontend:

- filtro por categoria
- filtro por cidade
- exibicao de banner do evento
- badges de categoria

Testes:

- criar evento com categoria valida
- rejeitar categoria inexistente quando aplicavel
- filtrar eventos por categoria
- filtrar eventos por cidade

## Fase 3 - Tipos de ingresso e regras de preco

Objetivo: permitir ingressos diferentes para o mesmo evento.

Status atual: implementado.

Tabela prevista:

- `TiposIngresso`

Campos:

- `Id`
- `EventoId`
- `Nome`
- `Beneficios`
- `Preco`
- `QuantidadeDisponivel`

Tipos iniciais:

- Normal
- VIP
- Premium
- Day Pass
- Meet and Greet

Reservas:

- adicionar `TipoIngressoId`
- calcular `ValorFinalPago` com base no tipo escolhido
- manter compatibilidade com `PrecoPadrao` quando nao houver tipo cadastrado

Regras:

- ingresso VIP deve ter preco proprio
- Meet and Greet deve ter vagas limitadas
- nao vender acima da quantidade disponivel

Frontend:

- selecao de tipo de ingresso
- exibicao de beneficios
- calculo visual de preco final

Testes:

- calcular preco VIP
- impedir venda acima da disponibilidade
- reservar ingresso normal
- reservar ingresso Meet and Greet com limite

## Fase 4 - Atividades internas

Objetivo: permitir que eventos tenham programacao interna.

Status atual: implementado.

Tabela prevista:

- `Atividades`
- `InscricoesAtividades`

Campos de atividade:

- `Id`
- `EventoId`
- `Nome`
- `Horario`
- `Tipo`
- `LimiteParticipantes`

Tipos de atividade:

- torneio
- karaoke
- workshop
- concurso de cosplay
- meet and greet

Endpoints previstos:

- `GET /api/eventos/{id}/atividades`
- `POST /api/atividades`
- `POST /api/atividades/{id}/inscricao`

Regras:

- impedir inscricao duplicada
- impedir inscricao acima do limite
- atividade deve pertencer a evento existente

Frontend:

- agenda do evento
- inscricao em atividade
- indicador de vagas restantes

Testes:

- criar atividade
- impedir inscricao duplicada
- impedir inscricao acima do limite

## Fase 5 - Convidados

Objetivo: cadastrar convidados e associar convidados aos eventos.

Status atual: implementado.

Tabelas previstas:

- `Convidados`
- `EventoConvidados`

Tipos:

- Cosplayer
- Streamer
- Influencer
- Voice Actor
- Banda
- Artista
- Youtuber

Endpoints previstos:

- `GET /api/convidados`
- `POST /api/convidados`
- `POST /api/eventos/{id}/convidados`
- `DELETE /api/eventos/{id}/convidados/{convidadoId}`

Frontend:

- lista de convidados do evento
- card de convidado
- area administrativa para associacao

Testes:

- cadastrar convidado
- associar convidado a evento
- remover convidado de evento

## Fase 6 - Check-in com QR Code

Objetivo: permitir validacao de entrada no evento.

Status atual: implementado.

Tabela prevista:

- `Checkins`

Campos:

- `Id`
- `ReservaId`
- `QrCode`
- `DataCheckin`
- `Status`

Endpoints previstos:

- `POST /api/checkin`
- `GET /api/reservas/{cpf}`

Regras:

- QR Code deve ser unico
- reserva cancelada nao pode fazer check-in
- check-in so pode acontecer uma vez
- reserva inexistente deve ser rejeitada

Frontend:

- exibir QR Code na reserva
- tela administrativa de validacao de check-in
- feedback claro para valido, usado, cancelado ou invalido

Testes:

- gerar QR Code unico
- impedir check-in duplicado
- rejeitar reserva cancelada
- aceitar check-in valido

## Fase 7 - Mapa de stands e expositores

Objetivo: permitir que o administrador organize espaços internos do evento para empresas, lojas, arenas e atrações.

Status atual: implementado como melhoria de organização do evento.

Tabela prevista:

- `StandsEspacos`
- precificacao de stands por m² ou preço fixo

Campos:

- `Id`
- `EventoId`
- `Setor`
- `Codigo`
- `PosicaoX`
- `PosicaoY`
- `Largura`
- `Altura`
- `Reservado`
- `NomeOcupante`
- `TipoOcupante`
- `Descricao`

Endpoints previstos:

- `GET /api/eventos/{id}/stands`
- `POST /api/admin/eventos/{id}/stands`
- `PUT /api/admin/eventos/{id}/mapa-imagem`
- `PUT /api/admin/eventos/{id}/stands/{standId}`
- `DELETE /api/admin/eventos/{id}/stands/{standId}`
- `POST /api/admin/eventos/{id}/stand-setores`
- `PUT /api/admin/eventos/{id}/stand-setores/{nomeAtual}`
- `DELETE /api/admin/eventos/{id}/stand-setores/{nome}`

Regras:

- stand pertence a um evento;
- stand reservado precisa ter nome de ocupante;
- cliente apenas visualiza o mapa;
- administrador pode reservar e liberar espaços;
- administrador pode aplicar organizacao automatica por grades 2x2, 3x3, 4x4, 5x5 e 8x8;
- grade automatica so fica disponivel quando comporta a quantidade atual de stands;
- depois da grade automatica, o administrador ainda pode arrastar os stands manualmente.

Frontend:

- aba `Mapa de stands` no painel administrativo;
- mapa em blocos clicáveis e planta enviada pelo administrador com organizacao automatica por grade e posicionamento manual;
- visualização publica no detalhe do evento;
- lista por linhas/setores.

Testes:

- tabela e indices existem;
- rota publica expõe o mapa;
- rota administrativa exige token;
- atualização grava ocupante e status;
- tela administrativa exibe as grades automaticas e mantem drag/drop.

## Fase 8 - Dashboard administrativo

Objetivo: oferecer visao administrativa do desempenho dos eventos.

Status atual: implementado.

Metricas:

- total de eventos
- total de reservas
- receita
- eventos mais populares
- ingressos vendidos
- check-ins realizados
- capacidade restante
- uso de cupons

Endpoint previsto:

- `GET /api/dashboard`

Frontend:

- cards de metricas
- tabelas resumidas
- filtros basicos por periodo ou evento

Testes:

- calcular total de reservas
- calcular receita
- listar eventos populares
- calcular capacidade restante

## Fase 9 - Avaliacoes e historico

Objetivo: completar a experiencia pos-evento.

Status atual: implementado.

Tabela prevista:

- `Avaliacoes`

Campos:

- `Id`
- `EventoId`
- `UsuarioCpf`
- `Nota`
- `Comentario`
- `CriadoEm`

Endpoints previstos:

- `POST /api/avaliacoes`
- `GET /api/eventos/{id}/avaliacoes`

Regras:

- usuario so deve avaliar evento reservado ou utilizado
- nota deve respeitar intervalo definido
- impedir avaliacoes duplicadas do mesmo usuario para o mesmo evento

Frontend:

- historico de reservas
- avaliacao do evento
- exibicao de avaliacoes no detalhe do evento

Testes:

- criar avaliacao valida
- impedir avaliacao duplicada
- rejeitar nota invalida

## Fase 10 - Curadoria administrativa de avaliacoes

Objetivo: permitir que o administrador acompanhe a qualidade pos-evento e remova avaliacoes indevidas.

Status atual: implementado como melhoria alem do PDF.

Entregas:

- metricas de total e media de avaliacoes no dashboard
- lista das ultimas avaliacoes no painel administrativo
- endpoint administrativo para remover avaliacao

Endpoint previsto:

- `DELETE /api/admin/avaliacoes/{id}`

Regras:

- apenas administrador pode remover avaliacao
- remocao inexistente deve retornar erro adequado
- dashboard deve refletir avaliacoes removidas

Testes:

- endpoint exige acesso administrativo
- dashboard expoe total, media e ultimas avaliacoes
- remocao usa identificador da avaliacao

## Ordem recomendada

1. Rebranding funcional.
2. Categorias e eventos enriquecidos.
3. Tipos de ingresso.
4. Atividades.
5. Convidados.
6. Check-in com QR Code.
7. Mapa de stands e expositores.
8. Dashboard.
9. Avaliacoes.
10. Curadoria administrativa de avaliacoes.

## Comparacao com o PDF atualizado

PDF considerado: `C:\Users\rapha\Downloads\Geek_Event_Platform_AI_Specification_Clean_SQLite.pdf`.

Atendido:

- stack mantida: C#, ASP.NET Core Minimal API, Blazor Server / Razor Components, Dapper, SQLite e xUnit
- sem Entity Framework, Clean Architecture, Microservices, Redis, Docker, CQRS, MediatR, JWT, Kubernetes, SQL Server ou MariaDB
- modulos de usuarios, eventos, reservas, cupons, atividades, convidados, mapa de stands, tipos de ingresso, dashboard e check-in
- tabelas `Convidados`, `EventoConvidados`, `StandsEspacos`, `Atividades`, `TiposIngresso`, `Reservas`, `Cupons`, `Checkins` e `Avaliacoes`
- regras de capacidade, inscricao duplicada, preco de VIP, limite de Meet and Greet e check-in unico
- testes xUnit cobrindo regras principais e riscos da pivotagem

Adaptado ao projeto atual:

- `CategoriasGeek` foi implementado pelo catalogo existente `GenerosMusicais`, usado como categoria geek.
- `BannerUrl` foi implementado pelo campo existente `ImagemUrl`.
- `Cidade` e `Local` usam os campos existentes `CidadeEvento` e `LocalEvento`.
- `QrCode` fica na tabela `Checkins`, mantendo uma relacao unica com `Reservas`.
- `GET /api/usuarios/{cpf}` usa a rota protegida `GET /api/usuarios/{cpf}/perfil`.

Pendente apenas se for exigido literalmente pelo PDF:

- criar campo `FaixaEtaria` em eventos
- criar campo `Organizadora` em eventos
- criar aliases publicos `GET /api/categorias` e `POST /api/categorias` apontando para o catalogo geek atual

## Checklist por entrega

Cada fase deve terminar com:

- build executando com sucesso
- testes principais passando
- rotas novas documentadas
- telas atualizadas quando houver impacto no frontend
- regras de negocio cobertas por testes quando possivel
- sem troca de stack ou alteracao fora do escopo da pivotagem
