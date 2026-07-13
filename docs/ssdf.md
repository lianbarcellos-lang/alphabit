# SSDF - Secure Software Development Framework

## Objetivo

Mapear praticas de seguranca do GeekTop usando a logica do SSDF: preparar, proteger, produzir software seguro e responder a vulnerabilidades.

## PO - Prepare the Organization

| Pratica | Como o projeto atende |
| --- | --- |
| Definir responsabilidades | `docs/topologia_times.md` divide papeis e responsabilidades |
| Definir gates | `docs/threat_model_e_gates.md` e `docs/pipeline_liberacao.md` bloqueiam entrega insegura |
| Registrar decisoes | `docs/adrs/` guarda decisoes tecnicas |

## PS - Protect the Software

| Pratica | Como o projeto atende |
| --- | --- |
| Proteger credenciais | Variaveis `AdminAccess__*` devem ser configuradas no Railway |
| Evitar credenciais reais no Git | Nao versionar senhas reais, tokens privados ou chaves de servico |
| Proteger codigo | Mudancas passam por build, testes e commit rastreavel |

## PW - Produce Well-Secured Software

| Pratica | Como o projeto atende |
| --- | --- |
| Validar entradas | Regras em `AlphabitRules` e validacoes nas rotas |
| Proteger SQL | Dapper com parametros nomeados |
| Proteger acesso | `EnsureAdminAccess` e `EnsureUserAccess` nas rotas sensiveis |
| Proteger senha | Hash de senha e comparacao segura em credenciais admin |
| Controlar duplicidade | Indices unicos em reservas, check-ins, avaliacoes e cadastros relevantes |

## RV - Respond to Vulnerabilities

| Pratica | Como responder |
| --- | --- |
| Identificar vulnerabilidade | Registrar em `docs/lesson.md` e abrir item no Kanban |
| Priorizar | Usar `docs/priorizacao_divida.md` e severidade do risco |
| Corrigir | Implementar menor mudanca segura |
| Validar | Rodar build, testes e smoke test |
| Publicar | Commit, push GitHub e redeploy Railway se necessario |

## Observacao

Credenciais de demonstracao podem aparecer na documentacao para apresentacao local, mas credenciais reais de producao nao devem ser versionadas.
