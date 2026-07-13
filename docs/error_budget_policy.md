# Error Budget Policy - GeekTop

## Objetivo

Definir como a equipe reage quando o SLO e consumido por erros.

## SLO de Referencia

O SLO principal esta em `docs/slo.md`: 95% de reservas concluidas com sucesso em janela de 7 dias.

## Niveis de Error Budget

| Nivel | Consumo do budget | Situacao | Acao |
| --- | --- | --- | --- |
| Nivel 1 - Normal | 0% a 50% | Erros isolados e sem impacto continuo | Continuar evolucao, registrar bugs e manter testes |
| Nivel 2 - Atencao | 51% a 80% | Erros recorrentes em fluxo importante | Pausar melhorias visuais, corrigir causa raiz e rodar smoke test |
| Nivel 3 - Critico | Acima de 80% ou SLO violado | Risco direto para apresentacao/producao | Congelar novas funcionalidades, corrigir checkout/check-in/login, validar Railway e so liberar apos testes |

## Regras

- Nenhuma melhoria estetica entra em producao no Nivel 3.
- Bugs de reserva, pagamento, check-in e login tem prioridade maxima.
- Todo incidente deve gerar anotacao em `docs/lesson.md`.
- A liberacao so retorna ao normal depois de build, testes e smoke test aprovados.

## Smoke Test Minimo

1. Abrir `https://geektop.store/eventos`.
2. Fazer login ou cadastro.
3. Abrir um evento.
4. Adicionar ingresso ao carrinho.
5. Finalizar reserva.
6. Abrir ingressos/reservas.
7. Validar QR Code no ADM.
