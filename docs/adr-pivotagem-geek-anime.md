# ADR - Pivotagem para Plataforma Geek / Anime

## Status

Aceito.

## Contexto

O projeto Alphabit nasceu como uma plataforma de reservas de ingressos. A nova direcao do produto e transformar essa base em uma plataforma de eventos Geek / Anime / Gaming, mantendo o que ja funciona e evitando uma reescrita completa.

A pivotagem precisa aproveitar a estrutura existente:

- API em ASP.NET Core Minimal API
- frontend em Blazor Server / Razor Components
- persistencia com SQLite
- acesso a dados com Dapper
- testes com xUnit
- logica atual de usuarios, eventos, reservas e cupons

Tambem foi definido que a pivotagem nao deve introduzir novas tecnologias estruturais como Entity Framework, JWT, MariaDB, SQL Server, microservices, CQRS, MediatR ou Clean Architecture. O Dockerfile usado no Railway e apenas empacotamento de deploy, nao uma nova camada arquitetural da aplicacao.

## Decisao

Foi decidido pivotar o produto para uma plataforma Geek / Anime sem trocar a stack principal do projeto.

A nova identidade funcional do sistema passa a priorizar:

- eventos de anime, games, cosplay, cultura pop e card games
- categorias geek para classificacao dos eventos
- atividades internas dos eventos
- convidados e associacao de convidados aos eventos
- mapa de stands e expositores por evento, com organizacao automatica por grade e ajuste manual
- tipos de ingresso como Normal, VIP, Premium, Day Pass e Meet and Greet
- check-in com QR Code
- dashboard administrativo
- avaliacoes e historico

A implementacao deve ser incremental. A primeira fase deve preservar e adaptar os modulos ja existentes antes de criar modulos novos.

## Consequencias

Pros:

- reduz risco de quebrar o projeto atual
- evita troca desnecessaria de banco, autenticacao ou arquitetura
- mantem o projeto simples de executar localmente
- permite entregar a pivotagem por fases
- reaproveita a logica existente de reservas, usuarios, eventos e cupons

Contras:

- o arquivo principal da API pode continuar crescendo se nao houver organizacao gradual
- SQLite limita alguns cenarios de concorrencia em comparacao com bancos cliente-servidor
- a ausencia de JWT mantem a autenticacao mais simples e menos robusta para producao
- algumas regras novas exigirao cuidado para nao quebrar fluxos antigos de reserva

## Regras de implementacao

- Manter SQLite como banco de dados.
- Manter Dapper como ferramenta de acesso a dados.
- Manter Minimal API como estilo do backend.
- Manter Blazor Server / Razor Components no frontend.
- Usar SQL parametrizado.
- Nao concatenar SQL com dados de usuario.
- Preservar a logica de reservas existente.
- Criar testes xUnit para cada regra de negocio nova ou alterada.
- Priorizar alteracoes pequenas, verificaveis e compativeis com o projeto atual.

## Criterios de sucesso

A pivotagem sera considerada bem encaminhada quando:

- a identidade visual e textual do sistema estiver alinhada com eventos Geek / Anime
- eventos tiverem categoria, cidade, local, faixa etaria, banner e organizadora
- reservas suportarem tipos de ingresso e status
- existir controle de capacidade para eventos e atividades
- existir mapa de stands por evento com alocacao administrativa e visualizacao publica
- o administrador conseguir organizar stands automaticamente por grade antes dos ajustes manuais
- check-in impedir duplicidade
- dashboard apresentar metricas administrativas basicas
- os testes principais continuarem passando
