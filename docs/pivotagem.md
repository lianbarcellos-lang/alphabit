# Pivotagem - Plataforma Geek / Anime

Este documento registra a direcao da pivotagem do Alphabit para uma plataforma de eventos Geek / Anime / Gaming, usando como referencia o PDF `Geek_Event_Platform_AI_Specification_Clean_SQLite.pdf`.

## Stack do projeto

A pivotagem deve manter a stack que ja existe no projeto:

- C#
- .NET 9
- ASP.NET Core Minimal API
- Blazor Server / Razor Components
- SQLite
- Dapper
- xUnit

Nao devem ser adicionados nesta pivotagem:

- Entity Framework
- Clean Architecture
- Microservices
- Redis
- Docker
- CQRS
- MediatR
- JWT
- Kubernetes
- SQL Server
- MariaDB

## Objetivo

Transformar o sistema atual de reservas de ingressos em uma plataforma voltada para eventos geek, anime e cultura pop, preservando a estrutura atual do projeto e a logica de reservas existente.

Exemplos de eventos que a plataforma deve atender:

- convencoes de anime
- eventos geek
- festivais de games
- eventos de cosplay
- torneios de card game
- meet and greet

## Regras importantes

- Preservar a estrutura atual do projeto.
- Preservar a logica de reservas existente.
- Usar Dapper para acesso ao banco.
- Manter SQLite como banco de dados.
- Usar consultas SQL parametrizadas.
- Evitar concatenacao de strings SQL.
- Manter a arquitetura Minimal API no backend.
- Evoluir o frontend em Blazor Server / Razor Components.

## Modulos principais

A plataforma deve evoluir para conter os seguintes modulos:

- Usuarios
- Eventos
- Reservas
- Cupons
- Categorias geek
- Atividades
- Convidados
- Tipos de ingresso
- Dashboard
- Check-in
- Avaliacoes

## Adaptacoes ao projeto atual

O PDF de referencia descreve alguns nomes ideais de campos e tabelas. Na implementacao do Alphabit, alguns pontos foram adaptados para preservar a base existente:

- `CategoriasGeek` foi atendido pelo catalogo atual `GenerosMusicais`, usado como categoria geek na interface.
- `BannerUrl` foi atendido pelo campo existente `ImagemUrl`.
- `Cidade` e `Local` foram atendidos por `CidadeEvento` e `LocalEvento`.
- `QrCode` da reserva foi implementado na tabela `Checkins`, com relacao unica por reserva.
- `GET /api/usuarios/{cpf}` foi mantido como rota protegida `GET /api/usuarios/{cpf}/perfil`.

Campos do PDF ainda nao criados de forma literal:

- `FaixaEtaria`
- `Organizadora`

Esses campos podem entrar em uma fase futura sem trocar stack, caso sejam exigidos na apresentacao.

## Eventos

Os eventos devem ser adaptados para o contexto geek/anime.

Campos desejados:

- Id
- Nome
- Categoria
- Cidade
- Local
- DataEvento
- CapacidadeTotal
- PrecoPadrao
- FaixaEtaria
- BannerUrl
- Organizadora

## Atividades

Eventos podem conter atividades internas, como:

- torneios
- karaoke
- workshops
- concursos de cosplay
- meet and greet

Campos desejados:

- Id
- EventoId
- Nome
- Horario
- Tipo
- LimiteParticipantes

## Tipos de ingresso

Tipos de ingresso previstos:

- Normal
- VIP
- Premium
- Day Pass
- Meet and Greet

## Reservas

As reservas atuais devem ser preservadas e expandidas gradualmente.

Campos desejados:

- UsuarioCpf
- EventoId
- TipoIngressoId
- CupomUtilizado
- ValorFinalPago
- QrCode
- Status

## Regras de negocio

- RN01 - impedir reservas acima da capacidade do evento.
- RN02 - impedir inscricao duplicada em atividade.
- RN03 - ingressos VIP devem ter precos diferentes.
- RN04 - meet and greet deve ter vagas limitadas.
- RN05 - impedir check-in duplicado.

## Tabelas previstas

- Usuarios
- Eventos
- CategoriasGeek
- Convidados
- EventoConvidados
- Atividades
- TiposIngresso
- Reservas
- Cupons
- Checkins
- Avaliacoes

## Endpoints previstos

- `POST /api/usuarios`
- `GET /api/usuarios/{cpf}`
- `POST /api/eventos`
- `GET /api/eventos`
- `POST /api/reservas`
- `GET /api/reservas/{cpf}`
- `POST /api/checkin`
- `GET /api/dashboard`

## Testes

Criar ou manter testes xUnit para:

- CPF duplicado
- validacao de reserva
- capacidade do evento
- check-in duplicado
- limite de atividades

## Roadmap

### Fase 1

- usuarios
- eventos
- reservas
- cupons

### Fase 2

- categorias
- banners
- pivotagem visual

### Fase 3

- convidados
- atividades
- tipos de ingresso
- dashboards

### Fase 4

- QR Code
- check-in
- avaliacoes

## Meta final

Criar uma plataforma robusta de eventos Geek / Anime, mantendo as tecnologias, a arquitetura e a base funcional que ja existem no projeto Alphabit.
