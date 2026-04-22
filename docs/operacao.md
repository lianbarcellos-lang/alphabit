# Operacao do TicketPrime

## Matriz de Riscos

| Risco | Probabilidade | Impacto | Ação | Gatilho |
| --- | --- | --- | --- | --- |
| Venda acima da capacidade do evento | Media | Alto | Bloquear novas reservas quando a capacidade for atingida | Soma de reservas igual ou maior que a capacidade |
| Uso incorreto de cupons | Media | Alto | Validar cupom, valor minimo e recalcular o total antes do insert | Cupom informado no checkout |
| Cadastro duplicado por CPF | Media | Medio | Retornar `400 BadRequest` e impedir novo insert | CPF ja existente na tabela `Usuarios` |
| Queda da API durante apresentacao | Baixa | Alto | Reiniciar a API, revisar logs locais e validar banco e rotas criticas | Falha de resposta em rota principal |

## Metrica Operacional

Fórmula: `(reservas concluidas com sucesso / tentativas de reserva) * 100`

Fonte de Dados: respostas das rotas `POST /api/reservas` e registros persistidos no banco SQLite

Frequência: verificacao a cada dia de uso ou antes de apresentacoes importantes

Ação se Violado: revisar regras de negocio, logs de erro, dados de cupom, capacidade dos eventos e corrigir a causa antes de liberar nova demonstracao

## Objetivo de Serviço

SLO: 95% de reservas concluidas com sucesso em uma janela de 7 dias

## Error Budget Policy

Error Budget Policy:
- se o SLO ficar abaixo da meta, o time deve interromper novas evolucoes visuais e priorizar estabilidade
- corrigir primeiro falhas de reserva, capacidade, cupom e integridade de dados
- executar novamente os testes automatizados antes de novas entregas
- somente retomar novas funcionalidades depois que a taxa minima prometida voltar a ser atendida
