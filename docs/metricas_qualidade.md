# Metricas de Qualidade - GeekTop

## Objetivo

Definir metricas de qualidade que complementam as metricas DORA.

## Metrica 1 - Taxa de Testes Automatizados Aprovados

| Campo | Valor |
| --- | --- |
| Nome | Taxa de testes automatizados aprovados |
| Objetivo | Garantir que regras principais continuam funcionando |
| Formula | `(testes aprovados / total de testes executados) * 100` |
| Fonte de dados | Saida de `dotnet test` |
| Frequencia de coleta | A cada commit relevante e antes de deploy |
| Responsavel | Desenvolvedor da mudanca |
| Meta/Acao | Meta: 100%; se falhar, bloquear commit/deploy ate corrigir |

## Metrica 2 - Taxa de Fluxos Criticos Validados

| Campo | Valor |
| --- | --- |
| Nome | Taxa de fluxos criticos validados manualmente |
| Objetivo | Confirmar que o usuario consegue usar o sistema alem dos testes automatizados |
| Formula | `(fluxos validados / fluxos planejados) * 100` |
| Fonte de dados | Checklist de entrega e smoke test |
| Frequencia de coleta | Antes de apresentacao e apos deploy Railway |
| Responsavel | Equipe |
| Meta/Acao | Meta: 100% dos fluxos de eventos, login, carrinho, reservas, admin, atividades e stands |

## Metrica 3 - Defeitos Encontrados em Apresentacao

| Campo | Valor |
| --- | --- |
| Nome | Defeitos em apresentacao |
| Objetivo | Medir qualidade percebida no momento de avaliacao |
| Formula | Quantidade de bugs bloqueantes encontrados durante demonstracao |
| Fonte de dados | Registro da equipe e feedback do professor |
| Frequencia de coleta | Por apresentacao |
| Responsavel | Equipe |
| Meta/Acao | Meta: 0 bug bloqueante; se ocorrer, registrar em `docs/lesson.md` e corrigir |
