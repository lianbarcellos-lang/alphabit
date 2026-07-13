# SLO - Objetivos de Nivel de Servico

## Ficha SLO Principal

| Campo | Valor |
| --- | --- |
| Servico | Reserva de ingresso no GeekTop |
| Jornada do usuario | Cliente escolhe evento, adiciona ingresso ao carrinho e finaliza reserva |
| SLI | Percentual de tentativas de reserva concluidas sem erro |
| SLO | 95% de reservas concluidas com sucesso em janela de 7 dias |
| Janela de medicao | 7 dias ou periodo de preparacao da apresentacao |
| Fonte de dados | Respostas de `POST /api/reservas`, registros em `Reservas` e smoke tests |
| Responsavel | Equipe GeekTop |
| Acao se violado | Acionar `docs/error_budget_policy.md`, congelar evolucao visual e corrigir fluxo de reserva |

## SLO Secundario

| Campo | Valor |
| --- | --- |
| Servico | Check-in administrativo |
| Jornada do usuario | Admin valida QR Code ou codigo manual de uma reserva |
| SLI | Percentual de check-ins validos processados corretamente |
| SLO | 95% de check-ins validos processados sem duplicidade ou erro indevido |
| Janela de medicao | 7 dias |
| Fonte de dados | Rota `POST /api/checkin` e tabela `Checkins` |
| Responsavel | Equipe GeekTop |
| Acao se violado | Priorizar correcao de QR Code, status da reserva e validacao manual |

## Fora do Escopo do SLO

- Erro causado por credencial de administrador digitada incorretamente.
- Falha de internet local do usuario.
- Tentativa de comprar evento sem vagas.
- Tentativa de validar QR Code ja usado.
