# Metricas DORA - GeekTop

As metricas DORA ajudam a acompanhar capacidade de entrega e estabilidade. Como o projeto e academico, os valores podem ser apurados manualmente a cada ciclo.

## 1. Deployment Frequency

| Campo | Valor |
| --- | --- |
| Nome | Frequencia de Deploy |
| Objetivo | Saber quantas vezes o projeto e publicado com sucesso |
| Formula | Numero de deploys concluidos no Railway por semana |
| Fonte de dados | Historico de deploys do Railway e commits no GitHub |
| Frequencia de coleta | Semanal ou antes da apresentacao |
| Responsavel | Equipe de desenvolvimento |
| Meta/Acao | Meta: pelo menos 1 deploy validado por entrega; se nao houver, revisar pipeline |

## 2. Lead Time for Changes

| Campo | Valor |
| --- | --- |
| Nome | Tempo de Entrega de Mudanca |
| Objetivo | Medir tempo entre inicio da alteracao e deploy validado |
| Formula | Hora do deploy validado - hora do primeiro commit da mudanca |
| Fonte de dados | GitHub, Railway e registro da equipe |
| Frequencia de coleta | Por mudanca relevante |
| Responsavel | Quem fez a mudanca |
| Meta/Acao | Meta: ate 1 dia para ajustes pequenos; se passar disso, quebrar tarefa |

## 3. Change Failure Rate

| Campo | Valor |
| --- | --- |
| Nome | Taxa de Falha de Mudanca |
| Objetivo | Saber quantas publicacoes quebram fluxo principal |
| Formula | Deploys com rollback ou hotfix / total de deploys |
| Fonte de dados | Railway, GitHub e checklist manual |
| Frequencia de coleta | Semanal |
| Responsavel | Equipe |
| Meta/Acao | Meta: abaixo de 20%; se passar, congelar melhorias e corrigir estabilidade |

## 4. Mean Time to Restore

| Campo | Valor |
| --- | --- |
| Nome | Tempo Medio de Restauracao |
| Objetivo | Medir rapidez para recuperar o sistema apos falha |
| Formula | Soma do tempo de indisponibilidade / numero de incidentes |
| Fonte de dados | Logs, horario do erro e horario da correcao |
| Frequencia de coleta | A cada incidente |
| Responsavel | Equipe |
| Meta/Acao | Meta: restaurar em ate 30 minutos durante apresentacao; se passar, melhorar rollback |
