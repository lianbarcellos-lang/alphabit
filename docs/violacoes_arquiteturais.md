# Violacoes Arquiteturais - GeekTop

## Objetivo

Registrar possiveis violacoes arquiteturais, riscos de acoplamento e desvios aceitos temporariamente. Este arquivo complementa `docs/analise_arquitetura.md`.

## Regras Arquiteturais Esperadas

| Regra | Descricao | Status |
| --- | --- | --- |
| UI nao deve acessar banco diretamente | A interface deve consumir a API por `AlphabitApiClient` | Atendida |
| Queries devem ser parametrizadas | Dapper deve receber parametros nomeados | Atendida nas rotas principais |
| Operacoes criticas devem usar transacao | Reservas, check-in, cupom, atividades e exclusoes devem proteger integridade | Atendida |
| Acesso admin deve ser protegido | Rotas administrativas precisam validar token/login admin | Atendida nas rotas criticas |
| Credenciais de producao nao devem ficar hardcoded | Railway deve usar variaveis de ambiente | Atendida para deploy; credenciais de demonstracao ficam documentadas |
| Alteracoes de stack devem passar por ADR | Mudancas como banco, ORM ou auth devem ser justificadas | Atendida via `docs/adrs/` |

## Violacoes ou Riscos Encontrados

| ID | Item | Tipo | Severidade | Situacao | Acao Recomendada |
| --- | --- | --- | --- | --- | --- |
| VA-01 | `Program.cs` concentra muitas rotas e funcoes auxiliares | Acoplamento | Media | Aceito temporariamente | Extrair grupos de endpoints por modulo apos a entrega |
| VA-02 | SQLite usado no Railway academico | Restricao operacional | Media | Aceito com mitigacao | Usar volume persistente e manter transacoes |
| VA-03 | Login administrativo simples por token/configuracao | Seguranca | Media | Aceito para escopo academico | Evoluir para Identity/JWT apenas se a stack permitir |
| VA-04 | Testes arquiteturais usam leitura textual de arquivos | Testabilidade | Baixa | Aceito | Manter como guarda de regressao ate criar testes de integracao |
| VA-05 | App e API no mesmo servico Railway | Deploy | Baixa | Aceito | Separar servicos apenas se houver necessidade real de escala |

## Itens Nao Considerados Violacao

| Item | Justificativa |
| --- | --- |
| Manter nomes internos `Alphabit.*` | Decisao consciente para evitar quebrar projeto e historico |
| Nao usar JWT | A stack final foi mantida conforme o projeto existente e o fluxo atual usa sessao/servicos |
| Nao usar Swagger obrigatorio | A prioridade foi entrega funcional e documentacao textual |
| Nao trocar SQLite por MariaDB/SQL Server | Troca de banco aumentaria risco perto da entrega |

## Acompanhamento

As acoes de melhoria estao priorizadas em:

- `docs/divida_tecnica.md`
- `docs/priorizacao_divida.md`
- `docs/plano_iteracao.md`
