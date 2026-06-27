# Arch - Arquitetura do GeekTop

## Resumo

O GeekTop e uma aplicacao web para eventos geek, composta por uma API em ASP.NET Core Minimal API, um frontend em Blazor Server e testes automatizados com xUnit.

## Arquitetura conceitual

A arquitetura conceitual descreve o projeto pela visao de negocio. O GeekTop e dividido em dois grandes contextos:

- Cliente: descobre eventos, pesquisa, filtra, compra ingressos, acessa reservas, usa QR Code, consulta mapa do evento, inscreve-se em atividades e avalia eventos.
- Administrador: cadastra eventos, gerencia categorias e cidades, cria programacao interna, controla convidados, cupons, stands, dashboard, avaliacoes e check-in.

Principais conceitos do dominio:

- Evento: produto principal da plataforma.
- Ingresso: entrada geral do evento, sem assento marcado.
- Atividade interna: programacao limitada dentro do evento, com inscricao e selecao de lugar.
- Reserva: compra feita pelo cliente.
- Check-in: validacao da entrada por QR Code ou codigo.
- Convidado: pessoa ou grupo associado ao evento.
- Stand: espaco reservado para empresa, loja, patrocinador, arena ou atracao.
- Avaliacao: feedback do cliente apos a reserva.

## Arquitetura tecnica

A arquitetura tecnica descreve como o sistema foi implementado. O projeto usa uma estrutura monolitica simples, com frontend, backend e banco mantidos em uma solucao .NET.

- Frontend: Blazor Server / Razor Components.
- Backend: ASP.NET Core Minimal API.
- Persistencia: SQLite.
- Acesso a dados: Dapper.
- Testes: xUnit.
- Interatividade pontual: JavaScript interop para QR scanner e drag/drop.
- Deploy: execucao local ou Railway em um unico servico Docker de empacotamento.

## Estrutura principal

- `src/Alphabit.API`: backend, rotas HTTP, regras de negocio, inicializacao do banco e consultas Dapper.
- `src/Alphabit.App`: interface Blazor Server, paginas do cliente, painel administrativo e servicos de estado.
- `tests/Alphabit.Tests`: testes de regras, seguranca e riscos de regressao.
- `docs`: documentacao de visao, requisitos, roadmap, ADRs, operacao e arquitetura.

## Fluxo da aplicacao

1. O cliente acessa a vitrine de eventos.
2. O app Blazor consome a API por `AlphabitApiClient`.
3. A API consulta o SQLite usando Dapper e SQL parametrizado.
4. O cliente escolhe ingresso, adiciona ao carrinho e finaliza a reserva.
5. A reserva gera QR Code para check-in.
6. O administrador gerencia eventos, atividades, convidados, cupons, stands, dashboard e check-in.

## Backend

O backend usa Minimal API para manter endpoints diretos e simples. O arquivo `Program.cs` concentra:

- criacao e migracao leve do banco;
- rotas publicas;
- rotas administrativas;
- validacoes;
- consultas SQL;
- seed de dados iniciais;
- indices SQLite idempotentes.

## Frontend

O frontend usa Blazor Server / Razor Components. As telas principais sao:

- eventos;
- detalhe do evento;
- carrinho;
- reservas;
- login/cadastro;
- painel administrativo;
- dashboard;
- mapa de stands;
- check-in.

## Banco de dados

O projeto usa SQLite para facilitar execucao local, apresentacao academica e deploy simples. O acesso e feito com Dapper, sempre com parametros nomeados.

Tabelas centrais:

- `Usuarios`
- `Eventos`
- `TiposIngresso`
- `Reservas`
- `Checkins`
- `Cupons`
- `Atividades`
- `InscricoesAtividades`
- `Convidados`
- `EventoConvidados`
- `StandsEspacos`
- `StandSetores`
- `Avaliacoes`

## Integracoes internas

- `CartState`: mantem carrinho do usuario no app.
- `UserSessionState`: controla usuario logado e token administrativo.
- `EventSearchState`: sincroniza busca de eventos.
- JavaScript interop: usado para QR scanner e drag/drop do mapa de stands.

## Restricoes mantidas

- Sem Entity Framework.
- Sem troca de banco para SQL Server/MariaDB.
- Sem microservices.
- Sem Clean Architecture completa nesta entrega.
- Sem deteccao automatica de planta por imagem.
- Sem recuperacao de senha exposta na interface nesta entrega.

## Deploy atual

Em producao, o GeekTop esta em `https://geektop.store`. O Railway executa um unico servico Docker: a API fica interna em `8081` e o App Blazor fica publico na porta definida por `$PORT`.

## Decisoes de performance

- Filtros de eventos sao cacheados no Blazor.
- Dashboard retorna compras com limite/paginacao.
- Totais do dashboard sao calculados por SQL agregado.
- Imagens novas em base64 sao limitadas.
- Indices SQLite sao criados com `IF NOT EXISTS`.
- Drag/drop do mapa e inicializado apenas quando a aba esta ativa.
