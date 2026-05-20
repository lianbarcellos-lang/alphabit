# Correção Alphabit (alphabit) — AV1

## Resumo

| Avaliação | Nota |
| --- | --- |
| **AV1** | **10 / 10** |

---

## AV1 — Detalhamento

| Item | Critério | Resultado | Justificativa |
| --- | --- | --- | --- |
| 1 | `docs/requisitos.md` com pelo menos 3 blocos contendo `Como`, `Quero` e `Para` | ✅ 1 | O arquivo possui 16 histórias de usuário (HU01 a HU16), todas no formato `Como <papel>, quero <ação>, para <objetivo>`. Exemplo: HU01 "Como cliente, quero me cadastrar com CPF, nome, email e senha, para acessar os eventos da plataforma." |
| 2 | `docs/requisitos.md` com pelo menos 1 cenário contendo `Dado`, `Quando` e `Então` | ✅ 1 | Seção "Criterios de aceitacao" traz dezenas de cenários BDD. Exemplo: "Cenario: criar conta com dados validos / Dado que o cliente ainda nao existe / Quando ele envia CPF, nome, email e senha / Entao a conta e criada com sucesso." |
| 3 | `README.md` com blocos de código Markdown contendo comandos de terminal (ex: `dotnet run`, `dotnet build`) | ✅ 1 | O README contém múltiplos blocos ```powershell com `dotnet user-secrets`, `dotnet run --project ...` para API e Blazor, e `dotnet test ...` para os testes. |
| 4 | Pasta `/db` com arquivo `.sql` contendo `CREATE TABLE` | ✅ 1 | `db/script.sql` possui `CREATE TABLE` para `Usuarios`, `Eventos`, `Cupons` e `Reservas`, com chaves primárias, estrangeiras e restrições `NOT NULL`. |
| 5 | `/src` com arquivos `.cs` contendo `app.MapGet` ou `app.MapPost` | ✅ 1 | `src/Alphabit.API/Program.cs` registra dezenas de rotas com `app.MapGet`, `app.MapPost`, `app.MapPut` e `app.MapDelete` (ex: `/api/usuarios`, `/api/eventos`, `/api/reservas`). |
| 6 | `/src` com retornos explícitos de `Results.BadRequest` ou `Results.NotFound` | ✅ 1 | Program.cs usa fail-fast em diversos handlers. Exemplos: `Results.BadRequest("Preencha CPF, nome e email.")`, `Results.NotFound("Evento nao encontrado.")`, `Results.BadRequest("Cupom ja existe.")`. |
| 7 | Uso do caractere `@` nas strings de query Dapper | ✅ 1 | Todas as queries com Dapper passam parâmetros nomeados: `@cpf`, `@email`, `@id`, `@codigo`, `@senhaHash`, `@eventoId`, `@valorMinimo`, entre outros. |
| 8 | Não usar `+` nem interpolação `$"{ }"` em comandos `SELECT/INSERT/UPDATE/DELETE` | ✅ 1 | Buscas por `$"...SELECT/INSERT/UPDATE/DELETE"` e por concatenação com `+` em strings de SQL não retornaram ocorrências. Todas as queries usam literais ou `@"..."` verbatim com parâmetros. |
| 9 | `/tests` com `.cs` contendo `[Fact]` ou `[Theory]` | ✅ 1 | `tests/Alphabit.Tests/AlphabitRulesTests.cs`, `AlphabitRiskTests.cs` e `UnitTest1.cs` contêm várias anotações `[Fact]`, provando que o xUnit está configurado. |
| 10 | `Assert.` dentro dos métodos de teste | ✅ 1 | Os arquivos de teste usam `Assert.Equal`, `Assert.True`, `Assert.False`, `Assert.Contains` e `Assert.DoesNotContain`. Nenhum teste sem Assert. |

**Total AV1: 10 / 10**

---

## Justificativa da nota final

O projeto **alphabit / Alphabit** atinge nota cheia na AV1 demonstrando forte aderência aos fundamentos de engenharia de software cobrados:

- **Documentação de requisitos** completa, com histórias de usuário no formato canônico e cenários BDD para cada regra de negócio.
- **README executável**, com comandos copy-paste para subir API, Blazor e rodar os testes.
- **Banco versionado** por script SQL manual com todas as entidades principais e integridade referencial.
- **Minimal API** com rotas HTTP explícitas e validações fail-fast retornando `BadRequest` e `NotFound` de forma coerente.
- **Acesso a dados seguro** com Dapper e parâmetros nomeados (`@param`), sem qualquer sinal de concatenação ou interpolação em comandos SQL.
- **Suíte xUnit** presente e funcional, com `[Fact]`/`[Theory]` e `Assert.` cobrindo regras de negócio (HashPassword, validação de evento/cupom/checkout) e revisão de riscos (proteção de rotas sensíveis, transações em deletes).

Todos os 10 critérios da AV1 foram atendidos sem ressalvas. **Nota final AV1: 10 / 10.**
