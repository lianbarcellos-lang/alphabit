# Operacao do GeekTop

Este documento consolida os riscos operacionais do GeekTop e referencia as fichas de SLO, metricas e politica de error budget usadas na apresentacao.

## Matriz de Riscos

| Risco | Probabilidade | Impacto | Estratégia | Ação | Gatilho |
| --- | --- | --- | --- | --- | --- |
| Venda acima da capacidade do evento | Media | Alto | Mitigar | Revalidar capacidade dentro da transacao e bloquear novas reservas quando o limite for atingido | Soma de reservas confirmadas igual ou maior que a capacidade |
| Uso incorreto de cupons | Media | Alto | Mitigar | Validar cupom, valor minimo e recalcular o total no checkout antes de persistir a reserva | Cupom informado no checkout ou desconto inesperado |
| Cadastro duplicado por CPF | Media | Medio | Prevenir | Retornar `400 BadRequest`, manter CPF como identificador de usuario e impedir novo insert | CPF ja existente na tabela `Usuarios` |
| Alocacao incorreta de stand | Media | Medio | Mitigar | Exigir acesso administrativo, validar ocupante e permitir ajuste manual de coordenadas apos organizacao automatica | Tentativa de reservar stand sem ocupante, por cliente ou fora da planta |
| Check-in duplicado | Media | Alto | Prevenir | Usar QR Code unico, indice unico por reserva e rejeitar check-in ja usado | Segundo POST em `/api/checkin` para a mesma reserva |
| Reserva cancelada tentando entrar | Baixa | Alto | Prevenir | Rejeitar validacao de check-in quando a reserva estiver cancelada | Check-in com reserva em status cancelado |
| Queda da API durante apresentacao | Baixa | Alto | Contingenciar | Reiniciar servico no Railway, verificar logs e validar rotas criticas | Falha de resposta em `/api/eventos` ou no App publico |
| Perda de dados SQLite no deploy | Media | Alto | Mitigar | Usar volume persistente no Railway e evitar recriar banco sem backup | Novo deploy sem reservas ou eventos esperados |

## Gatilhos de Risco

| Gatilho | Severidade | Verificacao | Resposta esperada |
| --- | --- | --- | --- |
| Build falhou | Alta | `dotnet build .\Alphabit.sln` | Corrigir antes de qualquer deploy |
| Testes falharam | Alta | `dotnet test .\tests\Alphabit.Tests\Alphabit.Tests.csproj --no-restore` | Bloquear publicacao ate todos passarem |
| SLO de reservas abaixo de 95% | Alta | Ficha em `docs/slo.md` | Ativar politica de error budget nivel 3 |
| Erro no checkout ou cupom | Alta | Smoke test de compra | Priorizar correcao transacional |
| Falha de login admin | Media | Teste manual do painel ADM | Conferir variaveis `AdminAccess__*` |
| QR Code nao valida | Media | Teste manual com codigo e camera | Manter validacao manual como contingencia |

## Referencias Operacionais

- `docs/slo.md`: ficha de SLO e SLI.
- `docs/error_budget_policy.md`: politica de error budget com 3 niveis.
- `docs/metricas_dora.md`: metricas DORA em formato de ficha.
- `docs/metricas_qualidade.md`: metricas de qualidade do projeto.
- `docs/threat_model_e_gates.md`: ameacas, controles e gates.
