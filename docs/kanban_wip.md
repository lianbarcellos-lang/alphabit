# Quadro Kanban e WIP - GeekTop

## Objetivo

Definir um quadro simples para organizar o trabalho da equipe e evitar muitas tarefas abertas ao mesmo tempo.

## Colunas

| Coluna | Significado | WIP |
| --- | --- | --- |
| Backlog | Ideias e requisitos ainda nao iniciados | Sem limite |
| Pronto para fazer | Itens entendidos e pequenos o suficiente | 5 |
| Em desenvolvimento | Codigo ou documento sendo alterado | 2 |
| Em revisao | Mudanca pronta para conferir | 2 |
| Em teste | Build, testes e validacao manual | 2 |
| Pronto para deploy | Commit pronto para publicar | 1 |
| Concluido | Publicado e validado | Sem limite |

## Politicas de WIP

- Nao iniciar nova funcionalidade se houver item quebrado em teste.
- Documentacao entra no mesmo fluxo que codigo.
- Bug de producao passa na frente de melhoria visual.
- Mudanca grande deve ser quebrada em partes menores.

## Exemplo de Cartoes

| Cartao | Coluna sugerida | Observacao |
| --- | --- | --- |
| Ajustar inscricao de atividades com assento | Concluido | Exige teste de bloqueio de assento ocupado |
| Atualizar docs da rubrica | Em desenvolvimento | Afeta ADR, riscos, metricas e DoD |
| Refatorar `Program.cs` | Backlog | Divida tecnica preventiva |
| Melhorar autenticacao admin | Backlog | Evolucao de seguranca |
