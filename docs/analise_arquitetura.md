# Analise de Arquitetura - GeekTop

## Objetivo

Registrar a arquitetura tecnica e conceitual do GeekTop para avaliacao, manutencao e apresentacao. O projeto e uma plataforma academica de eventos geek com vitrine publica, compra de ingressos, programacao interna, mapa de stands, check-in e painel administrativo.

## Contexto

O sistema nasceu da base tecnica Alphabit e foi pivotado para GeekTop. Os nomes internos `Alphabit.*` foram mantidos para reduzir risco de quebra em namespaces, projetos e banco local. A identidade exibida ao usuario e GeekTop.

## Arquitetura Conceitual

| Dominio | Responsabilidade |
| --- | --- |
| Cliente | Cadastro, login, compra, reservas, QR Code e avaliacoes |
| Administrador | Eventos, cupons, convidados, atividades, stands, check-in e dashboard |
| Evento | Dados comerciais, categoria geek, cidade, imagem, capacidade e preco |
| Reserva | Pedido do cliente, tipo de ingresso, quantidade, pagamento e QR Code |
| Atividade | Programacao interna com limite, assentos gerados por capacidade e inscricoes |
| Stand | Espacos comerciais organizados por linhas/setores e posicionados na planta |
| Operacao | Indicadores, riscos, SLO, error budget e processo de liberacao |

## Arquitetura Tecnica

| Camada | Implementacao | Responsabilidade |
| --- | --- | --- |
| Interface | `src/Alphabit.App` com Blazor Server / Razor Components | Telas do cliente e do administrador |
| Cliente HTTP | `AlphabitApiClient` | Centralizar chamadas da interface para a API |
| Estado de UI | `UserSessionState`, `CartState`, `EventSearchState` | Sessao, carrinho e busca |
| API | `src/Alphabit.API` com ASP.NET Core Minimal API | Regras, rotas, validacoes e persistencia |
| Dados | SQLite com Dapper | Consultas SQL parametrizadas e transacoes |
| Testes | `tests/Alphabit.Tests` com xUnit | Regras de negocio e riscos arquiteturais |
| Deploy | Dockerfile raiz + Railway | Publicacao do App e API no mesmo servico |

## Padrões Arquiteturais

| Padrao | Onde aparece | Motivo |
| --- | --- | --- |
| Minimal API | `src/Alphabit.API/Program.cs` | Reduzir complexidade e manter aderencia a stack do projeto |
| Transaction Script | Rotas de reserva, check-in, cupom, atividade e stands | Fluxos diretos com transacao e regras no mesmo caso de uso |
| SQL parametrizado | Chamadas Dapper | Evitar injecao SQL e manter controle das queries |
| State Container | `CartState`, `UserSessionState` | Compartilhar estado no Blazor sem depender de armazenamento externo |
| Server-side rendering interativo | Blazor Server | Manter UI rica com C# e reaproveitar conhecimento da stack |
| ADR | `docs/adrs/` e `docs/adr.md` | Registrar decisoes e consequencias tecnicas |

## Requisitos de Qualidade

| Requisito | Decisao de arquitetura |
| --- | --- |
| Simplicidade | Stack enxuta: .NET, Minimal API, Blazor, Dapper e SQLite |
| Manutencao | Documentos dedicados de arquitetura, ADR, divida tecnica e roadmap |
| Integridade | Transacoes em checkout, check-in, exclusoes e atualizacoes criticas |
| Seguranca | Token admin, hash de senha, validacao de acesso por CPF e SQL parametrizado |
| Portabilidade | Railway com Docker e dominio `geektop.store` |
| Testabilidade | xUnit com testes de regras e verificacoes arquiteturais |

## Limites Conhecidos

| Limite | Impacto | Tratamento |
| --- | --- | --- |
| `Program.cs` concentra muitas rotas | Arquivo grande e mais dificil de navegar | Registrar como divida tecnica e priorizar extracao gradual |
| SQLite em producao academica | Concorrencia menor que bancos cliente-servidor | Usar volume Railway e transacoes nas rotas criticas |
| Blazor Server depende de conexao persistente | Pode sofrer com queda de conexao do navegador | Manter fluxos simples e evitar operacoes longas na UI |
| Login sem recuperacao por e-mail | Evita promessa nao funcional | Funcionalidade removida da interface ate existir provedor real |

## Conclusao

A arquitetura atual e adequada para o escopo academico e para demonstracao no Railway. O principal risco tecnico e o crescimento do `Program.cs`; por isso, a evolucao recomendada e separar casos de uso e repositorios apenas quando houver tempo de manutencao, sem trocar a stack principal antes da entrega.
