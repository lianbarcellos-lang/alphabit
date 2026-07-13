# Priorizacao de Divida Tecnica

## Metodo

Foi usada uma matriz simples de priorizacao:

`Prioridade = Impacto + Urgencia + Frequencia - Esforco`

Escala de 1 a 5:

- Impacto: quanto afeta usuario, entrega ou manutencao.
- Urgencia: quanto precisa ser tratado antes da proxima entrega.
- Frequencia: quantas vezes aparece no fluxo.
- Esforco: custo estimado para corrigir.

## Backlog Priorizado

| Ordem | ID | Divida | Impacto | Urgencia | Frequencia | Esforco | Score | Decisao |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | DT-01 | `Program.cs` grande | 4 | 3 | 5 | 4 | 8 | Refatorar gradualmente apos entrega |
| 2 | DT-04 | Autenticacao admin simplificada | 5 | 3 | 3 | 4 | 7 | Planejar evolucao de seguranca |
| 3 | DT-03 | SQLite em Railway | 4 | 3 | 3 | 3 | 7 | Manter volume e avaliar banco externo |
| 4 | DT-02 | Testes por leitura textual | 3 | 2 | 4 | 3 | 6 | Complementar com testes de integracao |
| 5 | DT-06 | Ajuste manual do mapa de stands | 2 | 2 | 3 | 4 | 3 | Manter como melhoria futura |
| 6 | DT-05 | API e App no mesmo servico | 2 | 1 | 2 | 4 | 1 | Aceitar enquanto escopo for academico |

## Regra de Execucao

Nenhuma divida tecnica deve ser paga com refatoracao grande antes de uma apresentacao sem:

- branch ou commit separado;
- build aprovado;
- testes aprovados;
- plano de rollback;
- atualizacao de ADR quando mudar arquitetura.
