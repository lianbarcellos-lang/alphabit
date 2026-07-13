# Threat Model e Gates - GeekTop

## Objetivo

Registrar ameacas principais e gates de qualidade/seguranca antes de liberar mudancas.

## Ativos

| Ativo | Valor protegido |
| --- | --- |
| Usuarios | CPF, nome, email e senha hash |
| Reservas | Ingresso, pagamento, QR Code e historico |
| Admin | Criacao/edicao de eventos, cupons, stands e check-in |
| Banco SQLite | Dados de demonstracao e operacao |
| Dominio `geektop.store` | Disponibilidade e confianca da apresentacao |

## Atores

| Ator | Permissoes |
| --- | --- |
| Visitante | Ver eventos e detalhes |
| Cliente | Comprar, ver reservas, avaliar e se inscrever em atividades |
| Administrador | Gerenciar catalogos, eventos, cupons, convidados, atividades, stands e check-in |
| Atacante externo | Tentar acessar dados, burlar reserva, usar QR duplicado ou explorar inputs |

## STRIDE

| Categoria | Ameaca | Controle atual | Gate |
| --- | --- | --- | --- |
| Spoofing | Cliente tentando acessar CPF de outro usuario | `EnsureUserAccess` | Teste de rota protegida |
| Tampering | Alterar preco/cupom no checkout | Recalculo no backend e transacao | Teste de checkout |
| Repudiation | Usuario negar check-in | Registro em `Checkins` com status e data | Conferir historico |
| Information Disclosure | Listar usuarios sem admin | `EnsureAdminAccess` | Teste de rota admin |
| Denial of Service | Muitas requisicoes em SQLite | Escopo academico e fluxo simples | Monitorar Railway |
| Elevation of Privilege | Cliente chamar endpoint admin | Token/admin access | Teste de autorizacao |

## Gates de Liberacao

| Gate | Evidencia exigida |
| --- | --- |
| Build | `dotnet build .\Alphabit.sln --no-restore /nr:false -m:1` aprovado |
| Testes | `dotnet test .\tests\Alphabit.Tests\Alphabit.Tests.csproj --no-restore /nr:false -m:1` aprovado |
| Seguranca | Nenhum segredo real adicionado ao Git |
| Dados | Operacoes criticas com validacao e transacao |
| UI | Fluxos principais navegaveis no App |
| Deploy | Railway/DOMINIO validado quando houver publicacao |

## Decisao de Bloqueio

Se qualquer gate falhar, a mudanca nao deve ser publicada ate a correcao.
